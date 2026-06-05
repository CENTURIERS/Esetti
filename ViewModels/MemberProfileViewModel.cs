using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Esseti.Repositories.Interfaces;
using Esseti.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Models.Users;
using Models.ClubBase;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Controls.Chrome;

namespace Esseti.ViewModels
{
    /// <summary>
    /// Klasa pomocnicza trzymająca uproszczone informacje o danej aktywności przypisanej do członka.
    /// Służy do ładnego wyświetlenia na liście aktywności w profilu.
    /// </summary>
    public class ActivityProfileItem
    {
        /// <summary>
        /// Unikalny identyfikator (ID) aktywności.
        /// </summary>
        public int ActivityId { get; init; }

        /// <summary>
        /// Nazwa aktywności.
        /// </summary>
        public string Name { get; init; } = "";

        /// <summary>
        /// Osoba odpowiedzialna za tę aktywność.
        /// </summary>
        public string PersonInCharge { get; init; } = "";

        /// <summary>
        /// Data wydarzenia/aktywności jako tekst.
        /// </summary>
        public string Date { get; init; } = "";
    }

    /// <summary>
    /// Klasa pomocnicza reprezentująca pojedynczą kartę projektu, w którym członek bierze udział.
    /// Zawiera podstawowe info do wyświetlenia na kafelku.
    /// </summary>
    public class ProjectCardItem
    {
        /// <summary>
        /// Unikalny identyfikator (ID) projektu.
        /// </summary>
        public int ProjectId { get; init; }

        /// <summary>
        /// Nazwa projektu.
        /// </summary>
        public string Name { get; init; } = "";

        /// <summary>
        /// Imię i nazwisko lidera projektu.
        /// </summary>
        public string LeaderName { get; init; } = "";

        /// <summary>
        /// Liczba uczestników biorących udział w projekcie.
        /// </summary>
        public int ParticipantCount { get; init; }

        /// <summary>
        /// Nazwy sekcji powiązanych z tym projektem złączone w jeden tekst.
        /// </summary>
        public string SectionNames { get; init; } = "";

        /// <summary>
        /// Opis założeń lub stanu projektu.
        /// </summary>
        public string Description { get; init; } = "";
    }

    /// <summary>
    /// Model widoku obsługujący profil konkretnego członka.
    /// Ładuje dane o wybranej osobie, pozwala zmienić awatar, edytować szczegóły, zarządzać jej przynależnością do projektów/aktywności oraz ją usunąć.
    /// </summary>
    public partial class MemberProfileViewModel : ViewModelBase
    {
        /// <summary>
        /// Tytuł podstrony profilu.
        /// </summary>
        public override string PageTitle => "Profil członka";

        /// <summary>
        /// Serwis do przełączania widoków.
        /// </summary>
        private readonly INavigationService _navigationService;

        /// <summary>
        /// Repozytorium członków do zapytań bazodanowych.
        /// </summary>
        private readonly IMemberRepository _memberRepository;

        /// <summary>
        /// Identyfikator oglądanego członka koła.
        /// </summary>
        private readonly int _memberId;

        /// <summary>
        /// Załadowany obrazek awatara (zdjęcie profilowe).
        /// </summary>
        [ObservableProperty] private Bitmap? _avatar;

        /// <summary>
        /// Pełne imię i nazwisko członka.
        /// </summary>
        [ObservableProperty] private string _fullName = "";

        /// <summary>
        /// Rola pełniona w klubie (np. prezes, skarbnik, członek).
        /// </summary>
        [ObservableProperty] private string _role = "";

        /// <summary>
        /// Numer indeksu studenta.
        /// </summary>
        [ObservableProperty] private string _indexNumber = "";

        /// <summary>
        /// Adres e-mail.
        /// </summary>
        [ObservableProperty] private string _email = "";

        /// <summary>
        /// Numer telefonu.
        /// </summary>
        [ObservableProperty] private string _phoneNumber = "";

        /// <summary>
        /// Kierunek studiów.
        /// </summary>
        [ObservableProperty] private string _major = "";

        /// <summary>
        /// Sformatowana data dołączenia do koła.
        /// </summary>
        [ObservableProperty] private string _joinDate = "";

        /// <summary>
        /// Biogram lub krótki opis danej osoby.
        /// </summary>
        [ObservableProperty] private string _description = "";

        /// <summary>
        /// Nazwa uczelni wyższa, na której studiuje członek.
        /// </summary>
        [ObservableProperty] private string _collegeName = "";

        /// <summary>
        /// Nazwa wydziału uczelni.
        /// </summary>
        [ObservableProperty] private string _departmentName = "";

        /// <summary>
        /// Sformatowany adres wydziału uczelni.
        /// </summary>
        [ObservableProperty] private string _departmentAddress = "";

        /// <summary>
        /// Numer NIP uczelni.
        /// </summary>
        [ObservableProperty] private string _collegeNip = "";

        /// <summary>
        /// Czy profil jest w trakcie ładowania (pokazuje kręciołek/loader na widoku).
        /// </summary>
        [ObservableProperty] private bool _isLoading = true;

        /// <summary>
        /// Flaga mówiąca, czy ten członek ma przypisane jakieś projekty.
        /// </summary>
        [ObservableProperty] private bool _hasProjects;

        /// <summary>
        /// Flaga mówiąca, czy ten członek brał udział w jakichś aktywnościach.
        /// </summary>
        [ObservableProperty] private bool _hasActivities;

        /// <summary>
        /// Edytowane imię w formularzu edycji.
        /// </summary>
        [ObservableProperty] private string _editFirstName = "";

        /// <summary>
        /// Edytowane nazwisko w formularzu edycji.
        /// </summary>
        [ObservableProperty] private string _editLastName = "";

        /// <summary>
        /// Edytowany e-mail w formularzu edycji.
        /// </summary>
        [ObservableProperty] private string _editEmail = "";

        /// <summary>
        /// Edytowany numer telefonu w formularzu edycji.
        /// </summary>
        [ObservableProperty] private string _editPhoneNumber = "";

        /// <summary>
        /// Edytowany numer indeksu w formularzu edycji.
        /// </summary>
        [ObservableProperty] private string _editIndexNumber = "";

        /// <summary>
        /// Edytowany kierunek studiów w formularzu edycji.
        /// </summary>
        [ObservableProperty] private string _editMajor = "";

        /// <summary>
        /// Edytowany opis w formularzu edycji.
        /// </summary>
        [ObservableProperty] private string _editDescription = "";

        /// <summary>
        /// Edytowana rola w formularzu edycji.
        /// </summary>
        [ObservableProperty] private string _editRole = "";

        /// <summary>
        /// Komunikat błędu walidacji formularza edycji.
        /// </summary>
        [ObservableProperty] private string _editValidationError = "";

        /// <summary>
        /// Czy imię przy edycji jest błędne.
        /// </summary>
        [ObservableProperty] private bool _isEditFirstNameInvalid;

        /// <summary>
        /// Czy nazwisko przy edycji jest błędne.
        /// </summary>
        [ObservableProperty] private bool _isEditLastNameInvalid;

        /// <summary>
        /// Czy email przy edycji jest błędny.
        /// </summary>
        [ObservableProperty] private bool _isEditEmailInvalid;

        /// <summary>
        /// Czy numer indeksu przy edycji jest błędny.
        /// </summary>
        [ObservableProperty] private bool _isEditIndexNumberInvalid;

        /// <summary>
        /// Czy numer telefonu przy edycji jest błędny.
        /// </summary>
        [ObservableProperty] private bool _isEditPhoneNumberInvalid;

        /// <summary>
        /// Czy kierunek studiów przy edycji jest błędny.
        /// </summary>
        [ObservableProperty] private bool _isEditMajorInvalid;

        /// <summary>
        /// Czy cały formularz edycji jest poprawny.
        /// </summary>
        [ObservableProperty] private bool _isEditFormValid = true;

        /// <summary>
        /// Waliduje pola w formularzu edycji profilu członka.
        /// </summary>
        private void ValidateEditForm()
        {
            IsEditFirstNameInvalid = string.IsNullOrWhiteSpace(EditFirstName);
            IsEditLastNameInvalid = string.IsNullOrWhiteSpace(EditLastName);
            IsEditEmailInvalid = !string.IsNullOrWhiteSpace(EditEmail) && !IsValidEmail(EditEmail);
            IsEditIndexNumberInvalid = !string.IsNullOrWhiteSpace(EditIndexNumber) && !EditIndexNumber.All(char.IsDigit);
            IsEditPhoneNumberInvalid = !string.IsNullOrWhiteSpace(EditPhoneNumber) && !System.Text.RegularExpressions.Regex.IsMatch(EditPhoneNumber, @"^[+]*[(]{0,1}[0-9]{1,4}[)]{0,1}[-\s\./0-9]*$");
            IsEditMajorInvalid = string.IsNullOrWhiteSpace(EditMajor);

            IsEditFormValid = !IsEditFirstNameInvalid && !IsEditLastNameInvalid && !IsEditEmailInvalid && !IsEditIndexNumberInvalid && !IsEditPhoneNumberInvalid && !IsEditMajorInvalid;
        }

        /// <summary>
        /// Automatyczna walidacja przy zmianie danych w formularzu edycji.
        /// </summary>
        /// <param name="e">Argumenty zmiany właściwości.</param>
        protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(EditFirstName) ||
                e.PropertyName == nameof(EditLastName) ||
                e.PropertyName == nameof(EditEmail) ||
                e.PropertyName == nameof(EditIndexNumber) ||
                e.PropertyName == nameof(EditPhoneNumber) ||
                e.PropertyName == nameof(EditMajor))
            {
                ValidateEditForm();
            }
        }

        /// <summary>
        /// Lista aktywności przypisana do tego członka.
        /// </summary>
        public ObservableCollection<ActivityProfileItem> Activities { get; } = new();

        /// <summary>
        /// Lista projektów powiązana z tym członkiem.
        /// </summary>
        public ObservableCollection<ProjectCardItem> Projects { get; } = new();

        /// <summary>
        /// Domyślny awatar pobierany z zasobów apki, gdy użytkownik nie ma własnego zdjęcia.
        /// </summary>
        private static Bitmap? _defaultAvatar;

        /// <summary>
        /// Zapewnia bezpieczne pobranie domyślnego awatara bez wywalenia apki.
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
        /// Czy od razu po załadowaniu profilu otworzyć okienko edycji danych.
        /// </summary>
        private readonly bool _openEditImmediately;

        /// <summary>
        /// Konstruktor modelu widoku profilu. Inicjalizuje pola i uruchamia asynchroniczne ładowanie danych.
        /// </summary>
        /// <param name="memberId">Unikalny identyfikator członka koła.</param>
        /// <param name="navigationService">Serwis do nawigacji.</param>
        /// <param name="memberRepository">Repozytorium danych o członkach.</param>
        /// <param name="openEditImmediately">Czy od razu pokazać popup edycji (domyślnie false).</param>
        public MemberProfileViewModel(int memberId, INavigationService navigationService, IMemberRepository memberRepository, bool openEditImmediately = false)
        {
            _memberId = memberId;
            _navigationService = navigationService;
            _memberRepository = memberRepository;
            _openEditImmediately = openEditImmediately;
            _ = LoadAsync();
        }

        /// <summary>
        /// Komenda do zmiany awatara. Otwiera systemowy eksplorator plików i zapisuje nowe zdjęcie w bazie.
        /// </summary>
        [RelayCommand]
        private async Task ChangeAvatarAsync()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
            {
                var files = await desktop.MainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
                    Title = "Wybierz nowe zdjęcie profilowe",
                    AllowMultiple = false,
                    FileTypeFilter = new[] { FilePickerFileTypes.ImageAll } 
                });

                if (files.Count > 0)
                {
                    using (var stream = await files[0].OpenReadAsync())
                    using (var memoryStream = new MemoryStream())
                    {
                        await stream.CopyToAsync(memoryStream);
                        byte[] avatarData = memoryStream.ToArray();

                        await _memberRepository.UpdateMemberAvatarAsync(_memberId, avatarData);

                        using var ms = new MemoryStream(avatarData);
                        Avatar = new Bitmap(ms);
                    }
                }
            }

        }

        /// <summary>
        /// Asynchroniczne ładowanie pełnych szczegółów członka, uczelni, projektów i aktywności z bazy danych.
        /// </summary>
        private async Task LoadAsync()
        {
            try
            {
                var member = await _memberRepository.GetMemberByIdAsync(_memberId);
                if (member == null)
                {
                    Dispatcher.UIThread.Post(() => { IsLoading = false; Description = "Nie znaleziono użytkownika w bazie."; });
                    return;
                }

                Dispatcher.UIThread.Post(() =>
                {
                    if (member.MemberAvatar?.Length > 0)
                    {
                        try { using var ms = new MemoryStream(member.MemberAvatar); Avatar = new Bitmap(ms); }
                        catch { Avatar = SafeDefaultAvatar; }
                    }
                    else Avatar = SafeDefaultAvatar;

                    FullName = $"{member.FirstName} {member.LastName}".Trim();
                    Role = member.AuthorityRole?.Name ?? "Brak roli";
                    IndexNumber = member.IndexNumber ?? "";
                    Email = member.Account?.Email ?? "";
                    PhoneNumber = member.PhoneNumber ?? "";
                    Major = member.Major ?? "";
                    JoinDate = $"Od {member.JoinDate:dd.MM.yyyy} r.";
                    Description = string.IsNullOrWhiteSpace(member.Description) ? "Brak opisu." : member.Description;

                    var club = member.MemberClubs?.FirstOrDefault()?.Club;
                    var dept = club?.Department;
                    var college = dept?.College;

                    CollegeName = college?.Name ?? "Brak uczelni";
                    DepartmentName = dept?.Name ?? "Brak wydziału";
                    DepartmentAddress = dept != null
                        ? $"{dept.AddressLine}, {dept.PostalCode} {dept.City}".Trim(' ', ',')
                        : "";
                    CollegeNip = college?.NIP != null ? $"NIP: {college.NIP}" : "";

                    Activities.Clear();
                    foreach (var a in (member.Activities ?? new()).OrderByDescending(a => a.Date))
                    {
                        Activities.Add(new ActivityProfileItem
                        {
                            ActivityId = a.ActivityId,
                            Name = a.Name,
                            PersonInCharge = a.PersonInChargeName ?? "Uczestnik",
                            Date = a.Date.ToString("dd.MM.yy") + " r."
                        });
                    }
                    HasActivities = Activities.Any();

                    Projects.Clear();
                    foreach (var p in member.Projects ?? new())
                    {
                        Projects.Add(new ProjectCardItem
                        {
                            ProjectId = p.ProjectId,
                            Name = p.Name,
                            LeaderName = p.PersonInCharge != null
                                ? $"{p.PersonInCharge.FirstName} {p.PersonInCharge.LastName}".Trim()
                                : "Brak lidera",
                            ParticipantCount = p.Participants?.Count ?? 0,
                            SectionNames = p.Sections?.Any() == true
                                ? string.Join(", ", p.Sections.Select(s => s.Name))
                                : "Brak sekcji",
                            Description = p.Description ?? ""
                        });
                    }
                    HasProjects = Projects.Any();
                    IsLoading = false;
                    if (_openEditImmediately)
                    {
                        OpenEdit();
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Błąd ładowania profilu: {ex}");
                IsLoading = false;
            }
        }

        /// <summary>
        /// Komenda cofająca użytkownika do listy wszystkich członków.
        /// </summary>
        [RelayCommand]
        private void GoBack()
        {
            _navigationService.NavigateTo(App.Services.GetRequiredService<MembersViewModel>());
        }

        /// <summary>
        /// Komenda otwierająca popup edycji i wypełniająca go aktualnymi danymi członka.
        /// </summary>
        [RelayCommand]
        private void OpenEdit()
        {
            EditFirstName = FullName.Split(' ').FirstOrDefault() ?? "";
            EditLastName = FullName.Split(' ').LastOrDefault() ?? "";
            EditEmail = Email;
            EditPhoneNumber = PhoneNumber;
            EditIndexNumber = IndexNumber;
            EditMajor = Major;
            EditDescription = Description;
            EditRole = Role;

            ValidateEditForm();
            IsEditPopupVisible = true;
        }

        /// <summary>
        /// Komenda anulująca edycję i zamykająca popup.
        /// </summary>
        [RelayCommand]
        private void CancelEdit()
        {
            IsEditPopupVisible = false;
        }

        /// <summary>
        /// Pomocnicza walidacja adresu e-mail za pomocą wyrażenia regularnego.
        /// </summary>
        /// <param name="email">Adres e-mail.</param>
        /// <returns>Zwraca true, jeśli poprawny lub pusty, w przeciwnym razie false.</returns>
        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return true;
            return System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        /// <summary>
        /// Komenda asynchroniczna zapisująca wprowadzone w formularzu edycji zmiany w bazie danych.
        /// </summary>
        [RelayCommand]
        private async Task SaveEditAsync()
        {
            EditValidationError = string.Empty;

            if (string.IsNullOrWhiteSpace(EditFirstName))
            {
                EditValidationError = "Imię jest wymagane.";
                return;
            }

            if (string.IsNullOrWhiteSpace(EditLastName))
            {
                EditValidationError = "Nazwisko jest wymagane.";
                return;
            }

            if (!IsValidEmail(EditEmail))
            {
                EditValidationError = "Niepoprawny format adresu e-mail.";
                return;
            }

            if (!string.IsNullOrWhiteSpace(EditIndexNumber) && !EditIndexNumber.All(char.IsDigit))
            {
                EditValidationError = "Numer indeksu musi składać się wyłącznie z cyfr.";
                return;
            }

            var updatedMember = new Models.Users.Member 
            {
                MemberId = _memberId,
                FirstName = EditFirstName,
                LastName = EditLastName,
                PhoneNumber = EditPhoneNumber,
                IndexNumber = EditIndexNumber,
                Major = EditMajor,
                Description = EditDescription,
                Account = !string.IsNullOrWhiteSpace(EditEmail) ? new UserAccount { Email = EditEmail } : null,
                AuthorityRole = !string.IsNullOrWhiteSpace(EditRole) ? new AuthorityRole { Name = EditRole } : null
            };

            var remainingProjectIds = Projects.Select(p => p.ProjectId).ToList();
            var remainingActivityIds = Activities.Select(a => a.ActivityId).ToList();


            await _memberRepository.UpdateMemberAsync(updatedMember, remainingProjectIds, remainingActivityIds);

            await LoadAsync();

            IsEditPopupVisible = false;
        }

        /// <summary>
        /// Komenda usuwająca członka z danego projektu (tylko z listy wyświetlanej na ekranie edycji).
        /// </summary>
        /// <param name="project">Model widoku projektu.</param>
        [RelayCommand]
        private void RemoveFromProject(ProjectCardItem project)
        {
            if (project != null)
            {
                Projects.Remove(project);
                HasProjects = Projects.Any();
            }
        }

        /// <summary>
        /// Komenda usuwająca członka z danej aktywności (tylko na ekranie edycji).
        /// </summary>
        /// <param name="activity">Model widoku aktywności.</param>
        [RelayCommand]
        private void RemoveFromActivity(ActivityProfileItem activity)
        {
            if (activity != null)
            {
                Activities.Remove(activity);
                HasActivities = Activities.Any();
            }
        }

        /// <summary>
        /// Komenda otwierająca popup potwierdzenia całkowitego usunięcia tego członka z klubu.
        /// </summary>
        [RelayCommand]
        private void DeleteThisMember()
        {
            IsEditPopupVisible = false;
            IsPopupVisible = true;
        }

        /// <summary>
        /// Wykonuje faktyczne usunięcie członka z bazy danych i przekierowuje na listę członków.
        /// </summary>
        protected override async Task ExecuteConfirmDeleteAsync()
        {
            await _memberRepository.DeleteSingleMemberAsync(_memberId);
            _navigationService.NavigateTo(App.Services.GetRequiredService<MembersViewModel>());
        }
    }
}

