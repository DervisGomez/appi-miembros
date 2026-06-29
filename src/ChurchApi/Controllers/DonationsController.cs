using ChurchApi.Dtos;
using ChurchApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChurchApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DonationsController : ControllerBase
{
    private readonly IDonationService _donationService;

    public DonationsController(IDonationService donationService)
    {
        _donationService = donationService;
    }

    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<DonationMemberResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDonations([FromQuery] DonationQueryDto queryDto)
    {
        var pagedResponse = await _donationService.GetDonations(queryDto);
        return Ok(pagedResponse);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteDonation(int id)
    {
        await _donationService.DeleteDonation(id);
        return NoContent();
    }
}
