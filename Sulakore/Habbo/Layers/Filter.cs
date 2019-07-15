using System.Runtime.Serialization;

namespace Sulakore.Habbo.Layers
{
    [DataContract(Name = "filter")]
    public class Filter
    {
        [DataMember(Name = "alpha")]
        public int Alpha { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }
    }
}