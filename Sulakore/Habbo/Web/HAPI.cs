using System.Net;
using System.Text.Json;
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
    public static JsonSerializerOptions SerializerOptions { get; }

    static HAPI()
    {
        _handler = new HttpClientHandler
        {
            UseProxy = false,
            AutomaticDecompression = DecompressionMethods.All
        };

        _client = new HttpClient(_handler);
        _client.DefaultRequestHeaders.ConnectionClose = true;
        _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.131 Safari/537.36 Edg/92.0.902.67");

        SerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        SerializerOptions.Converters.Add(new DateTimeConverter());
    }

    public static Task<byte[]?> GetFigureDataAsync(string query) => ReadContentAsync<byte[]?>(HHotel.US.ToUri(), "/habbo-imaging/avatarimage?" + query);
    public static Task<HUser?> GetUserAsync(string name, HHotel hotel) => ReadContentAsync<HUser?>(hotel.ToUri(), "/api/public/users?name=" + name);
    public static Task<HProfile?> GetProfileAsync(string uniqueId) => ReadContentAsync<HProfile?>(uniqueId.AsSpan().ToHotel().ToUri(), $"/api/public/users/{uniqueId}/profile");

    public static async Task<string?> GetLatestRevisionAsync(HHotel hotel)
    {
        string? body = await ReadContentAsync<string?>(hotel.ToUri(), "/gamedata/external_variables/1").ConfigureAwait(false);
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
    public static async Task<HProfile?> GetProfileAsync(string name, HHotel hotel)
    {
        HUser? user = await GetUserAsync(name, hotel).ConfigureAwait(false);
        if (user?.ProfileVisible == true)
        {
            return await GetProfileAsync(user.UniqueId).ConfigureAwait(false);
        }
        return new HProfile { User = user };
    }

    public static async Task<T?> ReadContentAsync<T>(Uri baseUri, string path, Func<HttpContent, Task<T>>? contentConverter = null)
    {
        string uriAuthority = baseUri.GetLeftPart(UriPartial.Authority);
        using HttpRequestMessage request = new(HttpMethod.Get, uriAuthority + path);
        if (!string.IsNullOrWhiteSpace(path))
        {
            request.Headers.Add("Referer", uriAuthority);
        }

        using HttpResponseMessage response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return default;

        if (contentConverter != null)
            return await contentConverter(response.Content).ConfigureAwait(false);

        if (typeof(T) == typeof(string))
            return (T)(object)await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        if (typeof(T) == typeof(byte[]))
            return (T)(object)await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

        if (response.Content.Headers.ContentType?.MediaType == "application/json")
            return await response.Content.ReadFromJsonAsync<T>(SerializerOptions).ConfigureAwait(false);

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