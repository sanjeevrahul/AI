namespace FoundryAgent.Models;

public sealed class ModelConfiguration
{
    public required ModelProvider Provider { get; init; }

    public required string Model { get; init; }

    public string? Endpoint { get; init; }

    public string? ApiKey { get; init; }
}