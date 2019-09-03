using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

using Sulakore.Habbo.Layers;

namespace Sulakore.Habbo
{
    [DataContract]
    public class HPhoto
    {
        private long _timestamp;

        private static readonly DateTime _unixEpoch;
        private static readonly DataContractJsonSerializer _serializer;

        private List<Plane> _planes;
        [DataMember(Name = "planes", Order = 0)]
        public List<Plane> Planes
        {
            get => _planes ?? (_planes = new List<Plane>());
        }

        private List<Sprite> _sprites;
        [DataMember(Name = "sprites", Order = 1)]
        public List<Sprite> Sprites
        {
            get => _sprites ?? (_sprites = new List<Sprite>());
        }

        private Modifiers _modifiers;
        [DataMember(Name = "modifiers", Order = 2)]
        public Modifiers Modifiers
        {
            get => _modifiers ?? (_modifiers = new Modifiers());
            set { _modifiers = value; }
        }

        private List<Filter> _filters;
        [DataMember(Name = "filters", Order = 3)]
        public List<Filter> Filters
        {
            get => _filters ?? (_filters = new List<Filter>());
        }

        [DataMember(Name = "roomid", Order = 4)]
        public int RoomId { get; set; }

        [DataMember(Name = "zoom", Order = 5)]
        public int Zoom { get; set; }

        static HPhoto()
        {
            _serializer = new DataContractJsonSerializer(typeof(HPhoto));
            _unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        private long GetStatus(ref long mod)
        {
            _timestamp -= (mod = _timestamp % 100);
            return _timestamp / 100 % 23;
        }
        private long GetChecksum(long mod, long key)
        {
            return (mod + 13) * (key + 29);
        }
        private long GetTimestamp(string blob, long key)
        {
            byte[] data = Encoding.Default.GetBytes(blob);
            return _timestamp + Calculate(data, key, RoomId);
        }
        private long Calculate(byte[] data, long key, int roomId)
        {
            long tKey = key, tRoomId = roomId;
            for (int i = 0; i < data.Length; i++)
            {
                tKey = (tKey + data[i]) % 255;
                tRoomId = (tKey + tRoomId) % 255;
            }
            return (tKey + tRoomId) % 100;
        }

        public override string ToString()
        {
            string json = string.Empty;
            using (var jsonStream = new MemoryStream())
            {
                _serializer.WriteObject(jsonStream, this);

                json = Encoding.UTF8.GetString(jsonStream.ToArray());
                json = json.Remove(json.Length - 1, 1);
            }

            _timestamp = (long)(
                DateTime.UtcNow - _unixEpoch).TotalMilliseconds;

            long mod = 0;
            json += ",\"status\":" + GetStatus(ref mod);

            long key = json.Length;
            key = (key + _timestamp / 100 * 17) % 1493;

            return $"{json},\"timestamp\":{GetTimestamp(json, key)},\"checksum\":{GetChecksum(mod, key)}}}";
        }

        public static HPhoto Create(string photoJson)
        {
            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(photoJson)))
            {
                return (HPhoto)_serializer.ReadObject(memoryStream);
            }
        }
    }
}