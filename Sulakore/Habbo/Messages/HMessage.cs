using System;
using System.Diagnostics;

namespace Sulakore.Habbo.Messages
{
    [DebuggerDisplay("{Id,nq}")]
    public class HMessage : IEquatable<HMessage>
    {
        public ushort Id { get; }
        public string Name { get; }
        public bool IsOutgoing { get; }

        public static implicit operator ushort(HMessage message) => message?.Id ?? ushort.MaxValue;

        public HMessage(string name, bool isOutgoing, ushort id)
        {
            Id = id;
            Name = name;
            IsOutgoing = isOutgoing;
        }

        public override string ToString() => Name;
        public override int GetHashCode() => HashCode.Combine(IsOutgoing, Id);

        public override bool Equals(object obj) => obj is HMessage message && Equals(message);
        public bool Equals(HMessage other) => IsOutgoing == other.IsOutgoing && Id == other.Id;
    }
}