using System.Drawing;
using System.Runtime.Serialization;

namespace Sulakore.Habbo.Layers
{
    [DataContract(Name = "mask")]
    public class Mask
    {
        [DataMember(Name = "flipH", Order = 0, IsRequired = false)]
        public bool FlipH { get; set; }

        [DataMember(Name = "location", Order = 1, IsRequired = false)]
        public Point Location { get; set; }

        [DataMember(Name = "flipV", Order = 2, IsRequired = false)]
        public bool FlipV { get; set; }

        [DataMember(Name = "name", Order = 3, IsRequired = false)]
        public string Name { get; set; }
    }
}