namespace ChurchApi.Controllers;

using ChurchApi.Services;
using Microsoft.AspNetCore.Mvc;
using ChurchApi.Models;
using ChurchApi.Dtos;

[ApiController]
[Route("api/[controller]")]
public class DonationsController : ControllerBase
{
    private readonly IDonationService _donationService;
    public DonationsController(IDonationService donationService)
    {
        _donationService = donationService;
    }

    private static DonationMemberResponseDto ToDto(Donation donation)
    {
        return new DonationMemberResponseDto
        {
            Id = donation.Id,
            Amount = donation.Amount,
            Date = donation.Date,
            Description = donation.Description,
            Member = new MemberDonationResponseDto
            {
                Id = donation.Member.Id,
                Name = donation.Member.Name,
                LastName = donation.Member.LastName,
                Email = donation.Member.Email,
                Phone = donation.Member.Phone,
                Age = donation.Member.Age
            }
        };
    }

    [HttpGet]
    public async Task<IActionResult> GetDonations()
    {
        var donations = await _donationService.GetDonations();
        var response = donations.Select(ToDto).ToList();
        return Ok(response);
    }
}   