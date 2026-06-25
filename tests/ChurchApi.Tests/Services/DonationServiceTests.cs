using ChurchApi.Dtos;
using ChurchApi.Enums;
using ChurchApi.Exceptions;
using ChurchApi.Models;
using ChurchApi.Services;
using ChurchApi.Tests.Helpers;
using FluentAssertions;

namespace ChurchApi.Tests.Services;

public class DonationServiceTests
{
    [Fact]
    public async Task GetDonations_Should_Return_Paged_Donations()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var service = new DonationService(context);

        var member = await SeedMemberAsync(context);
        await SeedDonationAsync(context, member, amount: 100m, date: new DateTime(2024, 1, 1));
        await SeedDonationAsync(context, member, amount: 200m, date: new DateTime(2024, 2, 1));

        var queryDto = new DonationQueryDto
        {
            Page = 1,
            PageSize = 10,
            SortOrder = SortOrder.Desc
        };

        // Act
        var result = await service.GetDonations(queryDto);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalItems.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(1);
        result.Items[0].Amount.Should().Be(200m);
        result.Items[0].Member.Name.Should().Be(member.Name);
    }

    [Fact]
    public async Task GetDonations_Should_Filter_By_MemberId()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var service = new DonationService(context);

        var memberOne = await SeedMemberAsync(context, name: "John", lastName: "Doe");
        var memberTwo = await SeedMemberAsync(context, name: "Jane", lastName: "Smith");

        await SeedDonationAsync(context, memberOne, amount: 50m);
        await SeedDonationAsync(context, memberTwo, amount: 150m);

        var queryDto = new DonationQueryDto
        {
            MemberId = memberOne.Id
        };

        // Act
        var result = await service.GetDonations(queryDto);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Amount.Should().Be(50m);
        result.Items[0].Member.Id.Should().Be(memberOne.Id);
    }

    [Fact]
    public async Task GetDonations_Should_Throw_ValidationException_When_MinAmount_Is_Greater_Than_MaxAmount()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var service = new DonationService(context);

        var queryDto = new DonationQueryDto
        {
            MinAmount = 500m,
            MaxAmount = 100m
        };

        // Act
        Func<Task> act = () => service.GetDonations(queryDto);

        // Assert
        await act
            .Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("MinAmount cannot be greater than MaxAmount.");
    }

    [Fact]
    public async Task AddDonation_Should_Create_Donation_When_Member_Exists()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var service = new DonationService(context);

        var member = await SeedMemberAsync(context);

        var dto = new CreateDonationDto
        {
            Amount = 250m,
            Description = "Monthly offering"
        };

        // Act
        var result = await service.AddDonation(dto, member.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Amount.Should().Be(250m);
        result.Description.Should().Be("Monthly offering");
        result.MemberId.Should().Be(member.Id);

        var donationInDb = context.Donations.Single();
        donationInDb.Amount.Should().Be(250m);
        donationInDb.Description.Should().Be("Monthly offering");
    }

    [Fact]
    public async Task AddDonation_Should_Return_Null_When_Member_Does_Not_Exist()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var service = new DonationService(context);

        var dto = new CreateDonationDto
        {
            Amount = 100m,
            Description = "Offering"
        };

        // Act
        var result = await service.AddDonation(dto, memberId: 999);

        // Assert
        result.Should().BeNull();
        context.Donations.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteDonation_Should_Delete_Donation()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var service = new DonationService(context);

        var member = await SeedMemberAsync(context);
        var donation = await SeedDonationAsync(context, member, amount: 75m);

        // Act
        var result = await service.DeleteDonation(donation.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(donation.Id);
        result.Amount.Should().Be(75m);
        context.Donations.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteDonation_Should_Throw_NotFoundException_When_Donation_Does_Not_Exist()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var service = new DonationService(context);

        // Act
        Func<Task> act = () => service.DeleteDonation(999);

        // Assert
        await act
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Donation with id 999 was not found.");
    }

    private static async Task<Member> SeedMemberAsync(
        ChurchApi.Data.AppDbContext context,
        string name = "Dervis",
        string lastName = "Gomez")
    {
        var member = new Member
        {
            Name = name,
            LastName = lastName,
            Email = $"{name.ToLower()}@test.com",
            Phone = "123456789",
            Age = 30
        };

        context.Members.Add(member);
        await context.SaveChangesAsync();

        return member;
    }

    private static async Task<Donation> SeedDonationAsync(
        ChurchApi.Data.AppDbContext context,
        Member member,
        decimal amount,
        DateTime? date = null)
    {
        var donation = new Donation
        {
            Amount = amount,
            Description = "Test donation",
            MemberId = member.Id,
            Date = date ?? DateTime.UtcNow,
            Member = member
        };

        context.Donations.Add(donation);
        await context.SaveChangesAsync();

        return donation;
    }
}
