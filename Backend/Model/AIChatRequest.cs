using System.Text.Json.Serialization;

namespace Backend.Model;

public record AIChatRequest([property: JsonPropertyName("messages")] IList<AIChatMessage> Messages)
{
    [JsonPropertyName("session_state")]
    public Guid? SessionState;

    [JsonPropertyName("context")]
    public BinaryData? Context;
}
