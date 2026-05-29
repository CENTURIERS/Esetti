using Models.Enums;
using Models.University;
using System;
using System.Collections.Generic;

namespace Models.Users
{
    public class UserAccount
    {
        public int AccountId { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.EmailAddress]
        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string PasswordHash { get; set; } = string.Empty;

        public SystemRole SystemRole { get; set; } = SystemRole.User;

        public bool IsVerified { get; set; } = false;

        public DateTime? CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<College> Colleges { get; set; } = new();
    }
}


