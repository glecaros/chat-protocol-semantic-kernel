namespace Backend.Model;

public record AIChatCompletion(AIChatMessage Message, Guid SessionState)
{
    public BinaryData? Context;
}
