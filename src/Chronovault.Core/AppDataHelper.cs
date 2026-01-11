namespace Chronovault.Core;

/// <summary>
/// Provides cross-platform application data directory resolution.
/// </summary>
public static class AppDataHelper
{
    private const string AppName = "Chronovault";

    /// <summary>
    /// Gets the application data directory, creating it if necessary.
    /// Interactive mode: %LOCALAPPDATA%/Chronovault (user-specific)
    /// Service mode: C:\ProgramData\Chronovault (machine-wide, accessible by service account)
    /// </summary>
    public static string GetAppDataPath()
    {
        var appDataPath = Environment.UserInteractive
            ? GetUserAppDataPath()
            : GetServiceAppDataPath();

        Directory.CreateDirectory(appDataPath);
        Log.Debug("AppDataHelperGetAppDataPath Resolved: {Path} (UserInteractive={IsInteractive})",
            appDataPath, Environment.UserInteractive);
        return appDataPath;
    }

    /// <summary>
    /// User-specific app data: %LOCALAPPDATA%/Chronovault
    /// </summary>
    private static string GetUserAppDataPath()
    {
        var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(basePath, AppName);
    }

    /// <summary>
    /// Machine-wide app data for service mode: C:\ProgramData\Chronovault
    /// Uses CommonApplicationData which resolves to ProgramData on Windows.
    /// </summary>
    private static string GetServiceAppDataPath()
    {
        var basePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        return Path.Combine(basePath, AppName);
    }
}

