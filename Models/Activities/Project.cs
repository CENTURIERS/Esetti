using Models.Users;
using Models.ClubBase;
using System;
using System.Collections.Generic;

namespace Models.Activities
{
    public class Project
    {
        public int ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public string? AdditionalInformation { get; set; }

        public int? PersonInChargeId { get; set; }
        public Member? PersonInCharge { get; set; }

        public string? Github { get; set; }

        public int? EstimatedTime { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }

        public bool IsActive { get; set; }
        
        public List<Member> Participants { get; set; } = new();
        public List<Section> Sections { get; set; } = new();
        public List<ClubInfo> Clubs { get; set; } = new();
    }
}
