using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using Sulakore.Network;
using Sulakore.Habbo.Web.Json;

namespace Sulakore.Habbo.Web
{
    public static class HAPI
    {
        private static readonly HttpClient _client;
        private static readonly HttpClientHandler _handler;
        private static readonly JsonSerializerOptions _serializerOptions;

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

            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            _serializerOptions.Converters.Add(new DateTimeConverter());
        }

        public static Task<byte[]> GetFigureDataAsync(string query) => ReadContentAsync<byte[]>(HHotel.Com.ToUri(), "/habbo-imaging/avatarimage?" + query);
        public static async Task<HUser> GetUserAsync(string name, HHotel hotel) => await ReadContentAsync<HUser>(hotel.ToUri(), "/api/public/users?name=" + name);
        public static async Task<HProfile> GetProfileAsync(string uniqueId) => await ReadContentAsync<HProfile>(HotelEndPoint.GetHotel(uniqueId).ToUri(), $"/api/public/users/{uniqueId}/profile");

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
            if (user.IsProfileVisible == true)
            {
                return await GetProfileAsync(user.UniqueId).ConfigureAwait(false);
            }
            return new HProfile { User = user };
        }

        public static Task<HGame> GetGameAsync(string revision)
        {
            return ReadContentAsync(HHotel.Com.ToUri("images"), $"/gordon/{revision}/Habbo.swf", async content =>
            {
                var game = new HGame(await content.ReadAsStreamAsync().ConfigureAwait(false));
                game.Disassemble();
                return game;
            });
        }
        public static Task DownloadGameAsync(string revision, string fileName, Action<double> progress = null)
        {
            return ReadContentAsync(HHotel.Com.ToUri("images"), $"/gordon/{revision}/Habbo.swf", async content =>
            {
                var buffer = new byte[81920];
                using (Stream contentStream = await content.ReadAsStreamAsync().ConfigureAwait(false))
                using (var fileStream = File.Create(fileName))
                {
                    if (content.Headers.ContentLength == null)
                    {
                        await contentStream.CopyToAsync(fileStream).ConfigureAwait(false);
                        progress?.Invoke(100);
                    }
                    else
                    {
                        double totalBytesRead = 0;
                        while (totalBytesRead != content.Headers.ContentLength)
                        {
                            int bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                            await fileStream.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);

                            totalBytesRead += bytesRead;
                            double maximum = content.Headers.ContentLength ?? totalBytesRead;
                            progress?.Invoke(totalBytesRead / maximum * 100);
                        }
                    }
                }
                return fileName;
            });
        }

        public static async Task<HGame> GetLatestGameAsync(HHotel hotel)
        {
            string latestRevision = await GetLatestRevisionAsync(hotel).ConfigureAwait(false);
            return await GetGameAsync(latestRevision).ConfigureAwait(false);
        }
        public static async Task DownloadLatestGameAsync(HHotel hotel, string fileName, Action<double> progress = null)
        {
            string latestRevision = await GetLatestRevisionAsync(hotel).ConfigureAwait(false);
            await DownloadGameAsync(latestRevision, fileName, progress).ConfigureAwait(false);
        }

        public static async Task<T> ReadContentAsync<T>(Uri baseUri, string path, Func<HttpContent, Task<T>> contentConverter = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, baseUri.GetLeftPart(UriPartial.Authority) + path);
            using HttpResponseMessage response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            
            ServicePointManager.FindServicePoint(request.RequestUri).ConnectionLeaseTimeout = 30 * 1000;
            
            if (!response.IsSuccessStatusCode) return default;
            if (contentConverter == null)
            {
                Type genericType = typeof(T);

                if (genericType == typeof(string))
                    return (T)(object)await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (genericType == typeof(byte[]))
                    return (T)(object)await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

                if (response.Content.Headers.ContentType.MediaType == "application/json")
                    return await JsonSerializer.DeserializeAsync<T>(await response.Content.ReadAsStreamAsync().ConfigureAwait(false), _serializerOptions).ConfigureAwait(false);
                
                return default;
            }
            return await contentConverter(response.Content).ConfigureAwait(false);
        }
    }
}