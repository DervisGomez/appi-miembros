using System.ComponentModel.DataAnnotations;
namespace ChurchApi.Dtos;

public class LoginDto
{
    [Required]
    [MinLength(3)]
    public string Username { get; set; } = string.Empty;
    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
}