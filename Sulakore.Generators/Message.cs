using System.Runtime.Serialization;

namespace Sulakore.Generators;

[DataContract]
public sealed class Message
{
    [DataMember(Name = "name")]
    public string Name { get; set; }

    [DataMember(Name = "unityId")]
    public short UnityId { get; set; }

    [DataMember(Name = "unityStructure")]
    public string UnityStructure { get; set; }

    [DataMember(Name = "postShuffleHashes")]
    public uint[] PostShuffleHashes { get; set; }
}