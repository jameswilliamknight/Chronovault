// Re-export from Core for service registration convenience
// Actual implementation is in Core.Services.VaultScanner
// This file exists to maintain clean architecture boundaries while simplifying DI registration

using System.Security.Cryptography;
using System.Text;
using Chronovault.Core.Interfaces;
using Chronovault.Core.Models;

namespace Chronovault.Infrastructure.Services;

/// <summary>
/// Scans a source directory and computes a combined hash for change detection.
/// </summary>
public sealed class VaultScanner : IVaultScanner
{
    /// <summary>
    /// Scans all files in the vault and computes a combined SHA256 hash.
    /// Hash includes file paths (relative) and contents to detect renames and content changes.
    /// </summary>
    public async Task<VaultScanResult> ScanAsync(string vaultPath, CancellationToken ct)
    {
        if (!Directory.Exists(vaultPath))
        {
            throw new DirectoryNotFoundException($"Vault directory not found: {vaultPath}");
        }

        var files = Directory.EnumerateFiles(vaultPath, "*", SearchOption.AllDirectories)
            .Where(f => !IsIgnoredPath(f, vaultPath))
            .OrderBy(f => f) // Deterministic ordering for consistent hashing
            .ToList();

        using var combinedHasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        long totalBytes = 0;

        foreach (var filePath in files)
        {
            ct.ThrowIfCancellationRequested();

            var relativePath = Path.GetRelativePath(vaultPath, filePath);
            var pathBytes = Encoding.UTF8.GetBytes(relativePath);
            combinedHasher.AppendData(pathBytes);

            var fileBytes = await File.ReadAllBytesAsync(filePath, ct);
            combinedHasher.AppendData(fileBytes);
            totalBytes += fileBytes.Length;
        }

        var hashBytes = combinedHasher.GetHashAndReset();
        var hashString = Convert.ToHexString(hashBytes).ToLowerInvariant();

        return new VaultScanResult
        {
            Hash = hashString,
            FileCount = files.Count,
            TotalBytes = totalBytes,
            VaultPath = vaultPath,
            ScannedAt = DateTimeOffset.UtcNow
        };
    }

    private static bool IsIgnoredPath(string filePath, string vaultPath)
    {
        var relativePath = Path.GetRelativePath(vaultPath, filePath);
        var segments = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        return segments.Any(s =>
            s.StartsWith('.') ||
            s.Equals("node_modules", StringComparison.OrdinalIgnoreCase));
    }
}

