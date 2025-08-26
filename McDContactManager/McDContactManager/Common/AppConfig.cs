using System.Text.Json.Serialization;

namespace McDContactManager.Common;

public class AppConfig
{
    [JsonPropertyName("clientId")]
    public string? ClientId { get; set; }
    
    [JsonPropertyName("lastUsedSenderAddress")]
    public string? LastUsedSenderAddress { get; set; }
}