namespace ChurchApi.Dtos;

public class DonationMemberResponseDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public MemberDonationResponseDto Member { get; set; } = null!;
}
