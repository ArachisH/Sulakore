namespace Sulakore.Network.Formats;

/// <summary>
/// Provides functionality to format the binary representation of an object into 
/// </summary>
public interface IHFormattable
{
    bool TryFormat(Span<byte> destination, IHFormat format, out int bytesWritten, ReadOnlySpan<char> formatString);
}