using ChurchApi.Authentication;
using Microsoft.AspNetCore.Authentication;

namespace ChurchApi.Extensions;

public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        string jwtSecret)
    {
        services
            .AddAuthentication("Bearer")
            .AddScheme<JwtAuthenticationOptions, JwtAuthenticationHandler>(
                "Bearer",
                options => options.JwtSecret = jwtSecret);

        return services;
    }
}
