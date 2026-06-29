namespace ChurchApi.Mappers;

using ChurchApi.Dtos;
using ChurchApi.Models;

public static class UserMapper
{
    public static UserResponseDto ToDto(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
        };
    }
}
