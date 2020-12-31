namespace Sulakore.Habbo
{
    public interface IGame
    {
        Incoming In { get; }
        Outgoing Out { get; }

        bool IsUnity { get; }
        bool IsPostShuffle { get; }

        string Path { get; }
        string Revision { get; }

        short Resolve(string name, bool isOutgoing);
    }
}