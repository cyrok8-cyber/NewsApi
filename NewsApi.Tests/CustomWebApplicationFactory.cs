using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace NewsApi.Tests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        public string TestDbPath { get; } = Path.Combine(
            Path.GetTempPath(), $"newsapi_integration_test_{Guid.NewGuid():N}.db");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["DatabaseProvider"] = "Sqlite",
                    ["ConnectionStrings:DefaultConnection"] = $"Data Source={TestDbPath}"
                });
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (File.Exists(TestDbPath))
            {
                try { File.Delete(TestDbPath); } catch { }
            }
        }
    }
}