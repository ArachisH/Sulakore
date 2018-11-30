using Sulakore.Network;
using Sulakore.Habbo.Web;
using Sulakore.Habbo.Messages;

namespace Sulakore.Modules
{
    public interface IInstaller
    {
        Incoming In { get; }
        Outgoing Out { get; }

        HGame Game { get; }
        HGameData GameData { get; }
        IHConnection Connection { get; }
    }
}