using Sulakore.Network;
using Sulakore.Habbo.Web;
using Sulakore.Habbo.Messages;

namespace Sulakore.Modules
{
    public interface IInstaller
    {
        Incoming In { get; }
        Outgoing Out { get; }

        IHGame Game { get; }
        IHConnection Connection { get; }
    }
}