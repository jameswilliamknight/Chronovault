namespace Chronovault.Packager.Helpers;

/// <summary>
/// Helpers for locating project and solution directories.
/// </summary>
public static class PathHelpers
{
    // System folders to exclude when scanning /mnt/c/Users
    private static readonly HashSet<string> ExcludedUserFolders = [
        "Public", "Default", "Default User", "All Users", "desktop.ini"
    ];

    /// <summary>
    /// Gets Windows usernames by scanning /mnt/c/Users (WSL) or C:\Users (Windows).
    /// </summary>
    public static IEnumerable<string> GetWindowsUsernames()
    {
        var usersPath = Directory.Exists("/mnt/c/Users")
            ? "/mnt/c/Users"
            : @"C:\Users";

        if (!Directory.Exists(usersPath))
            return [];

        return Directory.EnumerateDirectories(usersPath)
            .Select(Path.GetFileName)
            .Where(name => name != null && !ExcludedUserFolders.Contains(name))
            .Cast<string>();
    }

    /// <summary>
    /// Finds the project directory by searching for the .csproj file.
    /// </summary>
    public static string GetProjectDirectory()
    {
        var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
        if (!string.IsNullOrEmpty(assemblyLocation))
        {
            var dir = Path.GetDirectoryName(assemblyLocation);
            while (dir != null && !File.Exists(Path.Combine(dir, "Chronovault.Packager.csproj")))
            {
                dir = Path.GetDirectoryName(dir);
            }
            if (dir != null) return dir;
        }
        return Directory.GetCurrentDirectory();
    }

    /// <summary>
    /// Finds the solution root by searching for a .sln file (max 3 levels up).
    /// </summary>
    public static string? FindSolutionRoot(string startDir)
    {
        var dir = new DirectoryInfo(startDir);
        for (var i = 0; i < 3 && dir != null; i++)
        {
            if (dir.GetFiles("*.sln").Length > 0)
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }
        return null;
    }
}

