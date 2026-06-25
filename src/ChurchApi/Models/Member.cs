namespace ChurchApi.Models;

public class Member
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public int Age { get; set; }
    public List<Donation> Donations { get; set; } = [];
}

