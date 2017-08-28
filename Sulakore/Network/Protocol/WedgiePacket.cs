using System.Collections.Generic;

namespace Sulakore.Network.Protocol
{
    public class WedgiePacket : HPacket
    {
        public WedgiePacket(bool isOutgoing)
            : base(GetResolver(isOutgoing))
        { }
        public WedgiePacket(bool isOutgoing, IList<byte> data)
            : base(GetResolver(isOutgoing), data)
        { }
        public WedgiePacket(bool isOutgoing, ushort id, params object[] values)
            : this(isOutgoing, Construct(isOutgoing, id, values))
        { }

        private static HFormat GetResolver(bool isOutgoing)
        {
            return (isOutgoing ? HFormat.WedgieOut : HFormat.WedgieIn);
        }
        public static byte[] Construct(bool isOutgoing, ushort id, params object[] values)
        {
            return GetResolver(isOutgoing).Construct(id, values);
        }
    }
}