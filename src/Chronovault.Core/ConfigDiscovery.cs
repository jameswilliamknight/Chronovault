namespace Chronovault.Core;

/// <summary>
/// Discovers configuration files across user profiles.
/// When running as a Windows service, the service account's profile differs from the user's,
/// so we search all user profiles for the .env file.
/// </summary>
public static class ConfigDiscovery
{
    private const string EnvFileName = ".env.chronovault";

    // System folders to exclude when scanning C:\Users
    private static readonly HashSet<string> SystemFolders =
    [
        "Public", "Default", "Default User", "All Users", "desktop.ini"
    ];

    /// <summary>
    /// Finds the .env.chronovault configuration file.
    /// Interactive mode: uses current user's profile.
    /// Service mode: searches all user profiles in C:\Users.
    /// </summary>
    /// <returns>Path to config file, or null if not found.</returns>
    /// <exception cref="InvalidOperationException">Thrown when multiple config files found (ambiguous).</exception>
    public static string? FindEnvFile()
    {
        // Interactive mode (debugging): use current user profile - preserves existing behaviour
        if (Environment.UserInteractive)
        {
            var userPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                EnvFileName);
            Log.Debug("ConfigDiscoveryFindEnvFile Interactive mode, checking: {Path}", userPath);
            return File.Exists(userPath) ? userPath : null;
        }

        // Service mode: search all user profiles
        Log.Debug("ConfigDiscoveryFindEnvFile Service mode, scanning user profiles");
        return FindEnvFileAcrossUserProfiles();
    }

    /// <summary>
    /// Searches C:\Users\*\.env.chronovault for config files.
    /// Returns the path if exactly one found, throws if multiple found.
    /// </summary>
    private static string? FindEnvFileAcrossUserProfiles()
    {
        const string usersDir = @"C:\Users";

        if (!Directory.Exists(usersDir))
        {
            Log.Warning("ConfigDiscoveryFindEnvFile Users directory not found: {Path}", usersDir);
            return null;
        }

        var envFiles = new List<string>();
        foreach (var dir in Directory.EnumerateDirectories(usersDir))
        {
            if (IsSystemFolder(Path.GetFileName(dir)))
                continue;

            var envPath = Path.Combine(dir, EnvFileName);
            if (File.Exists(envPath))
            {
                Log.Information("ConfigDiscoveryFindEnvFile Found config: {Path}", envPath);
                envFiles.Add(envPath);
            }
        }

        return envFiles.Count switch
        {
            1 => envFiles[0],
            0 => null,
            _ => throw new InvalidOperationException(
                $"Ambiguous configuration: multiple {EnvFileName} files found. " +
                $"Please remove duplicates or keep only one: {string.Join(", ", envFiles)}")
        };
    }

    /// <summary>
    /// Returns all user profile directories that could contain config files.
    /// Used for logging expected locations when no config is found.
    /// </summary>
    public static IEnumerable<string> GetPossibleConfigLocations()
    {
        const string usersDir = @"C:\Users";

        if (!Directory.Exists(usersDir))
            yield break;

        foreach (var dir in Directory.EnumerateDirectories(usersDir))
        {
            var folderName = Path.GetFileName(dir);
            if (!IsSystemFolder(folderName))
            {
                yield return Path.Combine(dir, EnvFileName);
            }
        }
    }

    private static bool IsSystemFolder(string? folderName)
        => folderName is null || SystemFolders.Contains(folderName);
}

