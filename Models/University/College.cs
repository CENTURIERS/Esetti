using Models.Users;
using Models.University;
using System.Collections.Generic;

namespace Models.University
{
    public class College
    {
        public int CollegeId { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(300)]
        public string Name { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.MaxLength(50)]
        public string? NameShort { get; set; }

        public byte[]? CollegeAvatar { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string? AddressLine { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(100)]
        public string? City { get; set; }

        [System.ComponentModel.DataAnnotations.RegularExpression(@"^\d{2}-\d{3}$")]
        public string? PostalCode { get; set; }

        [System.ComponentModel.DataAnnotations.Phone]
        [System.ComponentModel.DataAnnotations.MaxLength(20)]
        public string? Phone { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(15)]
        public string? NIP { get; set; }

        public List<CollegeDepartment> Departments { get; set; } = new();
        public List<UserAccount> UserAccounts { get; set; } = new();
    }
}

