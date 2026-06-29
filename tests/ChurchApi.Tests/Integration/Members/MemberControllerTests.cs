using System.Net;
using System.Net.Http.Json;
using ChurchApi.Dtos;
using ChurchApi.Tests.Integration.Helpers;
using ChurchApi.Tests.Integration.Infrastructure;
using FluentAssertions;

namespace ChurchApi.Tests.Integration.Members;

public class MemberControllerTests : IntegrationTestBase, IClassFixture<CustomWebApplicationFactory>
{
    public MemberControllerTests(CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetMembers_Should_Return_200()
    {
        // Arrange
        IntegrationAuthHelper.ClearAuthorization(Client);
        var token = await IntegrationAuthHelper.LoginAsAdminAsync(Client);
        IntegrationAuthHelper.SetBearerToken(Client, token);

        // Act
        var response = await Client.GetAsync("/api/members");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pagedResponse = await response.Content.ReadFromJsonAsync<PagedResponse<MemberResponseDto>>();
        pagedResponse.Should().NotBeNull();
        pagedResponse!.Items.Should().NotBeNull();
        pagedResponse.Page.Should().BeGreaterThan(0);
        pagedResponse.PageSize.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateMember_Should_Return_Created_When_User_Is_Admin()
    {
        // Arrange
        IntegrationAuthHelper.ClearAuthorization(Client);
        var token = await IntegrationAuthHelper.LoginAsAdminAsync(Client);
        IntegrationAuthHelper.SetBearerToken(Client, token);

        var request = new CreateMemberDto
        {
            Name = "John",
            LastName = "Doe",
            Email = $"john.doe.{Guid.NewGuid():N}@test.com",
            Phone = "5551234567",
            Age = 30
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/members", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdMember = await response.Content.ReadFromJsonAsync<MemberResponseDto>();
        createdMember.Should().NotBeNull();
        createdMember!.Name.Should().Be("John");
        createdMember.LastName.Should().Be("Doe");
        createdMember.Email.Should().Be(request.Email);
        createdMember.Id.Should().BeGreaterThan(0);

        var getResponse = await Client.GetAsync($"/api/members/{createdMember.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var persistedMember = await getResponse.Content.ReadFromJsonAsync<MemberResponseDto>();
        persistedMember.Should().NotBeNull();
        persistedMember!.Id.Should().Be(createdMember.Id);
        persistedMember.Email.Should().Be(request.Email);
    }

    [Fact]
    public async Task UpdateMember_Should_Return_200_When_User_Is_Admin()
    {
        // Arrange
        IntegrationAuthHelper.ClearAuthorization(Client);
        var token = await IntegrationAuthHelper.LoginAsAdminAsync(Client);
        IntegrationAuthHelper.SetBearerToken(Client, token);

        var createResponse = await Client.PostAsJsonAsync("/api/members", new CreateMemberDto
        {
            Name = "Maria",
            LastName = "Perez",
            Email = $"maria.perez.{Guid.NewGuid():N}@test.com",
            Phone = "5551112222",
            Age = 34
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdMember = await createResponse.Content.ReadFromJsonAsync<MemberResponseDto>();
        createdMember.Should().NotBeNull();

        var request = new UpdateMemberDto
        {
            Id = createdMember!.Id,
            Name = "Maria Elena",
            LastName = "Perez",
            Email = createdMember.Email,
            Phone = "5553334444",
            Age = 35
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/members/{createdMember.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedMember = await response.Content.ReadFromJsonAsync<MemberResponseDto>();
        updatedMember.Should().NotBeNull();
        updatedMember!.Id.Should().Be(createdMember.Id);
        updatedMember.Name.Should().Be("Maria Elena");
        updatedMember.Phone.Should().Be("5553334444");
        updatedMember.Age.Should().Be(35);
    }

    [Fact]
    public async Task DeleteMember_Should_Return_NoContent_When_User_Is_Admin()
    {
        // Arrange
        IntegrationAuthHelper.ClearAuthorization(Client);
        var token = await IntegrationAuthHelper.LoginAsAdminAsync(Client);
        IntegrationAuthHelper.SetBearerToken(Client, token);

        var createResponse = await Client.PostAsJsonAsync("/api/members", new CreateMemberDto
        {
            Name = "Carlos",
            LastName = "Santos",
            Email = $"carlos.santos.{Guid.NewGuid():N}@test.com",
            Phone = "5552223333",
            Age = 42
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdMember = await createResponse.Content.ReadFromJsonAsync<MemberResponseDto>();
        createdMember.Should().NotBeNull();

        // Act
        var response = await Client.DeleteAsync($"/api/members/{createdMember!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var content = await response.Content.ReadAsByteArrayAsync();
        content.Should().BeEmpty();
    }
}
