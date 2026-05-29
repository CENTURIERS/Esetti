using Models.Users;
using Models.Activities;
using Models.Other;
using Models.University;
using System.Collections.Generic;

namespace Models.ClubBase
{
    public class ClubInfo
    {
        public int ClubId { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public int? DepartmentId { get; set; }
        public CollegeDepartment? Department { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(100)]
        public string? ClubRoom { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(150)]
        public string? SupervisorName { get; set; }

        [System.ComponentModel.DataAnnotations.EmailAddress]
        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string? SupervisorEmail { get; set; }

        [System.ComponentModel.DataAnnotations.Phone]
        [System.ComponentModel.DataAnnotations.MaxLength(20)]
        public string? SupervisorPhone { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string? MeetingsSchedule { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(50)]
        public string? ShortName { get; set; }
        public byte[]? ClubPhoto { get; set; }

        public List<MemberClub> MemberClubs { get; set; } = new();
        public List<Project> Projects { get; set; } = new();
        public List<Trip> Trips { get; set; } = new();
    }
}



