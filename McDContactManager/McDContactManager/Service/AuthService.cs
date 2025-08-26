using Azure.Core;
using Azure.Identity;
using McDContactManager.Common;

namespace McDContactManager.Service;

public static class AuthService
{
    public static InteractiveBrowserCredential? Credential { get; private set; }

    private static readonly string[] GraphScopes =
    [
        "https://graph.microsoft.com/User.Read",
        "https://graph.microsoft.com/Mail.Read",
        "offline_access",
        "openid",
        "profile"
    ];
    
    private static AuthenticationRecord? _record;
    
    public static void Initialize()
    {
        var config = ConfigManager.Load();
        if (config == null || string.IsNullOrEmpty(config.ClientId)) {
            Console.WriteLine("Config file is missing or ClientId is empty.");
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
            AuthenticationRecord = _record
        });
    }

    public static async Task<bool> EnsureSignedInAsync()
    {
        if (Credential == null)
            Initialize();
        
        if (Credential == null)
            return false;
        
        try
        {
            // Ha még nincs account “rögzítve”, autentikáljunk interaktívan a végleges scope-okra
            if (_record == null)
            {
                var ibc = Credential;
                _record = await ibc.AuthenticateAsync(new TokenRequestContext(GraphScopes));
            }
            else
            {
                // már van record: kérünk silent tokent ugyanarra a scope-csomagra
                var _ = await Credential.GetTokenAsync(new TokenRequestContext(GraphScopes), default);
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Sign-in failed: " + ex.Message);
            return false;
        }
    }
    public static string[] Scopes => GraphScopes;
}