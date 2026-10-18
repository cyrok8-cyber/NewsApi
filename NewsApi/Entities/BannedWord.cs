namespace NewsApi.Entities
{
    // Слово, запрещённое к использованию в тексте новостей и комментариев
    public class BannedWord
    {
        public int Id { get; set; }
        public string Word { get; set; } = string.Empty;
    }
}