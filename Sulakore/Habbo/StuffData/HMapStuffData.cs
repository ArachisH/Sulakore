using System.Collections.Generic;

using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.StuffData
{
    public class HMapStuffData : HStuffData
    {
        public Dictionary<string, string> Data { get; set; }

        public HMapStuffData()
            : base(HStuffDataFormat.Map)
        { }
        public HMapStuffData(HPacket packet)
            : this()
        {
            int length = packet.ReadInt32();
            Data = new(length);

            for (int i = 0; i < length; i++)
            {
                Data[packet.ReadUTF8()] = packet.ReadUTF8();
            }
        }
    }
}
