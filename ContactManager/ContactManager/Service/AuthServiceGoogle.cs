using ContactManager.Common;
using ContactManager.data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;

namespace ContactManager.Service;

public static class AuthServiceGoogle
{
    private static UserCredential? _credential;
    public static GmailService? Gmail { get; private set; }

    public static async Task<bool> EnsureSignedInAsync()
    {
        if (Gmail != null) return true;

        var config = ConfigManager.Load();
        
        var scopes = new[] { GmailService.Scope.GmailReadonly }; // csak olvasás
        var clientSecrets = new ClientSecrets
        {
            ClientId = config.ClientId,
            ClientSecret = config.ClientSecret
        };
        
        // futásidős “memóriatároló”, semmi fájl
        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = clientSecrets,
            Scopes = scopes,
            DataStore = new InMemoryDataStore() // lásd lejjebb
        });

        // Felhasználói név lehet bármi lokális azonosító (pl. "user")
        //var codeReceiver = new LocalServerCodeReceiver(); // felugró böngészős login
        
        var dataStore = new InMemoryDataStore();
        var receiver = new LocalServerCodeReceiver();
        
        _credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            clientSecrets, scopes, "user", CancellationToken.None, dataStore, receiver);

        if (_credential == null) return false;

        Gmail = new GmailService(new BaseClientService.Initializer
        {
            HttpClientInitializer = _credential,
            ApplicationName = "ContactManager"
        });

        return true;
    }

    public static void SignOut()
    {
        Gmail = null;
        _credential = null;
    }
}