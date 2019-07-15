using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Sulakore.Habbo.Layers
{
    [DataContract(Name = "texCol")]
    public class TexCol
    {
        private List<string> _assetNames;
        [DataMember(Name = "assetNames")]
        public List<string> AssetNames
        {
            get => _assetNames ?? (_assetNames = new List<string>());
        }
    }
}