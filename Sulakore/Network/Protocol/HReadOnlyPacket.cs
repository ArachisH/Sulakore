using System;
using System.Text;
using System.Text.Unicode;

namespace Sulakore.Network.Protocol
{
    public ref struct HReadOnlyPacket
    {
        private readonly ReadOnlySpan<byte> _source;

        public short Id { get; }
        public IFormat Format { get; }
        public int Available => _source.Length - _position;

        private int _position;
        public int Position
        {
            get => _position;
            set => _position = value;
        }

        public static implicit operator ReadOnlySpan<byte>(HReadOnlyPacket packet) => packet._source;

        public HReadOnlyPacket(IFormat format, ReadOnlySpan<byte> source)
        {
            _position = 0;
            _source = source;

            Format = format;
            Id = format.ReadId(source);
        }

        public int ReadInt32(ref int position)
        {
            int value = Format.ReadInt32(_source.Slice(position), out int size);
            position += size;
            return value;
        }
        public int ReadInt32(int position = -1)
        {
            return ReadInt32(ref position > -1 ? ref position : ref _position);
        }

        public short ReadInt16(ref int position)
        {
            short value = Format.ReadInt16(_source.Slice(position), out int size);
            position += size;
            return value;
        }
        public short ReadInt16(int position = -1)
        {
            return ReadInt16(ref position > -1 ? ref position : ref _position);
        }

        public ushort ReadUInt16(ref int position)
        {
            ushort value = Format.ReadUInt16(_source.Slice(position), out int size);
            position += size;
            return value;
        }
        public ushort ReadUInt16(int position = -1)
        {
            return ReadUInt16(ref position > -1 ? ref position : ref _position);
        }

        public bool ReadBoolean(ref int position)
        {
            bool value = Format.ReadBoolean(_source.Slice(position), out int size);
            position += size;
            return value;
        }
        public bool ReadBoolean(int position = -1)
        {
            return ReadBoolean(ref position > -1 ? ref position : ref _position);
        }

        public float ReadFloat(ref int position)
        {
            float value = Format.ReadFloat(_source.Slice(position), out int size);
            position += size;
            return value;
        }
        public float ReadFloat(int position = -1)
        {
            return ReadFloat(ref position > -1 ? ref position : ref _position);
        }

        public double ReadDouble(ref int position)
        {
            double value = Format.ReadDouble(_source.Slice(position), out int size);
            position += size;
            return value;
        }
        public double ReadDouble(int position = -1)
        {
            return ReadDouble(ref position > -1 ? ref position : ref _position);
        }

        public string ReadString(ref int position)
        {
            return Encoding.UTF8.GetString(ReadUTF8(ref position));
        }
        public string ReadString(int position = -1)
        {
            return ReadString(ref position > -1 ? ref position : ref _position);
        }

        public ReadOnlySpan<byte> ReadUTF8(ref int position)
        {
            ReadOnlySpan<byte> value = Format.ReadUTF8(_source.Slice(position), out int size);
            position += size;
            return value;
        }
        public ReadOnlySpan<byte> ReadUTF8(int position = -1)
        {
            return ReadUTF8(ref position > -1 ? ref position : ref _position);
        }

        public ReadOnlySpan<char> ReadUTF16(ref int position)
        {
            ReadOnlySpan<byte> utf8Bytes = ReadUTF8(ref position);
            var value = new Span<char>(new char[utf8Bytes.Length]);

            Utf8.ToUtf16(utf8Bytes, value, out _, out _);
            return value;
        }
        public ReadOnlySpan<char> ReadUTF16(int position = -1)
        {
            return ReadUTF16(ref position > -1 ? ref position : ref _position);
        }
    }
}