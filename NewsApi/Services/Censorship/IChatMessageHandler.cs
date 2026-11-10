namespace NewsApi.Services.Censorship
{
    // Паттерн "Цепочка обязанностей" (Chain of Responsibility).
    // Сообщение передаётся от обработчика к обработчику. Каждый отвечает за одно правило цензуры:
    // либо исправляет текст и передаёт его дальше, либо отклоняет сообщение (исключение прерывает цепочку)
    public interface IChatMessageHandler
    {
        // Назначить следующий обработчик. Возвращает его же, чтобы цепочку можно было
        // собрать в одну строку: a.SetNext(b).SetNext(c)
        IChatMessageHandler SetNext(IChatMessageHandler next);

        Task HandleAsync(ChatMessageContext context);
    }
}
