using System.Runtime.Serialization;

namespace Sulakore.Generators;

[DataContract]
public sealed class Messages
{
    [DataMember(Name = "outgoing")]
    public Message[] Outgoing { get; set; }


    [DataMember(Name = "incoming")]
    public Message[] Incoming { get; set; }
}