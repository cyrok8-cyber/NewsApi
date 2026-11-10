using System.Text.RegularExpressions;
using NewsApi.Repositories;

namespace NewsApi.Services.Censorship
{
    // 1. Пустое сообщение отклоняется, лишние пробелы по краям убираются
    public class EmptyMessageHandler : ChatMessageHandler
    {
        protected override Task ProcessAsync(ChatMessageContext context)
        {
            context.Text = context.Text?.Trim() ?? string.Empty;

            if (context.Text.Length == 0)
                throw new ArgumentException("Сообщение не может быть пустым");

            return Task.CompletedTask;
        }
    }

    // 2. Слишком длинное сообщение отклоняется
    public class MaxLengthHandler : ChatMessageHandler
    {
        public const int MaxLength = 1000;

        protected override Task ProcessAsync(ChatMessageContext context)
        {
            if (context.Text.Length > MaxLength)
                throw new ArgumentException($"Сообщение не может быть длиннее {MaxLength} символов");

            return Task.CompletedTask;
        }
    }

    // 4. Ссылки на внешние ресурсы вырезаются: чат внутренний,
    // пересылать через него внешние ссылки запрещено
    public class LinkFilterHandler : ChatMessageHandler
    {
        public const string Replacement = "[ссылка удалена]";

        private static readonly Regex LinkRegex = new(
            @"(https?://|www\.)\S+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        protected override Task ProcessAsync(ChatMessageContext context)
        {
            if (LinkRegex.IsMatch(context.Text))
            {
                context.Text = LinkRegex.Replace(context.Text, Replacement);
                context.MarkCensored("links");
            }

            return Task.CompletedTask;
        }
    }

    // 5. Запрещённые слова заменяются звёздочками.
    // Список слов тот же, что и для новостей и комментариев (таблица BannedWords),
    // но в чате сообщение не отклоняется, а цензурируется
    public class BannedWordsHandler : ChatMessageHandler
    {
        private readonly IBannedWordRepository _bannedWordRepository;

        public BannedWordsHandler(IBannedWordRepository bannedWordRepository)
        {
            _bannedWordRepository = bannedWordRepository;
        }

        protected override async Task ProcessAsync(ChatMessageContext context)
        {
            var words = await _bannedWordRepository.GetAllWordsAsync();
            var censored = false;

            // Сначала длинные слова, чтобы короткое не "откусило" часть длинного
            foreach (var word in words.Where(w => !string.IsNullOrWhiteSpace(w)).OrderByDescending(w => w.Length))
            {
                var pattern = Regex.Escape(word);
                if (Regex.IsMatch(context.Text, pattern, RegexOptions.IgnoreCase))
                {
                    context.Text = Regex.Replace(context.Text, pattern,
                        match => new string('*', match.Length), RegexOptions.IgnoreCase);
                    censored = true;
                }
            }

            if (censored)
                context.MarkCensored("banned-words");
        }
    }

    // 3. "Антикапс": сообщение, написанное почти целиком заглавными буквами,
    // переводится в обычный регистр (первая буква остаётся заглавной)
    public class CapsLockHandler : ChatMessageHandler
    {
        private const int MinLetters = 10;
        private const double UpperRatio = 0.7;

        protected override Task ProcessAsync(ChatMessageContext context)
        {
            var letters = context.Text.Where(char.IsLetter).ToList();
            if (letters.Count < MinLetters)
                return Task.CompletedTask;

            var upper = letters.Count(char.IsUpper);
            if ((double)upper / letters.Count <= UpperRatio)
                return Task.CompletedTask;

            var lower = context.Text.ToLowerInvariant();
            context.Text = char.ToUpperInvariant(lower[0]) + lower[1..];
            context.MarkCensored("caps-lock");

            return Task.CompletedTask;
        }
    }
}
