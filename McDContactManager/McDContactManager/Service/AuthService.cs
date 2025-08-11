using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using Azure.Core;
using Azure.Identity;
using McDContactManager.Common;
using McDContactManager.data;
using McDContactManager.Model;
using Microsoft.Identity.Client;

namespace McDContactManager.Service;

public static class AuthService
{
    private static readonly string[] Scopes = ["User.Read", "Mail.Read"];

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