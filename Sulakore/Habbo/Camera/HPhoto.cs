using System;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sulakore.Habbo.Camera
{
    public class HPhoto
    {
        private static readonly JsonSerializerOptions _serializerOptions;

        public IList<Plane> Planes { get; set; } = new List<Plane>();
        public IList<Sprite> Sprites { get; set; } = new List<Sprite>();
        public Modifiers Modifiers { get; set; } = new Modifiers();
        public IList<Filter> Filters { get; set; } = new List<Filter>();

        [JsonPropertyName("roomid")]
        public int RoomId { get; set; }
        public int? Zoom { get; set; }

        static HPhoto()
        {
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreReadOnlyProperties = true,
                IgnoreNullValues = true
            };
        }

        private long GetStatus(ref long mod, ref long timestamp)
        {
            timestamp -= (mod = timestamp % 100);
            return timestamp / 100 % 23;
        }
        private long GetChecksum(long mod, long key)
        {
            return (mod + 13) * (key + 29);
        }
        private long GetTimestamp(string blob, long timestamp, long key)
        {
            byte[] data = Encoding.Default.GetBytes(blob);
            return timestamp + Calculate(data, key, RoomId);
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

        /// <summary>
        /// Serializes the photo data into a valid JSON representation and returns it.
        /// </summary>
        public override string ToString()
        {
            string json = JsonSerializer.Serialize(this, _serializerOptions)[..^1];

            long timestamp = (long)(
                DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds;

            long mod = 0;
            json += ",\"status\":" + GetStatus(ref mod, ref timestamp);

            long key = (json.Length + timestamp / 100 * 17) % 1493;

            return $"{json},\"timestamp\":{GetTimestamp(json, timestamp, key)},\"checksum\":{GetChecksum(mod, key)}}}";
        }

        public static HPhoto Create(byte[] photoJsonData)
            => JsonSerializer.Deserialize<HPhoto>(photoJsonData, _serializerOptions);

        public static HPhoto Create(string photoJson)
            => JsonSerializer.Deserialize<HPhoto>(photoJson, _serializerOptions);
    }
}