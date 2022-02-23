using Sulakore.Habbo;
using Sulakore.Network;

namespace Sulakore.Modules
{
    public interface IInstaller
    {
        public Incoming In { get; }
        public Outgoing Out { get; }

        IGame Game { get; }
        IHConnection Connection { get; }
    }
}