using System.Net;
using Downkyi.Core.Settings;

namespace Downkyi.Core.Bili.Web;

/// <summary>
/// Authenticated HTTP client for Bilibili APIs not covered by BiliSharp.
/// Reads cookies from LoginDatabase and adds standard Bilibili request headers.
/// </summary>
internal static class BiliWebClient
{
    private static readonly HttpClient _client;

    static BiliWebClient()
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
            UseCookies = false,
        };
        _client = new HttpClient(handler);
        _client.Timeout = TimeSpan.FromSeconds(30);
        _client.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
        _client.DefaultRequestHeaders.Add("Origin", "https://www.bilibili.com");
    }

    /// <summary>
    /// Makes an authenticated GET request to a Bilibili API endpoint.
    /// </summary>
    public static async Task<string> GetAsync(string url, string referer = "https://www.bilibili.com")
    {
        try
        {
            string userAgent = SettingsManager.Instance.GetUserAgent();
            string cookies = await LoginHelperV2.GetLoginInfoCookiesString();

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", userAgent);
            request.Headers.Add("Referer", referer);
            if (!string.IsNullOrEmpty(cookies))
            {
                request.Headers.Add("Cookie", cookies);
            }

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            NLog.LogManager.GetCurrentClassLogger().Error(e, "BiliWebClient.GetAsync failed for {Url}", url);
            return string.Empty;
        }
    }
}
