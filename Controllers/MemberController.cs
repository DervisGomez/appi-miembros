namespace ChurchApi.Controllers;

using ChurchApi.Services;
using Microsoft.AspNetCore.Mvc;
using ChurchApi.Models;
using ChurchApi.Dtos;
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

    

    [HttpGet]
    public async Task<IActionResult> GetMembers()
    {
        var members = await _memberService.GetMembers();
        var response = members.Select(MemberMapper.ToDto).ToList();
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMember(int id)
    {
        var member = await _memberService.GetMember(id);
        if (member == null)
        {
            return NotFound();
        }

        return Ok(MemberMapper.ToDto(member));
    }

    [HttpPost]
    public async Task<IActionResult> AddMember(CreateMemberDto dto)
    {
        var member = MemberMapper.ToModel(dto);
        await _memberService.AddMember(member);
        return CreatedAtAction(nameof(GetMember), new { id = member.Id }, MemberMapper.ToDto(member));
    }

    [HttpPut]
    public async Task<IActionResult> UpdateMember(UpdateMemberDto dto)
    {
                
        var updatedMember = await _memberService.UpdateMember(MemberMapper.ToModel(dto));
        if (updatedMember == null)
        {
            return NotFound();
        }
        return Ok(MemberMapper.ToDto(updatedMember));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMember(int id)
    {
        var deletedMember = await _memberService.DeleteMember(id);

        if (deletedMember == null)
        {
            return NotFound();
        }

        return Ok(MemberMapper.ToDto(deletedMember));
    }

    [HttpGet("{memberId}/donations")]
    public async Task<IActionResult> GetDonationsByMemberId(int memberId)
    {
        var donations = await _donationService.GetDonationsByMemberId(memberId);
        var response = donations.Select(DonationMapper.ToDto).ToList();
        return Ok(response);
    }

    [HttpPost("{memberId}/donations")]
    public async Task<IActionResult> AddDonation(CreateDonationDto dto, int memberId)
    {
        var donation = await _donationService.AddDonation(dto, memberId);
        if (donation == null)
        {
            return NotFound();
        }
        return CreatedAtAction(nameof(GetDonationsByMemberId), new { memberId = donation.MemberId }, DonationMapper.ToDto(donation));
    }
}