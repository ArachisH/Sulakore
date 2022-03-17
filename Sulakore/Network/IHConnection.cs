using Sulakore.Network.Buffers;
using Sulakore.Network.Formats;

namespace Sulakore.Network;

public interface IHConnection
{
    HNode Local { get; }
    HNode Remote { get; }

    IHFormat SendFormat { get; }
    IHFormat ReceiveFormat { get; }

    Task SendToClientAsync(HPacket packet);
    Task SendToServerAsync(HPacket packet);
}