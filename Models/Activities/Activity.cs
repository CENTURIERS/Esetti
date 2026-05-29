using Models.Users;
using System;
using System.Collections.Generic;

namespace Models.Activities
{
    public class Activity
    {
        public int ActivityId { get; set; }
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.MaxLength(500)]
        public string? AddressLine { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(100)]
        public string? City { get; set; }

        [System.ComponentModel.DataAnnotations.RegularExpression(@"^\d{2}-\d{3}$")]
        public string? PostalCode { get; set; }

        public DateTime Date { get; set; }
        public TimeSpan? Time { get; set; }

        public string? PersonInChargeName { get; set; }

        [System.ComponentModel.DataAnnotations.Phone]
        public string? PersonInChargePhone { get; set; }

        [System.ComponentModel.DataAnnotations.EmailAddress]
        public string? PersonInChargeEmail { get; set; }
        public string? AdditionalInformation { get; set; }

        public bool IsRepeatable { get; set; }
        public bool IsActive { get; set; } = true;

        public List<Member> Participants { get; set; } = new();
    }
}

