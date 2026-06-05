using Models.Enums;
using Models.University;
using System;
using System.Collections.Generic;

namespace Models.Users
{
    /// <summary>
    /// Klasa reprezentująca konto użytkownika w systemie. Służy głównie do logowania i autoryzacji.
    /// </summary>
    public class UserAccount
    {
        /// <summary>
        /// ID konta - klucz główny.
        /// </summary>
        public int AccountId { get; set; }

        /// <summary>
        /// Adres e-mail użytkownika, który służy jako login. Wymagany i unikalny.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.EmailAddress]
        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Zahashowane hasło użytkownika do autoryzacji.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Rola w systemie (np. czy to zwykły User, czy jakiś Admin).
        /// </summary>
        public SystemRole SystemRole { get; set; } = SystemRole.User;

        /// <summary>
        /// Flaga mówiąca o tym, czy konto zostało zweryfikowane (np. przez e-mail).
        /// </summary>
        public bool IsVerified { get; set; } = false;

        /// <summary>
        /// Kiedy konto zostało utworzone.
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Kiedy użytkownik ostatnio się logował do systemu.
        /// </summary>
        public DateTime? LastLogin { get; set; }

        /// <summary>
        /// Kiedy ostatnio zmieniano dane konta.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Lista uczelni powiązanych z tym kontem.
        /// </summary>
        public List<College> Colleges { get; set; } = new();
    }
}


