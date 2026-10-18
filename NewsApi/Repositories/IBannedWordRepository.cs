namespace NewsApi.Repositories
{
    public interface IBannedWordRepository
    {
        Task<bool> ContainsBannedWordAsync(string text);
    }
}