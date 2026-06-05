using Models.Users;
using Models.ClubBase;
using System;
using System.Collections.Generic;

namespace Models.Activities
{
    /// <summary>
    /// Klasa reprezentująca projekt realizowany przez koło naukowe (np. budowa łazika, nowa strona internetowa).
    /// </summary>
    public class Project : System.ComponentModel.DataAnnotations.IValidatableObject
    {
        /// <summary>
        /// ID projektu - klucz główny w bazie.
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// Nazwa projektu. Pole wymagane.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Krótki opis tego, co w projekcie robimy.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Dodatkowe uwagi lub informacje o projekcie.
        /// </summary>
        public string? AdditionalInformation { get; set; }

        /// <summary>
        /// ID lidera projektu - klucz obcy do tabeli studentów.
        /// </summary>
        public int? PersonInChargeId { get; set; }

        /// <summary>
        /// Referencja do lidera projektu (kierownika).
        /// </summary>
        public Member? PersonInCharge { get; set; }

        /// <summary>
        /// Adres URL do repozytorium projektu na GitHubie.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Url]
        public string? Github { get; set; }

        /// <summary>
        /// Szacowany czas potrzebny na realizację projektu (np. w godzinach).
        /// </summary>
        public int? EstimatedTime { get; set; }

        /// <summary>
        /// Planowana data rozpoczęcia projektu.
        /// </summary>
        public DateTime? DateStart { get; set; }

        /// <summary>
        /// Planowana data zakończenia projektu.
        /// </summary>
        public DateTime? DateEnd { get; set; }

        /// <summary>
        /// Flaga czy projekt jest aktywny (czyli czy wciąż nad nim pracujemy).
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Członkowie koła zaangażowani w ten projekt (relacja wiele do wielu).
        /// </summary>
        public List<Member> Participants { get; set; } = new();

        /// <summary>
        /// Sekcje zaangażowane w ten projekt (relacja wiele do wielu).
        /// </summary>
        public List<Section> Sections { get; set; } = new();

        /// <summary>
        /// Koła naukowe powiązane z tym projektem (relacja wiele do wielu).
        /// </summary>
        public List<ClubInfo> Clubs { get; set; } = new();

        /// <summary>
        /// Metoda walidująca poprawność dat. Data zakończenia nie może być przed datą rozpoczęcia.
        /// </summary>
        /// <param name="validationContext">Kontekst walidacji.</param>
        /// <returns>Wynik walidacji (błędy, jeśli wystąpiły).</returns>
        public IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> Validate(System.ComponentModel.DataAnnotations.ValidationContext validationContext)
        {
            if (DateStart.HasValue && DateEnd.HasValue && DateEnd.Value < DateStart.Value)
            {
                yield return new System.ComponentModel.DataAnnotations.ValidationResult(
                    "Data zakończenia projektu nie może być wcześniejsza niż data rozpoczęcia.",
                    new[] { nameof(DateEnd) });
            }
        }
    }
}


