using Sulakore.Network.Buffers;

namespace Sulakore.Network
{
    /// <summary>
    /// Represents an intercepted message that will be returned to the caller with blocking/replacing information.
    /// </summary>
    public class DataInterceptedEventArgs : EventArgs
    {
        private readonly DataInterceptedEventArgs _args;
        private readonly Func<DataInterceptedEventArgs, Task> _continuation;
        private readonly Func<DataInterceptedEventArgs, ValueTask<int>> _relayer;

        public int Step { get; }
        public bool IsOutgoing { get; }
        public Task WaitUntil { get; set; }

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

        public DataInterceptedEventArgs(DataInterceptedEventArgs args)
        {
            _args = args;
            _relayer = args._relayer;
            _continuation = args._continuation;

            Step = args.Step;
            IsOutgoing = args.IsOutgoing;
        }
        public DataInterceptedEventArgs(HPacket packet, int step, bool isOutgoing, Func<DataInterceptedEventArgs, Task> continuation = null, Func<DataInterceptedEventArgs, ValueTask<int>> relayer = null)
        {
            _relayer = relayer;
            _continuation = continuation;

            Step = step;
            Packet = packet;
            IsOutgoing = isOutgoing;
        }

        public void Relay()
        {
            if (_relayer == null) return;

            WasRelayed = true;
            Task.Run(() => _relayer(this));
        }
        public void Continue(bool relay = false)
        {
            if (!IsContinuable) return;

            if (relay) Relay();
            HasContinued = true;
            _continuation(this);
        }
    }
}