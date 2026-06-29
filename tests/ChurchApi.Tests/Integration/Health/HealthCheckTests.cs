using System.Net;
using System.Text.Json;
using ChurchApi.Tests.Integration.Infrastructure;
using FluentAssertions;

namespace ChurchApi.Tests.Integration.Health;

public class HealthCheckTests : IntegrationTestBase, IClassFixture<CustomWebApplicationFactory>
{
    public HealthCheckTests(CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Health_Should_Return_Ok_When_Database_Is_Available()
    {
        // Act
        var response = await Client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        var root = document.RootElement;

        root.GetProperty("status").GetString().Should().Be("Healthy");
        root.GetProperty("totalDuration").GetString().Should().NotBeNullOrWhiteSpace();

        var checks = root.GetProperty("checks");
        checks.GetArrayLength().Should().BeGreaterThan(0);
        checks.EnumerateArray().Should().Contain(check =>
            check.GetProperty("name").GetString() == "sqlserver"
            && check.GetProperty("status").GetString() == "Healthy");
    }
}
