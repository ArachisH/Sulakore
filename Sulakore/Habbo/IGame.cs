namespace Sulakore.Habbo
{
    public interface IGame<T> where T : HMessage
    {
        Incoming In { get; }
        Outgoing Out { get; }

        bool IsUnity { get; }
        bool IsPostShuffle { get; }

        string Path { get; }
        string Revision { get; }

        T GetMessage(short id, bool isOutgoing);
        short Resolve(string name, bool isOutgoing);
    }
    public interface IGame : IGame<HMessage> { }
}