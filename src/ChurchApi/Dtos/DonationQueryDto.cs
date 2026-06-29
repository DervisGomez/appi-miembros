using ChurchApi.Enums;

namespace ChurchApi.Dtos;

public class DonationQueryDto
{
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public int? MemberId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public SortOrder SortOrder { get; set; } = SortOrder.Desc;
}
