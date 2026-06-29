using ChurchApi.Data;
using ChurchApi.Dtos;
using ChurchApi.Enums;
using ChurchApi.Exceptions;
using ChurchApi.Helpers;
using ChurchApi.Interfaces;
using ChurchApi.Mappers;
using ChurchApi.Models;
using Microsoft.EntityFrameworkCore;

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

        try
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (PersistenceExceptionTranslator.IsUniqueConstraintViolation(exception))
        {
            _logger.LogWarning(
                exception,
                "Unique constraint violation while registering a user.");

            throw await BuildUserConflictException(registerDto);
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(
                exception,
                "Unexpected persistence error while registering a user.");

            throw;
        }

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
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(
                exception,
                "Unexpected persistence error while promoting user {UserId} to admin.",
                userId);

            throw;
        }

        _logger.LogInformation("User promoted to admin with id {UserId}", user.Id);

        return UserMapper.ToDto(user);
    }

    private async Task EnsureUserDoesNotExist(string username, string email)
    {
        if (await _context.Users.AnyAsync(u => u.Username == username))
        {
            throw new ConflictException("Username already exists.");
        }

        if (await _context.Users.AnyAsync(u => u.Email == email))
        {
            throw new ConflictException("Email already exists.");
        }
    }

    private async Task<ConflictException> BuildUserConflictException(RegisterDto registerDto)
    {
        var conflictMessage = await GetUserConflictMessage(registerDto);
        return new ConflictException(conflictMessage);
    }

    private async Task<string> GetUserConflictMessage(RegisterDto registerDto)
    {
        if (await _context.Users.AsNoTracking().AnyAsync(u => u.Username == registerDto.Username))
        {
            return "Username already exists.";
        }

        if (await _context.Users.AsNoTracking().AnyAsync(u => u.Email == registerDto.Email))
        {
            return "Email already exists.";
        }

        return "User already exists.";
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
