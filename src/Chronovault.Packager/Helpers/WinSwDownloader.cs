using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Chronovault.Packager.Helpers;

/// <summary>
/// Downloads WinSW from GitHub releases.
/// </summary>
public static class WinSwDownloader
{
    private const string ReleasesApiUrl = "https://api.github.com/repos/winsw/winsw/releases";
    private const string TagApiUrlFormat = "https://api.github.com/repos/winsw/winsw/releases/tags/{0}";

    /// <summary>
    /// Checks if a newer WinSW version is available (warns only, doesn't fail).
    /// Fetches all releases to include pre-releases in comparison.
    /// </summary>
    public static async Task CheckForNewerVersionAsync(string currentVersion)
    {
        try
        {
            using var client = HttpClientFactory.Create(timeout: TimeSpan.FromSeconds(5));

            // Fetch all releases (not just /latest which excludes pre-releases)
            var releases = await client.GetFromJsonAsync<List<GitHubRelease>>(
                $"{ReleasesApiUrl}?per_page=5");

            var latest = releases?.FirstOrDefault()?.TagName;
            if (latest != null && latest != currentVersion)
            {
                Console.WriteLine($"ℹ️  Note: Newer WinSW available ({latest}), using {currentVersion}");
            }
        }
        catch
        {
            // Silently ignore - version check is best-effort
        }
    }

    /// <summary>
    /// Downloads WinSW-x64.exe to the specified path.
    /// </summary>
    public static async Task<bool> DownloadAsync(string version, string outputPath)
    {
        using var client = HttpClientFactory.Create();

        try
        {
            var apiUrl = string.Format(TagApiUrlFormat, version);
            var release = await client.GetFromJsonAsync<GitHubRelease>(apiUrl);

            var asset = release?.Assets?.FirstOrDefault(a => a.Name == "WinSW-x64.exe");
            if (asset == null)
            {
                Console.WriteLine($"⚠️  Version {version} not found, trying latest release...");
                var releases = await client.GetFromJsonAsync<List<GitHubRelease>>($"{ReleasesApiUrl}?per_page=1");
                asset = releases?.FirstOrDefault()?.Assets?.FirstOrDefault(a => a.Name == "WinSW-x64.exe");
            }

            if (asset?.BrowserDownloadUrl == null)
            {
                Console.WriteLine("❌ Could not find WinSW-x64.exe download URL");
                return false;
            }

            Console.WriteLine($"📥 Downloading from: {asset.BrowserDownloadUrl}");

            await using var stream = await client.GetStreamAsync(asset.BrowserDownloadUrl);
            await using var fileStream = File.Create(outputPath);
            await stream.CopyToAsync(fileStream);

            Console.WriteLine($"   ✅ Downloaded WinSW to {Path.GetFileName(outputPath)}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Download failed: {ex.Message}");
            return false;
        }
    }

    // GitHub API DTOs
    private sealed class GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string? TagName { get; set; }

        [JsonPropertyName("assets")]
        public List<GitHubAsset>? Assets { get; set; }
    }

    private sealed class GitHubAsset
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("browser_download_url")]
        public string? BrowserDownloadUrl { get; set; }
    }
}
