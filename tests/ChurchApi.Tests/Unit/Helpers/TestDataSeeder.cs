using ChurchApi.Data;
using ChurchApi.Models;

namespace ChurchApi.Tests.Unit.Helpers;

public static class TestDataSeeder
{
    public static async Task<Member> CreateMemberAsync(
        AppDbContext context,
        string name = "Dervis",
        string lastName = "Gomez",
        string? email = null,
        string phone = "123456789",
        int age = 30)
    {
        var member = new Member
        {
            Name = name,
            LastName = lastName,
            Email = email ?? $"{name.ToLower()}@test.com",
            Phone = phone,
            Age = age
        };

        context.Members.Add(member);
        await context.SaveChangesAsync();

        return member;
    }

    public static async Task<Donation> CreateDonationAsync(
        AppDbContext context,
        Member member,
        decimal amount,
        DateTime? date = null,
        string description = "Test donation")
    {
        var donation = new Donation
        {
            Amount = amount,
            Description = description,
            MemberId = member.Id,
            Date = date ?? DateTime.UtcNow,
            Member = member
        };

        context.Donations.Add(donation);
        await context.SaveChangesAsync();

        return donation;
    }
}
