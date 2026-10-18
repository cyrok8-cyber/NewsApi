namespace NewsApi.Exceptions
{
    // Текст содержит запрещённое слово
    public class BannedWordException : Exception
    {
        public BannedWordException(string message) : base(message)
        {
        }
    }
}