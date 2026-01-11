using System.Text;
using Microsoft.Extensions.Options;
using Chronovault.Core.Interfaces;
using Chronovault.Core.Options;
using Chronovault.Infrastructure.Services;
using Chronovault.Service.Framework;

namespace Chronovault.Service;

/// <summary>
///     Periodic job that performs vault backups based on configured interval.
/// </summary>
public sealed class VaultBackupJob(
    BackupOrchestrator orchestrator,
    IBackupHistoryRepository historyRepository,
    IOptions<BackupOptions> options)
    : IPeriodicJob
{
    private readonly BackupOptions _options = options.Value;

    public TimeSpan Period => TimeSpan.FromMinutes(_options.IntervalMinutes > 0 ? _options.IntervalMinutes : 60);

    public JobName JobName => new("VaultBackup");

    public string Icon => "📦";

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        Log.Debug("VaultBackupJobExecuteAsync Starting backup cycle");

        var backupPerformed = await orchestrator.ExecuteBackupAsync(cancellationToken);

        // Log summary with last 5 history items
        await LogHistorySummaryAsync(backupPerformed, cancellationToken);
    }

    private async Task LogHistorySummaryAsync(bool newBackupCreated, CancellationToken ct)
    {
        var recentRecords = await historyRepository.GetRecentAsync(5, ct);

        var sb = new StringBuilder();
        var outcome = newBackupCreated ? "new backup created*" : "no changes";
        sb.AppendLine($"VaultBackupJobExecuteAsync Backup cycle completed ({outcome})");
        sb.AppendLine($"  Recent backups ({recentRecords.Count}):");

        if (recentRecords.Count == 0)
        {
            sb.AppendLine("    (none)");
        }
        else
        {
            var isFirst = true;
            foreach (var record in recentRecords)
            {
                var size = FormatBytes(record.TotalBytes);
                var date = record.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
                var path = record.ArchivePath ?? "(unknown)";
                var newMarker = (isFirst && newBackupCreated) ? " * (new)" : "";
                sb.AppendLine($"    • {date} | {record.FileCount} files | {size} | {path}{newMarker}");
                isFirst = false;
            }
        }

        Log.Information(sb.ToString().TrimEnd());
    }

    private static string FormatBytes(long bytes)
    {
        return bytes switch
        {
            >= 1_073_741_824 => $"{bytes / 1_073_741_824.0:F1} GB",
            >= 1_048_576 => $"{bytes / 1_048_576.0:F1} MB",
            >= 1_024 => $"{bytes / 1_024.0:F1} KB",
            _ => $"{bytes} B"
        };
    }
}