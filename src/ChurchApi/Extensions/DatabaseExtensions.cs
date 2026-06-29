using ChurchApi.Data;
using Microsoft.EntityFrameworkCore;

namespace ChurchApi.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddApplicationDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var sqlServerConnectionString = configuration.GetConnectionString("SqlServer");
        if (string.IsNullOrWhiteSpace(sqlServerConnectionString))
        {
            throw new InvalidOperationException(
                "SQL Server connection string is not configured. Set ConnectionStrings:SqlServer with user-secrets or an environment variable.");
        }

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(sqlServerConnectionString);
        });

        return services;
    }

    public static WebApplication ApplyDatabaseMigrations(this WebApplication app)
    {
        if (!app.Configuration.GetValue<bool>("Database:ApplyMigrations"))
        {
            return app;
        }

        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();

        return app;
    }
}
