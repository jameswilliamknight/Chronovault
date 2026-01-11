using Microsoft.Data.Sqlite;
using Chronovault.Core.Interfaces;
using Chronovault.Core.Models;

namespace Chronovault.Infrastructure.Persistence;

/// <summary>
///     SQLite-based repository for backup history tracking.
/// </summary>
/// <remarks>
///     I will consider an abstraction layer / ORM, dapper or EF, next time this service is worked on.
/// </remarks>
public sealed class BackupHistoryRepository : IBackupHistoryRepository
{
    private readonly string _connectionString;

    public BackupHistoryRepository()
    {
        // Database stored in LocalApplicationData/Chronovault
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var dbPath = Path.Combine(appDataPath, "Chronovault", "backup-history.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        _connectionString = $"Data Source={dbPath}";

        Log.Debug("BackupHistoryRepositoryCtor Database: {DbPath}", dbPath);
        Log.Debug("BackupHistoryRepositoryCtor Connection string: {ConnectionString}", _connectionString);
    }

    /// <summary>
    /// Checks if a backup with the given hash already exists.
    /// </summary>
    public async Task<bool> ExistsAsync(string hash, CancellationToken ct)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT 1 FROM BackupHistory WHERE Hash = @hash LIMIT 1";
        command.Parameters.AddWithValue("@hash", hash);

        var result = await command.ExecuteScalarAsync(ct);
        return result is not null;
    }

    /// <summary>
    /// Records a completed backup in the history table.
    /// </summary>
    public async Task AddAsync(BackupRecord record, CancellationToken ct)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO BackupHistory (Hash, CreatedAt, VaultPath, FileCount, TotalBytes, ArchivePath, ArchiveFileName)
            VALUES (@hash, @createdAt, @vaultPath, @fileCount, @totalBytes, @archivePath, @archiveFileName)
            """;

        command.Parameters.AddWithValue("@hash", record.Hash);
        command.Parameters.AddWithValue("@createdAt", record.CreatedAt.ToString("O")); // ISO 8601
        command.Parameters.AddWithValue("@vaultPath", record.VaultPath);
        command.Parameters.AddWithValue("@fileCount", record.FileCount);
        command.Parameters.AddWithValue("@totalBytes", record.TotalBytes);
        command.Parameters.AddWithValue("@archivePath", (object?)record.ArchivePath ?? DBNull.Value);
        command.Parameters.AddWithValue("@archiveFileName", (object?)record.ArchiveFileName ?? DBNull.Value);

        await command.ExecuteNonQueryAsync(ct);
    }

    /// <summary>
    /// Gets all backup records ordered by creation date (newest first).
    /// </summary>
    public async Task<IReadOnlyList<BackupRecord>> GetAllAsync(CancellationToken ct)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Hash, CreatedAt, VaultPath, FileCount, TotalBytes, ArchivePath, ArchiveFileName
            FROM BackupHistory
            ORDER BY CreatedAt DESC
            """;

        var records = new List<BackupRecord>();
        await using var reader = await command.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            records.Add(new BackupRecord
            {
                Hash = reader.GetString(0),
                CreatedAt = DateTimeOffset.Parse(reader.GetString(1)),
                VaultPath = reader.GetString(2),
                FileCount = reader.GetInt32(3),
                TotalBytes = reader.GetInt64(4),
                ArchivePath = reader.IsDBNull(5) ? null : reader.GetString(5),
                ArchiveFileName = reader.IsDBNull(6) ? null : reader.GetString(6)
            });
        }

        return records;
    }

    /// <summary>
    /// Gets the most recent N backup records (newest first).
    /// </summary>
    public async Task<IReadOnlyList<BackupRecord>> GetRecentAsync(int count, CancellationToken ct)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Hash, CreatedAt, VaultPath, FileCount, TotalBytes, ArchivePath, ArchiveFileName
            FROM BackupHistory
            ORDER BY CreatedAt DESC
            LIMIT @count
            """;
        command.Parameters.AddWithValue("@count", count);

        var records = new List<BackupRecord>();
        await using var reader = await command.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            records.Add(new BackupRecord
            {
                Hash = reader.GetString(0),
                CreatedAt = DateTimeOffset.Parse(reader.GetString(1)),
                VaultPath = reader.GetString(2),
                FileCount = reader.GetInt32(3),
                TotalBytes = reader.GetInt64(4),
                ArchivePath = reader.IsDBNull(5) ? null : reader.GetString(5),
                ArchiveFileName = reader.IsDBNull(6) ? null : reader.GetString(6)
            });
        }

        return records;
    }
}

