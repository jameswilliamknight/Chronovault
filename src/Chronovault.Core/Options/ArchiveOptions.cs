namespace Chronovault.Core.Options;

/// <summary>
/// Archive encryption settings.
/// </summary>
public sealed class ArchiveOptions
{
    /// <summary>
    /// Password for AES-256 encryption of the ZIP archive.
    /// Should be stored in ~/.env.chronovault, not appsettings.json.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}

