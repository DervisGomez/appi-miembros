using ChurchApi.Dtos;
using ChurchApi.Enums;
using ChurchApi.Interfaces;
using ChurchApi.Services;
using ChurchApi.Tests.Helpers;
using FluentAssertions;
using Moq;
using ChurchApi.Models;
using ChurchApi.Helpers;
using ChurchApi.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;

namespace ChurchApi.Tests.Unit.Services;

public class AuthServiceTests
{
    [Fact]
    public async Task Register_Should_Create_User()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var jwtTokenServiceMock = new Mock<IJwtTokenService>();
        var service = new AuthService(
            context,
            jwtTokenServiceMock.Object,
            NullLogger<AuthService>.Instance);

        var dto = new RegisterDto
        {
            Username = "dervis",
            Email = "dervis@test.com",
            Password = "123456"
        };

        // Act
        var result = await service.Register(dto);

        // Assert
        result.Should().NotBeNull();

        result.Username.Should().Be("dervis");

        result.Email.Should().Be("dervis@test.com");

        result.Role.Should().Be(UserRole.User);
    }

    [Fact]
    public async Task Register_Should_Throw_ConflictException_When_Email_Already_Exists()
    {
        // Arrange
        var context = TestDbContextFactory.Create();

        var jwtTokenServiceMock = new Mock<IJwtTokenService>();

        var service = new AuthService(
            context,
            jwtTokenServiceMock.Object,
            NullLogger<AuthService>.Instance);

        var existingUser = new User
        {
            Username = "otroUsuario",
            Email = "dervis@test.com",
            PasswordHash = AuthPasswordHasher.Hash("123456"),
            Role = UserRole.User
        };

        context.Users.Add(existingUser);

        await context.SaveChangesAsync();

        var dto = new RegisterDto
        {
            Username = "dervis",
            Email = "dervis@test.com",
            Password = "654321"
        };

        // Act
        Func<Task> act = () => service.Register(dto);

        // Assert
        await act
            .Should()
            .ThrowAsync<ConflictException>()
            .WithMessage("User already exists");
    }
    [Fact]
    public async Task Login_Should_Return_Token_When_Valid_Credentials()
    {
        // Arrange
        var context = TestDbContextFactory.Create();

        var user = new User
        {
            Username = "dervis",
            Email = "dervis@test.com",
            PasswordHash = AuthPasswordHasher.Hash("123456"),
            Role = UserRole.User
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var jwtTokenServiceMock = new Mock<IJwtTokenService>();
        jwtTokenServiceMock
            .Setup(x => x.GenerateToken(
            It.IsAny<int>(),
            It.IsAny<UserRole>()))
            .Returns("fake-jwt-token");

        var service = new AuthService(
            context,
            jwtTokenServiceMock.Object,
            NullLogger<AuthService>.Instance);

        var dto = new LoginDto
        {
            Username = "dervis",
            Password = "123456"
        };

        // Act
        var result = await service.Login(dto);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrWhiteSpace();
        jwtTokenServiceMock.Verify(x => x.GenerateToken(user.Id, user.Role), Times.Once);
    }

    [Fact]
    public async Task Login_Should_Throw_UnauthorizedException_When_User_Does_Not_Exist()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var jwtTokenServiceMock = new Mock<IJwtTokenService>();
        var service = new AuthService(
            context,
            jwtTokenServiceMock.Object,
            NullLogger<AuthService>.Instance);

        var dto = new LoginDto
        {
            Username = "dervis",
            Password = "123456"
        };

        // Act
        Func<Task> act = () => service.Login(dto);

        // Assert
        await act
            .Should()
            .ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid username or password");

        jwtTokenServiceMock.Verify(
            x => x.GenerateToken(It.IsAny<int>(), It.IsAny<UserRole>()),
            Times.Never);
    }

    [Fact]
    public async Task Login_Should_Throw_UnauthorizedException_When_Password_Is_Invalid()
    {
        // Arrange
        var context = TestDbContextFactory.Create();

        var user = new User
        {
            Username = "dervis",
            Email = "dervis@test.com",
            PasswordHash = AuthPasswordHasher.Hash("123456"),
            Role = UserRole.User
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var jwtTokenServiceMock = new Mock<IJwtTokenService>();
        var service = new AuthService(
            context,
            jwtTokenServiceMock.Object,
            NullLogger<AuthService>.Instance);

        var dto = new LoginDto
        {
            Username = "dervis",
            Password = "wrong-password"
        };

        // Act
        Func<Task> act = () => service.Login(dto);

        // Assert
        await act
            .Should()
            .ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid username or password");

        jwtTokenServiceMock.Verify(
            x => x.GenerateToken(It.IsAny<int>(), It.IsAny<UserRole>()),
            Times.Never);
    }
}
