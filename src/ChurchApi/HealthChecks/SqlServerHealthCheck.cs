using ChurchApi.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ChurchApi.HealthChecks;

public sealed class SqlServerHealthCheck : IHealthCheck
{
    private readonly AppDbContext _context;

    public SqlServerHealthCheck(AppDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var canConnect = await _context.Database.CanConnectAsync(cancellationToken);

        return canConnect
            ? HealthCheckResult.Healthy("Database connection is available.")
            : HealthCheckResult.Unhealthy("Database connection is not available.");
    }
}
