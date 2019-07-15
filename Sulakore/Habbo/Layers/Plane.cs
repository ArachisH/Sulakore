using System.Drawing;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Sulakore.Habbo.Layers
{
    [DataContract(Name = "plane")]
    public class Plane
    {
        private List<Point> _cornerPoints;
        [DataMember(Name = "cornerPoints", Order = 2)]
        public List<Point> CornerPoints
        {
            get => _cornerPoints ?? (_cornerPoints = new List<Point>());
        }

        [DataMember(Name = "bottomAligned", Order = 4)]
        public bool IsBottomAligned { get; set; }

        private List<Mask> _masks;
        [DataMember(Name = "masks", Order = 5, EmitDefaultValue = false)]
        public List<Mask> Masks
        {
            get => _masks ?? (_masks = new List<Mask>());
        }

        private List<TexCol> _texCols;
        [DataMember(Name = "texCols", Order = 3)]
        public List<TexCol> TexCols
        {
            get => _texCols ?? (_texCols = new List<TexCol>());
        }

        [DataMember(Name = "z", Order = 1)]
        public double Z { get; set; }

        [DataMember(Name = "color", Order = 0)]
        public int Color { get; set; }
    }
}