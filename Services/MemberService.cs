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

    public List<Member> GetMembers()
    {
        return _context.Members.ToList();
    }

    public void AddMember(Member member)
    {
        _context.Members.Add(member);
        _context.SaveChanges();
    }

    public Member? GetMember(int id)
    {
        if (id <= 0)
        {
            return null;
        }

        return _context.Members.FirstOrDefault(m => m.Id == id);
    }

    public Member? UpdateMember(Member member)
    {
        var existingMember = _context.Members
        .FirstOrDefault(m => m.Id == member.Id);

        if (existingMember == null)
        {
            return null;
        }

        existingMember.Name = member.Name;
        existingMember.LastName = member.LastName;
        existingMember.Email = member.Email;
        existingMember.Phone = member.Phone;
        existingMember.Age = member.Age;
        _context.SaveChanges();
        return existingMember;
    }

    public Member? DeleteMember(int id)
    {
        var member = _context.Members
        .FirstOrDefault(m => m.Id == id);

        if (member == null)
        {
            return null;
        }

        _context.Members.Remove(member);

        _context.SaveChanges();

        return member;
    }
}