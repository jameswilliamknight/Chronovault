namespace Chronovault.Core.Models;

/// <summary>
/// Result of scanning the source directory.
/// </summary>
public sealed record VaultScanResult
{
    /// <summary>
    /// Combined SHA256 hash of all file contents and paths.
    /// </summary>
    public required string Hash { get; init; }

    /// <summary>
    /// Number of files scanned.
    /// </summary>
    public required int FileCount { get; init; }

    /// <summary>
    /// Total size in bytes of all files.
    /// </summary>
    public required long TotalBytes { get; init; }

    /// <summary>
    /// Path to the vault that was scanned.
    /// </summary>
    public required string VaultPath { get; init; }

    /// <summary>
    /// When the scan was performed.
    /// </summary>
    public required DateTimeOffset ScannedAt { get; init; }
}

