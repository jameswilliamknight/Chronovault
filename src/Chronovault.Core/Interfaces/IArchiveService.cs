namespace Chronovault.Core.Interfaces;

/// <summary>
/// Service for creating encrypted archives from a directory.
/// </summary>
public interface IArchiveService
{
    /// <summary>
    /// Creates an AES-256 encrypted ZIP archive from the source directory.
    /// </summary>
    /// <param name="sourceDirectory">Directory to archive.</param>
    /// <param name="outputPath">Full path for the output archive file.</param>
    /// <param name="password">Password for AES-256 encryption.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Full path to the created archive.</returns>
    Task<string> CreateArchiveAsync(
        string sourceDirectory,
        string outputPath,
        string password,
        CancellationToken ct);
}

