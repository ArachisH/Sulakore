using System.Text;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sulakore.Habbo.Camera;

public sealed class HPhoto
{
    public IList<Plane> Planes { get; set; } = new List<Plane>();
    public IList<Sprite> Sprites { get; set; } = new List<Sprite>();
    public Modifiers Modifiers { get; set; } = new Modifiers();
    public IList<Filter> Filters { get; set; } = new List<Filter>();

    [JsonPropertyName("roomid")]
    public uint RoomId { get; set; }
    public int? Zoom { get; set; }

    /// <summary>
    /// Serializes the photo into a valid JSON representation and returns it.
    /// </summary>
    public override string ToString()
    {
        var bufferWriter = new ArrayBufferWriter<byte>(256);
        using var writer = new Utf8JsonWriter(bufferWriter);

        writer.WriteStartObject();
        
        writer.WritePropertyName("planes"u8);
        JsonSerializer.Serialize(writer, Planes, HPhotoJsonContext.Default.IListPlane);
        
        writer.WritePropertyName("sprites"u8);
        JsonSerializer.Serialize(writer, Sprites, HPhotoJsonContext.Default.IListSprite);
        
        writer.WritePropertyName("modifiers"u8);
        JsonSerializer.Serialize(writer, Modifiers, HPhotoJsonContext.Default.Modifiers);

        writer.WritePropertyName("filters"u8);
        JsonSerializer.Serialize(writer, Filters, HPhotoJsonContext.Default.IListFilter);
        
        writer.WriteNumber("roomid"u8, RoomId);
        if (Zoom is not null) writer.WriteNumber("zoom"u8, Zoom.GetValueOrDefault());

        ulong timestamp = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        (ulong quotinent, ulong remainder) = Math.DivRem(timestamp, 100);

        writer.WriteNumber("status"u8, quotinent % 23);

        // Flush the writer to get entire buffer for checksum calculation
        writer.Flush();
        
        uint key = ((uint)bufferWriter.WrittenCount + (uint)quotinent * 17) % 1493;
        
        timestamp += Fletcher100(bufferWriter.WrittenSpan, key, RoomId) * 100;
        writer.WriteNumber("timestamp"u8, timestamp);

        writer.WriteNumber("checksum"u8, (remainder + 13) * (key + 29));
        
        writer.WriteEndObject();
        writer.Flush();

        return Encoding.UTF8.GetString(bufferWriter.WrittenSpan);
    }

    /// <summary>
    /// Calculates slightly modified version of the position-dependent Fletcher-16 checksum.
    /// </summary>
    private static uint Fletcher100(ReadOnlySpan<byte> data, uint a, uint b)
    {
        for (int i = 0; i < data.Length; i++)
        {
            a = (a + data[i]) % 255;
            b = (a + b) % 255;
        }
        return (a + b) % 100;
    }

    /// <summary>
    /// Deserializes a photo object from its JSON representation.
    /// </summary>
    public static HPhoto? Create(string json)
        => JsonSerializer.Deserialize(json, HPhotoJsonContext.Default.HPhoto);
    /// <inheritdoc cref="Create(string)"/>
    public static HPhoto? Create(ReadOnlySpan<byte> utf8Json)
        => JsonSerializer.Deserialize(utf8Json, HPhotoJsonContext.Default.HPhoto);
}