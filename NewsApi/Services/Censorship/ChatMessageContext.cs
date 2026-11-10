namespace NewsApi.Services.Censorship
{
    // Сообщение, которое передаётся по цепочке обработчиков.
    // Каждый обработчик может изменить текст и отметить, что сработала цензура
    public class ChatMessageContext
    {
        public ChatMessageContext(string text)
        {
            OriginalText = text;
            Text = text;
        }

        // Текст в том виде, в каком его отправил пользователь
        public string OriginalText { get; }

        // Текущий текст - меняется по мере прохождения цепочки
        public string Text { get; set; }

        // true, если хотя бы один обработчик изменил текст
        public bool WasCensored { get; private set; }

        // Какие обработчики сработали (для отладки и тестов)
        public List<string> AppliedRules { get; } = new();

        public void MarkCensored(string rule)
        {
            WasCensored = true;
            AppliedRules.Add(rule);
        }
    }
}
