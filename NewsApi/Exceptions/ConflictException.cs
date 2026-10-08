namespace NewsApi.Exceptions
{
    // Бросаем при конфликте данных - например, логин уже занят при регистрации
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }
}