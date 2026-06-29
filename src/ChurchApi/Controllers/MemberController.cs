using Microsoft.AspNetCore.Authorization;
namespace ChurchApi.Controllers;

using ChurchApi.Services;
using Microsoft.AspNetCore.Mvc;
using ChurchApi.Models;
using ChurchApi.Dtos;
using ChurchApi.Exceptions;
using ChurchApi.Mappers;

[ApiController]
[Route("api/[controller]")]
public class MembersController : ControllerBase
{
    private readonly IMemberService _memberService;
    private readonly IDonationService _donationService;
    public MembersController(IMemberService memberService, IDonationService donationService)
    {
        _memberService = memberService;
        _donationService = donationService;
    } 
    
    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<MemberResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMembers([FromQuery] MemberQueryDto queryDto)
    {
        var pagedResponse = await _memberService.GetMembers(queryDto);
        return Ok(pagedResponse);
    }

    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MemberResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMember(int id)
    {
        var member = await _memberService.GetMember(id);
        return Ok(MemberMapper.ToDto(member));
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(MemberResponseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddMember(CreateMemberDto dto)
    {
        var member = MemberMapper.ToModel(dto);
        await _memberService.AddMember(member);
        return CreatedAtAction(nameof(GetMember), new { id = member.Id }, MemberMapper.ToDto(member));
    }


    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MemberResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateMember(int id, UpdateMemberDto dto)
    {
        if (id != dto.Id)
        {
            throw new ValidationException("Route id must match request body id.");
        }

        var updatedMember = await _memberService.UpdateMember(MemberMapper.ToModel(dto));
        return Ok(MemberMapper.ToDto(updatedMember));
    }


    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteMember(int id)
    {
        await _memberService.DeleteMember(id);
        return NoContent();
    }

    [Authorize]
    [HttpGet("{memberId}/donations")]
    [ProducesResponseType(typeof(List<DonationResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDonationsByMemberId(int memberId)
    {
        var donations = await _donationService.GetDonationsByMemberId(memberId);
        var response = donations.Select(DonationMapper.ToDto).ToList();
        return Ok(response);
    }

    [Authorize]
    [HttpPost("{memberId}/donations")]
    [ProducesResponseType(typeof(DonationResponseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddDonation(CreateDonationDto dto, int memberId)
    {
        var donation = await _donationService.AddDonation(dto, memberId);
        return CreatedAtAction(nameof(GetDonationsByMemberId), new { memberId = donation.MemberId }, DonationMapper.ToDto(donation));
    }
}
