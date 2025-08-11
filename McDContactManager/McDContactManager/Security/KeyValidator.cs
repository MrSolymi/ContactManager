using System.Security.Cryptography;
using System.Text;

namespace McDContactManager.Security;

public static class KeyValidator
{
    private const string ExpectedHash = "AFB7A055BDF166C65331A92E869A9CF502AB1455B22DA7CCF995AC6588FDA766";
    
    public static bool IsValid(string? key)
    {
        if (string.IsNullOrWhiteSpace(key)) return false;

        var input = key.Trim();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = Convert.ToHexString(SHA256.HashData(bytes));
        return string.Equals(hash, ExpectedHash, StringComparison.OrdinalIgnoreCase);
    }
}