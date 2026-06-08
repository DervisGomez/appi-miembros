using ChurchApi.Models;

namespace ChurchApi.Services;

public interface IMemberService
{
    Task<List<Member>> GetMembers();

    Task AddMember(Member member);

    Task<Member?> GetMember(int id);

    Task<Member?> UpdateMember(Member member);

    Task<Member?> DeleteMember(int id);
}