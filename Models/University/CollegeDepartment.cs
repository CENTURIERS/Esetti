namespace Models.University
{
    /// <summary>
    /// Klasa reprezentująca wydział uczelni (np. Wydział Informatyki i Zarządzania).
    /// </summary>
    public class CollegeDepartment
    {
        /// <summary>
        /// ID wydziału - klucz główny.
        /// </summary>
        public int CollegeDepartmentId { get; set; }
        
        /// <summary>
        /// ID uczelni macierzystej - klucz obcy.
        /// </summary>
        public int CollegeId { get; set; }

        /// <summary>
        /// Referencja do uczelni, do której należy ten wydział (relacja).
        /// </summary>
        public College? College { get; set; }

        /// <summary>
        /// Nazwa wydziału. Wymagane pole.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(300)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Adres wydziału (ulica, numer).
        /// </summary>
        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string? AddressLine { get; set; }

        /// <summary>
        /// Miasto, w którym znajduje się wydział.
        /// </summary>
        [System.ComponentModel.DataAnnotations.MaxLength(100)]
        public string? City { get; set; }

        /// <summary>
        /// Kod pocztowy wydziału (format XX-XXX).
        /// </summary>
        [System.ComponentModel.DataAnnotations.RegularExpression(@"^\d{2}-\d{3}$")]
        public string? PostalCode { get; set; }

        /// <summary>
        /// Numer telefonu kontaktowego do dziekanatu/wydziału.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Phone]
        [System.ComponentModel.DataAnnotations.MaxLength(20)]
        public string? Phone { get; set; }

        /// <summary>
        /// E-mail kontaktowy do wydziału.
        /// </summary>
        [System.ComponentModel.DataAnnotations.EmailAddress]
        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string? Email { get; set; }
    }
}


