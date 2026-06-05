using Models.ClubBase;
using System;
using System.Collections.Generic;

namespace Models.Other
{
    /// <summary>
    /// Klasa reprezentująca wyjazd organizowany przez koło (np. konferencja naukowa, integracja w górach).
    /// </summary>
    public class Trip
    {
        /// <summary>
        /// ID wyjazdu - klucz główny w bazie danych.
        /// </summary>
        public int TripId { get; set; }

        /// <summary>
        /// Nazwa lub cel wyjazdu. Wymagane pole.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Krótki opis wyjazdu (co będziemy robić, jaki jest plan).
        /// </summary>
        [System.ComponentModel.DataAnnotations.MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Zdjęcie z wyjazdu (np. grupowe fotki w formie tablicy bajtów).
        /// </summary>
        public byte[]? TripPhoto { get; set; }

        /// <summary>
        /// Data wyjazdu.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Lista kół naukowych uczestniczących w wyjeździe (relacja wiele do wielu).
        /// </summary>
        public List<ClubInfo> Clubs { get; set; } = new();
    }
}


