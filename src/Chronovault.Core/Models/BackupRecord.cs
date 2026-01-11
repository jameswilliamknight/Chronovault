namespace Chronovault.Core.Models;

/// <summary>
/// Record of a completed backup stored in SQLite.
/// </summary>
public sealed record BackupRecord
{
    /// <summary>
    /// Combined SHA256 hash (primary key).
    /// </summary>
    public required string Hash { get; init; }

    /// <summary>
    /// When the backup was created (UTC).
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Path to the vault that was backed up.
    /// </summary>
    public required string VaultPath { get; init; }

    /// <summary>
    /// Number of files in the backup.
    /// </summary>
    public required int FileCount { get; init; }

    /// <summary>
    /// Total size in bytes.
    /// </summary>
    public required long TotalBytes { get; init; }

    /// <summary>
    /// Path to the archive file (if stored locally).
    /// </summary>
    public string? ArchivePath { get; init; }

    /// <summary>
    /// Just the filename portion of the archive (for display/logging).
    /// </summary>
    public string? ArchiveFileName { get; init; }
}

