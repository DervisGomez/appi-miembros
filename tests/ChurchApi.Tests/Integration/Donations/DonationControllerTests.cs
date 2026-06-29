using System.Net;
using System.Net.Http.Json;
using ChurchApi.Dtos;
using ChurchApi.Tests.Integration.Helpers;
using ChurchApi.Tests.Integration.Infrastructure;
using FluentAssertions;

namespace ChurchApi.Tests.Integration.Donations;

public class DonationControllerTests : IntegrationTestBase, IClassFixture<CustomWebApplicationFactory>
{
    public DonationControllerTests(CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetDonations_Should_Return_200()
    {
        // Arrange
        IntegrationAuthHelper.ClearAuthorization(Client);
        var token = await IntegrationAuthHelper.LoginAsAdminAsync(Client);
        IntegrationAuthHelper.SetBearerToken(Client, token);

        // Act
        var response = await Client.GetAsync("/api/donations?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pagedResponse = await response.Content.ReadFromJsonAsync<PagedResponse<DonationMemberResponseDto>>();
        pagedResponse.Should().NotBeNull();
        pagedResponse!.Items.Should().NotBeNull();
        pagedResponse.Page.Should().Be(1);
        pagedResponse.PageSize.Should().Be(10);
        pagedResponse.TotalItems.Should().BeGreaterThanOrEqualTo(0);
        pagedResponse.TotalPages.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task CreateDonation_Should_Return_Created_When_User_Is_Admin()
    {
        // Arrange
        IntegrationAuthHelper.ClearAuthorization(Client);
        var token = await IntegrationAuthHelper.LoginAsAdminAsync(Client);
        IntegrationAuthHelper.SetBearerToken(Client, token);

        var memberResponse = await Client.PostAsJsonAsync("/api/members", new CreateMemberDto
        {
            Name = "Jane",
            LastName = "Smith",
            Email = $"jane.smith.{Guid.NewGuid():N}@test.com",
            Phone = "5559876543",
            Age = 28
        });
        memberResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var member = await memberResponse.Content.ReadFromJsonAsync<MemberResponseDto>();
        member.Should().NotBeNull();

        var donationRequest = new CreateDonationDto
        {
            Amount = 150m,
            Description = "Monthly offering"
        };

        // Act
        var response = await Client.PostAsJsonAsync($"/api/members/{member!.Id}/donations", donationRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdDonation = await response.Content.ReadFromJsonAsync<DonationResponseDto>();
        createdDonation.Should().NotBeNull();
        createdDonation!.Amount.Should().Be(150m);
        createdDonation.Description.Should().Be("Monthly offering");
        createdDonation.Id.Should().BeGreaterThan(0);

        var donationsResponse = await Client.GetAsync($"/api/members/{member.Id}/donations");
        donationsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var donations = await donationsResponse.Content.ReadFromJsonAsync<List<DonationResponseDto>>();
        donations.Should().ContainSingle();
        donations![0].Id.Should().Be(createdDonation.Id);
        donations[0].Amount.Should().Be(150m);
    }

    [Fact]
    public async Task DeleteDonation_Should_Return_NoContent_When_User_Is_Admin()
    {
        // Arrange
        IntegrationAuthHelper.ClearAuthorization(Client);
        var token = await IntegrationAuthHelper.LoginAsAdminAsync(Client);
        IntegrationAuthHelper.SetBearerToken(Client, token);

        var memberResponse = await Client.PostAsJsonAsync("/api/members", new CreateMemberDto
        {
            Name = "Luis",
            LastName = "Garcia",
            Email = $"luis.garcia.{Guid.NewGuid():N}@test.com",
            Phone = "5554445555",
            Age = 31
        });
        memberResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var member = await memberResponse.Content.ReadFromJsonAsync<MemberResponseDto>();
        member.Should().NotBeNull();

        var donationResponse = await Client.PostAsJsonAsync($"/api/members/{member!.Id}/donations", new CreateDonationDto
        {
            Amount = 75m,
            Description = "Special offering"
        });
        donationResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var donation = await donationResponse.Content.ReadFromJsonAsync<DonationResponseDto>();
        donation.Should().NotBeNull();

        // Act
        var response = await Client.DeleteAsync($"/api/donations/{donation!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var content = await response.Content.ReadAsByteArrayAsync();
        content.Should().BeEmpty();
    }
}
