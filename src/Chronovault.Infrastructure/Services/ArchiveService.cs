using Ionic.Zip;
using Chronovault.Core.Interfaces;

namespace Chronovault.Infrastructure.Services;

/// <summary>
/// Creates encrypted ZIP archives using DotNetZip with AES-256 encryption.
/// Pure .NET implementation - no external CLI dependencies.
/// </summary>
public sealed class ArchiveService : IArchiveService
{
    /// <summary>
    /// Creates an AES-256 encrypted ZIP archive from the source directory.
    /// </summary>
    public async Task<string> CreateArchiveAsync(
        string sourceDirectory,
        string outputPath,
        string password,
        CancellationToken ct)
    {
        if (!Directory.Exists(sourceDirectory))
        {
            throw new DirectoryNotFoundException($"Source directory not found: {sourceDirectory}");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be empty for encrypted archives", nameof(password));
        }

        Log.Debug("ArchiveServiceCreateArchiveAsync Source: {SourceDir}", sourceDirectory);
        Log.Debug("ArchiveServiceCreateArchiveAsync Requested output: {OutputPath}", outputPath);

        var outputDir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        // DotNetZip outputs ZIP format with AES-256 encryption
        var actualOutputPath = Path.ChangeExtension(outputPath, ".zip");
        Log.Debug("ArchiveServiceCreateArchiveAsync Actual output: {ActualOutputPath}", actualOutputPath);

        await Task.Run(() =>
        {
            using var zip = new ZipFile
            {
                Password = password,
                Encryption = EncryptionAlgorithm.WinZipAes256, // AES-256 encryption
                CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression
            };

            var files = Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories)
                .Where(f => !IsIgnoredPath(f, sourceDirectory));

            foreach (var file in files)
            {
                ct.ThrowIfCancellationRequested();

                var relativePath = Path.GetRelativePath(sourceDirectory, file);
                var directoryInArchive = Path.GetDirectoryName(relativePath) ?? string.Empty;

                // AddFile(filePath, directoryPathInArchive) preserves folder structure
                zip.AddFile(file, directoryInArchive);
            }

            Log.Information("ArchiveServiceCreateArchiveAsync Saving encrypted archive with {FileCount} files",
                zip.Count);

            zip.Save(actualOutputPath);
        }, ct);

        Log.Information("ArchiveServiceCreateArchiveAsync Created encrypted archive: {Path}", actualOutputPath);
        return actualOutputPath;
    }

    private static bool IsIgnoredPath(string filePath, string basePath)
    {
        var relativePath = Path.GetRelativePath(basePath, filePath);
        var segments = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        // Skip hidden files/folders (starting with .) and node_modules
        return segments.Any(s =>
            s.StartsWith('.') ||
            s.Equals("node_modules", StringComparison.OrdinalIgnoreCase));
    }
}
