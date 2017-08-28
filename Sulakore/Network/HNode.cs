using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;

using Sulakore.Crypto;
using Sulakore.Network.Protocol;

namespace Sulakore.Network
{
    public class HNode : IDisposable
    {
        private static readonly Dictionary<int, TcpListener> _listeners;

        public bool IsConnected
        {
            get { return Client.Connected; }
        }

        public Socket Client { get; }
        public HFormat InFormat { get; set; }
        public HFormat OutFormat { get; set; }
        public HotelEndPoint EndPoint { get; private set; }

        public RC4 Encrypter { get; set; }
        public bool IsEncrypting { get; set; }

        public RC4 Decrypter { get; set; }
        public bool IsDecrypting { get; set; }

        static HNode()
        {
            _listeners = new Dictionary<int, TcpListener>();
        }
        public HNode()
            : this(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
        { }
        public HNode(Socket client)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }
            Client = client;
            Client.NoDelay = true;
        }

        public Task ConnectAsync(string host, int port)
        {
            return ConnectAsync(HotelEndPoint.Parse(host, port));
        }
        public async Task ConnectAsync(IPEndPoint endpoint)
        {
            EndPoint = (endpoint as HotelEndPoint);
            if (EndPoint == null)
            {
                EndPoint = new HotelEndPoint(endpoint);
            }

            IAsyncResult result = Client.BeginConnect(endpoint, null, null);
            await Task.Factory.FromAsync(result, Client.EndConnect).ConfigureAwait(false);
        }
        public Task ConnectAsync(IPAddress address, int port)
        {
            return ConnectAsync(new HotelEndPoint(address, port));
        }
        public Task ConnectAsync(IPAddress[] addresses, int port)
        {
            return ConnectAsync(new HotelEndPoint(addresses[0], port));
        }

        public Task<HPacket> ReceivePacketAsync()
        {
            if (InFormat == null)
            {
                throw new NullReferenceException("Incoming format cannot be null.");
            }
            return InFormat.ReceivePacketAsync(this);
        }
        public Task<int> SendPacketAsync(HPacket packet)
        {
            return SendAsync(packet.ToBytes());
        }
        public Task<int> SendPacketAsync(string signature)
        {
            if (OutFormat == null)
            {
                throw new NullReferenceException("Outgoing format cannot be null.");
            }
            return SendAsync(HPacket.ToBytes(OutFormat, signature));
        }
        public Task<int> SendPacketAsync(ushort id, params object[] values)
        {
            if (OutFormat == null)
            {
                throw new NullReferenceException("Outgoing format cannot be null.");
            }
            return SendAsync(OutFormat.Construct(id, values));
        }

        public Task<int> SendAsync(byte[] buffer)
        {
            return SendAsync(buffer, buffer.Length);
        }
        public Task<int> SendAsync(byte[] buffer, int size)
        {
            return SendAsync(buffer, 0, size);
        }
        public Task<int> SendAsync(byte[] buffer, int offset, int size)
        {
            return SendAsync(buffer, offset, size, SocketFlags.None);
        }

        public Task<byte[]> ReceiveAsync(int size)
        {
            return ReceiveBufferAsync(size, SocketFlags.None);
        }
        public Task<int> ReceiveAsync(byte[] buffer)
        {
            return ReceiveAsync(buffer, buffer.Length);
        }
        public Task<int> ReceiveAsync(byte[] buffer, int size)
        {
            return ReceiveAsync(buffer, 0, size);
        }
        public Task<int> ReceiveAsync(byte[] buffer, int offset, int size)
        {
            return ReceiveAsync(buffer, offset, size, SocketFlags.None);
        }

        public Task<byte[]> PeekAsync(int size)
        {
            return ReceiveBufferAsync(size, SocketFlags.Peek);
        }
        public Task<int> PeekAsync(byte[] buffer)
        {
            return PeekAsync(buffer, buffer.Length);
        }
        public Task<int> PeekAsync(byte[] buffer, int size)
        {
            return PeekAsync(buffer, 0, size);
        }
        public Task<int> PeekAsync(byte[] buffer, int offset, int size)
        {
            return ReceiveAsync(buffer, offset, size, SocketFlags.Peek);
        }

        protected async Task<byte[]> ReceiveBufferAsync(int size, SocketFlags socketFlags)
        {
            var buffer = new byte[size];
            int read = await ReceiveAsync(buffer, 0, size, socketFlags).ConfigureAwait(false);

            var trimmedBuffer = new byte[read];
            Buffer.BlockCopy(buffer, 0, trimmedBuffer, 0, read);

            return trimmedBuffer;
        }
        protected async Task<int> SendAsync(byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            if (!IsConnected)
            {
                return 0;
            }

            if (IsEncrypting && Encrypter != null)
            {
                buffer = Encrypter.Parse(buffer);
            }

            IAsyncResult result = Client.BeginSend(buffer, offset, size, socketFlags, null, null);
            return await Task.Factory.FromAsync(result, Client.EndSend).ConfigureAwait(false);
        }
        protected async Task<int> ReceiveAsync(byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            if (!IsConnected)
            {
                return 0;
            }

            IAsyncResult result = Client.BeginReceive(buffer, offset, size, socketFlags, null, null);
            int read = await Task.Factory.FromAsync(result, Client.EndReceive).ConfigureAwait(false);

            if (read > 0 && IsDecrypting && Decrypter != null)
            {
                Decrypter.RefParse(buffer, offset, read,
                    socketFlags.HasFlag(SocketFlags.Peek));
            }
            return read;
        }

        public void Disconnect()
        {
            if (IsConnected)
            {
                try
                {
                    Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, false);
                    Client.Shutdown(SocketShutdown.Both);
                    Client.Disconnect(false);
                }
                catch (SocketException) { }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (IsConnected)
                {
                    Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                    Client.Shutdown(SocketShutdown.Both);
                }
                Client.Close();
            }
        }

        public static void StopListeners(int? port = null)
        {
            foreach (TcpListener listener in _listeners.Values)
            {
                if (port != null)
                {
                    if (port != ((IPEndPoint)listener.LocalEndpoint).Port) continue;
                }
                listener.Stop();
            }
        }
        public static async Task<HNode> AcceptAsync(int port)
        {
            TcpListener listener = null;
            if (!_listeners.TryGetValue(port, out listener))
            {
                listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
                _listeners.Add(port, listener);
            }

            try
            {
                listener.Start();
                Socket client = await listener.AcceptSocketAsync().ConfigureAwait(false);
                return new HNode(client);
            }
            finally
            {
                listener.Stop();
                if (_listeners.ContainsKey(port))
                {
                    _listeners.Remove(port);
                }
            }
        }

        public static Task<HNode> ConnectNewAsync(string host, int port)
        {
            return ConnectNewAsync(HotelEndPoint.Parse(host, port));
        }
        public static async Task<HNode> ConnectNewAsync(IPEndPoint endpoint)
        {
            HNode remote = null;
            try
            {
                remote = new HNode();
                await remote.ConnectAsync(endpoint).ConfigureAwait(false);
            }
            catch { remote = null; }
            finally
            {
                if (!remote?.IsConnected ?? false)
                {
                    remote = null;
                }
            }
            return remote;
        }
        public static Task<HNode> ConnectNewAsync(IPAddress address, int port)
        {
            return ConnectNewAsync(new HotelEndPoint(address, port));
        }
        public static Task<HNode> ConnectNewAsync(IPAddress[] addresses, int port)
        {
            return ConnectNewAsync(new HotelEndPoint(addresses[0], port));
        }
    }
}