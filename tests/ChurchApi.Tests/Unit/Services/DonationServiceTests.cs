using ChurchApi.Dtos;
using ChurchApi.Enums;
using ChurchApi.Exceptions;
using ChurchApi.Tests.Fixtures;
using ChurchApi.Tests.Unit.Helpers;
using FluentAssertions;

namespace ChurchApi.Tests.Unit.Services;

public class DonationServiceTests
{
    private const decimal LowAmount = 50m;
    private const decimal MidAmount = 100m;
    private const decimal HighAmount = 200m;

    private static readonly DateTime JanuaryFirst = new(2024, 1, 1);
    private static readonly DateTime FebruaryFirst = new(2024, 2, 1);
    private static readonly DateTime MarchFirst = new(2024, 3, 1);

    [Fact]
    public async Task GetDonations_Should_Return_Paged_Donations()
    {
        // Arrange
        using var fixture = new DonationServiceFixture();
        var member = await TestDataSeeder.CreateMemberAsync(fixture.Context);
        await TestDataSeeder.CreateDonationAsync(fixture.Context, member, MidAmount, JanuaryFirst);
        await TestDataSeeder.CreateDonationAsync(fixture.Context, member, HighAmount, FebruaryFirst);

        var queryDto = new DonationQueryDto
        {
            Page = 1,
            PageSize = 10,
            SortOrder = SortOrder.Desc
        };

        // Act
        var result = await fixture.Service.GetDonations(queryDto);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalItems.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(1);
        result.Items[0].Amount.Should().Be(HighAmount);
        result.Items[0].Member.Name.Should().Be(member.Name);
    }

    [Fact]
    public async Task GetDonations_Should_Filter_By_MemberId()
    {
        // Arrange
        using var fixture = new DonationServiceFixture();
        var memberOne = await TestDataSeeder.CreateMemberAsync(fixture.Context, name: "John", lastName: "Doe");
        var memberTwo = await TestDataSeeder.CreateMemberAsync(fixture.Context, name: "Jane", lastName: "Smith");

        await TestDataSeeder.CreateDonationAsync(fixture.Context, memberOne, LowAmount);
        await TestDataSeeder.CreateDonationAsync(fixture.Context, memberTwo, MidAmount + LowAmount);

        var queryDto = new DonationQueryDto { MemberId = memberOne.Id };

        // Act
        var result = await fixture.Service.GetDonations(queryDto);

        // Assert
        result.Items.Should().ContainSingle();
        result.Items[0].Amount.Should().Be(LowAmount);
        result.Items[0].Member.Id.Should().Be(memberOne.Id);
    }

    [Fact]
    public async Task GetDonations_Should_Filter_By_MinAmount()
    {
        // Arrange
        using var fixture = new DonationServiceFixture();
        var member = await TestDataSeeder.CreateMemberAsync(fixture.Context);

        await TestDataSeeder.CreateDonationAsync(fixture.Context, member, LowAmount, JanuaryFirst);
        await TestDataSeeder.CreateDonationAsync(fixture.Context, member, MidAmount, FebruaryFirst);
        await TestDataSeeder.CreateDonationAsync(fixture.Context, member, HighAmount, MarchFirst);

        var queryDto = new DonationQueryDto { MinAmount = MidAmount };

        // Act
        var result = await fixture.Service.GetDonations(queryDto);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Select(d => d.Amount).Should().ContainInOrder(HighAmount, MidAmount);
    }

    [Fact]
    public async Task GetDonations_Should_Filter_By_MaxAmount()
    {
        // Arrange
        using var fixture = new DonationServiceFixture();
        var member = await TestDataSeeder.CreateMemberAsync(fixture.Context);

        await TestDataSeeder.CreateDonationAsync(fixture.Context, member, LowAmount, JanuaryFirst);
        await TestDataSeeder.CreateDonationAsync(fixture.Context, member, MidAmount, FebruaryFirst);
        await TestDataSeeder.CreateDonationAsync(fixture.Context, member, HighAmount, MarchFirst);

        var queryDto = new DonationQueryDto { MaxAmount = MidAmount };

        // Act
        var result = await fixture.Service.GetDonations(queryDto);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Select(d => d.Amount).Should().ContainInOrder(MidAmount, LowAmount);
    }

    [Fact]
    public async Task GetDonations_Should_Filter_By_MinAmount_And_MaxAmount()
    {
        // Arrange
        using var fixture = new DonationServiceFixture();
        var member = await TestDataSeeder.CreateMemberAsync(fixture.Context);

        await TestDataSeeder.CreateDonationAsync(fixture.Context, member, LowAmount);
        await TestDataSeeder.CreateDonationAsync(fixture.Context, member, MidAmount);
        await TestDataSeeder.CreateDonationAsync(fixture.Context, member, HighAmount);

        var queryDto = new DonationQueryDto
        {
            MinAmount = LowAmount + 25m,
            MaxAmount = MidAmount + 50m
        };

        // Act
        var result = await fixture.Service.GetDonations(queryDto);

        // Assert
        result.Items.Should().ContainSingle();
        result.Items[0].Amount.Should().Be(MidAmount);
    }

    [Fact]
    public async Task GetDonations_Should_Return_Donations_In_Ascending_Order_When_SortOrder_Is_Asc()
    {
        // Arrange
        using var fixture = new DonationServiceFixture();
        var member = await TestDataSeeder.CreateMemberAsync(fixture.Context);

        await TestDataSeeder.CreateDonationAsync(fixture.Context, member, LowAmount, MarchFirst);
        await TestDataSeeder.CreateDonationAsync(fixture.Context, member, MidAmount, JanuaryFirst);
        await TestDataSeeder.CreateDonationAsync(fixture.Context, member, HighAmount, FebruaryFirst);

        var queryDto = new DonationQueryDto { SortOrder = SortOrder.Asc };

        // Act
        var result = await fixture.Service.GetDonations(queryDto);

        // Assert
        result.Items.Should().HaveCount(3);
        result.Items.Select(d => d.Date).Should().BeInAscendingOrder();
        result.Items[0].Amount.Should().Be(MidAmount);
        result.Items[2].Amount.Should().Be(LowAmount);
    }

    [Fact]
    public async Task GetDonations_Should_Return_Donations_In_Descending_Order_When_SortOrder_Is_Desc()
    {
        // Arrange
        using var fixture = new DonationServiceFixture();
        var member = await TestDataSeeder.CreateMemberAsync(fixture.Context);

        await TestDataSeeder.CreateDonationAsync(fixture.Context, member, LowAmount, JanuaryFirst);
        await TestDataSeeder.CreateDonationAsync(fixture.Context, member, MidAmount, MarchFirst);
        await TestDataSeeder.CreateDonationAsync(fixture.Context, member, HighAmount, FebruaryFirst);

        var queryDto = new DonationQueryDto { SortOrder = SortOrder.Desc };

        // Act
        var result = await fixture.Service.GetDonations(queryDto);

        // Assert
        result.Items.Should().HaveCount(3);
        result.Items.Select(d => d.Date).Should().BeInDescendingOrder();
        result.Items[0].Amount.Should().Be(MidAmount);
        result.Items[2].Amount.Should().Be(LowAmount);
    }

    [Fact]
    public async Task GetDonations_Should_Return_Second_Page_When_Page_Is_Two()
    {
        // Arrange
        using var fixture = new DonationServiceFixture();
        var member = await TestDataSeeder.CreateMemberAsync(fixture.Context);

        await TestDataSeeder.CreateDonationAsync(fixture.Context, member, LowAmount, JanuaryFirst);
        await TestDataSeeder.CreateDonationAsync(fixture.Context, member, MidAmount, FebruaryFirst);
        await TestDataSeeder.CreateDonationAsync(fixture.Context, member, HighAmount, MarchFirst);

        var queryDto = new DonationQueryDto
        {
            Page = 2,
            PageSize = 2,
            SortOrder = SortOrder.Asc
        };

        // Act
        var result = await fixture.Service.GetDonations(queryDto);

        // Assert
        result.Items.Should().ContainSingle();
        result.TotalItems.Should().Be(3);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(2);
        result.TotalPages.Should().Be(2);
        result.Items[0].Amount.Should().Be(HighAmount);
    }

    [Fact]
    public async Task GetDonations_Should_Return_Empty_Page_When_No_Donations_Match_Filter()
    {
        // Arrange
        using var fixture = new DonationServiceFixture();
        var member = await TestDataSeeder.CreateMemberAsync(fixture.Context);
        await TestDataSeeder.CreateDonationAsync(fixture.Context, member, MidAmount);

        var queryDto = new DonationQueryDto
        {
            MinAmount = HighAmount + 100m
        };

        // Act
        var result = await fixture.Service.GetDonations(queryDto);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalItems.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task GetDonations_Should_Throw_ValidationException_When_MinAmount_Is_Greater_Than_MaxAmount()
    {
        // Arrange
        using var fixture = new DonationServiceFixture();

        var queryDto = new DonationQueryDto
        {
            MinAmount = 500m,
            MaxAmount = 100m
        };

        // Act
        Func<Task> act = () => fixture.Service.GetDonations(queryDto);

        // Assert
        await act
            .Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("MinAmount cannot be greater than MaxAmount.");
    }

    [Fact]
    public async Task GetDonations_Should_Use_Default_Pagination_When_Page_And_PageSize_Are_Invalid()
    {
        // Arrange
        using var fixture = new DonationServiceFixture();
        var member = await TestDataSeeder.CreateMemberAsync(fixture.Context);
        await TestDataSeeder.CreateDonationAsync(fixture.Context, member, MidAmount);

        var queryDto = new DonationQueryDto
        {
            Page = 0,
            PageSize = 0
        };

        // Act
        var result = await fixture.Service.GetDonations(queryDto);

        // Assert
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task GetDonations_Should_Return_Only_OneHundred_Items_When_PageSize_Exceeds_Maximum()
    {
        // Arrange
        using var fixture = new DonationServiceFixture();
        var member = await TestDataSeeder.CreateMemberAsync(fixture.Context);

        for (var i = 0; i < 150; i++)
        {
            await TestDataSeeder.CreateDonationAsync(fixture.Context, member, amount: i + 1);
        }

        var queryDto = new DonationQueryDto { PageSize = 500 };

        // Act
        var result = await fixture.Service.GetDonations(queryDto);

        // Assert
        result.Items.Should().HaveCount(100);
        result.PageSize.Should().Be(100);
        result.TotalItems.Should().Be(150);
        result.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task GetDonationsByMemberId_Should_Return_Donations_For_Member()
    {
        // Arrange
        using var fixture = new DonationServiceFixture();
        var memberOne = await TestDataSeeder.CreateMemberAsync(fixture.Context, name: "John", lastName: "Doe");
        var memberTwo = await TestDataSeeder.CreateMemberAsync(fixture.Context, name: "Jane", lastName: "Smith");

        await TestDataSeeder.CreateDonationAsync(fixture.Context, memberOne, LowAmount);
        await TestDataSeeder.CreateDonationAsync(fixture.Context, memberOne, MidAmount);
        await TestDataSeeder.CreateDonationAsync(fixture.Context, memberTwo, HighAmount);

        // Act
        var result = await fixture.Service.GetDonationsByMemberId(memberOne.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Select(d => d.Amount).Should().BeEquivalentTo([LowAmount, MidAmount]);
        result.Should().OnlyContain(d => d.MemberId == memberOne.Id);
    }

    [Fact]
    public async Task GetDonationsByMemberId_Should_Return_Empty_List_When_Member_Has_No_Donations()
    {
        // Arrange
        using var fixture = new DonationServiceFixture();
        var member = await TestDataSeeder.CreateMemberAsync(fixture.Context);

        // Act
        var result = await fixture.Service.GetDonationsByMemberId(member.Id);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDonationsByMemberId_Should_Throw_NotFoundException_When_Member_Does_Not_Exist()
    {
        // Arrange
        using var fixture = new DonationServiceFixture();

        // Act
        Func<Task> act = () => fixture.Service.GetDonationsByMemberId(999);

        // Assert
        await act
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Member with id 999 was not found.");
    }

    [Fact]
    public async Task AddDonation_Should_Create_Donation_When_Member_Exists()
    {
        // Arrange
        using var fixture = new DonationServiceFixture();
        var member = await TestDataSeeder.CreateMemberAsync(fixture.Context);

        var dto = new CreateDonationDto
        {
            Amount = 250m,
            Description = "Monthly offering"
        };

        // Act
        var result = await fixture.Service.AddDonation(dto, member.Id);

        // Assert
        result.Should().NotBeNull();
        result.Amount.Should().Be(250m);
        result.Description.Should().Be("Monthly offering");
        result.MemberId.Should().Be(member.Id);
        result.Date.Should().Be(fixture.TimeProvider.GetUtcNow().UtcDateTime);

        var donationInDb = fixture.Context.Donations.Single();
        donationInDb.Should().BeEquivalentTo(result, options => options.ExcludingMissingMembers());
    }

    [Fact]
    public async Task AddDonation_Should_Throw_NotFoundException_When_Member_Does_Not_Exist()
    {
        // Arrange
        using var fixture = new DonationServiceFixture();

        var dto = new CreateDonationDto
        {
            Amount = 100m,
            Description = "Offering"
        };

        // Act
        Func<Task> act = () => fixture.Service.AddDonation(dto, memberId: 999);

        // Assert
        await act
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Member with id 999 was not found.");

        fixture.Context.Donations.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteDonation_Should_Delete_Donation()
    {
        // Arrange
        using var fixture = new DonationServiceFixture();
        var member = await TestDataSeeder.CreateMemberAsync(fixture.Context);
        var donation = await TestDataSeeder.CreateDonationAsync(fixture.Context, member, 75m);

        // Act
        var result = await fixture.Service.DeleteDonation(donation.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(donation.Id);
        result.Amount.Should().Be(75m);
        fixture.Context.Donations.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteDonation_Should_Throw_NotFoundException_When_Donation_Does_Not_Exist()
    {
        // Arrange
        using var fixture = new DonationServiceFixture();

        // Act
        Func<Task> act = () => fixture.Service.DeleteDonation(999);

        // Assert
        await act
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Donation with id 999 was not found.");
    }
}
