using FluentMigrator;

namespace Chronovault.Infrastructure.Migrations;

/// <summary>
/// Adds ArchiveFileName column for easier display in logs/reports.
/// </summary>
[Migration(2)]
public class M002_AddArchiveFileName : Migration
{
    public override void Up()
    {
        Alter.Table("BackupHistory")
            .AddColumn("ArchiveFileName").AsString(256).Nullable();
    }

    public override void Down()
    {
        // Not implemented - forward-only migrations
    }
}

