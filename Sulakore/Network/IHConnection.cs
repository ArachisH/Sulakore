namespace Sulakore.Network;

public interface IHConnection
{
    Habbo.Incoming? In { get; }
    Habbo.Outgoing? Out { get; }
    HotelEndPoint RemoteEndPoint { get; }

    ValueTask SendToClientAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default);
    ValueTask SendToServerAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default);
}