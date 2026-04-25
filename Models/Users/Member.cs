using Models.Activities;
using Models.ClubBase;
using Models.University;
using System;
using System.Collections.Generic;

namespace Models.Users
{
    public class Member
    {
        public int MemberId { get; set; }
        public int? AccountId { get; set; }
        public UserAccount? Account { get; set; }
        public AuthorityRole? AuthorityRole { get; set; }
        public int? RoleId { get; set; }
        public string? IndexNumber { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Major { get; set; }
        public string? PhoneNumber { get; set; }
        public byte[]? MemberAvatar { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime JoinDate { get; set; } = DateTime.Now;
        public List<MemberClub> MemberClubs { get; set; } = new();
        public List<SectionMember> SectionMembers { get; set; } = new();
        public List<Project> Projects { get; set; } = new();
        public List<Activity> Activities { get; set; } = new();
    }
}