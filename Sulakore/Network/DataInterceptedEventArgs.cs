using Sulakore.Network.Formats;

namespace Sulakore.Network
{
    /// <summary>
    /// Represents an intercepted message that will be returned to the caller with blocking/replacing information.
    /// </summary>
    public class DataInterceptedEventArgs : EventArgs
    {
        private readonly object _continueLock;
        private readonly DataInterceptedEventArgs _args;
        private readonly Func<DataInterceptedEventArgs, Task> _continuation;
        private readonly Func<DataInterceptedEventArgs, ValueTask<int>> _relayer;

        public int Step { get; }
        public HFormat Format { get; }
        public bool IsOutgoing { get; }
        public DateTime Timestamp { get; }
        public Task WaitUntil { get; set; }

        public ReadOnlyMemory<byte> ReplacementRegion { get; set; }
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

        private ReadOnlyMemory<byte> _packetRegion;
        public ReadOnlyMemory<byte> PacketRegion
        {
            get => _args?.PacketRegion ?? _packetRegion;
            set
            {
                if (_args != null)
                {
                    _args.PacketRegion = value;
                }
                _packetRegion = value;
            }
        }

        public DataInterceptedEventArgs(DataInterceptedEventArgs args)
        {
            _args = args;
            _relayer = args._relayer;
            _continuation = args._continuation;
            _continueLock = args._continueLock;

            Step = args.Step;
            Timestamp = args.Timestamp;
            IsOutgoing = args.IsOutgoing;
        }
        public DataInterceptedEventArgs(ReadOnlyMemory<byte> packetRegion, int step, bool isOutgoing, HFormat format, Func<DataInterceptedEventArgs, Task> continuation = null, Func<DataInterceptedEventArgs, ValueTask<int>> relayer = null)
        {
            Step = step;
            Format = format;
            IsOutgoing = isOutgoing;
            Timestamp = DateTime.Now;
            PacketRegion = packetRegion;

            _continueLock = new object();
            _continuation = continuation;

            _relayer = relayer;
        }

        public void Relay()
        {
            if (_relayer == null) return;
            lock (_continueLock)
            {
                WasRelayed = true;
                Task.Run(() => _relayer(this));
            }
        }
        public void Continue(bool relay = false)
        {
            if (!IsContinuable) return;
            lock (_continueLock)
            {
                if (relay) Relay();
                HasContinued = true;
                _continuation(this);
            }
        }
    }
}