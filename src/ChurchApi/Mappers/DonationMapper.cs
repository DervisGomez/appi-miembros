namespace ChurchApi.Mappers;

using ChurchApi.Models;
using ChurchApi.Dtos;

public static class DonationMapper
{
    public static DonationResponseDto ToDto(Donation donation)
    {
        return new DonationResponseDto
        {
            Id = donation.Id,
            Amount = donation.Amount,
            Date = donation.Date,
            Description = donation.Description,
        };
    }

    public static Donation ToModel(CreateDonationDto dto)
    {
        return new Donation { Amount = dto.Amount, Description = dto.Description };
    }

    public static DonationMemberResponseDto ToResponseDto(Donation donation)
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
            }
        };
    }
}
