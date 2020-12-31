using Sulakore.Habbo;
using Sulakore.Network;

namespace Sulakore.Modules
{
    public interface IInstaller
    {
        IGame Game { get; }
        IHConnection Connection { get; }
    }
}