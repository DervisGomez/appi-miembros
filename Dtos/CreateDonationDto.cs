namespace ChurchApi.Dtos;
using System.ComponentModel.DataAnnotations;

public class CreateDonationDto
{
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [MaxLength(255)]
    public string Description { get; set; } = string.Empty;
}