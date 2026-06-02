using Models.Users;
using Models.ClubBase;
using System;
using System.Collections.Generic;

namespace Models.Activities
{
    public class Project : System.ComponentModel.DataAnnotations.IValidatableObject
    {
        public int ProjectId { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public string? AdditionalInformation { get; set; }

        public int? PersonInChargeId { get; set; }
        public Member? PersonInCharge { get; set; }

        [System.ComponentModel.DataAnnotations.Url]
        public string? Github { get; set; }

        public int? EstimatedTime { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }

        public bool IsActive { get; set; } = true;
        
        public List<Member> Participants { get; set; } = new();
        public List<Section> Sections { get; set; } = new();
        public List<ClubInfo> Clubs { get; set; } = new();

        public IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> Validate(System.ComponentModel.DataAnnotations.ValidationContext validationContext)
        {
            if (DateStart.HasValue && DateEnd.HasValue && DateEnd.Value < DateStart.Value)
            {
                yield return new System.ComponentModel.DataAnnotations.ValidationResult(
                    "Data zakończenia projektu nie może być wcześniejsza niż data rozpoczęcia.",
                    new[] { nameof(DateEnd) });
            }
        }
    }
}


