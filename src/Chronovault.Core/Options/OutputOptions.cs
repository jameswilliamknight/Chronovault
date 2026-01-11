namespace Chronovault.Core.Options;

/// <summary>
/// Output/destination settings.
/// </summary>
public sealed class OutputOptions
{
    /// <summary>
    /// Directory where backup archives are stored.
    /// </summary>
    public string Path { get; set; } = string.Empty;
}

