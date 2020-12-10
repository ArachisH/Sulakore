using System.Threading.Tasks;

using Sulakore.Network.Protocol;

namespace Sulakore.Network
{
    public interface IHConnection
    {
        HNode Local { get; }
        HNode Remote { get; }

        ValueTask<int> SendToServerAsync(byte[] data);
        ValueTask<int> SendToServerAsync(HPacket packet);
        ValueTask<int> SendToServerAsync(ushort id, params object[] values);

        ValueTask<int> SendToClientAsync(byte[] data);
        ValueTask<int> SendToClientAsync(HPacket packet);
        ValueTask<int> SendToClientAsync(ushort id, params object[] values);
    }
}