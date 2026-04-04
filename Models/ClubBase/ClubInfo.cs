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

        public List<MemberClub> MemberClubs { get; set; } = new();
        public List<Project> Projects { get; set; } = new();
        public List<Trip> Trips { get; set; } = new();
    }
}

