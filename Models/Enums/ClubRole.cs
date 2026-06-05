namespace Models.Enums
{
    /// <summary>
    /// Rola, jaką student pełni bezpośrednio w danym kole naukowym (klubie).
    /// </summary>
    public enum ClubRole
    {
        /// <summary>
        /// Zwykły członek koła naukowego.
        /// </summary>
        Member = 0,

        /// <summary>
        /// Członek zarządu koła (np. skarbnik, sekretarz).
        /// </summary>
        BoardMember = 1,

        /// <summary>
        /// Opiekun naukowy koła (zazwyczaj nauczyciel akademicki).
        /// </summary>
        Supervisor = 2,

        /// <summary>
        /// Wiceprezes koła naukowego.
        /// </summary>
        VicePresident = 3,

        /// <summary>
        /// Prezes koła naukowego (szef całego zamieszania).
        /// </summary>
        President = 4
    }
}


