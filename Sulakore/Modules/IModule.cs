using System;

using Sulakore.Habbo;
using Sulakore.Network;

namespace Sulakore.Modules
{
    public interface IModule : IDisposable
    {
        bool IsStandalone { get; }
        IInstaller Installer { get; set; }

        void Synchronize(HGame game);
        void Synchronize(HGameData gameData);

        void HandleOutgoing(DataInterceptedEventArgs e);
        void HandleIncoming(DataInterceptedEventArgs e);
    }
}