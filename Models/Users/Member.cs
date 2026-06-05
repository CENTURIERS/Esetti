using Models.Activities;
using Models.ClubBase;
using Models.University;
using System;
using System.Collections.Generic;

namespace Models.Users
{
    /// <summary>
    /// Klasa reprezentująca członka koła naukowego. Taki typowy student zapisany do organizacji.
    /// </summary>
    public class Member
    {
        /// <summary>
        /// ID członka - klucz główny w bazie.
        /// </summary>
        public int MemberId { get; set; }

        /// <summary>
        /// ID konta użytkownika - klucz obcy, może być nullem jak nie ma jeszcze konta.
        /// </summary>
        public int? AccountId { get; set; }

        /// <summary>
        /// Referencja do konta użytkownika (relacja z tabelą kont).
        /// </summary>
        public UserAccount? Account { get; set; }

        /// <summary>
        /// Rola uprawnień przypisana do tego członka.
        /// </summary>
        public AuthorityRole? AuthorityRole { get; set; }

        /// <summary>
        /// ID roli uprawnień - klucz obcy.
        /// </summary>
        public int? RoleId { get; set; }

        /// <summary>
        /// Numer indeksu studenta (maksymalnie 20 znaków).
        /// </summary>
        [System.ComponentModel.DataAnnotations.MaxLength(20)]
        public string? IndexNumber { get; set; }

        /// <summary>
        /// Imię członka koła. Wymagane pole.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Nazwisko członka koła. Też wymagane.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Kierunek studiów (np. Informatyka albo Mechatronika).
        /// </summary>
        [System.ComponentModel.DataAnnotations.MaxLength(100)]
        public string? Major { get; set; }

        /// <summary>
        /// Numer telefonu studenta.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Phone]
        [System.ComponentModel.DataAnnotations.MaxLength(20)]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Awatar członka jako tablica bajtów (zdjęcie profilowe).
        /// </summary>
        public byte[]? MemberAvatar { get; set; }

        /// <summary>
        /// Krótki opis/biografia członka koła.
        /// </summary>
        [System.ComponentModel.DataAnnotations.MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Flaga czy aktywny członek (czy jeszcze z nami jest, czy już odszedł).
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Data dołączenia do koła naukowego.
        /// </summary>
        public DateTime JoinDate { get; set; }

        /// <summary>
        /// Relacja wiele do wielu - kluby/koła do których ten członek należy.
        /// </summary>
        public List<MemberClub> MemberClubs { get; set; } = new();

        /// <summary>
        /// Relacja wiele do wielu - powiązanie członka z sekcjami.
        /// </summary>
        public List<SectionMember> SectionMembers { get; set; } = new();

        /// <summary>
        /// Lista projektów, w których student bierze udział.
        /// </summary>
        public List<Project> Projects { get; set; } = new();

        /// <summary>
        /// Lista aktywności powiązanych z tym członkiem.
        /// </summary>
        public List<Activity> Activities { get; set; } = new();
    }
}

