using ChurchApi.Dtos;
using ChurchApi.Models;

namespace ChurchApi.Services;

public interface IDonationService
{
    Task<PagedResponse<DonationMemberResponseDto>> GetDonations(DonationQueryDto queryDto);
    Task<List<Donation>> GetDonationsByMemberId(int memberId);
    Task<Donation> AddDonation(CreateDonationDto dto, int memberId);
    Task<Donation> DeleteDonation(int id);
}
