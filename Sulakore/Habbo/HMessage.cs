using System.Diagnostics;

namespace Sulakore.Habbo
{
    [DebuggerDisplay("{Id,nq}")]
    public record HMessage(ushort Id, string Name, bool IsOutgoing)
    {
        public static implicit operator ushort(HMessage message) => message?.Id ?? ushort.MaxValue;
    }
}