namespace Chronovault.Core.Options;

/// <summary>
/// Source directory location settings.
/// </summary>
public sealed class VaultOptions
{
    /// <summary>
    /// Full path to the source directory to backup.
    /// </summary>
    public string Path { get; set; } = string.Empty;
}

