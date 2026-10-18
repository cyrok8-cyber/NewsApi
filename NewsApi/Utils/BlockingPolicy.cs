namespace NewsApi.Utils
{
    public static class BlockingPolicy
    {
        public const int MaxFailedLoginAttempts = 5;
        public static readonly TimeSpan LoginBlockDuration = TimeSpan.FromMinutes(30);

        public const int MaxFailedBannedWordAttempts = 5;
        public static readonly TimeSpan BannedWordBlockDuration = TimeSpan.FromHours(24);

        // Сколько живёт токен сессии после входа
        public static readonly TimeSpan SessionLifetime = TimeSpan.FromHours(12);
    }
}