using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace ComplectGroup.Tests.Integration;

public class SwaggerCoverageTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SwaggerCoverageTests(WebApplicationFactory<Program> factory)
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"complectgroup_swagger_{Guid.NewGuid()}.db");
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, cfg) =>
            {
                cfg.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = $"Data Source={dbPath}"
                });
            });
        }).CreateClient();
    }

    [Fact]
    public async Task SwaggerJson_IsAvailable()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(json));
    }

    [Fact]
    public async Task Swagger_CoversAllApiEndpoints()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        response.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var paths = doc.RootElement.GetProperty("paths");

        AssertPath(paths, "/api/chapters", "get", "post");
        AssertPath(paths, "/api/chapters/{id}", "get", "put", "delete");
        AssertPath(paths, "/api/parts", "get", "post");
        AssertPath(paths, "/api/parts/{id}", "get", "put", "delete");
        AssertPath(paths, "/api/parts/by-chapter/{chapterId}", "get");
        AssertPath(paths, "/api/complectations", "get", "post");
        AssertPath(paths, "/api/complectations/search", "get");
        AssertPath(paths, "/api/complectations/{id}", "get", "put", "delete");
        AssertPath(paths, "/warehouse/api/positions/{complectationId}", "get");
    }

    private static void AssertPath(JsonElement paths, string path, params string[] methods)
    {
        Assert.True(paths.TryGetProperty(path, out var item), $"Path not found in Swagger: {path}");
        foreach (var method in methods)
        {
            Assert.True(item.TryGetProperty(method, out _),
                $"Method {method.ToUpperInvariant()} not found for {path}");
        }
    }
}
