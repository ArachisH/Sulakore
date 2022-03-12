using Sulakore.Network.Buffers;

namespace Sulakore.Network;

/// <summary>
/// Represents an intercepted message that will be returned to the caller with blocking/replacing information.
/// </summary>
public class DataInterceptedEventArgs : EventArgs
{
    private readonly Func<DataInterceptedEventArgs, Task> _continuation;
    private readonly Func<DataInterceptedEventArgs, ValueTask<int>> _relayer;

    public int Step { get; }
    public bool IsOutgoing { get; }
    public Task WaitUntil { get; set; }

    public bool IsContinuable => _continuation != null && !HasContinued;

    public HPacket Packet { get; }
    public HPacket Replacement { get; set; }

    public bool IsBlocked { get; set; }
    public bool WasRelayed { get; private set; }
    public bool HasContinued { get; private set; }

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