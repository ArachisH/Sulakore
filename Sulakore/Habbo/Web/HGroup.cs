using System.Diagnostics;

namespace Sulakore.Habbo.Web
{
    [DebuggerDisplay("Name: {Name}")]
    public class HGroup
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string RoomId { get; set; }
        public string BadgeCode { get; set; }
        public string PrimaryColor { get; set; }
        public string SecondaryColor { get; set; }
        public bool IsAdmin { get; set; }
    }
}