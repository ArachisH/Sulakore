using System.Collections.Generic;

namespace Sulakore.Network.Protocol
{
    public class EvaWirePacket : HPacket
    {
        public EvaWirePacket()
            : base(HFormat.EvaWire)
        { }
        public EvaWirePacket(IList<byte> data)
            : base(HFormat.EvaWire, data)
        { }
        public EvaWirePacket(ushort id, params object[] values)
            : this(Construct(id, values))
        { }

        public static byte[] ToBytes(string signature)
        {
            return ToBytes(HFormat.EvaWire, signature);
        }
        public static byte[] Construct(ushort id, params object[] values)
        {
            return HFormat.EvaWire.Construct(id, values);
        }
    }
}