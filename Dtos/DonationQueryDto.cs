namespace ChurchApi.Dtos;

public class DonationQueryDto
{
    // public DateTime? StartDate { get; set; }
    // public DateTime? EndDate { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public int? MemberId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}