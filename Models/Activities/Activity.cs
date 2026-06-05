using Models.Users;
using System;
using System.Collections.Generic;

namespace Models.Activities
{
    /// <summary>
    /// Klasa reprezentująca aktywność lub wydarzenie organizowane przez koło (np. warsztaty, szkolenie, wykład).
    /// </summary>
    public class Activity
    {
        /// <summary>
        /// ID aktywności - klucz główny w bazie.
        /// </summary>
        public int ActivityId { get; set; }

        /// <summary>
        /// Nazwa aktywności/wydarzenia. Pole wymagane.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Ulica i numer miejsca, w którym odbywa się wydarzenie.
        /// </summary>
        [System.ComponentModel.DataAnnotations.MaxLength(500)]
        public string? AddressLine { get; set; }

        /// <summary>
        /// Miasto, w którym odbywa się wydarzenie.
        /// </summary>
        [System.ComponentModel.DataAnnotations.MaxLength(100)]
        public string? City { get; set; }

        /// <summary>
        /// Kod pocztowy miejsca wydarzenia (format XX-XXX).
        /// </summary>
        [System.ComponentModel.DataAnnotations.RegularExpression(@"^\d{2}-\d{3}$")]
        public string? PostalCode { get; set; }

        /// <summary>
        /// Data rozpoczęcia aktywności.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Godzina rozpoczęcia wydarzenia (jako TimeSpan).
        /// </summary>
        public TimeSpan? Time { get; set; }

        /// <summary>
        /// Imię i nazwisko osoby odpowiedzialnej za to wydarzenie (np. koordynatora).
        /// </summary>
        public string? PersonInChargeName { get; set; }

        /// <summary>
        /// Numer telefonu do osoby odpowiedzialnej.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Phone]
        public string? PersonInChargePhone { get; set; }

        /// <summary>
        /// E-mail do osoby odpowiedzialnej za wydarzenie.
        /// </summary>
        [System.ComponentModel.DataAnnotations.EmailAddress]
        public string? PersonInChargeEmail { get; set; }

        /// <summary>
        /// Jakieś dodatkowe informacje lub uwagi na temat wydarzenia.
        /// </summary>
        public string? AdditionalInformation { get; set; }

        /// <summary>
        /// Czy wydarzenie jest cykliczne (powtarzalne co jakiś czas).
        /// </summary>
        public bool IsRepeatable { get; set; }

        /// <summary>
        /// Flaga czy aktywny - czy wydarzenie jest jeszcze aktualne.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Uczestnicy tego wydarzenia (relacja wiele do wielu z tabelą studentów).
        /// </summary>
        public List<Member> Participants { get; set; } = new();
    }
}

