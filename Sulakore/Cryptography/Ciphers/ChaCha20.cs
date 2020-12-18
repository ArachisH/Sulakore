using System;
using System.Runtime.InteropServices;

namespace Sulakore.Cryptography.Ciphers
{
    public class ChaCha20 : IStreamCipher
    {
        private static readonly uint[] _constants = new uint[4] { 0x61707865, 0x3320646e, 0x79622d32, 0x6b206574 };

        private readonly uint[] _originalState;

        private bool _disposed;
        private byte[] _table;

        public ChaCha20(ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, uint blockCount = 1)
        {
            if (key.Length != 32) throw new ArgumentException("The provided key is not 256-bits.");
            if (nonce.Length != 12) throw new ArgumentException("The provided nonce is not 96-bits.");

            _table = new byte[64];
            Span<byte> tableBytes = _table.AsSpan();
            Span<uint> tableIntegers = MemoryMarshal.Cast<byte, uint>(tableBytes);

            _constants.CopyTo(tableIntegers);
            key.CopyTo(tableBytes.Slice(_constants.Length * sizeof(int)));

            tableIntegers[12] = blockCount;
            nonce.CopyTo(tableBytes.Slice(13 * sizeof(int)));

            tableIntegers.CopyTo(_originalState);
            for (int i = 0; i < 10; i++)
            {
                QuarterRound(tableIntegers, 0, 4, 8, 12);
                QuarterRound(tableIntegers, 1, 5, 9, 13);
                QuarterRound(tableIntegers, 2, 6, 10, 14);
                QuarterRound(tableIntegers, 3, 7, 11, 15);
                QuarterRound(tableIntegers, 0, 5, 10, 15);
                QuarterRound(tableIntegers, 1, 6, 11, 12);
                QuarterRound(tableIntegers, 2, 7, 8, 13);
                QuarterRound(tableIntegers, 3, 4, 9, 14);
            }
        }

        public void Process(Span<byte> data) => Process(data, data);
        public void Process(ReadOnlySpan<byte> data, Span<byte> parsed)
        {
            // TODO: https://tools.ietf.org/html/rfc7539#section-2.4.1
        }

        private static void QuarterRound(Span<uint> state, int a, int b, int c, int d)
        {
            state[d] = RotateLeft(state[d] ^ (state[a] += state[b]), 16);
            state[b] = RotateLeft(state[b] ^ (state[c] += state[d]), 12);
            state[d] = RotateLeft(state[d] ^ (state[a] += state[b]), 8);
            state[b] = RotateLeft(state[b] ^ (state[c] += state[d]), 7);
        }
        private static uint RotateLeft(uint value, int count) => (value << count) | (value >> (32 - count));

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _table = null;
            }
            _disposed = true;
        }
    }
}