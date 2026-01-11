namespace Chronovault.Core.Options;

/// <summary>
/// Configuration options for the backup service.
/// Loaded from ~/.env.chronovault and appsettings.json via IOptions&lt;BackupOptions&gt;.
/// </summary>
public sealed class BackupOptions
{
    /// <summary>
    /// Configuration section name in appsettings/environment.
    /// </summary>
    public const string SectionName = "Chronovault";

    /// <summary>
    /// Archive-related settings.
    /// </summary>
    public ArchiveOptions Archive { get; set; } = new();

    /// <summary>
    /// Vault-related settings.
    /// </summary>
    public VaultOptions Vault { get; set; } = new();

    /// <summary>
    /// Output-related settings.
    /// </summary>
    public OutputOptions Output { get; set; } = new();

    /// <summary>
    /// Backup interval in minutes. Default is 60 (hourly).
    /// </summary>
    public int IntervalMinutes { get; set; } = 60;
}
