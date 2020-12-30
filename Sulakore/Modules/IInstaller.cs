using Sulakore.Network;

namespace Sulakore.Modules
{
    public interface IInstaller
    {
        IHGame Game { get; }
        IHConnection Connection { get; }
    }
}