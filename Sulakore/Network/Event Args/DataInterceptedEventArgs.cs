using System;
using System.Threading.Tasks;

using Sulakore.Network.Protocol;

namespace Sulakore.Network
{
    /// <summary>
    /// Represents an intercepted message that will be returned to the caller with blocking/replacing information.
    /// </summary>
    public class DataInterceptedEventArgs : EventArgs
    {
        private readonly byte[] _ogData;
        private readonly string _ogString;
        private readonly object _continueLock;
        private readonly DataInterceptedEventArgs _args;
        private readonly Func<DataInterceptedEventArgs, Task<int>> _relayer;
        private readonly Func<DataInterceptedEventArgs, Task> _continuation;

        public int Step { get; }
        public bool IsOutgoing { get; }
        public DateTime Timestamp { get; }

        public bool IsOriginal => Packet.ToString().Equals(_ogString);
        public bool IsContinuable => _continuation != null && !HasContinued;

        private bool _isBlocked;
        public bool IsBlocked
        {
            get => _args?.IsBlocked ?? _isBlocked;
            set
            {
                if (_args != null)
                {
                    _args.IsBlocked = value;
                }
                _isBlocked = value;
            }
        }

        private HPacket _packet;
        public HPacket Packet
        {
            get => _args?.Packet ?? _packet;
            set
            {
                if (_args != null)
                {
                    _args.Packet = value;
                }
                _packet = value;
            }
        }

        private bool _wasRelayed;
        public bool WasRelayed
        {
            get => _args?.WasRelayed ?? _wasRelayed;
            private set
            {
                if (_args != null)
                {
                    _args.WasRelayed = value;
                }
                _wasRelayed = value;
            }
        }

        private bool _hasContinued;
        public bool HasContinued
        {
            get => _args?.HasContinued ?? _hasContinued;
            private set
            {
                if (_args != null)
                {
                    _args.HasContinued = value;
                }
                _hasContinued = value;
            }
        }

        public DataInterceptedEventArgs(DataInterceptedEventArgs args)
        {
            _args = args;
            _ogData = args._ogData;
            _ogString = args._ogString;
            _relayer = args._relayer;
            _continuation = args._continuation;
            _continueLock = args._continueLock;

            Step = args.Step;
            Timestamp = args.Timestamp;
            IsOutgoing = args.IsOutgoing;
        }
        public DataInterceptedEventArgs(HPacket packet, int step, bool isOutgoing)
        {
            _ogData = packet.ToBytes();
            _ogString = packet.ToString();

            Step = step;
            Packet = packet;
            IsOutgoing = isOutgoing;
            Timestamp = DateTime.Now;
        }
        public DataInterceptedEventArgs(HPacket packet, int step, bool isOutgoing, Func<DataInterceptedEventArgs, Task> continuation)
            : this(packet, step, isOutgoing)
        {
            _continueLock = new object();
            _continuation = continuation;
        }
        public DataInterceptedEventArgs(HPacket packet, int step, bool isOutgoing, Func<DataInterceptedEventArgs, Task> continuation, Func<DataInterceptedEventArgs, Task<int>> relayer)
            : this(packet, step, isOutgoing, continuation)
        {
            _relayer = relayer;
        }

        public void Continue()
        {
            Continue(false);
        }
        public void Continue(bool relay)
        {
            if (IsContinuable)
            {
                lock (_continueLock)
                {
                    if (relay)
                    {
                        WasRelayed = true;
                        _relayer?.Invoke(this);
                    }

                    HasContinued = true;
                    _continuation(this);
                }
            }
        }

        public byte[] GetOriginalData() => _ogData;

        /// <summary>
        /// Restores the intercepted data to its initial form, before it was replaced/modified.
        /// </summary>
        public void Restore()
        {
            if (!IsOriginal)
            {
                Packet = Packet.Format.CreatePacket(_ogData);
            }
        }
    }
}