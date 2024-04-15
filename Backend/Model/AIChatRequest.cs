// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Backend.Model;

public record AIChatRequest([property: JsonPropertyName("messages")] IList<AIChatMessage> Messages)
{
    [JsonPropertyName("sessionState")]
    public Guid? SessionState;

    [JsonPropertyName("context")]
    public BinaryData? Context;
}
