namespace Chronovault.Service;

/// <summary>
/// Loads configuration from a .env-style file with KEY=VALUE format.
/// Supports colon-separated nested keys (e.g., Chronovault:Vault:Path).
/// </summary>
public static class EnvFileLoader
{
    /// <summary>
    /// Loads configuration from the specified file path into the configuration builder.
    /// </summary>
    public static void LoadEnvFile(this IConfigurationBuilder configBuilder, string filePath)
    {
        if (!File.Exists(filePath))
        {
            Log.Warning("EnvFileLoaderLoadEnvFile File not found at {FilePath}", filePath);
            return;
        }

        var configValues = new Dictionary<string, string?>();

        try
        {
            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                // Skip empty lines and comments
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#'))
                {
                    continue;
                }

                // Parse KEY=VALUE
                var separatorIndex = trimmed.IndexOf('=');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                var key = trimmed[..separatorIndex].Trim();
                var value = trimmed[(separatorIndex + 1)..].Trim();

                // Remove surrounding quotes if present
                if ((value.StartsWith('"') && value.EndsWith('"')) ||
                    (value.StartsWith('\'') && value.EndsWith('\'')))
                {
                    value = value[1..^1];
                }

                configValues[key] = value;
                Log.Debug("EnvFileLoaderLoadEnvFile Loaded config key {Key}", key);
            }

            if (configValues.Count > 0)
            {
                configBuilder.AddInMemoryCollection(configValues);
                Log.Information("EnvFileLoaderLoadEnvFile Loaded {Count} configuration value(s) from {FilePath}",
                    configValues.Count, filePath);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "EnvFileLoaderLoadEnvFile Failed to load configuration from {FilePath}", filePath);
        }
    }
}

