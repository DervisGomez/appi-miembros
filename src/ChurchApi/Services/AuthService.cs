using ChurchApi.Data;
using ChurchApi.Dtos;
using ChurchApi.Enums;
using ChurchApi.Helpers;
using ChurchApi.Interfaces;
using ChurchApi.Models;
using Microsoft.EntityFrameworkCore;
using ChurchApi.Exceptions;

namespace ChurchApi.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(AppDbContext context, IJwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<UserResponseDto> Register(RegisterDto registerDto)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == registerDto.Username || u.Email == registerDto.Email);
        if (existingUser is not null)
        {
            throw new ConflictException("User already exists");
        }

        var user = new User
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            PasswordHash = AuthPasswordHasher.Hash(registerDto.Password),
            Role = UserRole.User,
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
        };
    }

    public async Task<AuthResponseDto> Login(LoginDto loginDto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginDto.Username || u.Email == loginDto.Username);
        if (user is null || !AuthPasswordHasher.Verify(loginDto.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid username or password");
        }
        return new AuthResponseDto
        {
            Token = _jwtTokenService.GenerateToken(user.Id, user.Role),
        };
    }

    public async Task<UserResponseDto?> PromoteToAdmin(int userId)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user is null)
        {
            throw new NotFoundException($"User with id {userId} was not found.");
        }

        user.Role = UserRole.Admin;
        await _context.SaveChangesAsync();

        return new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
        };
    }
}
