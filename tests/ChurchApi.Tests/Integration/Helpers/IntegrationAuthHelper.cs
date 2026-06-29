using System.Net.Http.Headers;
using System.Net.Http.Json;
using ChurchApi.Dtos;

namespace ChurchApi.Tests.Integration.Helpers;

public static class IntegrationAuthHelper
{
    public static async Task<UserResponseDto> RegisterAsync(
        HttpClient client,
        string username,
        string email,
        string password)
    {
        var request = new RegisterDto
        {
            Username = username,
            Email = email,
            Password = password
        };

        var response = await client.PostAsJsonAsync("/api/auth/register", request);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<UserResponseDto>())!;
    }

    public static async Task<string> LoginAsync(
        HttpClient client,
        string username,
        string password)
    {
        var request = new LoginDto
        {
            Username = username,
            Password = password
        };

        var response = await client.PostAsJsonAsync("/api/auth/login", request);
        response.EnsureSuccessStatusCode();

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return authResponse!.Token;
    }

    public static async Task<string> LoginAsAdminAsync(HttpClient client)
    {
        return await LoginAsync(
            client,
            IntegrationTestDataSeeder.AdminUsername,
            IntegrationTestDataSeeder.AdminPassword);
    }

    public static async Task<string> RegisterAndLoginAsync(
        HttpClient client,
        string username,
        string email,
        string password)
    {
        await RegisterAsync(client, username, email, password);
        return await LoginAsync(client, username, password);
    }

    public static void SetBearerToken(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public static void ClearAuthorization(HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = null;
    }
}
