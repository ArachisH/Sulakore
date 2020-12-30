namespace Sulakore.Habbo
{
    public interface IHGame
    {
        Incoming In { get; }
        Outgoing Out { get; }

        bool IsUnity { get; }
        bool IsPostShuffle { get; }

        string Path { get; }
        string Revision { get; }

        void InjectKeyShouter();
        short Resolve(string name);
    }
}