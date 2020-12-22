using System;
using System.Collections.Generic;

namespace Sulakore.Habbo.Web
{
    [System.Diagnostics.DebuggerDisplay("Name: {Name}")]
    public class HUser
    {
        public string UniqueId { get; set; }
        public string Name { get; set; }
        public string FigureString { get; set; }
        public string Motto { get; set; }
        public bool Online { get; set; }

        public bool? BuildersClubMember { get; set; }
        public bool? HabboClubMember { get; set; }

        public DateTime? LastWebAccess { get; set; }
        public DateTime? CreationTime { get; set; }
        public DateTime? MemberSince { get; set; }

        public long SessionLogId { get; set; }
        public long LoginLogId { get; set; }

        public string Email { get; set; }
        public long IdentityId { get; set; }

        public bool EmailVerified { get; set; }
        public bool? IdentityVerified { get; set; }
        public string IdentityType { get; set; }
        public bool? Trusted { get; set; }

        public IEnumerable<string> Force { get; set; }
        public int AccountId { get; set; }
        public string Country { get; set; }
        public IEnumerable<string> Traits { get; set; }
        public string Partner { get; set; }

        public bool? ProfileVisible { get; set; }

        public IEnumerable<HBadge> SelectedBadges { get; set; }

        public bool? Banned { get; set; }

        public int? CurrentLevel { get; set; }
        public int? CurrentLevelCompletePercent { get; set; }
        public int? TotalExperience { get; set; }
        
        public int? StarGemCount { get; set; }
    }
}