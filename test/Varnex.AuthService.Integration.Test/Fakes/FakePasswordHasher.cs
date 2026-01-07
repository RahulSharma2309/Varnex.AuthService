using Ep.Platform.Security;

namespace Varnex.AuthService.Integration.Test.Fakes;

/// <summary>
/// Simple password hasher for tests that keeps the password in plain text.
/// </summary>
public sealed class FakePasswordHasher : IPasswordHasher
{
    public static string HashPasswordStatic(string password) => password;

    public string HashPassword(string password) => HashPasswordStatic(password);

    public bool VerifyPassword(string password, string passwordHash) => password == passwordHash;
}

