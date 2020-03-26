using System;
using System.Diagnostics;
using System.Collections.Generic;

using System.Text.Json.Serialization;

namespace Sulakore.Habbo.Web
{
    [DebuggerDisplay("Name: {Name}")]
    public class HUser
    {
        public string UniqueId { get; set; }
        public string Name { get; set; }
        public string FigureString { get; set; }
        public string Motto { get; set; }

        [JsonPropertyName("buildersClubMember")]
        public bool? IsBuildersClubMember { get; set; }
        
        [JsonPropertyName("habboClubMember")]
        public bool? IsHabboClubMember { get; set; }

        public DateTime? LastWebAccess { get; set; }
        public DateTime? CreationTime { get; set; }
        public DateTime? MemberSince { get; set; }

        public long SessionLogId { get; set; }
        public long LoginLogId { get; set; }

        public string Email { get; set; }
        public long IdentityId { get; set; }

        [JsonPropertyName("emailVerified")]
        public bool? IsEmailVerified { get; set; }

        [JsonPropertyName("identityVerified")]
        public bool? IsIdentityVerified { get; set; }

        public string IdentityType { get; set; }

        [JsonPropertyName("trusted")]
        public bool? IsTrusted { get; set; }

        public IEnumerable<string> Force { get; set; }
        public int AccountId { get; set; }
        public string Country { get; set; }
        public IEnumerable<string> Traits { get; set; }
        public string Partner { get; set; }

        [JsonPropertyName("profileVisible")]
        public bool? IsProfileVisible { get; set; }

        public IEnumerable<HBadge> SelectedBadges { get; set; }

        [JsonPropertyName("banned")]
        public bool? IsBanned { get; set; }
    }
}