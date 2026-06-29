namespace ChurchApi.Services;

using ChurchApi.Data;
using ChurchApi.Dtos;
using ChurchApi.Enums;
using ChurchApi.Exceptions;
using ChurchApi.Mappers;
using ChurchApi.Models;
using Microsoft.EntityFrameworkCore;

public class DonationService : IDonationService
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 100;

    private readonly AppDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<DonationService> _logger;

    public DonationService(
        AppDbContext context,
        TimeProvider timeProvider,
        ILogger<DonationService> logger)
    {
        _context = context;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<PagedResponse<DonationMemberResponseDto>> GetDonations(DonationQueryDto queryDto)
    {
        ValidateDonationQuery(queryDto);

        var query = BuildDonationQuery(queryDto);
        var totalItems = await query.CountAsync();
        var (page, pageSize) = NormalizePaging(queryDto.Page, queryDto.PageSize);

        var donations = await ApplyPaging(query, page, pageSize).ToListAsync();
        var items = donations.Select(DonationMapper.ToResponseDto).ToList();

        return BuildPagedResponse(items, page, pageSize, totalItems);
    }

    public async Task<List<Donation>> GetDonationsByMemberId(int memberId)
    {
        await EnsureMemberExists(memberId);

        return await _context.Donations.Where(d => d.MemberId == memberId).Include(d => d.Member).ToListAsync();
    }

    public async Task<Donation> AddDonation(CreateDonationDto dto, int memberId)
    {
        await EnsureMemberExists(memberId);

        var donation = CreateDonation(dto, memberId);
        await _context.Donations.AddAsync(donation);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Donation created with id {DonationId} for member {MemberId}",
            donation.Id,
            memberId);

        return donation;   
    }
    public async Task<Donation> DeleteDonation(int id)
    {
        var donation = await GetDonationOrThrow(id);

        _context.Donations.Remove(donation);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Donation deleted with id {DonationId}", donation.Id);

        return donation;
    }

    private static void ValidateDonationQuery(DonationQueryDto queryDto)
    {
        if (queryDto.MinAmount is not null
            && queryDto.MaxAmount is not null
            && queryDto.MinAmount > queryDto.MaxAmount)
        {
            throw new ValidationException("MinAmount cannot be greater than MaxAmount.");
        }
    }

    private IQueryable<Donation> BuildDonationQuery(DonationQueryDto queryDto)
    {
        var query = _context.Donations
            .Include(d => d.Member)
            .AsQueryable();

        query = ApplyFilters(query, queryDto);
        return ApplySorting(query, queryDto.SortOrder);
    }

    private static IQueryable<Donation> ApplyFilters(
        IQueryable<Donation> query,
        DonationQueryDto queryDto)
    {
        if (queryDto.MemberId is not null)
        {
            query = query.Where(d => d.MemberId == queryDto.MemberId);
        }

        if (queryDto.MinAmount is not null)
        {
            query = query.Where(d => d.Amount >= queryDto.MinAmount);
        }

        if (queryDto.MaxAmount is not null)
        {
            query = query.Where(d => d.Amount <= queryDto.MaxAmount);
        }

        return query;
    }

    private static IQueryable<Donation> ApplySorting(
        IQueryable<Donation> query,
        SortOrder sortOrder)
    {
        return sortOrder == SortOrder.Asc
            ? query.OrderBy(d => d.Date)
            : query.OrderByDescending(d => d.Date);
    }

    private static IQueryable<Donation> ApplyPaging(
        IQueryable<Donation> query,
        int page,
        int pageSize)
    {
        return query
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
    }

    private static (int Page, int PageSize) NormalizePaging(int requestedPage, int requestedPageSize)
    {
        var page = requestedPage < 1 ? DefaultPage : requestedPage;
        var pageSize = requestedPageSize < 1 ? DefaultPageSize : requestedPageSize;

        return (page, Math.Min(pageSize, MaxPageSize));
    }

    private static PagedResponse<DonationMemberResponseDto> BuildPagedResponse(
        List<DonationMemberResponseDto> items,
        int page,
        int pageSize,
        int totalItems)
    {
        return new PagedResponse<DonationMemberResponseDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = CalculateTotalPages(totalItems, pageSize)
        };
    }

    private static int CalculateTotalPages(int totalItems, int pageSize)
    {
        return totalItems == 0
            ? 0
            : (int)Math.Ceiling((double)totalItems / pageSize);
    }

    private async Task EnsureMemberExists(int memberId)
    {
        var memberExists = await _context.Members.AnyAsync(m => m.Id == memberId);
        if (!memberExists)
        {
            throw new NotFoundException($"Member with id {memberId} was not found.");
        }
    }

    private async Task<Donation> GetDonationOrThrow(int id)
    {
        var donation = await _context.Donations.FirstOrDefaultAsync(d => d.Id == id);
        if (donation is null)
        {
            throw new NotFoundException($"Donation with id {id} was not found.");
        }

        return donation;
    }

    private Donation CreateDonation(CreateDonationDto dto, int memberId)
    {
        var donation = DonationMapper.ToModel(dto);
        donation.MemberId = memberId;
        donation.Date = _timeProvider.GetUtcNow().UtcDateTime;

        return donation;
    }
}
