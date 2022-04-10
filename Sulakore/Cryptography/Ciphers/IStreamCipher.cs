namespace Sulakore.Cryptography.Ciphers;

public interface IStreamCipher
{
    void Process(Span<byte> data);
    void Process(ReadOnlySpan<byte> data, Span<byte> parsed);
}