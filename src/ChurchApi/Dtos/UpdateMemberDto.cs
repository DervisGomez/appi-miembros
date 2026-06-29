using System.ComponentModel.DataAnnotations;

namespace ChurchApi.Dtos;

public class UpdateMemberDto
{
    [Required]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Phone]
    [MaxLength(255)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [Range(1, 100)]
    public int Age { get; set; }
}
