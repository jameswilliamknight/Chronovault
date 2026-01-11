using Microsoft.Extensions.Options;
using Chronovault.Core.Interfaces;
using Chronovault.Core.Options;

namespace Chronovault.Infrastructure.Destinations;

/// <summary>
/// Stores backup archives on the local file system.
/// Implements IBackupDestination for the destination abstraction pattern.
/// </summary>
public sealed class LocalFileBackupService(
    IOptions<BackupOptions> options) : IBackupService
{
    private readonly BackupOptions _options = options.Value;

    /// <summary>
    /// Copies the archive to the configured output directory.
    /// </summary>
    public async Task<bool> TransferAsync(string archivePath, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.Output.Path))
        {
            Log.Error("LocalFileDestinationTransferAsync Output path not configured");
            return false;
        }

        try
        {
            Log.Debug("LocalFileDestinationTransferAsync Output dir: {OutputDir}", _options.Output.Path);
            Directory.CreateDirectory(_options.Output.Path);

            var fileName = Path.GetFileName(archivePath);
            var destinationPath = Path.Combine(_options.Output.Path, fileName);

            Log.Debug("LocalFileDestinationTransferAsync Filename: {FileName}", fileName);
            Log.Debug("LocalFileDestinationTransferAsync Destination: {DestinationPath}", destinationPath);

            // Use async file copy
            await using var sourceStream = new FileStream(
                archivePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 81920,
                useAsync: true);

            await using var destinationStream = new FileStream(
                destinationPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);

            await sourceStream.CopyToAsync(destinationStream, ct);

            Log.Information("LocalFileDestinationTransferAsync Backup now located at {Path}", destinationPath);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "LocalFileDestinationTransferAsync Failed to copy archive");
            return false;
        }
    }
}

