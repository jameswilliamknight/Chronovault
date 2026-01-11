namespace Chronovault.Packager.Helpers;

/// <summary>
/// Formatting utilities.
/// </summary>
public static class FormatHelpers
{
    /// <summary>
    /// Formats bytes as human-readable string (KB/MB).
    /// </summary>
    public static string FormatBytes(long bytes)
    {
        return bytes switch
        {
            >= 1_000_000 => $"{bytes / 1_000_000.0:F1} MB",
            >= 1_000 => $"{bytes / 1_000.0:F1} KB",
            _ => $"{bytes} B"
        };
    }
}

