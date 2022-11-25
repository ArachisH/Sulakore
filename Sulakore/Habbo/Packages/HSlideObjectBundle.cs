using System.Globalization;

using Sulakore.Network.Formats;

namespace Sulakore.Habbo.Packages;

public class HSlideObjectBundle
{
    /// <summary>
    /// The room object's identifier which causes the item(s) movement.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The moving object bundle.
    /// </summary>
    public HSlideObject[] Objects { get; set; }

    /// <summary>
    /// Represents possible entity sliding on the object.
    /// </summary>
    public HSlideObject Entity { get; set; }

    public HSlideObjectBundle(IHFormat format, ref ReadOnlySpan<byte> packetSpan)
    {
        int locationX = format.Read<int>(ref packetSpan);
        int locationY = format.Read<int>(ref packetSpan);

        int targetX = format.Read<int>(ref packetSpan);
        int targetY = format.Read<int>(ref packetSpan);
        HPoint location, target;

        Objects = new HSlideObject[format.Read<int>(ref packetSpan)];
        for (int i = 0; i < Objects.Length; i++)
        {
            int objectId = format.Read<int>(ref packetSpan);

            location = new HPoint(locationX, locationY, float.Parse(format.ReadUTF8(ref packetSpan), CultureInfo.InvariantCulture));
            target = new HPoint(targetX, targetY, float.Parse(format.ReadUTF8(ref packetSpan), CultureInfo.InvariantCulture));

            Objects[i] = new HSlideObject(objectId, location, target);
        }

        Id = format.Read<int>(ref packetSpan);
        if (!packetSpan.IsEmpty)
        {
            var type = (HMoveType)format.Read<int>(ref packetSpan);

            int entityIndex = format.Read<int>(ref packetSpan);
            location = new HPoint(locationX, locationY, float.Parse(format.ReadUTF8(ref packetSpan), CultureInfo.InvariantCulture));
            target = new HPoint(targetX, targetY, float.Parse(format.ReadUTF8(ref packetSpan), CultureInfo.InvariantCulture));

            Entity = new HSlideObject(entityIndex, location, target, type);
        }
    }
}