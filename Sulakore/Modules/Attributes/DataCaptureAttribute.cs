using System.Reflection;

using Sulakore.Habbo;
using Sulakore.Network;

namespace Sulakore.Modules
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public abstract class DataCaptureAttribute : Attribute, IEquatable<DataCaptureAttribute>
    {
        public short Id { get; }
        public uint Hash { get; }
        public abstract bool IsOutgoing { get; }

        internal object Target { get; set; }
        internal MethodInfo Method { get; set; }

        public static bool operator !=(DataCaptureAttribute left, DataCaptureAttribute right) => !(left == right);
        public static bool operator ==(DataCaptureAttribute left, DataCaptureAttribute right) => EqualityComparer<DataCaptureAttribute>.Default.Equals(left, right);

        public DataCaptureAttribute(short id)
        {
            Id = id;
        }
        public DataCaptureAttribute(uint hash)
        {
            Hash = hash;
        }

        internal void Invoke(DataInterceptedEventArgs args)
        {
            object[] parameters = CreateValues(args);
            object result = Method?.Invoke(Target, parameters);
            switch (result)
            {
                case bool isBlocked:
                {
                    args.IsBlocked = isBlocked;
                    break;
                }
            }
        }
        private object[] CreateValues(DataInterceptedEventArgs args)
        {
            ParameterInfo[] parameters = Method.GetParameters();
            var values = new object[parameters.Length];

            int position = 0;
            var packet = new HReadOnlyPacket(args.Packet.Span, args.Format);
            for (int i = 0; i < values.Length; i++)
            {
                ParameterInfo parameter = parameters[i];
                if (parameter.Name.Equals("id", StringComparison.OrdinalIgnoreCase))
                {
                    values[i] = packet.Id;
                }
                values[i] = Type.GetTypeCode(parameter.ParameterType) switch
                {
                    TypeCode.Byte => packet[position++],
                    TypeCode.Int32 => packet.ReadInt32(ref position),
                    TypeCode.Int16 => packet.ReadInt16(ref position),
                    TypeCode.String => packet.ReadUTF8(ref position),
                    TypeCode.Single => packet.ReadFloat(ref position),
                    TypeCode.Double => packet.ReadDouble(ref position),
                    TypeCode.Boolean => packet.ReadBoolean(ref position),
                    _ => CreateUnknownValueType(packet, ref position, args, parameter.ParameterType)
                };
            }
            return values;
        }
        private object CreateUnknownValueType(HReadOnlyPacket packet, ref int position, DataInterceptedEventArgs args, Type parameterType) => parameterType.Name switch
        {
            nameof(DataInterceptedEventArgs) => args,
            nameof(ReadOnlyMemory<byte>) => args.Packet,
            nameof(HPoint) => new HPoint(packet.ReadInt32(ref position), packet.ReadInt32(ref position)),

            _ => null
        };

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Identifier, IsOutgoing, Method);
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as DataCaptureAttribute);

        }
        public bool Equals(DataCaptureAttribute other)
        {
            return other != null &&
                Id == other.Id &&
                Identifier == other.Identifier &&
                IsOutgoing == other.IsOutgoing &&
                EqualityComparer<MethodInfo>.Default.Equals(Method, other.Method);
        }
    }
}