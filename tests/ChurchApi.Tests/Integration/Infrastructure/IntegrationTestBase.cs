using System.Net.Http;

namespace ChurchApi.Tests.Integration.Infrastructure;

public abstract class IntegrationTestBase
{
    protected readonly HttpClient Client;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Client = factory.CreateClient();
    }
}