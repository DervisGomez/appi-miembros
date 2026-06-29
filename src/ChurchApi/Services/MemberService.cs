using ChurchApi.Data;
using ChurchApi.Dtos;
using ChurchApi.Enums;
using ChurchApi.Exceptions;
using ChurchApi.Mappers;
using ChurchApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ChurchApi.Services;

public class MemberService : IMemberService
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 100;

    private readonly AppDbContext _context;

    public MemberService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResponse<MemberResponseDto>> GetMembers(MemberQueryDto queryDto)
    {
        var query = BuildMemberQuery(queryDto);

        var totalItems = await query.CountAsync();
        var (page, pageSize) = NormalizePaging(queryDto.Page, queryDto.PageSize);

        var members = await ApplyPaging(query, page, pageSize)
            .ToListAsync();

        var items = members.Select(MemberMapper.ToDto).ToList();

        return BuildPagedResponse(items, page, pageSize, totalItems);
    }

    public async Task AddMember(Member member)
    {
        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();
    }

    public async Task<Member> GetMember(int id)
    {
        if (id <= 0)
        {
            throw new NotFoundException($"Member with id {id} was not found.");
        }

        return await GetMemberWithDonationsOrThrow(id);
    }

    public async Task<Member> UpdateMember(Member member)
    {
        var existingMember = await GetMemberOrThrow(member.Id);

        existingMember.Name = member.Name;
        existingMember.LastName = member.LastName;
        existingMember.Email = member.Email;
        existingMember.Phone = member.Phone;
        existingMember.Age = member.Age;
        await _context.SaveChangesAsync();
        return existingMember;
    }

    public async Task<Member> DeleteMember(int id)
    {
        var member = await GetMemberOrThrow(id);

        _context.Members.Remove(member);

        await _context.SaveChangesAsync();

        return member;
    }

    private IQueryable<Member> BuildMemberQuery(MemberQueryDto queryDto)
    {
        var query = _context.Members
            .Include(m => m.Donations)
            .AsQueryable();

        return ApplySorting(query, queryDto.SortOrder);
    }

    private static IQueryable<Member> ApplySorting(
        IQueryable<Member> query,
        SortOrder sortOrder)
    {
        return sortOrder == SortOrder.Asc
            ? query.OrderBy(m => m.Name).ThenBy(m => m.LastName)
            : query.OrderByDescending(m => m.Name).ThenByDescending(m => m.LastName);
    }

    private static IQueryable<Member> ApplyPaging(
        IQueryable<Member> query,
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

    private static PagedResponse<MemberResponseDto> BuildPagedResponse(
        List<MemberResponseDto> items,
        int page,
        int pageSize,
        int totalItems)
    {
        return new PagedResponse<MemberResponseDto>
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

    private async Task<Member> GetMemberOrThrow(int id)
    {
        var member = await _context.Members.FirstOrDefaultAsync(m => m.Id == id);
        if (member is null)
        {
            throw new NotFoundException($"Member with id {id} was not found.");
        }

        return member;
    }

    private async Task<Member> GetMemberWithDonationsOrThrow(int id)
    {
        var member = await _context.Members
            .Include(m => m.Donations)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (member is null)
        {
            throw new NotFoundException($"Member with id {id} was not found.");
        }

        return member;
    }
}
