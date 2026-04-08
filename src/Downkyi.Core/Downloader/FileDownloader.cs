using NLog;

namespace Downkyi.Core.Downloader;

/// <summary>
/// Simple HttpClient-based single-file downloader.
/// Replaces the deprecated HttpWebRequest-based MultiThreadDownloader for Bilibili stream downloads.
/// </summary>
public static class FileDownloader
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private static readonly HttpClient Client = new(new HttpClientHandler
    {
        AllowAutoRedirect = true,
        MaxAutomaticRedirections = 5,
    })
    {
        Timeout = TimeSpan.FromMinutes(30),
    };

    /// <summary>
    /// Downloads a URL to a local file, reporting progress and supporting cancellation.
    /// </summary>
    /// <param name="url">Source URL.</param>
    /// <param name="filePath">Destination file path (directory will be created if needed).</param>
    /// <param name="headers">Optional extra HTTP headers (e.g. Referer, Cookie).</param>
    /// <param name="progress">
    ///   Progress callback: (bytesReceived, totalBytes).
    ///   totalBytes is -1 if the server did not send Content-Length.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    public static async Task DownloadAsync(
        string url,
        string filePath,
        Dictionary<string, string>? headers = null,
        IProgress<(long bytesReceived, long totalBytes)>? progress = null,
        CancellationToken ct = default)
    {
        string? dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.TryAddWithoutValidation("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        if (headers != null)
            foreach (var (k, v) in headers)
                request.Headers.TryAddWithoutValidation(k, v);

        using var response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        long total = response.Content.Headers.ContentLength ?? -1;

        await using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 65536, useAsync: true);
        await using var stream = await response.Content.ReadAsStreamAsync(ct);

        byte[] buffer = new byte[65536];  // 64 KB chunks
        long received = 0;
        int read;

        while ((read = await stream.ReadAsync(buffer, ct)) > 0)
        {
            await fs.WriteAsync(buffer.AsMemory(0, read), ct);
            received += read;
            progress?.Report((received, total));
        }
    }
}
