using System;
using System.Runtime.InteropServices;

namespace Sulakore.Cryptography.Ciphers
{
    public class ChaCha20 : IStreamCipher
    {
        private static readonly uint[] _constants = new uint[4] { 0x61707865, 0x3320646e, 0x79622d32, 0x6b206574 };

        private readonly byte[] _state;

        public bool IsDecrypting { get; set; }

        private byte[] _block;
        private bool _disposed;
        private int _position = -1;

        public ChaCha20(ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, uint blockCount = 0)
        {
            if (key.Length != 32) throw new ArgumentException("The provided key is not 256-bits.");
            if (nonce.Length != 8 && nonce.Length != 12) throw new ArgumentException("The provided nonce can only be 96-bit or 64-bit.");

            _state = new byte[64];
            Span<uint> state = MemoryMarshal.Cast<byte, uint>(_state);

            // 4 Unsigned Integers
            _constants.CopyTo(state);

            // 8 Unsigned Integers
            key.CopyTo(_state.AsSpan(_constants.Length * sizeof(uint)));

            // 1 Unsigned Integer
            state[12] = blockCount;

            // 3 Unsigned Integers
            nonce.CopyTo(_state.AsSpan((nonce.Length == 8 ? 14 : 13) * sizeof(uint)));

            if (blockCount > 0)
            {
                for (int i = 0; i < blockCount; i++)
                {
                    RefreshBlock();
                }
                _position = 0; // Change from -1, as -1 will force the creation of a new block.
            }
        }

        public void Process(Span<byte> data) => Process(data, data);
        public void Process(ReadOnlySpan<byte> data, Span<byte> parsed)
        {
            for (int i = 0; i < parsed.Length; i++)
            {
                if (_position == -1 || _position == 64)
                {
                    RefreshBlock();
                    _position = 0;
                }
                parsed[i] = (byte)(data[i] ^ _block[_position++]);
            }
        }

        private void RefreshBlock()
        {
            _block = new byte[64];
            _state.CopyTo(_block, 0);

            Span<uint> block = MemoryMarshal.Cast<byte, uint>(_block);
            for (int i = 0; i < 10; i++)
            {
                QuarterRound(block, 0, 4, 8, 12);
                QuarterRound(block, 1, 5, 9, 13);
                QuarterRound(block, 2, 6, 10, 14);
                QuarterRound(block, 3, 7, 11, 15);
                QuarterRound(block, 0, 5, 10, 15);
                QuarterRound(block, 1, 6, 11, 12);
                QuarterRound(block, 2, 7, 8, 13);
                QuarterRound(block, 3, 4, 9, 14);
            }

            Span<uint> state = MemoryMarshal.Cast<byte, uint>(_state);
            for (int i = 0; i < 16; i++)
            {
                block[i] = CarrylessAdd(block[i], state[i]);
            }
            state[12]++;
        }

        private static void QuarterRound(Span<uint> state, int a, int b, int c, int d)
        {
            state[d] = RotateLeft(CarrylessXor(state[d], (state[a] = CarrylessAdd(state[a], state[b]))), 16);
            state[b] = RotateLeft(CarrylessXor(state[b], (state[c] = CarrylessAdd(state[c], state[d]))), 12);
            state[d] = RotateLeft(CarrylessXor(state[d], (state[a] = CarrylessAdd(state[a], state[b]))), 8);
            state[b] = RotateLeft(CarrylessXor(state[b], (state[c] = CarrylessAdd(state[c], state[d]))), 7);
        }

        private static uint CarrylessXor(uint left, uint right) => unchecked(left ^ right);
        private static uint CarrylessAdd(uint left, uint right) => unchecked(left + right);
        private static uint RotateLeft(uint value, int count) => unchecked((value << count) | (value >> (32 - count)));

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
                _block = null;
            }
            _disposed = true;
        }
    }
}