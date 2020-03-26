using System.Diagnostics;

namespace Sulakore.Habbo.Web
{
    [DebuggerDisplay("Name: {Name}")]
    public class HBadge
    {
        public int BadgeIndex { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}