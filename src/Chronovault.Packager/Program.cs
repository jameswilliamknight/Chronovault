using System.IO.Compression;
using Chronovault.Packager;
using Chronovault.Packager.Helpers;

// Parse command line arguments
// Usage: dotnet run -- [version] [output-path]
// In CI (detected via GITHUB_ACTIONS env var), version is required. Locally, defaults to 0.0.1 for dev convenience.
var isCi = Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";
if (isCi && args.Length == 0)
{
    Console.WriteLine("❌ Error: Version argument is required in CI environment");
    return 1;
}
var version = args.Length > 0 ? args[0] : "0.0.1";
var outputZipPath = args.Length > 1 ? args[1] : null; // Optional: where to copy final zip

// Configuration
var projectDir = PathHelpers.GetProjectDirectory();
var solutionRoot = PathHelpers.FindSolutionRoot(projectDir)
    ?? throw new InvalidOperationException("Could not find solution root (.sln file)");
var serviceProjectDir = Path.Combine(solutionRoot, "src", "Chronovault.Service");
var templatesDir = Path.Combine(projectDir, "Templates");
var buildDir = Path.Combine(solutionRoot, "build");
var packageDir = Path.Combine(buildDir, version);

const string winswVersion = "v3.0.0-alpha.11";

// Single service name - no environment variants for personal tools
const string serviceName = "Chronovault";
const string serviceDisplayName = "Chronovault Backup Service";

Console.WriteLine("🔨 Building Chronovault Windows Service Package...");
Console.WriteLine($"📁 Solution root: {solutionRoot}");
Console.WriteLine($"📦 Build directory: {buildDir}");
Console.WriteLine($"🏷️  Package version: {version}");
Console.WriteLine($"🏷️  Service name: {serviceName}");
if (!string.IsNullOrEmpty(outputZipPath))
    Console.WriteLine($"📤 Output path: {outputZipPath}");

// Check for newer WinSW version (warn only, don't change behaviour)
await WinSwDownloader.CheckForNewerVersionAsync(winswVersion);

// Clean and create build directories
if (Directory.Exists(buildDir))
{
    Directory.Delete(buildDir, recursive: true);
}
Directory.CreateDirectory(packageDir);

// Step 1: Publish the service project
Console.WriteLine();
Console.WriteLine("🏗️  Building .NET application as self-contained Windows executable...");

var publishResult = await ProcessHelpers.RunCommandAsync("dotnet", [
    "publish", serviceProjectDir,
    "--configuration", "Release",
    "--runtime", "win-x64",
    "--self-contained", "true",
    "--output", packageDir,
    "-p:PublishSingleFile=true",
    "-p:PublishTrimmed=false",
    "-p:IncludeNativeLibrariesForSelfExtract=true",
    "-p:EnableCompressionInSingleFile=true",
    "-p:DebugType=None"
]);

if (publishResult != 0)
{
    Console.WriteLine("❌ Error: dotnet publish failed");
    return 1;
}

// Step 2: Download WinSW
Console.WriteLine();
Console.WriteLine("⬇️  Downloading WinSW-x64.exe from GitHub releases...");

var winswPath = Path.Combine(packageDir, $"{serviceName}.exe");
var downloaded = await WinSwDownloader.DownloadAsync(winswVersion, winswPath);
if (!downloaded)
{
    Console.WriteLine("❌ Error: Failed to download WinSW");
    return 1;
}

// Step 3: Process templates
Console.WriteLine();
Console.WriteLine("📄 Processing template files...");

var model = new PackageModel
{
    Version = version,
    Environment = "Release",
    ServiceName = serviceName,
    ServiceDisplayName = serviceDisplayName
};

model.ApplyTemplates(templatesDir, packageDir);

// Step 4: List package contents
Console.WriteLine();
Console.WriteLine("📋 Package contents:");
foreach (var file in Directory.EnumerateFiles(packageDir))
{
    var fileInfo = new FileInfo(file);
    Console.WriteLine($"   {fileInfo.Name} ({FormatHelpers.FormatBytes(fileInfo.Length)})");
}

// Step 5: Create ZIP package
Console.WriteLine();
Console.WriteLine("🗜️  Creating Windows-compatible ZIP package...");

var zipPath = Path.Combine(buildDir, $"Chronovault-{version}.zip");
ZipFile.CreateFromDirectory(packageDir, zipPath, CompressionLevel.Optimal, includeBaseDirectory: true);

// Copy to custom output path if specified
if (!string.IsNullOrEmpty(outputZipPath))
{
    // Expand template placeholders in path (e.g. {{dt_utc}} → 20260101T120000Z)
    var expandedPath = TemplateHelpers.ExpandPathTemplate(outputZipPath, version);

    var finalPath = expandedPath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)
        ? expandedPath
        : Path.Combine(expandedPath, Path.GetFileName(zipPath));

    Directory.CreateDirectory(Path.GetDirectoryName(finalPath) ?? ".");
    File.Copy(zipPath, finalPath, overwrite: true);

    Console.WriteLine();
    Console.WriteLine("✅ Package created successfully!");
    Console.WriteLine($"📦 Package location: {finalPath}");
}
else
{
    Console.WriteLine();
    Console.WriteLine("✅ Package created successfully!");
    Console.WriteLine($"📦 Package location: {zipPath}");
}
Console.WriteLine();
Console.WriteLine("🚀 To install on Windows:");
Console.WriteLine("   1. Extract the ZIP file to a folder (e.g. C:\\Services\\)");
Console.WriteLine($"   2. Navigate to the {version} folder");
Console.WriteLine($"   3. Right-click {serviceName}.install.bat and 'Run as Administrator'");
Console.WriteLine($"   4. Run {serviceName}.start.bat to start the service");
Console.WriteLine();
Console.WriteLine("💡 Configure via .env.chronovault in any user profile:");
var windowsUsers = PathHelpers.GetWindowsUsernames().ToList();
if (windowsUsers.Count > 0)
{
    Console.WriteLine($"   Create: C:\\Users\\{windowsUsers[0]}\\.env.chronovault");
}
else
{
    Console.WriteLine("   Create: C:\\Users\\<username>\\.env.chronovault");
}
Console.WriteLine();
Console.WriteLine("   Example contents:");
Console.WriteLine("   Chronovault:Vault:Path=C:\\Users\\<you>\\Documents\\MyFolder");
Console.WriteLine("   Chronovault:Output:Path=C:\\Backups\\Chronovault");
Console.WriteLine("   Chronovault:Archive:Password=YourSecretPassword");
Console.WriteLine("   Chronovault:IntervalMinutes=60");
Console.WriteLine();
Console.WriteLine("   The service auto-discovers the config file from C:\\Users\\*\\");

return 0;
