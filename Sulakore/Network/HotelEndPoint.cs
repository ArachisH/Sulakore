using System;
using System.Net;

using Sulakore.Habbo;

namespace Sulakore.Network
{
    public class HotelEndPoint : IPEndPoint
    {
        private string _host;
        public string Host
        {
            get => _host;
            set
            {
                _host = value;
                Hotel = GetHotel(value);
            }
        }

        public HHotel Hotel { get; private set; }

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
                            value = value.Substring(hostIndex + 5);
                        }

                        int comDotIndex = value.IndexOf("com.");
                        if (comDotIndex != -1)
                        {
                            value = value.Remove(comDotIndex + 3, 1);
                        }

                        if (value[0] == '.') value = value.Substring(1);
                        value = value.Substring(0, value.StartsWith("com") ? 5 : 2);
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
        public static HotelEndPoint GetEndPoint(HHotel hotel)
        {
            int port = hotel == HHotel.Com ? 38101 : 30000;
            string host = $"game-{GetRegion(hotel)}.habbo.com";
            return Parse(host, port);
        }

        public static HotelEndPoint Parse(string host, int port)
        {
            IPAddress[] ips = Dns.GetHostAddresses(host);
            return new HotelEndPoint(ips[0], port, host);
        }
        public static bool TryParse(string host, int port, out HotelEndPoint endPoint)
        {
            try
            {
                endPoint = Parse(host, port);
                return true;
            }
            catch
            {
                endPoint = null;
                return false;
            }
        }

        public override string ToString()
        {
            return $"{(string.IsNullOrWhiteSpace(Host) ? Address.ToString() : Host)}:{Port}";
        }
    }
}