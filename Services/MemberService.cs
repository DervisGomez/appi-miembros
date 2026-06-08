using ChurchApi.Data;
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

    public async Task<List<Member>> GetMembers()
    {
        return await _context.Members.ToListAsync();
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
            return null;
        }

        return await _context.Members.Include(m => m.Donations).FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Member?> UpdateMember(Member member)
    {
        var existingMember = await _context.Members
        .FirstOrDefaultAsync(m => m.Id == member.Id);

        if (existingMember == null)
        {
            return null;
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

        if (member == null)
        {
            return null;
        }

        _context.Members.Remove(member);

        await _context.SaveChangesAsync();

        return member;
    }
}