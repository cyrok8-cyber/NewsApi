using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace NewsApi.Tests
{
    // Сквозные проверки редакций через HTTP на временной SQLite-базе с данными из data.sql:
    // reporter1 (id=2) и reporter3 (id=5) - редакция "Городские вести" (id=1),
    // reporter2 (id=3) - одиночка, subscriber1 (id=4)
    public class EditorialOfficeIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public EditorialOfficeIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        private async Task<HttpClient> LoginAsync(string username, string password)
        {
            var client = _factory.CreateClient(); // у каждого клиента своя cookie сессии
            var response = await client.PostAsJsonAsync("/api/auth/login", new { username, password });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return client;
        }

        [Fact]
        public async Task GetAll_Anonymous_ReturnsSeededOffices()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/editorial-offices");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("Городские вести", body);
        }

        [Fact]
        public async Task NewsList_OfficeNewsSignedByOfficeFirst()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/news?page=1&pageSize=100");
            var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var signatures = json.RootElement.GetProperty("items").EnumerateArray()
                .Select(n => n.GetProperty("publishedBy").GetString())
                .ToList();

            Assert.Contains("Редакция «Городские вести», редактор: Журналистов Пётр Петрович", signatures);
            Assert.Contains("Пишущая Мария Сергеевна", signatures); // одиночка - без редакции
        }

        [Fact]
        public async Task Create_ByReporter_ReturnsForbidden()
        {
            var client = await LoginAsync("reporter1", "report123");

            var response = await client.PostAsJsonAsync("/api/editorial-offices", new { name = "Своя редакция" });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Create_Anonymous_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync("/api/editorial-offices", new { name = "Без входа" });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Admin_CreatesOffice_AddsSingleReporter_ReporterPublishesOnBehalfOfOffice()
        {
            var admin = await LoginAsync("admin", "admin123");
            var officeName = $"Тестовая редакция {Guid.NewGuid():N}"[..40];

            // 1. Админ создаёт редакцию
            var createResponse = await admin.PostAsJsonAsync("/api/editorial-offices", new { name = officeName });
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            var officeId = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync())
                .RootElement.GetProperty("id").GetInt32();

            // 2. Добавляет журналиста-одиночку reporter2 (id=3)
            var addResponse = await admin.PostAsync($"/api/editorial-offices/{officeId}/members/3", null);
            Assert.Equal(HttpStatusCode.NoContent, addResponse.StatusCode);

            // 3. Подписчика добавить нельзя
            var addSubscriber = await admin.PostAsync($"/api/editorial-offices/{officeId}/members/4", null);
            Assert.Equal(HttpStatusCode.BadRequest, addSubscriber.StatusCode);

            // 4. reporter2 публикует новость - она выходит от имени редакции
            var reporter = await LoginAsync("reporter2", "report456");
            var newsResponse = await reporter.PostAsJsonAsync("/api/news", new
            {
                title = "Новость от редакции",
                text = "Проверка публикации от имени редакции"
            });
            Assert.Equal(HttpStatusCode.Created, newsResponse.StatusCode);
            var publishedBy = JsonDocument.Parse(await newsResponse.Content.ReadAsStringAsync())
                .RootElement.GetProperty("publishedBy").GetString();
            Assert.Equal($"Редакция «{officeName}», редактор: Пишущая Мария Сергеевна", publishedBy);

            // 5. Админ исключает reporter2 - он снова одиночка
            var removeResponse = await admin.DeleteAsync($"/api/editorial-offices/{officeId}/members/3");
            Assert.Equal(HttpStatusCode.NoContent, removeResponse.StatusCode);

            var userJson = JsonDocument.Parse(await admin.GetStringAsync("/api/users/3")).RootElement;
            Assert.Equal(JsonValueKind.Null, userJson.GetProperty("editorialOffice").ValueKind);
        }
    }
}
