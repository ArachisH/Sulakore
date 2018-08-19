using System.Collections.Generic;

using Sulakore.Network.Protocol;

namespace Sulakore.Habbo
{
    public abstract class HData : IHabboData
    {
        private readonly Dictionary<int, object> _remnantQueue;

        protected HFormat Format { get; }
        protected ushort PacketId { get; }
        protected Queue<object> Remnants { get; }

        public HData(HPacket packet)
        {
            _remnantQueue = new Dictionary<int, object>();

            PacketId = packet.Id;
            Format = packet.Format;
            Remnants = new Queue<object>();
        }

        protected void Queue
        protected object[] ReadData(HPacket packet, int category)
        {
            var values = new List<object>();
            switch (category & 0xFF)
            {
                case 0: /* LegacyStuffData */
                {
                    values.Add(packet.ReadUTF8());
                    break;
                }
                case 1: /* MapStuffData */
                {
                    int count = packet.ReadInt32();
                    values.Add(count);

                    for (int j = 0; j < count; j++)
                    {
                        values.Add(packet.ReadUTF8());
                        values.Add(packet.ReadUTF8());
                    }
                    break;
                }
                case 2: /* StringArrayStuffData */
                {
                    int count = packet.ReadInt32();
                    values.Add(count);

                    for (int j = 0; j < count; j++)
                    {
                        values.Add(packet.ReadUTF8());
                    }
                    break;
                }
                case 3: /* VoteResultStuffData */
                    {
                    values.Add(packet.ReadUTF8());
                    values.Add(packet.ReadInt32());
                    break;
                }
                case 5: /* IntArrayStuffData */
                {
                    int count = packet.ReadInt32();
                    values.Add(count);

                    for (int j = 0; j < count; j++)
                    {
                        values.Add(packet.ReadInt32());
                    }
                    break;
                }
                case 6: /* HighScoreStuffData */
                {
                    values.Add(packet.ReadUTF8());
                    values.Add(packet.ReadInt32());
                    values.Add(packet.ReadInt32());

                    int count = packet.ReadInt32();
                    values.Add(count);

                    for (int j = 0; j < count; j++)
                    {
                        int score = packet.ReadInt32();
                        values.Add(score);

                        int subCount = packet.ReadInt32();
                        values.Add(subCount);

                        for (int k = 0; k < subCount; k++)
                        {
                            values.Add(packet.ReadUTF8());
                        }
                    }
                    break;
                }
                case 7: /* CrackableStuffData */
                {
                    values.Add(packet.ReadUTF8());
                    values.Add(packet.ReadInt32());
                    values.Add(packet.ReadInt32());
                    break;
                }
            }
            if (((category & 0xFF00) & 0x100) > 0)
            {
                values.Add(packet.ReadInt32());
                values.Add(packet.ReadInt32());
            }
            return values.ToArray();
        }

        public byte[] ToBytes()
        {
            return ToPacket().ToBytes();
        }
        public virtual HPacket ToPacket()
        {
            HPacket packet = Format.CreatePacket(PacketId);
            WriteTo(packet);
            return packet;
        }
        public abstract void WriteTo(HPacket packet);
    }
}