using System.Collections.Generic;

namespace Sulakore.Network.Protocol
{
    public class WedgiePacket : HPacket
    {
        public WedgiePacket(bool isOutgoing)
            : base(GetFormat(isOutgoing))
        { }
        public WedgiePacket(bool isOutgoing, IList<byte> data)
            : base(GetFormat(isOutgoing), data)
        { }
        public WedgiePacket(bool isOutgoing, ushort id, params object[] values)
            : this(isOutgoing, Construct(isOutgoing, id, values))
        { }

        private static HFormat GetFormat(bool isOutgoing)
        {
            return isOutgoing ? HFormat.WedgieOut : HFormat.WedgieIn;
        }
        public static byte[] Construct(bool isOutgoing, ushort id, params object[] values)
        {
            return GetFormat(isOutgoing).Construct(id, values);
        }
    }
}