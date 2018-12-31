using System;

using Sulakore.Network;

namespace Sulakore.Modules
{
    public interface IModule : IDisposable
    {
        bool IsStandalone { get; }
        IInstaller Installer { get; set; }

        void OnConnected();
        void HandleOutgoing(DataInterceptedEventArgs e);
        void HandleIncoming(DataInterceptedEventArgs e);
    }
}