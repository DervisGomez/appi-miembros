namespace ChurchApi.Controllers;

using ChurchApi.Services;
using Microsoft.AspNetCore.Mvc;
using ChurchApi.Models;
using ChurchApi.Dtos;
using ChurchApi.Mappers;

[ApiController]
[Route("api/[controller]")]
public class DonationsController : ControllerBase
{
    private readonly IDonationService _donationService;
    public DonationsController(IDonationService donationService)
    {
        _donationService = donationService;
    }


    [HttpGet]
    public async Task<IActionResult> GetDonations([FromQuery] DonationQueryDto queryDto)
    {
        var pagedResponse = await _donationService.GetDonations(queryDto);
        
        return Ok(pagedResponse);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDonation(int id)
    {
        var deletedDonation = await _donationService.DeleteDonation(id);
        if (deletedDonation == null)
        {
            return NotFound();
        }
        return Ok(deletedDonation);
    }
}   