using ChurchApi.Enums;

namespace ChurchApi.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(int userId, UserRole role);
}
