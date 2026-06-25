namespace ChurchApi.Dtos;

using ChurchApi.Enums;

public class MemberQueryDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public SortOrder SortOrder { get; set; } = SortOrder.Asc;
}
