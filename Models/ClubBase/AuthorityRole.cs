namespace Models.ClubBase
{
    public class AuthorityRole
    {
        public int RoleId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Permissions { get; set; } = 1;
    }
}