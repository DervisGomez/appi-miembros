using ChurchApi.Data;
using ChurchApi.Dtos;
using ChurchApi.Enums;
using ChurchApi.Exceptions;
using ChurchApi.Helpers;
using ChurchApi.Mappers;
using ChurchApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ChurchApi.Services;

public class MemberService : IMemberService
{
    private readonly AppDbContext _context;
    private readonly ILogger<MemberService> _logger;

    public MemberService(
        AppDbContext context,
        ILogger<MemberService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResponse<MemberResponseDto>> GetMembers(MemberQueryDto queryDto)
    {
        var query = BuildMemberQuery(queryDto);

        var totalItems = await query.CountAsync();
        var (page, pageSize) = PaginationHelper.NormalizePaging(queryDto.Page, queryDto.PageSize);

        var members = await PaginationHelper.ApplyPaging(query, page, pageSize)
            .ToListAsync();

        var items = members.Select(MemberMapper.ToDto).ToList();

        return PaginationHelper.BuildPagedResponse(items, page, pageSize, totalItems);
    }

    public async Task AddMember(Member member)
    {
        try
        {
            await _context.Members.AddAsync(member);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (PersistenceExceptionTranslator.IsUniqueConstraintViolation(exception))
        {
            _logger.LogWarning(
                exception,
                "Unique constraint violation while creating a member.");

            throw new ConflictException("Email already exists.");
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(
                exception,
                "Unexpected persistence error while creating a member.");

            throw;
        }
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

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (PersistenceExceptionTranslator.IsUniqueConstraintViolation(exception))
        {
            _logger.LogWarning(
                exception,
                "Unique constraint violation while updating member {MemberId}.",
                member.Id);

            throw new ConflictException("Email already exists.");
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(
                exception,
                "Unexpected persistence error while updating member {MemberId}.",
                member.Id);

            throw;
        }

        return existingMember;
    }

    public async Task<Member> DeleteMember(int id)
    {
        var member = await GetMemberOrThrow(id);

        _context.Members.Remove(member);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (PersistenceExceptionTranslator.IsForeignKeyConstraintViolation(exception))
        {
            _logger.LogWarning(
                exception,
                "Foreign key constraint violation while deleting member {MemberId}.",
                id);

            throw new ConflictException("Cannot delete member because it has associated donations.");
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(
                exception,
                "Unexpected persistence error while deleting member {MemberId}.",
                id);

            throw;
        }

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
