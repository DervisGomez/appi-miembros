using ChurchApi.Data;
using ChurchApi.Services;
using ChurchApi.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

namespace ChurchApi.Tests.Fixtures;

public sealed class DonationServiceFixture : IDisposable
{
    private static readonly DateTimeOffset FixedUtcNow = new(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);

    public AppDbContext Context { get; }
    public FakeTimeProvider TimeProvider { get; }
    public DonationService Service { get; }

    public DonationServiceFixture()
    {
        Context = TestDbContextFactory.Create();
        TimeProvider = new FakeTimeProvider(FixedUtcNow);
        Service = new DonationService(
            Context,
            TimeProvider,
            NullLogger<DonationService>.Instance);
    }

    public void Dispose() => Context.Dispose();
}
