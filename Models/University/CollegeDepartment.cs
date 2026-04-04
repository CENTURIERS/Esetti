namespace Models.University
{
    public class CollegeDepartment
    {
        public int CollegeDepartmentId { get; set; }
        
        public int CollegeId { get; set; }
        public College? College { get; set; }

        public string? Name { get; set; }

        public string? AddressLine { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }

        public string? Phone { get; set; }
        public string? Email { get; set; }
    }
}
