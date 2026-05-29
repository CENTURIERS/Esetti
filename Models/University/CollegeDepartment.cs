namespace Models.University
{
    public class CollegeDepartment
    {
        public int CollegeDepartmentId { get; set; }
        
        public int CollegeId { get; set; }
        public College? College { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(300)]
        public string Name { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string? AddressLine { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(100)]
        public string? City { get; set; }

        [System.ComponentModel.DataAnnotations.RegularExpression(@"^\d{2}-\d{3}$")]
        public string? PostalCode { get; set; }

        [System.ComponentModel.DataAnnotations.Phone]
        [System.ComponentModel.DataAnnotations.MaxLength(20)]
        public string? Phone { get; set; }

        [System.ComponentModel.DataAnnotations.EmailAddress]
        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string? Email { get; set; }
    }
}


