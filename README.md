# Chronovault

## Project Structure

```
src/
├── Chronovault.Core/           # Domain interfaces, models, options
│   ├── Interfaces/                # IBackupDestination, IArchiveService, IVaultScanner
│   ├── Models/                    # BackupRecord, VaultScanResult
│   └── Options/                   # BackupOptions (IOptions<T> configuration)
├── Chronovault.Infrastructure/ # External concerns
│   ├── Destinations/              # LocalFileDestination (S3Destination future)
│   ├── Migrations/                # FluentMigrator migrations
│   ├── Persistence/               # SQLite repository, MigrationRunner
│   └── Services/                  # VaultScanner, ArchiveService, BackupOrchestrator
├── Chronovault.Service/        # Windows Service entry point
│   ├── Program.cs                 # Host configuration, DI, Serilog
│   └── Worker.cs                  # BackgroundService with timer
└── Chronovault.Packager/       # Build/package console app
    ├── Program.cs                 # Minimal orchestration
    ├── TemplateEngine.cs          # Scriban template processing
    ├── Helpers/                   # Static helper classes
    │   ├── PathHelpers.cs         # Solution/project directory discovery
    │   ├── ProcessHelpers.cs      # Command execution
    │   ├── HttpClientFactory.cs   # Pre-configured HttpClient instances
    │   ├── WinSwDownloader.cs     # GitHub release downloads
    │   └── FormatHelpers.cs       # Byte formatting
    └── Templates/                 # WinSW XML and bat file templates
```

## Configuration

Create `~/.env.chronovault` file with:

```ini
Chronovault:Vault:Path=C:\Users\<you>\Documents\MyFolder
Chronovault:Output:Path=C:\Backups\Chronovault
Chronovault:Archive:Password=YourSecretPassword
Chronovault:IntervalMinutes=60
```

## Build & Package

```bash
# Build and create Windows service package
./build.sh                              # defaults to version 0.0.1
./build.sh 0.1.0                        # specify version
./build.sh 0.1.0 /mnt/c/builds/chronovault/                       # copy zip to Windows folder
./build.sh 0.1.0 /mnt/c/builds/chronovault/Chronovault.zip               # copy with custom filename
./build.sh 0.1.0 /mnt/c/builds/chronovault/{{dt_utc}}_Chronovault.zip    # → 20260101T120000Z_Chronovault.zip
./build.sh 0.1.0 /mnt/c/builds/chronovault/{{date_utc}}_Chronovault.zip  # → 20260101_Chronovault.zip
./build.sh 0.1.0 /mnt/c/builds/chronovault/Chronovault_{{time_utc}}.zip  # → Chronovault_120000.zip
./build.sh 0.1.0 /mnt/c/builds/chronovault/Chronovault_{{version}}.zip   # → Chronovault_0.1.0.zip

# Output: build/Chronovault-{version}.zip (plus optional copy location)
# Placeholders: {{dt_utc}}, {{date_utc}}, {{time_utc}}, {{version}}
```

## Install on Windows

1. Extract the ZIP to `C:\Services\Chronovault\`
2. Create `C:\Users\<you>\.env.chronovault` with configuration (see above)
3. Run `Chronovault.install.bat` as Administrator
4. Run `Chronovault.start.bat` to start

## Data Storage

- SQLite database: `%LOCALAPPDATA%/Chronovault/backup-history.db`
- Logs: `%LOCALAPPDATA%/Chronovault/logs/`

## Architecture Notes

- **Clean Architecture**: Core contains only domain logic, Infrastructure handles external concerns
- **Change Detection**: SHA256 hash of all vault files; skips backup if unchanged
- **Archive Format**: ZIP with AES-256 encryption (DotNetZip - pure .NET, no external dependencies)
- **Destination Abstraction**: `IBackupDestination` interface allows future S3 implementation
