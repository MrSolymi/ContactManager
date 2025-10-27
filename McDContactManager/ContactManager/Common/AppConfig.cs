using System.Text.Json.Serialization;

namespace ContactManager.Common;

public class AppConfig
{
    [JsonPropertyName("clientId")]
    public string? ClientId { get; set; }
    
    [JsonPropertyName("clientSecret")]
    public string? ClientSecret { get; set; }
    
    [JsonPropertyName("lastUsedSenderAddress")]
    public string? LastUsedSenderAddress { get; set; }
}