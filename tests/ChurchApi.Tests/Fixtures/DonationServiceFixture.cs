using ChurchApi.Data;
using ChurchApi.Services;
using ChurchApi.Tests.Helpers;

namespace ChurchApi.Tests.Fixtures;

public sealed class DonationServiceFixture : IDisposable
{
    public AppDbContext Context { get; }
    public DonationService Service { get; }

    public DonationServiceFixture()
    {
        Context = TestDbContextFactory.Create();
        Service = new DonationService(Context);
    }

    public void Dispose() => Context.Dispose();
}
