using System.Text.Json.Serialization;

namespace Backend.Model;
public struct AIChatMessage
{
    [JsonPropertyName("content")]
    public string Content { get; set; }

    [JsonPropertyName("role")]
    public AIChatRole Role { get; set; }
}

