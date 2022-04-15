using Sulakore.Habbo;

namespace Sulakore.Network;

public interface IHConnection
{
    Incoming? In { get; }
    Outgoing? Out { get; }
    HotelEndPoint ServerEndPoint { get; }

    ValueTask SendToClientAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default);
    ValueTask SendToServerAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default);
}