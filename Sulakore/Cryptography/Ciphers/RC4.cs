﻿namespace Sulakore.Cryptography.Ciphers
{
    public class RC4 : IStreamCipher
    {
        private int _i, _j;
        private int[] _table;
        private bool _disposed;

        public RC4(byte[] key)
        {
            _table = new int[256];
            for (int i = 0; i < 256; i++)
            {
                _table[i] = i;
            }
            for (int j = 0, x = 0; j < _table.Length; j++)
            {
                x += _table[j];
                x += key[j % key.Length];
                x %= _table.Length;
                IStreamCipher.Swap<int>(j, x, _table);
            }
        }

        public void Process(Span<byte> data)
        {
            Process(data, data);
        }
        public void Process(ReadOnlySpan<byte> data, Span<byte> parsed)
        {
            for (int k = 0; k < data.Length; k++)
            {
                _i++;
                _i %= _table.Length;
                _j += _table[_i];
                _j %= _table.Length;

                IStreamCipher.Swap<int>(_i, _j, _table);
                int rightXOR = _table[_i] + _table[_j];
                rightXOR = _table[rightXOR % _table.Length];

                parsed[k] = (byte)(data[k] ^ rightXOR);
            }
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