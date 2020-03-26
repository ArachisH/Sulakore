using System.Diagnostics;
using System.Collections.Generic;

namespace Sulakore.Habbo.Web
{
    [DebuggerDisplay("Name: {User?.Name}")]
    public class HProfile
    {
        public HUser User { get; set; }
        public IEnumerable<HGroup> Groups { get; set; }
        public IEnumerable<HBadge> Badges { get; set; }
        public IEnumerable<HFriend> Friends { get; set; }
        public IEnumerable<HRoom> Rooms { get; set; }
    }
}