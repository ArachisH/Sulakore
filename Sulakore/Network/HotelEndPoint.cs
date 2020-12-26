using System;
using System.Net;
using System.Threading.Tasks;

using Sulakore.Habbo;

namespace Sulakore.Network
{
    public class HotelEndPoint : IPEndPoint
    {
        public string Host { get; init; }
        public HHotel Hotel { get; init; }

        public HotelEndPoint(IPEndPoint endPoint)
            : base(endPoint.Address, endPoint.Port)
        { }
        public HotelEndPoint(long address, int port)
            : base(address, port)
        { }
        public HotelEndPoint(IPAddress address, int port)
            : base(address, port)
        { }
        public HotelEndPoint(IPAddress address, int port, string host)
            : base(address, port)
        {
            Host = host;
        }

        public static HHotel GetHotel(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length < 2) return HHotel.Unknown;
            if (value.StartsWith("hh")) value = value[2..4];
            if (value.StartsWith("game-")) value = value[5..7];

            switch (value)
            {
                case "us": return HHotel.Com;
                case "br": return HHotel.ComBr;
                case "tr": return HHotel.ComTr;
                default:
                {
                    if (value.Length != 2 && value.Length != 5)
                    {
                        int hostIndex = value.LastIndexOf("habbo");
                        if (hostIndex != -1)
                        {
                            value = value[(hostIndex + 5)..];
                        }

                        int comDotIndex = value.IndexOf("com.");
                        if (comDotIndex != -1)
                        {
                            value = value.Remove(comDotIndex + 3, 1);
                        }

                        if (value[0] == '.') value = value[1..];
                        if (value.Length != 3)
                        {
                            value = value.Substring(0, value.StartsWith("com") ? 5 : 2);
                        }
                    }
                    if (Enum.TryParse(value, true, out HHotel hotel) && Enum.IsDefined(typeof(HHotel), hotel)) return hotel;
                    break;
                }
            }
            return HHotel.Unknown;
        }
        public static string GetRegion(HHotel hotel) => hotel switch
        {
            HHotel.Com => "us",
            HHotel.ComBr => "br",
            HHotel.ComTr => "tr",
            _ => hotel.ToString().ToLower(),
        };

        public static HotelEndPoint Create(string host) => Create(GetHotel(host));
        public static HotelEndPoint Create(HHotel hotel) => hotel != HHotel.Unknown ? Parse($"game-{GetRegion(hotel)}.habbo.com", 30001) : null;

        public static HotelEndPoint Parse(string host, int port)
        {
            return ParseAsync(host, port).Result;
        }
        public static async Task<HotelEndPoint> ParseAsync(string host, int port)
        {
            IPAddress[] ips = await Dns.GetHostAddressesAsync(host).ConfigureAwait(false);
            return new HotelEndPoint(ips[0], port, host) { Host = host, Hotel = GetHotel(host) };
        }

        public override string ToString()
        {
            return $"{(string.IsNullOrWhiteSpace(Host) ? Address.ToString() : Host)}:{Port}";
        }
    }
}