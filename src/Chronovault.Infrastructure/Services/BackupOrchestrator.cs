using Microsoft.Extensions.Options;
using Chronovault.Core.Interfaces;
using Chronovault.Core.Models;
using Chronovault.Core.Options;

namespace Chronovault.Infrastructure.Services;

/// <summary>
///     Orchestrates the backup process: scan → check history → archive → upload.
/// </summary>
public sealed class BackupOrchestrator(
    IVaultScanner vaultScanner,
    IArchiveService archiveService,
    IBackupService destination,
    IBackupHistoryRepository historyRepository,
    IOptions<BackupOptions> options)
{
    private readonly BackupOptions _options = options.Value;

    /// <summary>
    /// Executes a backup cycle. Returns true if backup was performed, false if skipped.
    /// </summary>
    public async Task<bool> ExecuteBackupAsync(CancellationToken ct)
    {
        // Assumes the source is a directory on the local file system.
        if (string.IsNullOrWhiteSpace(_options.Vault.Path))
        {
            Log.Error("BackupOrchestratorExecuteBackupAsync Vault path not configured");
            return false;
        }

        // Assumes we're making a password-protected archive / zip file.
        if (string.IsNullOrWhiteSpace(_options.Archive.Password))
        {
            Log.Error("BackupOrchestratorExecuteBackupAsync Archive password not configured");
            return false;
        }
        
        Log.Information("BackupOrchestratorExecuteBackupAsync Starting backup scan for {VaultPath}",
            _options.Vault.Path);

        // Step 1: Scan vault and compute hash
        var scanResult = await vaultScanner.ScanAsync(_options.Vault.Path, ct);
        Log.Information(
            "BackupOrchestratorExecuteBackupAsync Scanned {FileCount} files ({TotalMB:F2} MB), hash: {Hash}",
            scanResult.FileCount,
            scanResult.TotalBytes / (1024.0 * 1024.0),
            scanResult.Hash[..12]);

        // Step 2: Check if this hash already exists (skip if no changes)
        if (await historyRepository.ExistsAsync(scanResult.Hash, ct))
        {
            Log.Information("BackupOrchestratorExecuteBackupAsync No changes detected, skipping backup");
            return false;
        }

        // Step 3: Create encrypted archive
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd_HHmmss");
        var archiveName = $"chronovault-backup-{timestamp}";
        var tempDir = Path.GetTempPath();
        var tempPath = Path.Combine(tempDir, archiveName);

        Log.Debug("BackupOrchestratorExecuteBackupAsync Temp dir: {TempDir}", tempDir);
        Log.Debug("BackupOrchestratorExecuteBackupAsync Archive name: {ArchiveName}", archiveName);
        Log.Information("BackupOrchestratorExecuteBackupAsync Creating archive at {TempPath}", tempPath);

        var archiveTempPath = await archiveService.CreateArchiveAsync(
            _options.Vault.Path,
            tempPath,
            _options.Archive.Password,
            ct);

        try
        {
            // Step 4: Transfer archive to destination
            Log.Information("BackupOrchestratorExecuteBackupAsync Transferring to {Destination}",
                _options.Output.Path);

            var uploaded = await destination.TransferAsync(archiveTempPath, ct);
            if (!uploaded)
            {
                Log.Error("BackupOrchestratorExecuteBackupAsync Upload failed");
                return false;
            }

            // Step 5: Record in history - store final destination path, not temp path
            var archiveTempFileName = Path.GetFileName(archiveTempPath);
            var finalPath = Path.Combine(_options.Output.Path, archiveTempFileName);
            Log.Debug("BackupOrchestratorExecuteBackupAsync Final path: {FinalPath}", finalPath);
            var record = new BackupRecord
            {
                Hash = scanResult.Hash,
                CreatedAt = DateTimeOffset.UtcNow,
                VaultPath = scanResult.VaultPath,
                FileCount = scanResult.FileCount,
                TotalBytes = scanResult.TotalBytes,
                ArchivePath = finalPath,
                ArchiveFileName = archiveTempFileName
            };
            
            // Consider more atomicity, two phase recording: create record when process started, update when ended.
            await historyRepository.AddAsync(record, ct);

            Log.Information(
                "BackupOrchestratorExecuteBackupAsync Backup completed successfully: {FinalPath}",
                finalPath);

            return true;
        }
        finally
        {
            // Clean up temp file
            if (File.Exists(archiveTempPath) && !archiveTempPath.StartsWith(_options.Output.Path))
            {
                File.Delete(archiveTempPath);
            }
        }
    }
}

