using System.IO;
using System.Text.Json;
using System.Windows;
using Azure.Core;
using Azure.Identity;
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
        Credential = new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions
        {
            ClientId = EnvLoader.Get("CLIENT_ID"),
            RedirectUri = new Uri("http://localhost"),
            TenantId = "common"
        });
    }
}