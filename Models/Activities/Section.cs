using Models.Users;
using System;
using System.Collections.Generic;

namespace Models.Activities
{
    public class Section
    {
        public int SectionId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;

        public string? Meetings { get; set; }

        public DateTime? CreatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        public List<SectionMember> SectionMembers { get; set; } = new();
        public List<Project> Projects { get; set; } = new();
    }
}