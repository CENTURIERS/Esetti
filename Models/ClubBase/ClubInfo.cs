using Models.Users;
using Models.Activities;
using Models.Other;
using Models.University;
using System.Collections.Generic;

namespace Models.ClubBase
{
    /// <summary>
    /// Klasa z informacjami o konkretnym kole naukowym (lub klubie studenckim).
    /// </summary>
    public class ClubInfo
    {
        /// <summary>
        /// ID koła naukowego - klucz główny.
        /// </summary>
        public int ClubId { get; set; }

        /// <summary>
        /// Nazwa koła naukowego. Wymagane pole.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// ID wydziału uczelni, przy którym koło działa - klucz obcy (może być nullem).
        /// </summary>
        public int? DepartmentId { get; set; }

        /// <summary>
        /// Wydział uczelni, na którym działa to koło (relacja).
        /// </summary>
        public CollegeDepartment? Department { get; set; }

        /// <summary>
        /// Pokój/sala, w której koło ma swoją siedzibę.
        /// </summary>
        [System.ComponentModel.DataAnnotations.MaxLength(100)]
        public string? ClubRoom { get; set; }

        /// <summary>
        /// Imię i nazwisko opiekuna naukowego koła (np. jakiegoś doktora czy profesora).
        /// </summary>
        [System.ComponentModel.DataAnnotations.MaxLength(150)]
        public string? SupervisorName { get; set; }

        /// <summary>
        /// Adres e-mail opiekuna naukowego koła.
        /// </summary>
        [System.ComponentModel.DataAnnotations.EmailAddress]
        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string? SupervisorEmail { get; set; }

        /// <summary>
        /// Telefon kontaktowy do opiekuna koła.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Phone]
        [System.ComponentModel.DataAnnotations.MaxLength(20)]
        public string? SupervisorPhone { get; set; }

        /// <summary>
        /// Harmonogram spotkań koła (np. 'każdy wtorek o 18:00').
        /// </summary>
        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string? MeetingsSchedule { get; set; }

        /// <summary>
        /// Skrócona nazwa koła (np. KN Solide, KNE).
        /// </summary>
        [System.ComponentModel.DataAnnotations.MaxLength(50)]
        public string? ShortName { get; set; }

        /// <summary>
        /// Zdjęcie profilowe koła naukowego w postaci tablicy bajtów.
        /// </summary>
        public byte[]? ClubPhoto { get; set; }

        /// <summary>
        /// Powiązanie koła z członkami (relacja wiele do wielu za pomocą tabeli pośredniczącej).
        /// </summary>
        public List<MemberClub> MemberClubs { get; set; } = new();

        /// <summary>
        /// Projekty, które są prowadzone przez to koło (relacja).
        /// </summary>
        public List<Project> Projects { get; set; } = new();

        /// <summary>
        /// Wyjazdy integracyjne lub naukowe organizowane przez koło.
        /// </summary>
        public List<Trip> Trips { get; set; } = new();
    }
}



