using System.Security.Cryptography;
using System.Text;

namespace Application.JWT.Helper;

public static class PasswordHelper
{
    public static string GetHashedPassword(string? password)
    {
        if (string.IsNullOrEmpty(password))
            return string.Empty;

        using SHA256 sha = SHA256.Create();
        var textDataStream = Encoding.UTF8.GetBytes(password);
        var hash = sha.ComputeHash(textDataStream);
        return BitConverter.ToString(hash).Replace("-", "");
    }
    
    public static bool ComparePasswords(string password, string? hashedPassword)
    {
        var hashedPasswordToCompare = GetHashedPassword(password);
        return hashedPasswordToCompare == hashedPassword;
    }
}