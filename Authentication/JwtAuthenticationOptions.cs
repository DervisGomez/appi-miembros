using Microsoft.AspNetCore.Authentication;

namespace ChurchApi.Authentication;

public class JwtAuthenticationOptions : AuthenticationSchemeOptions
{
    public string JwtSecret { get; set; } = string.Empty;
}
