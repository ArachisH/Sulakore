using System.Net;
using System.Net.Http.Json;

using Sulakore.Habbo.Web.Json;

namespace Sulakore.Habbo.Web;

public static class HAPI
{
    private static readonly HttpClient _client;
    private static readonly HttpClientHandler _handler;

    public static CookieContainer Cookies
    {
        get => _handler.CookieContainer;
        set => _handler.CookieContainer = value;
    }

    static HAPI()
    {
        _handler = new HttpClientHandler
        {
            UseProxy = false
        };

        _client = new HttpClient(_handler);
        _client.DefaultRequestHeaders.ConnectionClose = true;
        _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.5060.53 Safari/537.36");
    }

    public static Task<byte[]?> GetFigureDataAsync(string query, CancellationToken cancellationToken = default) => GetAsync<byte[]?>(HHotel.US.ToUri(), "/habbo-imaging/avatarimage?" + query, cancellationToken);
    public static Task<HUser?> GetUserAsync(string name, HHotel hotel, CancellationToken cancellationToken = default) => GetAsync<HUser?>(hotel.ToUri(), "/api/public/users?name=" + name, cancellationToken);
    public static Task<HProfile?> GetProfileAsync(string uniqueId, CancellationToken cancellationToken = default) => GetAsync<HProfile?>(uniqueId.AsSpan().ToHotel().ToUri(), $"/api/public/users/{uniqueId}/profile", cancellationToken);

    public static async Task<string?> GetLatestRevisionAsync(HHotel hotel, CancellationToken cancellationToken = default)
    {
        string? body = await GetAsync<string?>(hotel.ToUri(), "/gamedata/external_variables/1", cancellationToken).ConfigureAwait(false);
        if (body == null) return null;

        int revisionStartIndex = body.LastIndexOf("/gordon/") + 8;
        if (revisionStartIndex != 7)
        {
            int revisionEndIndex = body.IndexOf('/', revisionStartIndex);
            if (revisionEndIndex != -1)
            {
                return body[revisionStartIndex..revisionEndIndex];
            }
        }
        return null;
    }
    public static async Task<HProfile?> GetProfileAsync(string name, HHotel hotel, CancellationToken cancellationToken = default)
    {
        HUser? user = await GetUserAsync(name, hotel, cancellationToken).ConfigureAwait(false);
        if (user?.ProfileVisible == true)
        {
            return await GetProfileAsync(user.UniqueId, cancellationToken).ConfigureAwait(false);
        }
        return new HProfile { User = user };
    }

    public static async Task<T?> GetAsync<T>(Uri baseUri, string path,
        CancellationToken cancellationToken = default,
        Func<HttpContent, CancellationToken, Task<T>>? contentConverter = null)
    {
        ArgumentNullException.ThrowIfNull(baseUri);

        string uriAuthority = baseUri.GetLeftPart(UriPartial.Authority);
        using HttpRequestMessage request = new(HttpMethod.Get, uriAuthority + path);
        if (!string.IsNullOrWhiteSpace(path))
        {
            request.Headers.Add("Referer", uriAuthority);
        }

        using HttpResponseMessage response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return default;

        if (contentConverter != null)
            return await contentConverter(response.Content, cancellationToken).ConfigureAwait(false);

        if (typeof(T) == typeof(string))
            return (T)(object)await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (typeof(T) == typeof(byte[]))
            return (T)(object)await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);

        if (response.Content.Headers.ContentType?.MediaType == "application/json")
            return (T?)await response.Content.ReadFromJsonAsync(typeof(T), HJsonContext.Default, cancellationToken).ConfigureAwait(false);

        return default;
    }

    private static Task<HttpResponseMessage> SendMessageAsync(HHotel hotel, string path, HttpMethod method)
    {
        string uriAuthority = hotel.ToUri().GetLeftPart(UriPartial.Authority);
        HttpRequestMessage requestMsg = new(method, uriAuthority + path);
        if (!string.IsNullOrWhiteSpace(path))
        {
            requestMsg.Headers.Add("Referer", uriAuthority);
        }
        return _client.SendAsync(requestMsg);
    }
}