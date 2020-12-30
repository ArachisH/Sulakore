using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Net.Http.Json;
using System.Threading.Tasks;

using Sulakore.Network;
using Sulakore.Habbo.Web.Json;

namespace Sulakore.Habbo.Web
{
    public static class HAPI
    {
        private static readonly HttpClient _client;
        private static readonly HttpClientHandler _handler;

        public static JsonSerializerOptions SerializerOptions { get; }

        public const string CHROME_USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36";

        static HAPI()
        {
            _handler = new HttpClientHandler
            {
                UseProxy = false,
                UseCookies = false,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            _client = new HttpClient(_handler);
            _client.DefaultRequestHeaders.ConnectionClose = true;
            _client.DefaultRequestHeaders.Add("User-Agent", CHROME_USER_AGENT);
            _client.DefaultRequestHeaders.Add("Cookie", "YPF8827340282Jdskjhfiw_928937459182JAX666=127.0.0.1");

            SerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            SerializerOptions.Converters.Add(new DateTimeConverter());
        }

        public static Task<byte[]> GetFigureDataAsync(string query) => ReadContentAsync<byte[]>(HHotel.Com.ToUri(), "/habbo-imaging/avatarimage?" + query);
        public static Task<HUser> GetUserAsync(string name, HHotel hotel) => ReadContentAsync<HUser>(hotel.ToUri(), "/api/public/users?name=" + name);
        public static Task<HProfile> GetProfileAsync(string uniqueId) => ReadContentAsync<HProfile>(HotelEndPoint.GetHotel(uniqueId).ToUri(), $"/api/public/users/{uniqueId}/profile");

        public static async Task<string> GetLatestRevisionAsync(HHotel hotel)
        {
            string body = await ReadContentAsync<string>(hotel.ToUri(), "/gamedata/external_variables/1").ConfigureAwait(false);
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
        public static async Task<HProfile> GetProfileAsync(string name, HHotel hotel)
        {
            HUser user = await GetUserAsync(name, hotel).ConfigureAwait(false);
            if (user.ProfileVisible == true)
            {
                return await GetProfileAsync(user.UniqueId).ConfigureAwait(false);
            }
            return new HProfile { User = user };
        }

        public static async Task<T> ReadContentAsync<T>(Uri baseUri, string path, Func<HttpContent, Task<T>> contentConverter = null)
        {
            using HttpRequestMessage request = new(HttpMethod.Get, baseUri.GetLeftPart(UriPartial.Authority) + path);
            using HttpResponseMessage response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            ServicePointManager.FindServicePoint(request.RequestUri).ConnectionLeaseTimeout = 30 * 1000;

            if (!response.IsSuccessStatusCode) return default;

            if (contentConverter != null)
                return await contentConverter(response.Content).ConfigureAwait(false);

            Type genericType = typeof(T);

            if (genericType == typeof(string))
                return (T)(object)await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (genericType == typeof(byte[]))
                return (T)(object)await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

            if (response.Content.Headers.ContentType.MediaType == "application/json")
                return await response.Content.ReadFromJsonAsync<T>(SerializerOptions).ConfigureAwait(false);

            return default;
        }
    }
}