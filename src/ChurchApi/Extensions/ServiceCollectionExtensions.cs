using ChurchApi.Interfaces;
using ChurchApi.Services;

namespace ChurchApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IMemberService, MemberService>();
        services.AddScoped<IDonationService, DonationService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddSingleton(TimeProvider.System);

        services.AddControllers();

        return services;
    }
}
