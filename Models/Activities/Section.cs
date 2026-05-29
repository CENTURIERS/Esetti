using Models.Users;
using System;
using System.Collections.Generic;

namespace Models.Activities
{
    public class Section
    {
        public int SectionId { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(50)]
        public string ShortName { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string? Meetings { get; set; }

        public DateTime? CreatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        public List<SectionMember> SectionMembers { get; set; } = new();
        public List<Project> Projects { get; set; } = new();
    }
}

