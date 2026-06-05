using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.IO;

namespace Esseti.ViewModels.Member
{
    /// <summary>
    /// Model widoku reprezentujący pojedynczy wiersz/kafelek członka na liście (np. w widoku członków lub centrum dokumentów).
    /// Zawiera podstawowe dane do wyświetlenia, obsługuje ładowanie awatara oraz zaznaczanie elementu.
    /// </summary>
    public partial class MemberItemViewModel : ViewModelBase
    {
        /// <summary>
        /// Unikalny identyfikator członka w bazie danych.
        /// </summary>
        public int MemberId { get; }

        /// <summary>
        /// Flaga informująca, czy dany członek jest zaznaczony na liście checkboxem.
        /// </summary>
        [ObservableProperty]
        private bool _isSelected;

        /// <summary>
        /// Zdekodowany obrazek awatara gotowy do wyświetlenia.
        /// </summary>
        [ObservableProperty]
        private Bitmap? _avatar;

        /// <summary>
        /// Imię członka.
        /// </summary>
        [ObservableProperty]
        private string _firstName = string.Empty;

        /// <summary>
        /// Nazwisko członka.
        /// </summary>
        [ObservableProperty]
        private string _lastName = string.Empty;

        /// <summary>
        /// Rola/stanowisko (np. prezes, członek).
        /// </summary>
        [ObservableProperty]
        private string _role = string.Empty;

        /// <summary>
        /// Numer indeksu studenta.
        /// </summary>
        [ObservableProperty]
        private string _indexNumber = string.Empty;

        /// <summary>
        /// Adres e-mail studenta.
        /// </summary>
        [ObservableProperty]
        private string _email = string.Empty;

        /// <summary>
        /// Nazwa wydziału uczelni.
        /// </summary>
        [ObservableProperty]
        private string _collegeDepartment = string.Empty;

        /// <summary>
        /// Kierunek studiów.
        /// </summary>
        [ObservableProperty]
        private string _major = string.Empty;

        /// <summary>
        /// Data dołączenia do koła.
        /// </summary>
        [ObservableProperty]
        private string _joinDate = string.Empty;

        /// <summary>
        /// Czy członek jest aktywny w kole naukowym.
        /// </summary>
        [ObservableProperty]
        private bool _isActive = true;

        /// <summary>
        /// Krótki opis członka.
        /// </summary>
        [ObservableProperty]
        private string _description = string.Empty;

        /// <summary>
        /// Numer telefonu członka.
        /// </summary>
        [ObservableProperty]
        private string _phoneNumber = string.Empty;

        /// <summary>
        /// Specjalna flaga oznaczająca systemowy kafelek do dodawania nowego członka (np. kafelek z plusem).
        /// </summary>
        [ObservableProperty]
        private bool _isSystemAddTile;

        /// <summary>
        /// Właściwość łącząca imię i nazwisko w jeden ciąg znaków.
        /// </summary>
        public string FullName => $"{FirstName} {LastName}".Trim();

        /// <summary>
        /// Sformatowana data dołączenia do koła naukowe.
        /// </summary>
        public string FullFromDate => $"Od {JoinDate} r.";

        /// <summary>
        /// Cache na domyślny awatar pobrany z zasobów aplikacji.
        /// </summary>
        private static Bitmap? _defaultAvatar;

        /// <summary>
        /// Bezpieczne pobranie domyślnego awatara w przypadku braku lub błędu wczytywania własnego obrazka.
        /// </summary>
        private static Bitmap? SafeDefaultAvatar
        {
            get
            {
                if (_defaultAvatar != null) return _defaultAvatar;
                try { _defaultAvatar = new Bitmap(AssetLoader.Open(new Uri("avares://Esseti/Assets/user-default.png"))); }
                catch { }
                return _defaultAvatar;
            }
        }

        /// <summary>
        /// Konstruktor tworzący model widoku kafelka członka. Wypełnia dane i inicjalizuje awatar.
        /// </summary>
        /// <param name="memberId">Identyfikator członka.</param>
        /// <param name="avatar">Tablica bajtów obrazka awatara.</param>
        /// <param name="firstName">Imię.</param>
        /// <param name="lastName">Nazwisko.</param>
        /// <param name="role">Rola.</param>
        /// <param name="indexNumber">Numer indeksu.</param>
        /// <param name="email">E-mail.</param>
        /// <param name="phoneNumber">Telefon.</param>
        /// <param name="collegeDepartment">Wydział uczelni.</param>
        /// <param name="major">Kierunek studiów.</param>
        /// <param name="joinDate">Data dołączenia.</param>
        /// <param name="isActive">Czy aktywny.</param>
        /// <param name="description">Krótki opis.</param>
        /// <param name="isSystemAddTile">Czy to jest kafelek z przyciskiem plus.</param>
        public MemberItemViewModel(int memberId, byte[] avatar, string firstName, string lastName, string role, string indexNumber, string email, string phoneNumber, string collegeDepartment, string major, string joinDate, bool isActive, string description = "", bool isSystemAddTile = false)
        {
            MemberId = memberId;
            FirstName = firstName;
            LastName = lastName;
            Role = role;
            IndexNumber = indexNumber;
            Email = email;
            PhoneNumber = phoneNumber;
            CollegeDepartment = collegeDepartment;
            Major = major;
            JoinDate = joinDate;
            IsActive = isActive;
            Description = description;
            IsSystemAddTile = isSystemAddTile;

            if (avatar != null && avatar.Length > 0)
            {
                try { using var ms = new MemoryStream(avatar); Avatar = new Bitmap(ms); }
                catch { Avatar = SafeDefaultAvatar; }
            }
            else Avatar = SafeDefaultAvatar;
        }

        /// <summary>
        /// Zwalnia zasoby systemowe, w szczególności bitmapę awatara, aby uniknąć wycieków pamięci.
        /// </summary>
        public override void Dispose()
        {
            if (Avatar != null && Avatar != _defaultAvatar)
            {
                Avatar.Dispose();
                Avatar = null;
            }
            base.Dispose();
        }
    }
}
