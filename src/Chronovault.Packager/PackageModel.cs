namespace Chronovault.Packager;

/// <summary>
/// Model for template rendering containing package configuration.
/// Properties use snake_case for Scriban compatibility.
/// </summary>
public sealed class PackageModel
{
    public required string Version { get; init; }
    public required string Environment { get; init; }
    public required string ServiceName { get; init; }
    public required string ServiceDisplayName { get; init; }

    // Snake_case aliases for Scriban templates
    public string version => Version;
    public string environment => Environment;
    public string service_name => ServiceName;
    public string service_display_name => ServiceDisplayName;
}
