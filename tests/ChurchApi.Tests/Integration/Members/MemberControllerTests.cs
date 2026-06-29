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
}
