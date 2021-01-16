using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Sulakore.Network.Protocol
{
    //public class HPacket
    //{
    //    private static readonly Regex _structurePattern;

    //    private readonly List<byte> _body;
    //    private readonly Memory<byte> _packet;
    //    private readonly ReadOnlyMemory<byte> asd;

    //    private int _position;
    //    public int Position
    //    {
    //        get => _position;
    //        set => _position = value;
    //    }

    //    public short Id
    //    {
    //        get => Format.ReadId(_packet.Span);
    //        set
    //        {

    //        }
    //    }

    //    public IFormat Format { get; }
    //    public int BodyLength => _body.Count;
    //    public int ReadableBytes => GetReadableBytes(Position);

    //    static HPacket()
    //    {
    //        _structurePattern = new Regex(@"{(?<kind>id|i|s|b|d|u):(?<value>[^}]*)\}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    //    }
    //    public HPacket(IFormat format)
    //    {
    //        _packet = null;
    //        _position = 0;
    //        _body = new List<byte>();

    //        Format = format;
    //    }
    //    public HPacket(IFormat format, IList<byte> data)
    //        : this(format)
    //    {
    //        //_body.AddRange(format.GetBody(data));

    //        //_toBytesCache = new byte[data.Count];
    //        //data.CopyTo(_toBytesCache, 0);

    //        //Id = format.GetId(data);
    //    }

    //    #region Read Methods
    //    public int ReadInt32()
    //    {
    //        return ReadInt32(ref _position);
    //    }
    //    public int ReadInt32(int position)
    //    {
    //        return ReadInt32(ref position);
    //    }
    //    public virtual int ReadInt32(ref int position)
    //    {
    //        Position = 1;
    //        //int value = Format.ReadInt32(_body, position);
    //        //position += Format.GetSize(value);
    //        return 0;
    //    }

    //    public string ReadUTF8()
    //    {
    //        return ReadUTF8(ref _position);
    //    }
    //    public string ReadUTF8(int position)
    //    {
    //        return ReadUTF8(ref position);
    //    }
    //    public virtual string ReadUTF8(ref int position)
    //    {
    //        //string value = Format.ReadUTF8(_body, position);
    //        //position += Format.GetSize(value);
    //        return "";
    //    }

    //    public bool ReadBoolean()
    //    {
    //        return ReadBoolean(ref _position);
    //    }
    //    public bool ReadBoolean(int position)
    //    {
    //        return ReadBoolean(ref position);
    //    }
    //    public virtual bool ReadBoolean(ref int position)
    //    {
    //        //bool value = Format.ReadBoolean(_body, position);
    //        //position += Format.GetSize(value);
    //        return true;
    //    }

    //    public ushort ReadUInt16()
    //    {
    //        return ReadUInt16(ref _position);
    //    }
    //    public ushort ReadUInt16(int position)
    //    {
    //        return ReadUInt16(ref position);
    //    }
    //    public virtual ushort ReadUInt16(ref int position)
    //    {
    //        //ushort value = Format.ReadUInt16(_body, position);
    //        //position += Format.GetSize(value);
    //        return 6;
    //    }

    //    public double ReadDouble()
    //    {
    //        return ReadDouble(ref _position);
    //    }
    //    public double ReadDouble(int position)
    //    {
    //        return ReadDouble(ref position);
    //    }
    //    public virtual double ReadDouble(ref int position)
    //    {
    //        //double value = Format.ReadDouble(_body, position);
    //        //position += Format.GetSize(value);
    //        return 1;
    //    }

    //    public byte ReadByte()
    //    {
    //        return ReadByte(ref _position);
    //    }
    //    public byte ReadByte(int position)
    //    {
    //        return ReadByte(ref position);
    //    }
    //    public virtual byte ReadByte(ref int position)
    //    {
    //        return _body[position++];
    //    }

    //    public byte[] ReadBytes(int length)
    //    {
    //        return ReadBytes(length, ref _position);
    //    }
    //    public byte[] ReadBytes(int length, int position)
    //    {
    //        return ReadBytes(length, ref position);
    //    }
    //    public virtual byte[] ReadBytes(int length, ref int position)
    //    {
    //        var chunk = new byte[length];
    //        for (int i = 0; i < length; i++)
    //        {
    //            chunk[i] = _body[position++];
    //        }
    //        return chunk;
    //    }
    //    #endregion

    //    #region Write Methods
    //    public void Write(int value)
    //    {
    //        Write(value, _body.Count);
    //    }
    //    public void Write(int value, int position)
    //    {
    //        //Write(Format.ReadBytes(value), position);
    //    }

    //    public void Write(string value)
    //    {
    //        Write(value, _body.Count);
    //    }
    //    public void Write(string value, int position)
    //    {
    //        //Write(Format.WriteString(value), position);
    //    }

    //    public void Write(bool value)
    //    {
    //        Write(value, _body.Count);
    //    }
    //    public void Write(bool value, int position)
    //    {
    //        //Write(Format.WriteBoolean(value), position);
    //    }

    //    public void Write(ushort value)
    //    {
    //        Write(value, _body.Count);
    //    }
    //    public void Write(ushort value, int position)
    //    {
    //        //Write(Format.WriteUInt16(value), position);
    //    }

    //    public void Write(double value)
    //    {
    //        Write(value, _body.Count);
    //    }
    //    public void Write(double value, int position)
    //    {
    //        //Write(Format.WriteDouble(value), position);
    //    }

    //    public void Write(byte value)
    //    {
    //        Write(value, _body.Count);
    //    }
    //    public void Write(byte value, int position)
    //    {
    //        Write(new[] { value }, position);
    //    }

    //    public void Write(byte[] value)
    //    {
    //        Write(value, _body.Count);
    //    }
    //    public void Write(byte[] value, int position)
    //    {
    //        _body.InsertRange(position, value);
    //        ResetCache();
    //    }
    //    #endregion

    //    #region Replace Methods
    //    public void Replace(int value)
    //    {
    //        Replace(value, Position);
    //    }
    //    public void Replace(int value, int position)
    //    {
    //        //byte[] data = Format.ReadBytes(value);
    //        //Replace(data, position);
    //    }

    //    public void Replace(string value)
    //    {
    //        Replace(value, Position);
    //    }
    //    public void Replace(string value, int position)
    //    {
    //        //byte[] data = Format.WriteString(value);
    //        //int removeLength = ReadUInt16(position);

    //        //Replace(data, removeLength + 2, position);
    //    }

    //    public void Replace(bool value)
    //    {
    //        Replace(value, Position);
    //    }
    //    public void Replace(bool value, int position)
    //    {
    //        //byte[] data = Format.WriteBoolean(value);
    //        //Replace(data, position);
    //    }

    //    public void Replace(ushort value)
    //    {
    //        Replace(value, Position);
    //    }
    //    public void Replace(ushort value, int position)
    //    {
    //        //byte[] data = Format.WriteUInt16(value);
    //        //Replace(data, position);
    //    }

    //    public void Replace(double value)
    //    {
    //        Replace(value, Position);
    //    }
    //    public void Replace(double value, int position)
    //    {
    //        //byte[] data = Format.WriteDouble(value);
    //        //Replace(data, position);
    //    }

    //    public void Replace(byte value)
    //    {
    //        Replace(value, Position);
    //    }
    //    public void Replace(byte value, int position)
    //    {
    //        _body[position] = value;
    //    }

    //    public void Replace(byte[] value)
    //    {
    //        Replace(value);
    //    }
    //    public void Replace(byte[] value, int position)
    //    {
    //        Replace(value, value.Length, position);
    //    }
    //    public void Replace(byte[] value, int length, int position)
    //    {
    //        _body.RemoveRange(position, length);
    //        _body.InsertRange(position, value);

    //        ResetCache();
    //    }
    //    #endregion

    //    private void ResetCache()
    //    {

    //    }
    //    public int GetReadableBytes(int position)
    //    {
    //        return _body.Count - position;
    //    }

    //    public byte[] ToBytes()
    //    {
    //        return null;
    //    }
    //    public override string ToString()
    //    {
    //        string result = Encoding.Latin1.GetString(ToBytes());
    //        for (int i = 0; i <= 13; i++)
    //        {
    //            result = result.Replace(((char)i).ToString(),
    //                "[" + i + "]");
    //        }
    //        return result;
    //    }

    //    public bool Equals(HPacket packet)
    //    {
    //        if (packet.Id != Id) return false;
    //        if (packet.BodyLength != BodyLength) return false;
    //        return packet.ToBytes().SequenceEqual(ToBytes());
    //    }

    //    public static byte[] ToBytes(IFormat format, string signature)
    //    {
    //        MatchCollection matches = _structurePattern.Matches(signature);
    //        if (matches.Count == 0)
    //        {
    //            for (int i = 0; i <= 13; i++)
    //            {
    //                signature = signature.Replace("[" + i + "]",
    //                    ((char)i).ToString());
    //            }
    //            return Encoding.Latin1.GetBytes(signature);
    //        }
    //        else
    //        {
    //            ushort id = 0;
    //            var values = new List<object>(matches.Count);
    //            foreach (Match match in matches)
    //            {
    //                string value = match.Groups["value"].Value;
    //                string kind = match.Groups["kind"].Value.ToLower();
    //                switch (kind)
    //                {
    //                    case "id":
    //                    {
    //                        if (!ushort.TryParse(value, out id))
    //                        {
    //                            throw new ArgumentException("Unable to locate the '{id:N}' parameter.", nameof(signature));
    //                        }
    //                        break;
    //                    }
    //                    case "i": values.Add(int.Parse(value)); break;
    //                    case "s": values.Add(value); break;
    //                    case "b":
    //                    {
    //                        value = value.Trim().ToLower();
    //                        if (!byte.TryParse(value, out byte bValue))
    //                        {
    //                            values.Add(value == "true");
    //                        }
    //                        else values.Add(bValue);
    //                        break;
    //                    }
    //                    case "d": values.Add(double.Parse(value)); break;
    //                    case "u": values.Add(ushort.Parse(value)); break;
    //                }
    //            }
    //            return null;
    //            //return format.Construct(id, values.ToArray());
    //        }
    //    }
    //}
}