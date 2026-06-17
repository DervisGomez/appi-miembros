namespace ChurchApi.Mappers;

using ChurchApi.Models;
using ChurchApi.Dtos;

public static class MemberMapper
{
    public static MemberResponseDto ToDto(Member member)
    {
        return new MemberResponseDto
        {
            Id = member.Id,
            Name = member.Name,
            LastName = member.LastName,
            Email = member.Email,
            Phone = member.Phone,
            Age = member.Age,
            Donations = member.Donations.Select(DonationMapper.ToDto).ToList()
        };
    }

    public static Member ToModel(CreateMemberDto dto)
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

    public static Member ToModel(UpdateMemberDto dto)
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
}