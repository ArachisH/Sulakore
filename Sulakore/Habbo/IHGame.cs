using Sulakore.Habbo.Messages;

namespace Sulakore.Habbo.Web
{
    public interface IHGame
    {
        dynamic In { get; }
        dynamic Out { get; }

        bool IsUnity { get; }
        bool IsPostShuffle { get; }

        string Path { get; }
        string Revision { get; }

        void InjectKeyShouter();
    }
}