using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace NewsApi.Tests
{
    // Чат редакции через HTTP. По данным из data.sql:
    // reporter1 и reporter3 - редакция "Городские вести" (id=1), reporter2 - одиночка
    public class ChatIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public ChatIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        private async Task<HttpClient> LoginAsync(string username, string password)
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/auth/login", new { username, password });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return client;
        }

        [Fact]
        public async Task Member_SendsMessage_ColleagueSeesCensoredText()
        {
            var reporter1 = await LoginAsync("reporter1", "report123");

            var send = await reporter1.PostAsJsonAsync("/api/editorial-offices/1/chat", new
            {
                text = "Коллеги, тут какой-то дурак прислал https://spam.example.com"
            });
            Assert.Equal(HttpStatusCode.Created, send.StatusCode);

            var sent = JsonDocument.Parse(await send.Content.ReadAsStringAsync()).RootElement;
            Assert.Equal("Коллеги, тут какой-то ***** прислал [ссылка удалена]", sent.GetProperty("text").GetString());
            Assert.True(sent.GetProperty("wasCensored").GetBoolean());

            // Коллега по редакции видит сообщение уже в цензурированном виде
            var reporter3 = await LoginAsync("reporter3", "report789");
            var chat = JsonDocument.Parse(await reporter3.GetStringAsync("/api/editorial-offices/1/chat")).RootElement;
            var texts = chat.GetProperty("items").EnumerateArray()
                .Select(m => m.GetProperty("text").GetString())
                .ToList();

            Assert.Contains("Коллеги, тут какой-то ***** прислал [ссылка удалена]", texts);
        }

        [Fact]
        public async Task SingleReporter_CannotReadOfficeChat()
        {
            var reporter2 = await LoginAsync("reporter2", "report456");

            var response = await reporter2.GetAsync("/api/editorial-offices/1/chat");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Subscriber_CannotReadOfficeChat()
        {
            var subscriber = await LoginAsync("subscriber1", "sub123");

            var response = await subscriber.GetAsync("/api/editorial-offices/1/chat");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Anonymous_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/editorial-offices/1/chat");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Admin_CanReadButCannotWrite()
        {
            var admin = await LoginAsync("admin", "admin123");

            var read = await admin.GetAsync("/api/editorial-offices/1/chat");
            Assert.Equal(HttpStatusCode.OK, read.StatusCode);

            var write = await admin.PostAsJsonAsync("/api/editorial-offices/1/chat", new { text = "Привет" });
            Assert.Equal(HttpStatusCode.Forbidden, write.StatusCode);
        }
    }
}
