using ChurchApi.Data;
using ChurchApi.Dtos;
using ChurchApi.Enums;
using ChurchApi.Helpers;
using ChurchApi.Interfaces;
using ChurchApi.Mappers;
using ChurchApi.Models;
using Microsoft.EntityFrameworkCore;
using ChurchApi.Exceptions;

namespace ChurchApi.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AppDbContext context,
        IJwtTokenService jwtTokenService,
        ILogger<AuthService> logger)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<UserResponseDto> Register(RegisterDto registerDto)
    {
        await EnsureUserDoesNotExist(registerDto.Username, registerDto.Email);

        var user = CreateUser(registerDto);

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User registered with id {UserId}", user.Id);

        return UserMapper.ToDto(user);
    }

    public async Task<AuthResponseDto> Login(LoginDto loginDto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == loginDto.Username || u.Email == loginDto.Username);
        var authenticatedUser = ValidateCredentials(user, loginDto.Password);

        _logger.LogInformation(
            "User authenticated with id {UserId} and role {Role}",
            authenticatedUser.Id,
            authenticatedUser.Role);

        return new AuthResponseDto
        {
            Token = _jwtTokenService.GenerateToken(authenticatedUser.Id, authenticatedUser.Role),
        };
    }

    public async Task<UserResponseDto> PromoteToAdmin(int userId)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user is null)
        {
            throw new NotFoundException($"User with id {userId} was not found.");
        }

        user.Role = UserRole.Admin;
        await _context.SaveChangesAsync();

        _logger.LogInformation("User promoted to admin with id {UserId}", user.Id);

        return UserMapper.ToDto(user);
    }

    private async Task EnsureUserDoesNotExist(string username, string email)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username || u.Email == email);

        if (existingUser is not null)
        {
            throw new ConflictException("User already exists");
        }
    }

    private static User CreateUser(RegisterDto registerDto)
    {
        return new User
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            PasswordHash = AuthPasswordHasher.Hash(registerDto.Password),
            Role = UserRole.User,
        };
    }

    private static User ValidateCredentials(User? user, string password)
    {
        if (user is null || !AuthPasswordHasher.Verify(password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid username or password");
        }

        return user;
    }
}
