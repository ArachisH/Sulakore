using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;

using Sulakore.Habbo;
using Sulakore.Network;
using Sulakore.Habbo.Web;
using Sulakore.Habbo.Messages;
using Sulakore.Network.Protocol;

namespace Sulakore.Modules
{
    public class TService : IModule
    {
        private readonly TService _parent;
        private readonly IModule _container;
        private readonly List<DataCaptureAttribute> _unknownDataAttributes;
        private readonly Dictionary<ushort, List<DataCaptureAttribute>> _outDataAttributes, _inDataAttributes;

        public virtual bool IsStandalone { get; }

        private IInstaller _installer;
        public virtual IInstaller Installer
        {
            get => (_parent?.Installer ?? _installer);
            set => _installer = value;
        }

        public Incoming In => Installer.In;
        public Outgoing Out => Installer.Out;

        public HGame Game => Installer.Game;
        public HGameData GameData => Installer.GameData;
        public IHConnection Connection => Installer.Connection;

        public static IPEndPoint DefaultModuleServer { get; }

        static TService()
        {
            DefaultModuleServer = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8055);
        }

        public TService(IModule container)
            : this(container, null, null)
        { }
        public TService(IModule container, IPEndPoint moduleServer)
            : this(container, null, moduleServer)
        { }

        protected TService()
            : this(null, null, null)
        { }
        protected TService(IPEndPoint moduleServer)
            : this(null, null, moduleServer)
        { }

        protected TService(TService parent)
            : this(null, parent, null)
        { }
        protected TService(TService parent, IPEndPoint moduleServer)
            : this(null, parent, moduleServer)
        { }

        private TService(IModule container, TService parent, IPEndPoint moduleServer)
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
                HNode installerNode = HNode.ConnectNewAsync(moduleServer ?? DefaultModuleServer).Result;
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
                else throw new Exception($"Failure to establish the connection with: {moduleServer}");
            }
        }

        public virtual void OnConnected()
        {
            var unresolved = new Dictionary<string, IList<string>>();
            foreach (PropertyInfo property in _container.GetType().GetAllProperties())
            {
                var messageIdAtt = property.GetCustomAttribute<MessageIdAttribute>();
                if (string.IsNullOrWhiteSpace(messageIdAtt?.Hash)) continue;

                ushort[] ids = Game.GetMessageIds(messageIdAtt.Hash);
                if (ids != null)
                {
                    property.SetValue(_container, ids[0]);
                }
                else
                {
                    if (!unresolved.TryGetValue(messageIdAtt.Hash, out IList<string> users))
                    {
                        users = new List<string>();
                        unresolved.Add(messageIdAtt.Hash, users);
                    }
                    users.Add("Property: " + property.Name);
                }
            }
            foreach (DataCaptureAttribute dataCaptureAtt in _unknownDataAttributes)
            {
                if (string.IsNullOrWhiteSpace(dataCaptureAtt.Identifier)) continue;

                ushort[] ids = Game.GetMessageIds(dataCaptureAtt.Identifier);
                if (ids != null)
                {
                    AddCallback(dataCaptureAtt, ids[0]);
                }
                else
                {
                    var identifiers = (dataCaptureAtt.IsOutgoing ? Out : (Identifiers)In);
                    if (identifiers.TryGetId(dataCaptureAtt.Identifier, out ushort id))
                    {
                        AddCallback(dataCaptureAtt, id);
                    }
                    else
                    {
                        if (!unresolved.TryGetValue(dataCaptureAtt.Identifier, out IList<string> users))
                        {
                            users = new List<string>();
                            unresolved.Add(dataCaptureAtt.Identifier, users);
                        }
                        users.Add(dataCaptureAtt.GetType().Name + ": " + dataCaptureAtt.Method.Name);
                    }
                }
            }
            if (unresolved.Count > 0)
            {
                throw new HashResolvingException(Game.Revision, unresolved);
            }
        }

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
            var moduleAtt = _container.GetType().GetCustomAttribute<ModuleAttribute>();
            if (moduleAtt != null)
            {
                name = moduleAtt.Name;
                description = moduleAtt.Description;
            }
            packet.Write(moduleAssembly.GetName().Version.ToString());

            packet.Write(name);
            packet.Write(description);

            var authors = new List<AuthorAttribute>();
            var authorsAtts = _container.GetType().GetCustomAttributes<AuthorAttribute>();
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

            public Incoming In { get; }
            public Outgoing Out { get; }

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
                    [2] = HandleOnConnected
                };

                In = new Incoming();
                Out = new Outgoing();
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
            private void HandleOnConnected(HPacket packet)
            {
                GameData.Source = packet.ReadUTF8();

                int gameLength = packet.ReadInt32();
                Game = new HGame(packet.ReadBytes(gameLength));
                Game.Location = packet.ReadUTF8();

                Game.Disassemble();
                Game.GenerateMessageHashes();

                int hashesDataLength = packet.ReadInt32();
                byte[] hashesData = packet.ReadBytes(hashesDataLength);
                using (var hashesStream = new StreamReader(new MemoryStream(hashesData)))
                {
                    In.Load(Game, hashesStream);
                    hashesStream.BaseStream.Position = 0;
                    Out.Load(Game, hashesStream);
                }
                _module.OnConnected();
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