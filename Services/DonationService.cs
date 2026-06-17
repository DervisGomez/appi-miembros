namespace ChurchApi.Services;

using ChurchApi.Data;
using ChurchApi.Dtos;
using ChurchApi.Enums;
using ChurchApi.Mappers;
using ChurchApi.Models;
using Microsoft.EntityFrameworkCore;

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
        Console.WriteLine(queryDto.SortOrder);
        Console.WriteLine((int)queryDto.SortOrder);
        if (queryDto.SortOrder == SortOrder.Asc)
        {
            query = query.OrderBy(d => d.Date);
        }
        else
        {
            query = query.OrderByDescending(d => d.Date);
        }
        var totalItems = await query.CountAsync();
        var page = queryDto.Page < 1
            ? 1
            : queryDto.Page;

        var pageSize = queryDto.PageSize < 1
            ? 10
            : queryDto.PageSize;

        pageSize = Math.Min(pageSize, 100);
        // query = query.Skip((page - 1) * pageSize).Take(pageSize);

        
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        var items = await query
        // .OrderByDescending(d => d.Date)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(d => new DonationMemberResponseDto
        {
            Id = d.Id,
            Amount = d.Amount,
            Date = d.Date,
            Description = d.Description,

            Member = new MemberDonationResponseDto
            {
                Id = d.Member.Id,
                Name = d.Member.Name,
                LastName = d.Member.LastName
            }
        })
        .ToListAsync();

        
        // var items = await query.ToListAsync();
        var response = new PagedResponse<DonationMemberResponseDto>
        {
            Items = items,
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
    public async Task<Donation?> DeleteDonation(int id)
    {
        var donation = await _context.Donations.FirstOrDefaultAsync(d => d.Id == id);
        if (donation == null)
        {
            return null;
        }
        _context.Donations.Remove(donation);
        await _context.SaveChangesAsync();
        return donation;
    }
}