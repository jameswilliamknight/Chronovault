namespace Chronovault.Packager.Helpers;

/// <summary>
/// Creates pre-configured HttpClient instances for this app.
/// </summary>
public static class HttpClientFactory
{
    /// <summary>
    /// Creates an HttpClient with standard defaults (User-Agent for GitHub API compatibility).
    /// </summary>
    public static HttpClient Create(TimeSpan? timeout = null)
    {
        var client = new HttpClient();
        if (timeout.HasValue)
        {
            client.Timeout = timeout.Value;
        }
        client.DefaultRequestHeaders.UserAgent.ParseAdd("dotnet-httpclient");
        return client;
    }
}

