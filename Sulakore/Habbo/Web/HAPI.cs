using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Sulakore.Network;

namespace Sulakore.Habbo.Web
{
    public static class HAPI
    {
        private static readonly HttpClient _client;
        private static readonly HttpClientHandler _handler;

        public const string CHROME_USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 Safari/537.36";

        static HAPI()
        {
            _handler = new HttpClientHandler
            {
                UseProxy = false,
                UseCookies = false,
                AutomaticDecompression = (DecompressionMethods.GZip | DecompressionMethods.Deflate)
            };

            _client = new HttpClient(_handler);
            _client.DefaultRequestHeaders.ConnectionClose = true;
            _client.DefaultRequestHeaders.Add("User-Agent", CHROME_USER_AGENT);
            _client.DefaultRequestHeaders.Add("Cookie", "YPF8827340282Jdskjhfiw_928937459182JAX666=127.0.0.1");
        }

        public static async Task<string> GetLatestRevisionAsync(HHotel hotel)
        {
            string body = await ReadContentAsync<string>(hotel.ToUri(), "/gamedata/external_variables/1").ConfigureAwait(false);
            int revisionStartIndex = (body.LastIndexOf("/gordon/") + 8);
            if (revisionStartIndex != 7)
            {
                int revisionEndIndex = body.IndexOf('/', revisionStartIndex);
                if (revisionEndIndex != -1)
                {
                    return body.Substring(revisionStartIndex, revisionEndIndex - revisionStartIndex);
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
        public static Task<string> DownloadGameAsync(string revision, string directoryName)
        {
            return ReadContentAsync(HHotel.Com.ToUri("images"), $"/gordon/{revision}/Habbo.swf", async content =>
            {
                string gamePath = Path.Combine(directoryName, "gordon", revision, "Habbo.swf");
                Directory.CreateDirectory(Path.GetDirectoryName(gamePath));

                using (Stream contentStream = await content.ReadAsStreamAsync().ConfigureAwait(false))
                using (var fileStream = File.Create(gamePath))
                {
                    await contentStream.CopyToAsync(fileStream).ConfigureAwait(false);
                }
                return gamePath;
            });
        }

        public static async Task<HGame> GetLatestGameAsync(HHotel hotel)
        {
            string latestRevision = await GetLatestRevisionAsync(hotel).ConfigureAwait(false);
            return await GetGameAsync(latestRevision).ConfigureAwait(false);
        }
        public static async Task<string> DownloadLatestGameAsync(HHotel hotel, string path)
        {
            string latestRevision = await GetLatestRevisionAsync(hotel).ConfigureAwait(false);
            return await DownloadGameAsync(latestRevision, path).ConfigureAwait(false);
        }

        public static Task<byte[]> GetFigureDataAsync(string query) =>
            ReadContentAsync<byte[]>(HHotel.Com.ToUri(), ("/habbo-imaging/avatarimage?" + query));

        public static async Task<HUser> GetUserAsync(string name, HHotel hotel) =>
            HUser.Create(await ReadContentAsync<string>(hotel.ToUri(), ("/api/public/users?name=" + name)));

        public static async Task<HProfile> GetProfileAsync(string uniqueId) =>
            HProfile.Create(await ReadContentAsync<string>(HotelEndPoint.GetHotel(uniqueId).ToUri(), $"/api/public/users/{uniqueId}/profile"));

        public static async Task<T> ReadContentAsync<T>(Uri baseUri, string path, Func<HttpContent, Task<T>> contentConverter = null)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUri.GetLeftPart(UriPartial.Authority)}{path}"))
            using (HttpResponseMessage response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
            {
                ServicePointManager.FindServicePoint(request.RequestUri).ConnectionLeaseTimeout = (30 * 1000);
                if (!response.IsSuccessStatusCode) return default(T);
                if (contentConverter == null)
                {
                    Type genericType = typeof(T);
                    if (genericType == typeof(string))
                    {
                        return (T)(object)await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                    else if (genericType == typeof(byte[]))
                    {
                        return (T)(object)await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    }
                    else return default(T);
                }
                return await contentConverter(response.Content).ConfigureAwait(false);
            }
        }
    }
}