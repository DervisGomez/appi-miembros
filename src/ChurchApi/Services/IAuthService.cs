using ChurchApi.Dtos;
namespace ChurchApi.Services;

public interface IAuthService
{
    Task<UserResponseDto> Register(RegisterDto registerDto);
    Task<AuthResponseDto> Login(LoginDto loginDto);

    Task<UserResponseDto> PromoteToAdmin(int userId);
}
