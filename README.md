# Chronovault

## How It Works

Chronovault runs as a Windows service and periodically checks your source folder for changes. It calculates a SHA256 hash of all files and compares it against previous backups stored in a local SQLite database. If nothing has changed, it skips the backup entirely. When changes are detected, it creates an encrypted ZIP archive using AES-256 and saves it to your configured output location.


## Architecture

Chronovault follows clean architecture with four projects:

- **Chronovault.Core** — Domain interfaces, models, and options
- **Chronovault.Infrastructure** — SQLite persistence, file operations, backup destinations
- **Chronovault.Service** — Windows service entry point and scheduling
- **Chronovault.Packager** — Build tooling for creating distributable packages

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