using System.Runtime.Serialization;

namespace Sulakore.Habbo.Layers
{
    [DataContract(Name = "sprite")]
    public class Sprite
    {
       // [DataMember(Name = "flipH", IsRequired = false)]
       // public bool FlipHorizontally { get; set; }

        [DataMember(Name = "x", Order = 0)]
        public int X { get; set; }

        [DataMember(Name = "width", Order = 1, IsRequired = false, EmitDefaultValue = false)]
        public int Width { get; set; }

        [DataMember(Name = "y", Order = 2)]
        public int Y { get; set; }

        [DataMember(Name = "height", Order = 3, IsRequired = false, EmitDefaultValue = false)]
        public int Height { get; set; }

        [DataMember(Name = "z", Order = 4)]
        public double Z { get; set; }

        [DataMember(Name = "frame", Order = 5, IsRequired = false, EmitDefaultValue = false)]
        public bool Frame { get; set; }

        [DataMember(Name = "skew", Order = 6, IsRequired = false, EmitDefaultValue = false)]
        public double Skew { get; set; }

        [DataMember(Name = "alpha", Order = 7, IsRequired = false, EmitDefaultValue = false)]
        public int Alpha { get; set; }

        [DataMember(Name = "color", Order = 8, IsRequired = false, EmitDefaultValue = false)]
        public int Color { get; set; }

        [DataMember(Name = "name", Order = 9)]
        public string Name { get; set; }

        [DataMember(Name = "blendMode", EmitDefaultValue = false)]
        public string BlendMode { get; set; }

        public override string ToString() => Name;
    }
}