using System;

namespace Sulakore.Cryptography.Ciphers
{
    public interface IStreamCipher : IDisposable
    {
        void Process(Span<byte> data);
        void Process(ReadOnlySpan<byte> data, Span<byte> parsed);

        protected static void Swap<T>(int a, int b, Span<T> table)
        {
            T temp = table[a];
            table[a] = table[b];
            table[b] = temp;
        }
    }
}