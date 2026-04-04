using Models.Users;
using System;
using System.Collections.Generic;

namespace Models.Activities
{
    public class Activity
    {
        public int ActivityId { get; set; }
        public string Name { get; set; } = string.Empty;

        public string? AddressLine { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }

        public DateTime Date { get; set; }
        public TimeSpan? Time { get; set; }

        public string? PersonInChargeName { get; set; }
        public string? PersonInChargePhone { get; set; }
        public string? PersonInChargeEmail { get; set; }
        public string? AdditionalInformation { get; set; }

        public bool IsRepeatable { get; set; }

        public List<Member> Participants { get; set; } = new();
    }
}