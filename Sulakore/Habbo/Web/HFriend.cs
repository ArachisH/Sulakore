using System.Diagnostics;

namespace Sulakore.Habbo.Web
{
    [DebuggerDisplay("Name: {Name}")]
    public class HFriend
    {
        public string Name { get; set; }
        public string Motto { get; set; }
        public string UniqueId { get; set; }
        public string FigureString { get; set; }
    }
}