using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;

using Sulakore.Habbo;
using Sulakore.Network;
using Sulakore.Network.Protocol;

namespace Sulakore.Modules
{
    public class TService : IModule
    {
        private readonly TService _parent;
        private readonly IModule _container;
        private readonly List<DataCaptureAttribute> _unknownDataAttributes;
        private readonly Dictionary<ushort, List<DataCaptureAttribute>> _outDataAttributes, _inDataAttributes;

        public const int REMOTE_MODULE_PORT = 8055;

        public virtual bool IsStandalone { get; }

        private IInstaller _installer;
        public virtual IInstaller Installer
        {
            get => (_parent?.Installer ?? _installer);
            set => _installer = value;
        }

        public HGame Game => Installer.Game;
        public HGameData GameData => Installer.GameData;
        public IHConnection Connection => Installer.Connection;

        protected TService()
            : this(null)
        { }
        protected TService(TService parent)
            : this(null, parent)
        { }
        protected TService(IModule container)
            : this(container, null)
        { }
        private TService(IModule container, TService parent)
        {
            _parent = parent;
            _container = (container ?? this);
            _unknownDataAttributes = (parent?._unknownDataAttributes ?? new List<DataCaptureAttribute>());
            _inDataAttributes = (parent?._inDataAttributes ?? new Dictionary<ushort, List<DataCaptureAttribute>>());
            _outDataAttributes = (parent?._outDataAttributes ?? new Dictionary<ushort, List<DataCaptureAttribute>>());

            Installer = _container.Installer;
            IsStandalone = (parent != null ? false : _container.IsStandalone);
            if (LicenseManager.UsageMode != LicenseUsageMode.Runtime) return;

            foreach (MethodInfo method in _container.GetType().GetAllMethods())
            {
                foreach (var dataCaptureAtt in method.GetCustomAttributes<DataCaptureAttribute>())
                {
                    if (dataCaptureAtt == null) continue;

                    dataCaptureAtt.Method = method;
                    if (_unknownDataAttributes.Any(dca => dca.Equals(dataCaptureAtt))) continue;

                    dataCaptureAtt.Target = _container;
                    if (dataCaptureAtt.Id != null)
                    {
                        AddCallback(dataCaptureAtt, (ushort)dataCaptureAtt.Id);
                    }
                    else _unknownDataAttributes.Add(dataCaptureAtt);
                }
            }

            if (!IsStandalone || (Assembly.GetAssembly(_container.GetType()) != Assembly.GetEntryAssembly())) return;
            while (true)
            {
                HNode installerNode = HNode.ConnectNewAsync("127.0.0.1", REMOTE_MODULE_PORT).Result;
                if (installerNode != null)
                {
                    installerNode.InFormat = HFormat.EvaWire;
                    installerNode.OutFormat = HFormat.EvaWire;

                    var infoPacketOut = new EvaWirePacket(0);
                    WriteModuleInfo(infoPacketOut);

                    installerNode.SendPacketAsync(infoPacketOut).Wait();
                    Installer = _container.Installer = new DummyInstaller(_container, installerNode);
                    break;
                }
                else throw new Exception($"Failed to connect to the host on port '{REMOTE_MODULE_PORT}'.");
            }
        }

        public virtual void Synchronize(HGame game)
        {
            foreach (PropertyInfo property in _container.GetType().GetAllProperties())
            {
                var messageIdAtt = property.GetCustomAttribute<MessageIdAttribute>();
                if (string.IsNullOrWhiteSpace(messageIdAtt?.Hash)) continue;

                ushort id = game.GetMessageIds(messageIdAtt.Hash).First();
                property.SetValue(_container, id);
            }

            foreach (DataCaptureAttribute dataCaptureAtt in _unknownDataAttributes)
            {
                if (string.IsNullOrWhiteSpace(dataCaptureAtt.Hash)) continue;

                ushort id = game.GetMessageIds(dataCaptureAtt.Hash).First();
                AddCallback(dataCaptureAtt, id);
            }
        }
        public virtual void Synchronize(HGameData gameData)
        { }

        public virtual void HandleIncoming(DataInterceptedEventArgs e) => HandleData(_inDataAttributes, e);
        public virtual void HandleOutgoing(DataInterceptedEventArgs e) => HandleData(_outDataAttributes, e);
        private void HandleData(IDictionary<ushort, List<DataCaptureAttribute>> callbacks, DataInterceptedEventArgs e)
        {
            if (callbacks.TryGetValue(e.Packet.Id, out List<DataCaptureAttribute> attributes))
            {
                foreach (DataCaptureAttribute attribute in attributes)
                {
                    e.Packet.Position = 0;
                    attribute.Invoke(e);
                }
            }
        }

        private void WriteModuleInfo(HPacket packet)
        {
            var moduleAssembly = Assembly.GetAssembly(_container.GetType());

            var description = string.Empty;
            string name = moduleAssembly.GetName().Name;
            var moduleAtt = GetType().GetCustomAttribute<ModuleAttribute>();
            if (moduleAtt != null)
            {
                name = moduleAtt.Name;
                description = moduleAtt.Description;
            }

            packet.Write(moduleAssembly.GetName().Version.ToString());

            packet.Write(name);
            packet.Write(description);

            var authors = new List<AuthorAttribute>();
            var authorsAtts = GetType().GetCustomAttributes<AuthorAttribute>();
            if (authorsAtts != null)
            {
                authors.AddRange(authorsAtts);
            }

            packet.Write(authors.Count);
            foreach (AuthorAttribute author in authors)
            {
                packet.Write(author.Name);
            }
        }
        private void AddCallback(DataCaptureAttribute attribute, ushort id)
        {
            Dictionary<ushort, List<DataCaptureAttribute>> callbacks =
                (attribute.IsOutgoing ? _outDataAttributes : _inDataAttributes);

            if (!callbacks.TryGetValue(id, out List<DataCaptureAttribute> attributes))
            {
                attributes = new List<DataCaptureAttribute>();
                callbacks.Add(id, attributes);
            }
            attributes.Add(attribute);
        }

        public virtual void Dispose()
        {
            _inDataAttributes.Clear();
            _outDataAttributes.Clear();
            _unknownDataAttributes.Clear();
        }

        private class DummyInstaller : IInstaller, IHConnection
        {
            private readonly IModule _module;
            private readonly HNode _installerNode;
            private readonly Dictionary<ushort, Action<HPacket>> _moduleEvents;
            private readonly Dictionary<DataInterceptedEventArgs, string> _dataIdentifiers;

            HNode IHConnection.Local => throw new NotSupportedException();
            HNode IHConnection.Remote => throw new NotSupportedException();

            public HGame Game { get; set; }
            public HGameData GameData { get; }
            public IHConnection Connection => this;

            public DummyInstaller(IModule module, HNode installerNode)
            {
                _module = module;
                _installerNode = installerNode;
                _dataIdentifiers = new Dictionary<DataInterceptedEventArgs, string>();

                _moduleEvents = new Dictionary<ushort, Action<HPacket>>
                {
                    [1] = HandleData,
                    [3] = HandleGameSynchronize,
                    [4] = HandleGameDataSynchronize
                };

                GameData = new HGameData();
                Task handleInstallerDataTask = HandleInstallerDataAsync();
            }

            private void HandleData(HPacket packet)
            {
                string identifier = packet.ReadUTF8();

                int step = packet.ReadInt32();
                bool isOutgoing = packet.ReadBoolean();
                var format = HFormat.GetFormat(packet.ReadUTF8());
                bool canContinue = packet.ReadBoolean();

                int ogDataLength = packet.ReadInt32();
                byte[] ogData = packet.ReadBytes(ogDataLength);

                var args = new DataInterceptedEventArgs(format.CreatePacket(ogData), step, isOutgoing, ContinueAsync);
                _dataIdentifiers.Add(args, identifier);

                bool isOriginal = packet.ReadBoolean();
                if (!isOriginal)
                {
                    int packetLength = packet.ReadInt32();
                    byte[] packetData = packet.ReadBytes(packetLength);
                    args.Packet = format.CreatePacket(packetData);
                }

                if (isOutgoing)
                {
                    _module.HandleOutgoing(args);
                }
                else
                {
                    _module.HandleIncoming(args);
                }

                if (!args.WasRelayed)
                {
                    HPacket handledDataPacket = CreateHandledDataPacket(args, false);
                    _installerNode.SendPacketAsync(handledDataPacket);
                }
            }
            private void HandleGameSynchronize(HPacket packet)
            {
                string path = packet.ReadUTF8();
                Game = new HGame(File.ReadAllBytes(path));
                Game.Location = path;

                Game.Disassemble();
                Game.GenerateMessageHashes();

                _module.Synchronize(Game);
            }
            private void HandleGameDataSynchronize(HPacket packet)
            {
                GameData.Source = packet.ReadUTF8();
                _module.Synchronize(GameData);
            }

            private async Task HandleInstallerDataAsync()
            {
                try
                {
                    HPacket packet = await _installerNode.ReceivePacketAsync().ConfigureAwait(false);
                    if (packet == null) Environment.Exit(0);

                    Task handleInstallerDataTask = HandleInstallerDataAsync();
                    if (_moduleEvents.TryGetValue(packet.Id, out Action<HPacket> handler))
                    {
                        handler(packet);
                    }
                }
                catch { Environment.Exit(0); }
            }
            private async Task ContinueAsync(DataInterceptedEventArgs args)
            {
                HPacket handledDataPacket = CreateHandledDataPacket(args, true);
                await _installerNode.SendPacketAsync(handledDataPacket).ConfigureAwait(false);
            }

            public Task<int> SendToClientAsync(byte[] data)
            {
                return _installerNode.SendPacketAsync(2, false, data.Length, data);
            }
            public Task<int> SendToClientAsync(HPacket packet)
            {
                return SendToClientAsync(packet.ToBytes());
            }
            public Task<int> SendToClientAsync(ushort id, params object[] values)
            {
                return SendToClientAsync(EvaWirePacket.Construct(id, values));
            }

            public Task<int> SendToServerAsync(byte[] data)
            {
                return _installerNode.SendPacketAsync(2, true, data.Length, data);
            }
            public Task<int> SendToServerAsync(HPacket packet)
            {
                return SendToServerAsync(packet.ToBytes());
            }
            public Task<int> SendToServerAsync(ushort id, params object[] values)
            {
                return SendToServerAsync(EvaWirePacket.Construct(id, values));
            }

            private HPacket CreateHandledDataPacket(DataInterceptedEventArgs args, bool isContinuing)
            {
                var handledDataPacket = new EvaWirePacket(1);
                handledDataPacket.Write(_dataIdentifiers[args]);

                handledDataPacket.Write(isContinuing);
                if (isContinuing)
                {
                    handledDataPacket.Write(args.WasRelayed);
                }
                else
                {
                    byte[] packetData = args.Packet.ToBytes();
                    handledDataPacket.Write(packetData.Length);
                    handledDataPacket.Write(packetData);
                    handledDataPacket.Write(args.IsBlocked);
                }
                return handledDataPacket;
            }
        }
    }
}