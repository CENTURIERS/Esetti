namespace Models.Enums
{
    /// <summary>
    /// Rola konta w całym systemie (odpowiedzialna za globalne uprawnienia).
    /// </summary>
    public enum SystemRole
    {
        /// <summary>
        /// Główny administrator całego systemu (taki root, może wszystko).
        /// </summary>
        SuperAdmin = 0,

        /// <summary>
        /// Administrator na poziomie konkretnej uczelni.
        /// </summary>
        CollegeAdmin = 1,

        /// <summary>
        /// Zwykły zalogowany użytkownik systemu (np. student).
        /// </summary>
        User = 2
    }
}


