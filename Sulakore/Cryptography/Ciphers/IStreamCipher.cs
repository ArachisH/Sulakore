using System;

namespace Sulakore.Cryptography.Ciphers
{
    public interface IStreamCipher
    {
        void Process(Span<byte> data);
        void Process(ReadOnlySpan<byte> data, Span<byte> parsed);

        protected static void Swap(int a, int b, Span<int> table)
        {
            int temp = table[a];
            table[a] = table[b];
            table[b] = temp;
        }
    }
}