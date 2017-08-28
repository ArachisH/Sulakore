using System.Threading.Tasks;

using Sulakore.Network.Protocol;

namespace Sulakore.Network
{
    public interface IHConnection
    {
        HNode Local { get; }
        HNode Remote { get; }

        Task<int> SendToServerAsync(byte[] data);
        Task<int> SendToServerAsync(HPacket packet);
        Task<int> SendToServerAsync(ushort id, params object[] values);

        Task<int> SendToClientAsync(byte[] data);
        Task<int> SendToClientAsync(HPacket packet);
        Task<int> SendToClientAsync(ushort id, params object[] values);
    }
}