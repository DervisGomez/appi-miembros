using ChurchApi.Dtos;
using ChurchApi.Enums;
using ChurchApi.Exceptions;
using ChurchApi.Models;
using ChurchApi.Services;
using ChurchApi.Tests.Helpers;
using FluentAssertions;

namespace ChurchApi.Tests.Services;

public class MemberServiceTests
{
    [Fact]
    public async Task GetMembers_Should_Return_Paged_Members()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var service = new MemberService(context);

        await TestDataSeeder.CreateMemberAsync(context, name: "Alice", lastName: "Brown");
        await TestDataSeeder.CreateMemberAsync(context, name: "Bob", lastName: "Smith");

        var queryDto = new MemberQueryDto
        {
            Page = 1,
            PageSize = 10,
            SortOrder = SortOrder.Asc
        };

        // Act
        var result = await service.GetMembers(queryDto);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalItems.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(1);
        result.Items[0].Name.Should().Be("Alice");
        result.Items[1].Name.Should().Be("Bob");
    }

    [Fact]
    public async Task GetMembers_Should_Sort_By_Name_Descending()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var service = new MemberService(context);

        await TestDataSeeder.CreateMemberAsync(context, name: "Alice", lastName: "Brown");
        await TestDataSeeder.CreateMemberAsync(context, name: "Bob", lastName: "Smith");

        var queryDto = new MemberQueryDto
        {
            SortOrder = SortOrder.Desc
        };

        // Act
        var result = await service.GetMembers(queryDto);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items[0].Name.Should().Be("Bob");
        result.Items[1].Name.Should().Be("Alice");
    }

    [Fact]
    public async Task AddMember_Should_Create_Member()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var service = new MemberService(context);

        var member = new Member
        {
            Name = "Dervis",
            LastName = "Gomez",
            Email = "dervis@test.com",
            Phone = "123456789",
            Age = 30
        };

        // Act
        await service.AddMember(member);

        // Assert
        member.Id.Should().BeGreaterThan(0);

        var memberInDb = context.Members.Single();
        memberInDb.Name.Should().Be("Dervis");
        memberInDb.LastName.Should().Be("Gomez");
        memberInDb.Email.Should().Be("dervis@test.com");
        memberInDb.Phone.Should().Be("123456789");
        memberInDb.Age.Should().Be(30);
    }

    [Fact]
    public async Task UpdateMember_Should_Update_Member()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var service = new MemberService(context);

        var existingMember = await TestDataSeeder.CreateMemberAsync(context, name: "John", lastName: "Doe");

        var updatedMember = new Member
        {
            Id = existingMember.Id,
            Name = "Jonathan",
            LastName = "Doe",
            Email = "jonathan@test.com",
            Phone = "987654321",
            Age = 35
        };

        // Act
        var result = await service.UpdateMember(updatedMember);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Jonathan");
        result.Email.Should().Be("jonathan@test.com");
        result.Phone.Should().Be("987654321");
        result.Age.Should().Be(35);

        var memberInDb = context.Members.Single();
        memberInDb.Name.Should().Be("Jonathan");
        memberInDb.Email.Should().Be("jonathan@test.com");
    }

    [Fact]
    public async Task UpdateMember_Should_Throw_NotFoundException_When_Member_Does_Not_Exist()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var service = new MemberService(context);

        var member = new Member
        {
            Id = 999,
            Name = "Ghost",
            LastName = "Member",
            Email = "ghost@test.com",
            Phone = "000000000",
            Age = 25
        };

        // Act
        Func<Task> act = () => service.UpdateMember(member);

        // Assert
        await act
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Member with id 999 was not found.");
    }

    [Fact]
    public async Task DeleteMember_Should_Delete_Member()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var service = new MemberService(context);

        var member = await TestDataSeeder.CreateMemberAsync(context);

        // Act
        var result = await service.DeleteMember(member.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(member.Id);
        result.Name.Should().Be(member.Name);
        context.Members.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteMember_Should_Throw_NotFoundException_When_Member_Does_Not_Exist()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var service = new MemberService(context);

        // Act
        Func<Task> act = () => service.DeleteMember(999);

        // Assert
        await act
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Member with id 999 was not found.");
    }

}
