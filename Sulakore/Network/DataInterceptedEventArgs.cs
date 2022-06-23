using System.Diagnostics.CodeAnalysis;

using Sulakore.Habbo;
using Sulakore.Network.Buffers;
using Sulakore.Network.Formats;

namespace Sulakore.Network;

/// <summary>
/// Represents an intercepted message that will be returned to the caller with blocking/replacing information.
/// </summary>
public sealed class DataInterceptedEventArgs : EventArgs
{
    private readonly Func<Task>? _continuation;
    private readonly Func<DataInterceptedEventArgs, ValueTask>? _relayer;

    public int Step { get; }
    public DateTime Timestamp { get; }
    public Task? WaitUntil { get; set; }

    [MemberNotNullWhen(true, "_continuation")]
    public bool IsContinuable => _continuation != null && !HasContinued;

    public IHFormat Format { get; }
    public HMessage Message { get; }
    public ReadOnlyMemory<byte> Buffer { get; }

    public bool IsBlocked { get; set; }
    public bool WasRelayed { get; private set; }
    public bool HasContinued { get; private set; }

    public DataInterceptedEventArgs(ReadOnlyMemory<byte> buffer, IHFormat format, HMessage message, int step, Func<Task>? continuation = null, Func<DataInterceptedEventArgs, ValueTask>? relayer = null)
    {
        _relayer = relayer;
        _continuation = continuation;

        Step = step;
        Buffer = buffer;
        Format = format;
        Message = message;
        Timestamp = DateTime.Now;
    }

    public HPacketReader GetPacketReader() => new(Format, Buffer.Span);

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
        _continuation();
    }
}