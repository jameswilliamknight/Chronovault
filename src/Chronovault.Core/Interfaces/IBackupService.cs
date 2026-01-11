namespace Chronovault.Core.Interfaces;

/// <summary>
/// Abstraction for backup destinations (local file system, S3, etc.).
/// </summary>
public interface IBackupService
{
    /// <summary>
    /// Uploads/copies the archive to the destination.
    /// </summary>
    /// <param name="archivePath">Full path to the archive file.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if upload succeeded, false otherwise.</returns>
    Task<bool> TransferAsync(string archivePath, CancellationToken ct);
}


