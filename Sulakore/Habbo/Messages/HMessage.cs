using System;
using System.Diagnostics;

namespace Sulakore.Habbo.Messages
{
    [DebuggerDisplay("{Id,nq}")]
    public class HMessage : IEquatable<HMessage>
    {
        public ushort Id { get; set; }
        public bool IsOutgoing { get; set; }

        public string Hash { get; set; }
        public string Name { get; set; }
        public string Structure { get; set; }

        public static implicit operator ushort(HMessage message) => message?.Id ?? ushort.MaxValue;

        public HMessage()
            : this(ushort.MaxValue, false, null, null, null)
        { }
        public HMessage(ushort id, bool isOutgoing)
        {
            Id = id;
            IsOutgoing = isOutgoing;
        }
        public HMessage(ushort id, bool isOutgoing, string hash, string name, string structure)
        {
            Id = id;
            Hash = hash;
            Name = name;
            Structure = structure;
            IsOutgoing = isOutgoing;
        }

        public override string ToString() => Id.ToString();
        public bool Equals(HMessage other) => Id == other.Id;
    }
}