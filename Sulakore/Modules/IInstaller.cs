using Sulakore.Network;
using Sulakore.Habbo.Web;

namespace Sulakore.Modules
{
    public interface IInstaller
    {
        IHGame Game { get; }
        IHConnection Connection { get; }
    }
}