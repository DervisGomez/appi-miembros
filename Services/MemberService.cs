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
    private readonly AppDbContext _context;

    public MemberService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResponse<MemberResponseDto>> GetMembers(MemberQueryDto queryDto)
    {
        var query = _context.Members.AsQueryable();

        if (queryDto.SortOrder == SortOrder.Asc)
        {
            query = query.OrderBy(m => m.Name).ThenBy(m => m.LastName);
        }
        else
        {
            query = query.OrderByDescending(m => m.Name).ThenByDescending(m => m.LastName);
        }

        var totalItems = await query.CountAsync();

        var page = queryDto.Page < 1 ? 1 : queryDto.Page;
        var pageSize = queryDto.PageSize < 1 ? 10 : queryDto.PageSize;
        pageSize = Math.Min(pageSize, 100);

        var totalPages = totalItems == 0
            ? 0
            : (int)Math.Ceiling((double)totalItems / pageSize);

        var members = await query
            .Include(m => m.Donations)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = members.Select(MemberMapper.ToDto).ToList();

        return new PagedResponse<MemberResponseDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };
    }

    public async Task AddMember(Member member)
    {
        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();
    }

    public async Task<Member?> GetMember(int id)
    {
        if (id <= 0)
        {
            throw new NotFoundException($"Member with id {id} was not found.");
        }

        var member = await _context.Members
            .Include(m => m.Donations)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (member is null)
        {
            throw new NotFoundException($"Member with id {id} was not found.");
        }

        return member;
    }

    public async Task<Member?> UpdateMember(Member member)
    {
        var existingMember = await _context.Members
        .FirstOrDefaultAsync(m => m.Id == member.Id);

        if (existingMember is null)
        {
            throw new NotFoundException($"Member with id {member.Id} was not found.");
        }

        existingMember.Name = member.Name;
        existingMember.LastName = member.LastName;
        existingMember.Email = member.Email;
        existingMember.Phone = member.Phone;
        existingMember.Age = member.Age;
        await _context.SaveChangesAsync();
        return existingMember;
    }

    public async Task<Member?> DeleteMember(int id)
    {
        var member = await _context.Members
        .FirstOrDefaultAsync(m => m.Id == id);

        if (member is null)
        {
            throw new NotFoundException($"Member with id {id} was not found.");
        }

        _context.Members.Remove(member);

        await _context.SaveChangesAsync();

        return member;
    }
}