using Sulakore.Habbo;
using Sulakore.Network;

namespace Sulakore.Modules
{
    public interface IInstaller
    {
        HGame Game { get; }
        HGameData GameData { get; }
        IHConnection Connection { get; }
    }
}