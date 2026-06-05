using Models.Users;
using System;
using System.Collections.Generic;

namespace Models.Activities
{
    /// <summary>
    /// Klasa reprezentująca konkretną sekcję w kole naukowym (np. sekcja programistyczna, sekcja marketingu).
    /// </summary>
    public class Section
    {
        /// <summary>
        /// ID sekcji - klucz główny w bazie.
        /// </summary>
        public int SectionId { get; set; }

        /// <summary>
        /// Pełna nazwa sekcji. Wymagane pole.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Skrócona nazwa sekcji (np. 'NET', 'AI', 'HR'). Też wymagane.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(50)]
        public string ShortName { get; set; } = string.Empty;

        /// <summary>
        /// Informacje o spotkaniach sekcji (np. 'czwartki o 17:00 w sali 102').
        /// </summary>
        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string? Meetings { get; set; }

        /// <summary>
        /// Data utworzenia sekcji.
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Flaga czy sekcja jest aktywna i nadal działa.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Powiązanie członków z sekcją (relacja wiele do wielu przez tabelę pośredniczącą).
        /// </summary>
        public List<SectionMember> SectionMembers { get; set; } = new();

        /// <summary>
        /// Projekty realizowane przez tę sekcję (relacja wiele do wielu).
        /// </summary>
        public List<Project> Projects { get; set; } = new();
    }
}

