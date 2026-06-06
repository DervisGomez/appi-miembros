using ChurchApi.Models;

namespace ChurchApi.Services;

public interface IMemberService
{
    List<Member> GetMembers();

    void AddMember(Member member);

    Member? GetMember(int id);

    Member? UpdateMember(Member member);

    Member? DeleteMember(int id);
}