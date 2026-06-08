using ChurchApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ChurchApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Member> Members { get; set; }
    public DbSet<Donation> Donations { get; set; }
}