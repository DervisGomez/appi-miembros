namespace ChurchApi.Controllers;

using ChurchApi.Services;
using Microsoft.AspNetCore.Mvc;
using ChurchApi.Models;
using ChurchApi.Dtos;

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

    private static DonationResponseDto ToDto(Donation donation)
    {
        return new DonationResponseDto
        {
            Id = donation.Id,
            Amount = donation.Amount,
            Date = donation.Date,
            Description = donation.Description,
            // MemberId = donation.MemberId
        };
    }

    private static Donation ToModel(CreateDonationDto dto)
    {
        return new Donation { Amount = dto.Amount, Description = dto.Description };
    }

    private static MemberResponseDto ToDto(Member member)
    {
        return new MemberResponseDto
        {
            Id = member.Id,
            Name = member.Name,
            LastName = member.LastName,
            Email = member.Email,
            Phone = member.Phone,
            Age = member.Age,
            Donations = member.Donations.Select(ToDto).ToList()
        };
    }

    private static Member ToModel(CreateMemberDto dto)
    {
        return new Member
        {
            Name = dto.Name,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Age = dto.Age
        };
    }

    private static Member ToModel(UpdateMemberDto dto)
    {
        return new Member
        {
            Id = dto.Id,
            Name = dto.Name,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Age = dto.Age
        };
    }

    [HttpGet]
    public async Task<IActionResult> GetMembers()
    {
        var members = await _memberService.GetMembers();
        var response = members.Select(ToDto).ToList();
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

        return Ok(ToDto(member));
    }

    [HttpPost]
    public async Task<IActionResult> AddMember(CreateMemberDto dto)
    {
        var member = ToModel(dto);
        await _memberService.AddMember(member);
        return CreatedAtAction(nameof(GetMember), new { id = member.Id }, ToDto(member));
    }

    [HttpPut]
    public async Task<IActionResult> UpdateMember(UpdateMemberDto dto)
    {
                
        var updatedMember = await _memberService.UpdateMember(ToModel(dto));
        if (updatedMember == null)
        {
            return NotFound();
        }
        return Ok(ToDto(updatedMember));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMember(int id)
    {
        var deletedMember = await _memberService.DeleteMember(id);

        if (deletedMember == null)
        {
            return NotFound();
        }

        return Ok(ToDto(deletedMember));
    }

    [HttpGet("{memberId}/donations")]
    public async Task<IActionResult> GetDonationsByMemberId(int memberId)
    {
        var donations = await _donationService.GetDonationsByMemberId(memberId);
        var response = donations.Select(ToDto).ToList();
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
        return CreatedAtAction(nameof(GetDonationsByMemberId), new { memberId = donation.MemberId }, ToDto(donation));
    }
}