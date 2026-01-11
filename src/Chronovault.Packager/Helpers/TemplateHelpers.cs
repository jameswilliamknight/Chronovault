namespace Chronovault.Packager.Helpers;

/// <summary>
/// Helpers for expanding template placeholders in paths and strings.
/// </summary>
public static class TemplateHelpers
{
    /// <summary>
    /// Expands template placeholders in the given path.
    /// Supported placeholders: {{dt_utc}}, {{date_utc}}, {{time_utc}}, {{version}}
    /// </summary>
    public static string ExpandPathTemplate(string path, string version)
    {
        var now = DateTime.UtcNow;
        return path
            .Replace("{{dt_utc}}", now.ToString("yyyyMMddTHHmmss") + "Z")  // ISO 8601 compact
            .Replace("{{date_utc}}", now.ToString("yyyyMMdd"))
            .Replace("{{time_utc}}", now.ToString("HHmmss"))
            .Replace("{{version}}", version);
    }
}

