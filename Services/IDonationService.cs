namespace ChurchApi.Services;

using ChurchApi.Models;
using ChurchApi.Dtos;

public interface IDonationService
{
    Task<PagedResponse<DonationMemberResponseDto>> GetDonations(DonationQueryDto queryDto);
    Task<List<Donation>> GetDonationsByMemberId(int memberId);
    // Task<Donation?> GetDonation(int id);
    Task<Donation?> AddDonation(CreateDonationDto dto, int memberId);
    // Task<Donation?> UpdateDonation(Donation donation);
    Task<Donation?> DeleteDonation(int id);
}