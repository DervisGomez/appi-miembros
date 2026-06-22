using ChurchApi.Data;
using ChurchApi.Dtos;
using ChurchApi.Enums;
using ChurchApi.Helpers;
using ChurchApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ChurchApi.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<UserResponseDto> Register(RegisterDto registerDto)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == registerDto.Username || u.Email == registerDto.Email);
        if (existingUser is not null)
        {
            throw new Exception("User already exists");
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
        if (user is null)
        {
            throw new Exception("Invalid username or password");
        }
        if (!AuthPasswordHasher.Verify(loginDto.Password, user.PasswordHash))
        {
            throw new Exception("Invalid username or password");
        }
        return new AuthResponseDto
        {
            Token = GenerateToken(user.Id, user.Role),
        };
    }

    private string GenerateToken(int userId, UserRole role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var secret = Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? _configuration["Jwt:Secret"];

        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new InvalidOperationException(
                "JWT secret is not configured. Set Jwt:Secret in appsettings or the JWT_SECRET environment variable.");
        }

        var key = Encoding.UTF8.GetBytes(secret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()), new Claim(ClaimTypes.Role, role.ToString()) }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<UserResponseDto?> PromoteToAdmin(int userId)
{
    var user = await _context.Users.FindAsync(userId);

    if (user == null)
    {
        return null;
    }

    user.Role = UserRole.Admin;

    await _context.SaveChangesAsync();

    return new UserResponseDto
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email,
        Role = user.Role
    };
}
}
