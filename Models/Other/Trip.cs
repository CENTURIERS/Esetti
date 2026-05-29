using Models.ClubBase;
using System;
using System.Collections.Generic;

namespace Models.Other
{
    public class Trip
    {
        public int TripId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public byte[]? TripPhoto { get; set; }
        public DateTime Date { get; set; }

        public List<ClubInfo> Clubs { get; set; } = new();
    }
}
