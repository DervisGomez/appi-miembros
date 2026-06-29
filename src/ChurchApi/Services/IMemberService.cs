using ChurchApi.Dtos;
using ChurchApi.Models;

namespace ChurchApi.Services;

public interface IMemberService
{
    Task<PagedResponse<MemberResponseDto>> GetMembers(MemberQueryDto queryDto);

    Task AddMember(Member member);

    Task<Member> GetMember(int id);

    Task<Member> UpdateMember(Member member);

    Task<Member> DeleteMember(int id);
}
