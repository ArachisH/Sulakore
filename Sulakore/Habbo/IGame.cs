using Sulakore.Network.Formats;

namespace Sulakore.Habbo;

public interface IGame
{
    bool IsPostShuffle { get; }
    HPlatform Platform { get; }

    IHFormat SendPacketFormat { get; }
    IHFormat ReceivePacketFormat { get; }

    string? Path { get; }
    string? Revision { get; }
    int MinimumConnectionAttempts { get; }

    bool TryResolveMessage(string name, uint hash, bool isOutgoing, out HMessage message);
}