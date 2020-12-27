using System.Globalization;

using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages
{
#nullable enable
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
        public HSlideObject? Entity { get; set; }

        public HSlideObjectBundle(HPacket packet)
        {
            HPoint location = new HPoint(packet.ReadInt32(), packet.ReadInt32());
            HPoint target = new HPoint(packet.ReadInt32(), packet.ReadInt32());

            Objects = new HSlideObject[packet.ReadInt32()];
            for (int i = 0; i < Objects.Length; i++)
            {
                int objectId = packet.ReadInt32();
                location.Z = double.Parse(packet.ReadUTF8(), CultureInfo.InvariantCulture);
                target.Z = double.Parse(packet.ReadUTF8(), CultureInfo.InvariantCulture);

                Objects[i] = new HSlideObject(objectId, location, target);
            }

            Id = packet.ReadInt32();

            if (packet.ReadableBytes > 0)
            {
                HMoveType type = (HMoveType)packet.ReadInt32();

                int entityIndex = packet.ReadInt32();
                location.Z = double.Parse(packet.ReadUTF8(), CultureInfo.InvariantCulture);
                target.Z = double.Parse(packet.ReadUTF8(), CultureInfo.InvariantCulture);

                Entity = new HSlideObject(entityIndex, location, target, type);
            }
        }
    }
}
