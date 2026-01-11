using Chronovault.Core.Models;

namespace Chronovault.Core.Interfaces;

/// <summary>
/// Scans the source directory and computes a combined hash for change detection.
/// </summary>
public interface IVaultScanner
{
    /// <summary>
    /// Scans the vault directory and returns scan results including the combined hash.
    /// </summary>
    /// <param name="vaultPath">Path to the source directory.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Scan result with hash and metadata.</returns>
    Task<VaultScanResult> ScanAsync(string vaultPath, CancellationToken ct);
}

