using Models.Users;
using Models.University;
using System.Collections.Generic;

namespace Models.University
{
    /// <summary>
    /// Klasa reprezentująca uczelnię (np. politechnikę albo uniwerek).
    /// </summary>
    public class College
    {
        /// <summary>
        /// ID uczelni - klucz główny w bazie danych.
        /// </summary>
        public int CollegeId { get; set; }

        /// <summary>
        /// Pełna nazwa uczelni. Wymagane pole.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(300)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Skrócona nazwa uczelni (np. PWr, AGH, PW).
        /// </summary>
        [System.ComponentModel.DataAnnotations.MaxLength(50)]
        public string? NameShort { get; set; }

        /// <summary>
        /// Logo/avatar uczelni w postaci tablicy bajtów.
        /// </summary>
        public byte[]? CollegeAvatar { get; set; }

        /// <summary>
        /// Ulica i numer budynku uczelni.
        /// </summary>
        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string? AddressLine { get; set; }

        /// <summary>
        /// Miasto, w którym mieści się uczelnia.
        /// </summary>
        [System.ComponentModel.DataAnnotations.MaxLength(100)]
        public string? City { get; set; }

        /// <summary>
        /// Kod pocztowy uczelni (walidowany wyrażeniem regularnym typu XX-XXX).
        /// </summary>
        [System.ComponentModel.DataAnnotations.RegularExpression(@"^\d{2}-\d{3}$")]
        public string? PostalCode { get; set; }

        /// <summary>
        /// Numer telefonu kontaktowego do uczelni.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Phone]
        [System.ComponentModel.DataAnnotations.MaxLength(20)]
        public string? Phone { get; set; }

        /// <summary>
        /// NIP uczelni do rozliczeń (maksymalnie 15 znaków).
        /// </summary>
        [System.ComponentModel.DataAnnotations.MaxLength(15)]
        public string? NIP { get; set; }

        /// <summary>
        /// Wydziały należące do tej uczelni (relacja jeden do wielu).
        /// </summary>
        public List<CollegeDepartment> Departments { get; set; } = new();

        /// <summary>
        /// Konta użytkowników powiązane z tą uczelnią (relacja wiele do wielu).
        /// </summary>
        public List<UserAccount> UserAccounts { get; set; } = new();
    }
}

