using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace NewsApi.Tests
{
    public class AuthIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AuthIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOk()
        {
            var response = await _client.PostAsJsonAsync("/api/auth/login", new
            {
                username = "admin",
                password = "admin123"
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("admin", body);
        }

        [Fact]
        public async Task Login_WrongPassword_ReturnsUnauthorized()
        {
            var response = await _client.PostAsJsonAsync("/api/auth/login", new
            {
                username = "admin",
                password = "wrongpassword"
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetUser_WithoutLogin_ReturnsUnauthorized()
        {
            var response = await _client.GetAsync("/api/users/1");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task FullSessionLifecycle_Works()
        {
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
            {
                username = "admin",
                password = "admin123"
            });
            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

            var protectedResponse = await _client.GetAsync("/api/users/2");
            Assert.Equal(HttpStatusCode.OK, protectedResponse.StatusCode);

            var logoutResponse = await _client.PostAsync("/api/auth/logout", null);
            Assert.Equal(HttpStatusCode.Unauthorized, logoutResponse.StatusCode);

            var afterLogoutResponse = await _client.GetAsync("/api/users/2");
            Assert.Equal(HttpStatusCode.Unauthorized, afterLogoutResponse.StatusCode);
        }
    }
}