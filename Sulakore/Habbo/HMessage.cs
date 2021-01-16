using System.Diagnostics;

namespace Sulakore.Habbo
{
    [DebuggerDisplay("{Id,nq}")]
    public record HMessage(short Id, string Name, bool IsOutgoing)
    {
        public static implicit operator short(HMessage message) => message?.Id ?? -1;
    }
}