namespace NewsApi.Repositories
{
    public interface IBannedWordRepository
    {
        Task<bool> ContainsBannedWordAsync(string text);

        // Весь список запрещённых слов - нужен цензуре чата, чтобы заменять их звёздочками
        Task<List<string>> GetAllWordsAsync();
    }
}
