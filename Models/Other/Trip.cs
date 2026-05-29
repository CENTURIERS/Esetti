using Models.ClubBase;
using System;
using System.Collections.Generic;

namespace Models.Other
{
    public class Trip
    {
        public int TripId { get; set; }
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.MaxLength(1000)]
        public string? Description { get; set; }
        public byte[]? TripPhoto { get; set; }
        public DateTime Date { get; set; }

        public List<ClubInfo> Clubs { get; set; } = new();
    }
}


