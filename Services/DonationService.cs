namespace ChurchApi.Services;

using ChurchApi.Models;
using ChurchApi.Dtos;
using ChurchApi.Data;
using Microsoft.EntityFrameworkCore;

public class DonationService : IDonationService
{
    private readonly AppDbContext _context;

    public DonationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Donation>> GetDonations()
    {
        return await _context.Donations.Include(d => d.Member).ToListAsync();
    }

    public async Task<List<Donation>> GetDonationsByMemberId(int memberId)
    {
        return await _context.Donations.Where(d => d.MemberId == memberId).Include(d => d.Member).ToListAsync();
    }

    public async Task<Donation> AddDonation(CreateDonationDto dto, int memberId)
    {
        var member = await _context.Members.FirstOrDefaultAsync(m => m.Id == memberId);
        if (member == null)
        {
            return null;
        }
        var donation = new Donation { Amount = dto.Amount, Description = dto.Description, MemberId = memberId, Date = DateTime.Now };
        await _context.Donations.AddAsync(donation);
        await _context.SaveChangesAsync();
        return donation;   
    }
}