namespace Models.ClubBase
{
    /// <summary>
    /// Klasa określająca rolę uprawnień w organizacji (np. prezes, wiceprezes, skarbnik itp.).
    /// </summary>
    public class AuthorityRole
    {
        /// <summary>
        /// ID roli - klucz główny w bazie.
        /// </summary>
        public int RoleId { get; set; }

        /// <summary>
        /// Nazwa roli (np. 'Prezes', 'Skarbnik'). Wymagane pole.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Krótki opis roli (czym dana osoba się zajmuje).
        /// </summary>
        [System.ComponentModel.DataAnnotations.MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Wartość liczbowa reprezentująca uprawnienia (np. maska bitowa).
        /// </summary>
        public int Permissions { get; set; } = 1;
    }
}

