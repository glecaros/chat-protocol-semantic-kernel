using System.Text.Json;
using System.Text.Json.Serialization;
using Backend.Model;

namespace Backend.Converters;

public class AIChatRoleConverter : JsonConverter<AIChatRole>
{
    public override AIChatRole Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException();
        }

        var role = reader.GetString()!;
        return new AIChatRole(role);
    }

    public override void Write(Utf8JsonWriter writer, AIChatRole value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
