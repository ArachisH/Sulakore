using Sulakore.Network.Buffers;
using Sulakore.Network.Formats;

namespace Sulakore.Network;

public interface IHConnection
{
    HNode Local { get; }
    HNode Remote { get; }

    HFormat SendFormat { get; }
    HFormat ReceiveFormat { get; }

    Task SendToClientAsync(HPacket packet);
    Task SendToServerAsync(HPacket packet);
}