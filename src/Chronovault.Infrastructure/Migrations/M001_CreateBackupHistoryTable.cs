using FluentMigrator;

namespace Chronovault.Infrastructure.Migrations;

/// <summary>
/// Creates the BackupHistory table for tracking completed backups.
/// </summary>
[Migration(1)]
public class M001_CreateBackupHistoryTable : Migration
{
    public override void Up()
    {
        Create.Table("BackupHistory")
            .WithColumn("Hash").AsString(64).NotNullable().PrimaryKey() // SHA256 hex = 64 chars
            .WithColumn("CreatedAt").AsString().NotNullable() // ISO 8601 datetime string
            .WithColumn("VaultPath").AsString(1024).NotNullable()
            .WithColumn("FileCount").AsInt32().NotNullable()
            .WithColumn("TotalBytes").AsInt64().NotNullable()
            .WithColumn("ArchivePath").AsString(1024).Nullable();

        // Index for querying by creation date (most recent backups)
        Create.Index("IX_BackupHistory_CreatedAt")
            .OnTable("BackupHistory")
            .OnColumn("CreatedAt")
            .Descending();
    }

    public override void Down()
    {
        Delete.Table("BackupHistory");
    }
}

