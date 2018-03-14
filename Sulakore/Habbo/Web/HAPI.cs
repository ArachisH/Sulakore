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

        public static Task<HProfile> GetProfileAsync(string uniqueId) =>
            GetContentAsync<HProfile>(HotelEndPoint.GetHotel(uniqueId), $"/api/public/users/{uniqueId}/profile");

        public static Task<HUser> GetUserAsync(string name, HHotel hotel) =>
            GetContentAsync<HUser>(hotel, ("/api/public/users?name=" + name));

        public static Task<byte[]> GetFigureDataAsync(string figureId, char size) =>
            GetContentAsync<byte[]>(HHotel.Com, $"/habbo-imaging/avatarimage?size={size}&figure={figureId}");

        private static async Task<T> GetContentAsync<T>(HHotel hotel, string path)
        {
            var address = new Uri($"https://www.habbo.{hotel.ToDomain()}{path}");
            using (var request = new HttpRequestMessage(HttpMethod.Get, address))
            using (HttpResponseMessage response = await _client.SendAsync(request).ConfigureAwait(false))
            {
                ServicePointManager.FindServicePoint(address).ConnectionLeaseTimeout = (30 * 1000);
                if (!response.IsSuccessStatusCode) return default(T);

                object content = null;
                Type genericType = typeof(T);
                switch (Type.GetTypeCode(genericType))
                {
                    case TypeCode.String:
                    content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    break;

                    case TypeCode.Object:
                    if (genericType == typeof(byte[]))
                    {
                        content = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    }
                    else if (genericType == typeof(HUser))
                    {
                        content = HUser.Create(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    }
                    else if (genericType == typeof(HProfile))
                    {
                        content = HProfile.Create(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    }
                    break;
                }
                return (T)content;
            }
        }
    }
}