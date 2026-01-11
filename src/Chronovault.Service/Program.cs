using Chronovault.Core;
using Chronovault.Core.Interfaces;
using Chronovault.Core.Options;
using Chronovault.Infrastructure.Destinations;
using Chronovault.Infrastructure.Migrations;
using Chronovault.Infrastructure.Persistence;
using Chronovault.Infrastructure.Services;
using Chronovault.Service;
using Chronovault.Service.Framework;
using FluentMigrator.Runner;
using Serilog.Events;

// Bootstrap Serilog for startup errors before host is built
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("ProgramStartup Starting Chronovault Backup Service");

    var builder = Host.CreateApplicationBuilder(args);

    // Load configuration from .env file
    // Interactive mode: uses ~/.env.chronovault
    // Service mode: searches C:\Users\*\.env.chronovault (auto-discovery)
    var envFilePath = ConfigDiscovery.FindEnvFile();
    if (envFilePath is null)
    {
        var locations = string.Join(", ", ConfigDiscovery.GetPossibleConfigLocations().Take(5));
        Log.Warning("ProgramStartup No .env.chronovault found. Expected locations: {Locations}", locations);
    }
    else
    {
        Log.Information("ProgramStartup Using config: {EnvFilePath}", envFilePath);
        builder.Configuration.LoadEnvFile(envFilePath);
    }

    // Also load from environment variables for container/service scenarios
    builder.Configuration.AddEnvironmentVariables();

    // Configure Serilog with rolling file and Event Log sinks
    var appDataPath = AppDataHelper.GetAppDataPath();
    var logPath = Path.Combine(appDataPath, "logs", "chronovault-.log"); // Serilog inserts date before extension
    Log.Information("ProgramStartup App data: {AppDataPath}", appDataPath);
    Log.Information("ProgramStartup Log file: {LogDir}/chronovault-{{yyyyMMdd}}.log",
        Path.Combine(appDataPath, "logs"));

    builder.Services.AddSerilog((services, loggerConfig) =>
    {
        loggerConfig
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: "{Timestamp:HH:mm:ss.fff} {Level:u3} {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}");

        // Windows Event Log sink (only on Windows)
        if (OperatingSystem.IsWindows())
        {
            loggerConfig.WriteTo.EventLog(
                source: "Chronovault",
                logName: "Application",
                restrictedToMinimumLevel: LogEventLevel.Warning);
        }
    });

    // Bind BackupOptions from configuration
    builder.Services.Configure<BackupOptions>(builder.Configuration.GetSection(BackupOptions.SectionName));

    // Register services following clean architecture
    builder.Services.AddSingleton<IArchiveService, ArchiveService>();
    builder.Services.AddSingleton<IVaultScanner, VaultScanner>();
    builder.Services.AddSingleton<IBackupService, LocalFileBackupService>();
    builder.Services.AddSingleton<IBackupHistoryRepository, BackupHistoryRepository>();
    builder.Services.AddSingleton<BackupOrchestrator>();

    // FluentMigrator runner for SQLite migrations
    var dbPath = Path.Combine(appDataPath, "backup-history.db");
    var connectionString = $"Data Source={dbPath}";
    Log.Information("ProgramStartup Database: {DbPath}", dbPath);

    builder.Services
        .AddFluentMigratorCore()
        .ConfigureRunner(rb => rb
            .AddSQLite()
            .WithGlobalConnectionString(connectionString)
            .ScanIn(typeof(M001_CreateBackupHistoryTable).Assembly).For.Migrations())
        .AddLogging(lb => lb.AddFluentMigratorConsole()); // TODO: integrate with Serilog

    // Windows Service support
    builder.Services.AddWindowsService(options =>
    {
        options.ServiceName = "Chronovault";
    });

    // Periodic job registration - job resolved per-execution via scope
    builder.Services.AddScoped<VaultBackupJob>();
    builder.Services.AddHostedService<PeriodicHostedService<VaultBackupJob>>();

    var host = builder.Build();

    // Run migrations before starting the service
    Log.Information("ProgramStartup Running database migrations");
    using (var scope = host.Services.CreateScope())
    {
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }
    Log.Information("ProgramStartup Migrations completed");

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "ProgramStartup Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
