namespace NewsApi.Exceptions
{
    // Бросаем, когда сущность не найдена - перехватывается глобальным обработчиком и превращается в 404
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }
}