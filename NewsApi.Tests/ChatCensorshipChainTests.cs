using Moq;
using NewsApi.Repositories;
using NewsApi.Services.Censorship;
using Xunit;

namespace NewsApi.Tests
{
    // Проверяем каждое звено цепочки цензуры и всю цепочку целиком
    public class ChatCensorshipChainTests
    {
        private readonly ChatCensorshipChain _chain;

        public ChatCensorshipChainTests()
        {
            var bannedWordRepositoryMock = new Mock<IBannedWordRepository>();
            bannedWordRepositoryMock.Setup(r => r.GetAllWordsAsync())
                .ReturnsAsync(new List<string> { "дурак", "идиот" });

            _chain = new ChatCensorshipChain(
                new EmptyMessageHandler(),
                new MaxLengthHandler(),
                new LinkFilterHandler(),
                new BannedWordsHandler(bannedWordRepositoryMock.Object),
                new CapsLockHandler());
        }

        [Fact]
        public async Task CleanMessage_PassesUnchanged()
        {
            var result = await _chain.ProcessAsync("  Коллеги, материал готов  ");

            Assert.Equal("Коллеги, материал готов", result.Text);
            Assert.False(result.WasCensored);
            Assert.Empty(result.AppliedRules);
        }

        [Theory]
        [InlineData("")]
        [InlineData("     ")]
        public async Task EmptyMessage_IsRejected(string text)
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _chain.ProcessAsync(text));
        }

        [Fact]
        public async Task TooLongMessage_IsRejected()
        {
            var text = new string('а', MaxLengthHandler.MaxLength + 1);

            await Assert.ThrowsAsync<ArgumentException>(() => _chain.ProcessAsync(text));
        }

        [Fact]
        public async Task Links_AreRemoved()
        {
            var result = await _chain.ProcessAsync("Смотри https://example.com/page и www.test.by");

            Assert.Equal($"Смотри {LinkFilterHandler.Replacement} и {LinkFilterHandler.Replacement}", result.Text);
            Assert.True(result.WasCensored);
            Assert.Contains("links", result.AppliedRules);
        }

        [Fact]
        public async Task BannedWords_AreMaskedWithAsterisks_CaseInsensitive()
        {
            var result = await _chain.ProcessAsync("Ну ты Дурак, просто идиотский поступок");

            Assert.Equal("Ну ты *****, просто *****ский поступок", result.Text);
            Assert.True(result.WasCensored);
            Assert.Contains("banned-words", result.AppliedRules);
        }

        [Fact]
        public async Task CapsLock_IsConvertedToNormalCase()
        {
            var result = await _chain.ProcessAsync("ВСЕ СРОЧНО НА ПЛАНЁРКУ");

            Assert.Equal("Все срочно на планёрку", result.Text);
            Assert.Contains("caps-lock", result.AppliedRules);
        }

        [Fact]
        public async Task ShortAbbreviation_IsNotTreatedAsCapsLock()
        {
            var result = await _chain.ProcessAsync("Отчёт для НКА");

            Assert.Equal("Отчёт для НКА", result.Text);
            Assert.False(result.WasCensored);
        }

        [Fact]
        public async Task AllRules_AppliedInChainOrder()
        {
            var result = await _chain.ProcessAsync("ЭТОТ ДУРАК ОПЯТЬ КИНУЛ WWW.SPAM.COM");

            Assert.Equal("Этот ***** опять кинул [ссылка удалена]", result.Text);
            Assert.Equal(new[] { "caps-lock", "links", "banned-words" }, result.AppliedRules);
        }
    }
}
