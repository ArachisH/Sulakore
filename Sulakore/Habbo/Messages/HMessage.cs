using System;
using System.Diagnostics;

namespace Sulakore.Habbo.Messages
{
    [DebuggerDisplay("{Id,nq}")]
    public class HMessage : IEquatable<HMessage>
    {
        public string Hash { get; }
        public string Name { get; }
        public bool IsUnity { get; }
        public bool IsOutgoing { get; }

        public ushort Id { get; set; }
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

        public override string ToString() => Name;
        public override int GetHashCode() => HashCode.Combine(IsUnity, IsOutgoing, Id);

        public override bool Equals(object obj) => obj is HMessage message && Equals(message);
        public bool Equals(HMessage other) => IsUnity == other.IsUnity && IsOutgoing == other.IsOutgoing && Id == other.Id;
    }
}