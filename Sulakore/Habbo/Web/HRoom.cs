using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Sulakore.Habbo.Web
{
    [DebuggerDisplay("Name: {Name}")]
    public class HRoom
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public DateTime CreationTime { get; set; }

        public string HabboGroupId { get; set; }

        public IEnumerable<string> Tags { get; set; }
        public int MaximumVisitors { get; set; }

        public bool ShowOwnerName { get; set; }

        public string OwnerName { get; set; }
        public string OwnerUniqueId { get; set; }
        public IEnumerable<string> Categories { get; set; }
        public int Rating { get; set; }
        public string ThumbnailUrl { get; set; }
        public string ImageUrl { get; set; }
        public string UniqueId { get; set; }
    }
}