using System;
using System.Runtime.InteropServices;

namespace Sulakore.Cryptography.Ciphers
{
    public class ChaCha20 : IStreamCipher
    {
        private static readonly int[] _constants = new int[4] { 0x61707865, 0x3320646e, 0x79622d32, 0x6b206574 };

        private bool _disposed;
        private byte[] _table;

        public ChaCha20(ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, int blockCount = 0)
        {
            if (key.Length != 32) throw new ArgumentException("The provided key is not 256-bits.");
            if (nonce.Length != 12) throw new ArgumentException("The provided nonce is not 96-bits.");

            _table = new byte[64];
            Span<byte> tableBytes = _table.AsSpan();
            Span<int> tableIntegers = MemoryMarshal.Cast<byte, int>(tableBytes);

            _constants.CopyTo(tableIntegers);
            key.CopyTo(tableBytes.Slice(_constants.Length * sizeof(int)));

            tableIntegers[12] = blockCount;
            nonce.CopyTo(tableBytes.Slice(13 * sizeof(int)));

            QuarterRound(tableIntegers, 0, 0, 0, 0);
        }

        public void Process(Span<byte> data)
        {
            throw new NotImplementedException();
        }
        public void Process(ReadOnlySpan<byte> data, Span<byte> parsed)
        {
            throw new NotImplementedException();
        }

        private void QuarterRound(Span<int> state, uint a, uint b, uint c, uint d)
        {

        }

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