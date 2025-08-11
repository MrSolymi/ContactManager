using Azure.Identity;
using McDContactManager.Common;

namespace McDContactManager.Service;

public static class AuthService
{
    public static InteractiveBrowserCredential? Credential { get; private set; }

    public static void Initialize()
    {
        var config = ConfigManager.Load();
        if (config == null) {
            Console.WriteLine("Config file is missing.");
            return;
        }
        
        Credential = new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions
        {
            ClientId = config.ClientId,
            RedirectUri = new Uri("http://localhost"),
            TenantId = "common",
            TokenCachePersistenceOptions = new TokenCachePersistenceOptions
            {
                Name = "TemporaryLoginCache",
                UnsafeAllowUnencryptedStorage = true
            },
            AuthenticationRecord = null
        });
    }
}