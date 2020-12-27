using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages
{
    public class HUserSearchResult
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Motto { get; set; }
        
        public bool IsOnline { get; set; }
        public bool CanFollow { get; set; }
        
        public HGender Gender { get; set; }
        public string Figure { get; set; }

        public string RealName { get; set; }

        public HUserSearchResult(HPacket packet)
        {
            Id = packet.ReadInt32();
            Name = packet.ReadUTF8();
            Motto = packet.ReadUTF8();

            IsOnline = packet.ReadBoolean();
            CanFollow = packet.ReadBoolean();

            packet.ReadUTF8();

            Gender = packet.ReadInt32() == 1 ? HGender.Male : HGender.Female; //TODO: HExtension, ffs sulake
            Figure = packet.ReadUTF8();
            
            RealName = packet.ReadUTF8();
        }

        public static (HUserSearchResult[] friends, HUserSearchResult[] others) Parse(HPacket packet)
        {
            var friends = new HUserSearchResult[packet.ReadInt32()];
            for (int i = 0; i < friends.Length; i++)
            {
                friends[i] = new HUserSearchResult(packet);
            }

            var others = new HUserSearchResult[packet.ReadInt32()];
            for (int i = 0; i < others.Length; i++)
            {
                others[i] = new HUserSearchResult(packet);
            }
            return (friends, others);
        }
    }
}
