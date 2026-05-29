namespace Models.ClubBase
{
    public class AuthorityRole
    {
        public int RoleId { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.MaxLength(500)]
        public string? Description { get; set; }
        public int Permissions { get; set; } = 1;
    }
}

