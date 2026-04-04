using Models.Users;
using System.Collections.Generic;

namespace Models.University
{
    public class College
    {
        public int CollegeId { get; set; }

        public string? Name { get; set; }
        public string? NameShort { get; set; }

        public byte[]? CollegeAvatar { get; set; }

        public string? AddressLine { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? Phone { get; set; }
        public string? NIP { get; set; }

        public List<UserAccount> UserAccounts { get; set; } = new();
    }
}