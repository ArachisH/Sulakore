namespace Sulakore.Habbo;

public interface IGame
{
    bool IsUnity { get; }
    bool IsPostShuffle { get; }

    string Path { get; }
    string Revision { get; }

    bool TryResolveMessage(string name, uint hash, bool isOutgoing, out HMessage message);
}