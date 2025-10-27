using System.Security.Cryptography;
using System.Text;

namespace ContactManager.Security;

public static class KeyValidator
{
    private const string ExpectedIdHash = "23034F18434F07F088BE8C984DF285B2F7CC5A2891A3A2A77AB71C5550406011";
    private const string ExpectedSecretHash = "7B9EC41788AE3579E989B59EE8AA86A4C0070DC0E604256BA7D9D82F7001BE81";
    
    public static bool IsValid(string? id, string? secret)
    {
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrEmpty(secret)) return false;

        var inputId = id.Trim();
        var bytesId = Encoding.UTF8.GetBytes(inputId);
        var hashId = Convert.ToHexString(SHA256.HashData(bytesId));
        var isIdOk = string.Equals(hashId, ExpectedIdHash, StringComparison.OrdinalIgnoreCase);
        
        var inputSecret = secret.Trim();
        var bytesSecret = Encoding.UTF8.GetBytes(inputSecret);
        var hashSecret = Convert.ToHexString(SHA256.HashData(bytesSecret));
        var isSecretOk = string.Equals(hashSecret, ExpectedSecretHash, StringComparison.OrdinalIgnoreCase);
        
        return isIdOk && isSecretOk;
    }
}