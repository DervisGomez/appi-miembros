using Microsoft.AspNetCore.Identity;

namespace ChurchApi.Helpers;

public static class AuthPasswordHasher
{
    private static readonly PasswordHasher<object> Hasher = new();

    public static string Hash(string password) =>
        Hasher.HashPassword(null!, password);

    public static bool Verify(string password, string hash) =>
        Hasher.VerifyHashedPassword(null!, hash, password) != PasswordVerificationResult.Failed;
}
