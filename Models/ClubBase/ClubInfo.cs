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

        public string? Name { get; set; }

        public int? DepartmentId { get; set; }
        public CollegeDepartment? Department { get; set; }

        public string? ClubRoom { get; set; }
        public string? SupervisorName { get; set; }
        public string? SupervisorEmail { get; set; }
        public string? SupervisorPhone { get; set; }
        public string? MeetingsSchedule { get; set; }
        public string? ShortName { get; set; }
        public byte[]? ClubPhoto { get; set; }

        public List<MemberClub> MemberClubs { get; set; } = new();
        public List<Project> Projects { get; set; } = new();
        public List<Trip> Trips { get; set; } = new();
    }
}

