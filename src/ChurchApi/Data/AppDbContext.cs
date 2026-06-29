using ChurchApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ChurchApi.Data;

public class AppDbContext : DbContext
{
    private const int UserUsernameMaxLength = 100;
    private const int EmailMaxLength = 255;
    private const int PasswordHashMaxLength = 500;
    private const int MemberNameMaxLength = 100;
    private const int MemberPhoneMaxLength = 50;
    private const int DonationDescriptionMaxLength = 255;

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Member> Members { get; set; }
    public DbSet<Donation> Donations { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUsers(modelBuilder);
        ConfigureMembers(modelBuilder);
        ConfigureDonations(modelBuilder);
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(user => user.Id);

            entity.Property(user => user.Username)
                .IsRequired()
                .HasMaxLength(UserUsernameMaxLength);

            entity.Property(user => user.Email)
                .IsRequired()
                .HasMaxLength(EmailMaxLength);

            entity.Property(user => user.PasswordHash)
                .IsRequired()
                .HasMaxLength(PasswordHashMaxLength);

            entity.Property(user => user.Role)
                .IsRequired();

            entity.HasIndex(user => user.Username)
                .IsUnique();

            entity.HasIndex(user => user.Email)
                .IsUnique();
        });
    }

    private static void ConfigureMembers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(member => member.Id);

            entity.Property(member => member.Name)
                .IsRequired()
                .HasMaxLength(MemberNameMaxLength);

            entity.Property(member => member.LastName)
                .IsRequired()
                .HasMaxLength(MemberNameMaxLength);

            entity.Property(member => member.Email)
                .IsRequired()
                .HasMaxLength(EmailMaxLength);

            entity.Property(member => member.Phone)
                .IsRequired()
                .HasMaxLength(MemberPhoneMaxLength);

            entity.HasIndex(member => member.Email)
                .IsUnique();

            entity.HasIndex(member => new { member.Name, member.LastName });
        });
    }

    private static void ConfigureDonations(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Donation>(entity =>
        {
            entity.HasKey(donation => donation.Id);

            entity.Property(donation => donation.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            entity.Property(donation => donation.Date)
                .IsRequired();

            entity.Property(donation => donation.Description)
                .IsRequired()
                .HasMaxLength(DonationDescriptionMaxLength);

            entity.HasOne(donation => donation.Member)
                .WithMany(member => member.Donations)
                .HasForeignKey(donation => donation.MemberId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            entity.HasIndex(donation => donation.MemberId);
            entity.HasIndex(donation => donation.Date);
            entity.HasIndex(donation => donation.Amount);
        });
    }
}
