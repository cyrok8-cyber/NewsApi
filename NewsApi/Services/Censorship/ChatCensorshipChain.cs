namespace NewsApi.Services.Censorship
{
    public interface IChatCensorshipChain
    {
        // Пропустить текст через все обработчики цензуры
        Task<ChatMessageContext> ProcessAsync(string text);
    }

    // Собирает обработчики в цепочку в нужном порядке:
    // пустое -> длина -> капс -> ссылки -> запрещённые слова.
    // Капс идёт раньше фильтров: сначала текст приводится к нормальному виду,
    // иначе вставка "[ссылка удалена]" исказила бы подсчёт заглавных букв.
    // Чтобы добавить новое правило, достаточно написать обработчик и вставить его в цепочку здесь
    public class ChatCensorshipChain : IChatCensorshipChain
    {
        private readonly IChatMessageHandler _head;

        public ChatCensorshipChain(
            EmptyMessageHandler emptyHandler,
            MaxLengthHandler maxLengthHandler,
            LinkFilterHandler linkFilterHandler,
            BannedWordsHandler bannedWordsHandler,
            CapsLockHandler capsLockHandler)
        {
            emptyHandler
                .SetNext(maxLengthHandler)
                .SetNext(capsLockHandler)
                .SetNext(linkFilterHandler)
                .SetNext(bannedWordsHandler);

            _head = emptyHandler;
        }

        public async Task<ChatMessageContext> ProcessAsync(string text)
        {
            var context = new ChatMessageContext(text);
            await _head.HandleAsync(context);
            return context;
        }
    }
}
