using ChurchApi.Data;
using ChurchApi.Enums;
using ChurchApi.Helpers;
using ChurchApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ChurchApi.Tests.Integration.Helpers;

public static class IntegrationTestDataSeeder
{
    public const string AdminUsername = "admin";
    public const string AdminEmail = "admin@test.com";
    public const string AdminPassword = "Admin1234!";

    public static async Task SeedAdminUserAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync(u => u.Username == AdminUsername))
        {
            return;
        }

        context.Users.Add(new User
        {
            Username = AdminUsername,
            Email = AdminEmail,
            PasswordHash = AuthPasswordHasher.Hash(AdminPassword),
            Role = UserRole.Admin
        });

        await context.SaveChangesAsync();
    }
}
