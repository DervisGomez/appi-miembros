using System.Net;
using System.Net.Http.Json;
using ChurchApi.Dtos;
using ChurchApi.Tests.Integration.Helpers;
using ChurchApi.Tests.Integration.Infrastructure;
using FluentAssertions;

namespace ChurchApi.Tests.Integration.Auth;

public class AuthControllerTests : IntegrationTestBase, IClassFixture<CustomWebApplicationFactory>
{
    public AuthControllerTests(CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Register_Should_Return_Created_When_Request_Is_Valid()
    {
        // Arrange
        var username = $"user_{Guid.NewGuid():N}";
        var email = $"{username}@test.com";

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Username = username,
            Email = email,
            Password = "12345678"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var user = await response.Content.ReadFromJsonAsync<UserResponseDto>();
        user.Should().NotBeNull();
        user!.Username.Should().Be(username);
        user.Email.Should().Be(email);
        user.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Login_Should_Return_JwtToken_When_Credentials_Are_Valid()
    {
        // Arrange
        var username = $"user_{Guid.NewGuid():N}";
        var email = $"{username}@test.com";
        const string password = "12345678";

        await IntegrationAuthHelper.RegisterAsync(Client, username, email, password);

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            Username = username,
            Password = password
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().NotBeNullOrWhiteSpace();
    }
}
