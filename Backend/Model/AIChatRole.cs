namespace Backend.Model;

public struct AIChatRole
{
    private readonly string _value { get; }

    public AIChatRole(string value)
    {
        _value = value;
    }

    public static AIChatRole System { get; } = new AIChatRole("system");
    public static AIChatRole Assistant { get; } = new AIChatRole("assistant");
    public static AIChatRole User { get; } = new AIChatRole("user");

    public static implicit operator AIChatRole(string value) => new AIChatRole(value);

    public override string ToString() => _value;
}
