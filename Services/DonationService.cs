namespace ChurchApi.Services;

using ChurchApi.Models;
using ChurchApi.Dtos;
using ChurchApi.Data;
using Microsoft.EntityFrameworkCore;
using ChurchApi.Mappers;

public class DonationService : IDonationService
{
    private readonly AppDbContext _context;

    public DonationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResponse<DonationMemberResponseDto>> GetDonations(DonationQueryDto queryDto)
    {
        var query = _context.Donations.Include(d => d.Member).AsQueryable();
        if (queryDto.MemberId != null)
        {
            query = query.Where(d => d.MemberId == queryDto.MemberId);
        }
        if (queryDto.MinAmount != null)
        {
            query = query.Where(d => d.Amount >= queryDto.MinAmount);
        }
        if (queryDto.MaxAmount != null)
        {
            query = query.Where(d => d.Amount <= queryDto.MaxAmount);
        }
        var totalItems = await query.CountAsync();
        var page = queryDto.Page < 1
            ? 1
            : queryDto.Page;

        var pageSize = queryDto.PageSize < 1
            ? 10
            : queryDto.PageSize;

        pageSize = Math.Min(pageSize, 100);
        query = query.Skip((page - 1) * pageSize).Take(pageSize);

        
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
        var items = await query.ToListAsync();
        var response = new PagedResponse<DonationMemberResponseDto>
        {
            Items = items
                .Select(DonationMapper.ToResponseDto)
                .ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };
        return response;
    }

    public async Task<List<Donation>> GetDonationsByMemberId(int memberId)
    {
        return await _context.Donations.Where(d => d.MemberId == memberId).Include(d => d.Member).ToListAsync();
    }

    public async Task<Donation?> AddDonation(CreateDonationDto dto, int memberId)
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