using System.Globalization;

using Sulakore.Network.Protocol;

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

    public HSlideObjectBundle(ref HReadOnlyPacket packet)
    {
        var location = new HPoint(packet.Read<int>(), packet.Read<int>());
        var target = new HPoint(packet.Read<int>(), packet.Read<int>());

        Objects = new HSlideObject[packet.Read<int>()];
        for (int i = 0; i < Objects.Length; i++)
        {
            int objectId = packet.Read<int>();
            location.Z = double.Parse(packet.Read<string>(), CultureInfo.InvariantCulture);
            target.Z = double.Parse(packet.Read<string>(), CultureInfo.InvariantCulture);

            Objects[i] = new HSlideObject(objectId, location, target);
        }
        Id = packet.Read<int>();

        if (packet.Available > 0)
        {
            var type = (HMoveType)packet.Read<int>();

            int entityIndex = packet.Read<int>();
            location.Z = double.Parse(packet.Read<string>(), CultureInfo.InvariantCulture);
            target.Z = double.Parse(packet.Read<string>(), CultureInfo.InvariantCulture);

            Entity = new HSlideObject(entityIndex, location, target, type);
        }
    }
}