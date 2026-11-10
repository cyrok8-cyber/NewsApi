namespace NewsApi.Services.Censorship
{
    // Базовый обработчик: хранит ссылку на следующий и передаёт ему сообщение
    // после своей проверки. Наследникам достаточно реализовать ProcessAsync
    public abstract class ChatMessageHandler : IChatMessageHandler
    {
        private IChatMessageHandler? _next;

        public IChatMessageHandler SetNext(IChatMessageHandler next)
        {
            _next = next;
            return next;
        }

        public async Task HandleAsync(ChatMessageContext context)
        {
            await ProcessAsync(context);

            if (_next != null)
                await _next.HandleAsync(context);
        }

        // Проверка конкретного правила. Чтобы отклонить сообщение - выбросить исключение
        protected abstract Task ProcessAsync(ChatMessageContext context);
    }
}
