namespace Sulakore.Network;

public interface IHConnection
{
    HNode Local { get; }
    HNode Remote { get; }

    ValueTask<int> SendToServerAsync(ReadOnlyMemory<byte> data);
    ValueTask<int> SendToServerAsync(short id, params object[] values);

    ValueTask<int> SendToClientAsync(ReadOnlyMemory<byte> data);
    ValueTask<int> SendToClientAsync(short id, params object[] values);
}