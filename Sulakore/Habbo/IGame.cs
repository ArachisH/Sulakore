namespace Sulakore.Habbo;

public interface IGame
{
    bool IsPostShuffle { get; }
    HGameArchitecture Architecture { get; }

    string Path { get; }
    string Revision { get; }
    int MinimumConnectionAttempts { get; }

    bool TryResolveMessage(string name, uint hash, bool isOutgoing, out HMessage message);
}