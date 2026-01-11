using Chronovault.Core.Models;

namespace Chronovault.Core.Interfaces;

/// <summary>
/// Repository for tracking backup history (hash-based deduplication).
/// </summary>
public interface IBackupHistoryRepository
{
    /// <summary>
    /// Checks if a backup with this hash already exists.
    /// </summary>
    Task<bool> ExistsAsync(string hash, CancellationToken ct);

    /// <summary>
    /// Records a completed backup.
    /// </summary>
    Task AddAsync(BackupRecord record, CancellationToken ct);

    /// <summary>
    /// Gets all backup records (for diagnostics).
    /// </summary>
    Task<IReadOnlyList<BackupRecord>> GetAllAsync(CancellationToken ct);

    /// <summary>
    /// Gets the most recent N backup records (newest first).
    /// </summary>
    Task<IReadOnlyList<BackupRecord>> GetRecentAsync(int count, CancellationToken ct);
}

