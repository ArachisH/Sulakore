using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Sulakore.Habbo.Messages
{
    [DebuggerDisplay("{ToString(),nq}")]
    public class HMessage : IEquatable<HMessage>
    {
        [DataMember]
        public ushort Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Hash { get; set; }

        [DataMember]
        public string Structure { get; set; }

        public HMessage()
            : this(ushort.MaxValue, null, null, null)
        { }
        public HMessage(ushort id)
            : this(id, null, null, null)
        { }
        public HMessage(ushort id, string hash)
            : this(id, hash, null, null)
        { }
        public HMessage(ushort id, string hash, string name)
            : this(id, hash, name, null)
        { }
        public HMessage(ushort id, string hash, string name, string structure)
        {
            Id = id;
            Hash = hash;
            Name = name;
            Structure = structure;
        }

        public static implicit operator ushort(HMessage message) => message.Id;
        //public static implicit operator HMessage(ushort id) => new HMessage(id);
        //public static implicit operator HMessage(int id) => new HMessage((ushort)id);

        public override string ToString() => Id.ToString();
        public bool Equals(HMessage other) => Id == other.Id;
    }
}