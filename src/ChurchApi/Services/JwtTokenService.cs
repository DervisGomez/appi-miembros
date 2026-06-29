using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChurchApi.Enums;
using ChurchApi.Interfaces;
using ChurchApi.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ChurchApi.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _jwtOptions;
    private readonly TimeProvider _timeProvider;

    public JwtTokenService(IOptions<JwtOptions> jwtOptions, TimeProvider timeProvider)
    {
        _jwtOptions = jwtOptions.Value;
        _timeProvider = timeProvider;
    }

    public string GenerateToken(int userId, UserRole role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(BuildTokenDescriptor(userId, role));

        return tokenHandler.WriteToken(token);
    }

    private SecurityTokenDescriptor BuildTokenDescriptor(int userId, UserRole role)
    {
        return new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(BuildClaims(userId, role)),
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            Expires = _timeProvider.GetUtcNow().UtcDateTime.AddMinutes(_jwtOptions.ExpirationMinutes),
            SigningCredentials = BuildSigningCredentials(),
        };
    }

    private static Claim[] BuildClaims(int userId, UserRole role)
    {
        return
        [
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role.ToString())
        ];
    }

    private SigningCredentials BuildSigningCredentials()
    {
        var key = Encoding.UTF8.GetBytes(_jwtOptions.Secret);
        return new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature);
    }
}
