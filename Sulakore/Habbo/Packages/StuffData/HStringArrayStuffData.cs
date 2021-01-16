using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages.StuffData
{
    public class HStringArrayStuffData : HStuffData
    {
        public string[] Data { get; set; }

        public HStringArrayStuffData()
            : base(HStuffDataFormat.StringArray)
        { }
        public HStringArrayStuffData(HReadOnlyPacket packet)
            : this()
        {
            Data = new string[packet.ReadInt32()];
            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] = packet.ReadString();
            }
        }
    }
}
