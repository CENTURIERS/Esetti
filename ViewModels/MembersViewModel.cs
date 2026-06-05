using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Esseti.Repositories.Interfaces;
using Esseti.Services;
using Esseti.ViewModels.Member;
using Microsoft.Extensions.DependencyInjection;
using Models.Users;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.Controls.ApplicationLifetimes;
using System.IO;
using Models.ClubBase;

namespace Esseti.ViewModels
{
    /// <summary>
    /// Model widoku do obsługi listy członków koła/klubu.
    /// Ogarnia wyświetlanie członków, wyszukiwanie, stronicowanie, a także wyskakujące okienka (popupy) do dodawania i edytowania osób.
    /// </summary>
    public partial class MembersViewModel : ViewModelBase
    {
        /// <summary>
        /// Tytuł wyświetlany na górze strony.
        /// </summary>
        public override string PageTitle => "Lista członków";

        /// <summary>
        /// Pokazujemy nagłówek z akcjami (np. wyszukiwarka, przyciski).
        /// </summary>
        public override bool ShowActionHeader => true;

        /// <summary>
        /// Tekst pomocniczy w wyszukiwarce.
        /// </summary>
        public override string SearchPlaceholder => "Szukaj członków...";

        /// <summary>
        /// Kolekcja członków pokazywana na widoku (bindowana do listy).
        /// </summary>
        public ObservableCollection<MemberItemViewModel> Members { get; } = new();

        /// <summary>
        /// Repozytorium do komunikacji z bazą danych w sprawach członków.
        /// </summary>
        private readonly IMemberRepository _memberRepository;

        /// <summary>
        /// Lista wszystkich pobranych członków z bazy, służy do filtrowania w pamięci.
        /// </summary>
        private readonly List<MemberItemViewModel> _allMembers = new();

        /// <summary>
        /// Tymczasowy obiekt członka, którego użytkownik chce usunąć.
        /// </summary>
        private MemberItemViewModel? _memberToDelete;

        /// <summary>
        /// Imię nowego członka w formularzu dodawania.
        /// </summary>
        [ObservableProperty] 
        private string _newFirstName = string.Empty;

        /// <summary>
        /// Nazwisko nowego członka w formularzu dodawania.
        /// </summary>
        [ObservableProperty] 
        private string _newLastName = string.Empty;

        /// <summary>
        /// Email nowego członka w formularzu dodawania.
        /// </summary>
        [ObservableProperty] 
        private string _newEmail = string.Empty;

        /// <summary>
        /// Numer telefonu nowego członka w formularzu dodawania.
        /// </summary>
        [ObservableProperty] 
        private string _newPhoneNumber = string.Empty;

        /// <summary>
        /// Numer indeksu studenta w formularzu dodawania.
        /// </summary>
        [ObservableProperty] 
        private string _newIndexNumber = string.Empty;

        /// <summary>
        /// Tablica bajtów z obrazkiem awatara nowego członka.
        /// </summary>
        [ObservableProperty] 
        private byte[]? _newAvatar;

        /// <summary>
        /// Opis/biogram nowego członka.
        /// </summary>
        [ObservableProperty]
        private string _newDescription = string.Empty;

        /// <summary>
        /// Kierunek studiów nowego członka.
        /// </summary>
        [ObservableProperty]
        private string _newMajor = string.Empty;

        /// <summary>
        /// Lista dostępnych ról w klubie (np. prezes, członek) pobrana z bazy.
        /// </summary>
        public ObservableCollection<AuthorityRole> AvailableRoles { get; } = new();

        /// <summary>
        /// Lista dostępnych wydziałów uczelni pobrana z bazy.
        /// </summary>
        public ObservableCollection<Models.University.CollegeDepartment> AvailableDepartments { get; } = new();

        /// <summary>
        /// Wybrany wydział w formularzu dodawania.
        /// </summary>
        [ObservableProperty]
        private Models.University.CollegeDepartment? _selectedDepartment;

        /// <summary>
        /// Wybrany wydział w formularzu edycji.
        /// </summary>
        [ObservableProperty]
        private Models.University.CollegeDepartment? _editSelectedDepartment;

        /// <summary>
        /// Wybrana rola w formularzu dodawania.
        /// </summary>
        [ObservableProperty]
        private AuthorityRole? _selectedRole;

        /// <summary>
        /// Czy imię przy dodawaniu jest niepoprawne.
        /// </summary>
        [ObservableProperty] private bool _isAddFirstNameInvalid;

        /// <summary>
        /// Czy nazwisko przy dodawaniu jest niepoprawne.
        /// </summary>
        [ObservableProperty] private bool _isAddLastNameInvalid;

        /// <summary>
        /// Czy email przy dodawaniu jest niepoprawny.
        /// </summary>
        [ObservableProperty] private bool _isAddEmailInvalid;

        /// <summary>
        /// Czy numer indeksu przy dodawaniu jest niepoprawny.
        /// </summary>
        [ObservableProperty] private bool _isAddIndexNumberInvalid;

        /// <summary>
        /// Czy cały formularz dodawania jest poprawny.
        /// </summary>
        [ObservableProperty] private bool _isAddFormValid = true;

        /// <summary>
        /// Czy imię przy edycji jest niepoprawne.
        /// </summary>
        [ObservableProperty] private bool _isEditFirstNameInvalid;

        /// <summary>
        /// Czy nazwisko przy edycji jest niepoprawne.
        /// </summary>
        [ObservableProperty] private bool _isEditLastNameInvalid;

        /// <summary>
        /// Czy email przy edycji jest niepoprawny.
        /// </summary>
        [ObservableProperty] private bool _isEditEmailInvalid;

        /// <summary>
        /// Czy numer indeksu przy edycji jest niepoprawny.
        /// </summary>
        [ObservableProperty] private bool _isEditIndexNumberInvalid;

        /// <summary>
        /// Czy cały formularz edycji jest poprawny.
        /// </summary>
        [ObservableProperty] private bool _isEditFormValid = true;

        /// <summary>
        /// Sprawdza poprawność pól w formularzu dodawania i aktualizuje stany walidacji.
        /// </summary>
        private void ValidateAddForm()
        {
            IsAddFirstNameInvalid = string.IsNullOrWhiteSpace(NewFirstName);
            IsAddLastNameInvalid = string.IsNullOrWhiteSpace(NewLastName);
            IsAddEmailInvalid = string.IsNullOrWhiteSpace(NewEmail) || !IsValidEmail(NewEmail);
            IsAddIndexNumberInvalid = string.IsNullOrWhiteSpace(NewIndexNumber) || !NewIndexNumber.All(char.IsDigit) || NewIndexNumber.Length < 4;

            IsAddFormValid = !IsAddFirstNameInvalid && !IsAddLastNameInvalid && !IsAddEmailInvalid && !IsAddIndexNumberInvalid;
        }

        /// <summary>
        /// Sprawdza poprawność pól w formularzu edycji i aktualizuje stany walidacji.
        /// </summary>
        private void ValidateEditForm()
        {
            IsEditFirstNameInvalid = string.IsNullOrWhiteSpace(EditFirstName);
            IsEditLastNameInvalid = string.IsNullOrWhiteSpace(EditLastName);
            IsEditEmailInvalid = string.IsNullOrWhiteSpace(EditEmail) || !IsValidEmail(EditEmail);
            IsEditIndexNumberInvalid = string.IsNullOrWhiteSpace(EditIndexNumber) || !EditIndexNumber.All(char.IsDigit) || EditIndexNumber.Length < 4;

            IsEditFormValid = !IsEditFirstNameInvalid && !IsEditLastNameInvalid && !IsEditEmailInvalid && !IsEditIndexNumberInvalid;
        }

        /// <summary>
        /// Metoda wywoływana, gdy zmieni się jakakolwiek właściwość. Wykorzystujemy ją do automatycznej walidacji formularzy na bieżąco.
        /// </summary>
        /// <param name="e">Argumenty zdarzenia zmiany właściwości.</param>
        protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(NewFirstName) ||
                e.PropertyName == nameof(NewLastName) ||
                e.PropertyName == nameof(NewEmail) ||
                e.PropertyName == nameof(NewIndexNumber))
            {
                ValidateAddForm();
            }
            if (e.PropertyName == nameof(EditFirstName) ||
                e.PropertyName == nameof(EditLastName) ||
                e.PropertyName == nameof(EditEmail) ||
                e.PropertyName == nameof(EditIndexNumber))
            {
                ValidateEditForm();
            }
        }

        /// <summary>
        /// Czy popup z edycją na liście ma być otwarty.
        /// </summary>
        [ObservableProperty]
        private bool _isListEditPopupVisible;

        /// <summary>
        /// Imię edytowanego członka.
        /// </summary>
        [ObservableProperty]
        private string _editFirstName = string.Empty;

        /// <summary>
        /// Nazwisko edytowanego członka.
        /// </summary>
        [ObservableProperty]
        private string _editLastName = string.Empty;

        /// <summary>
        /// E-mail edytowanego członka.
        /// </summary>
        [ObservableProperty]
        private string _editEmail = string.Empty;

        /// <summary>
        /// Numer telefonu edytowanego członka.
        /// </summary>
        [ObservableProperty]
        private string _editPhoneNumber = string.Empty;

        /// <summary>
        /// Numer indeksu edytowanego członka.
        /// </summary>
        [ObservableProperty]
        private string _editIndexNumber = string.Empty;

        /// <summary>
        /// Awatar edytowanego członka jako tablica bajtów.
        /// </summary>
        [ObservableProperty]
        private byte[]? _editAvatar;

        /// <summary>
        /// Opis edytowanego członka.
        /// </summary>
        [ObservableProperty]
        private string _editDescription = string.Empty;

        /// <summary>
        /// Kierunek studiów edytowanego członka.
        /// </summary>
        [ObservableProperty]
        private string _editMajor = string.Empty;

        /// <summary>
        /// Wybrana rola w formularzu edycji.
        /// </summary>
        [ObservableProperty]
        private AuthorityRole? _editSelectedRole;

        /// <summary>
        /// Tekst błędu walidacji formularza dodawania.
        /// </summary>
        [ObservableProperty]
        private string _addValidationError = string.Empty;

        /// <summary>
        /// Tekst błędu walidacji formularza edycji.
        /// </summary>
        [ObservableProperty]
        private string _editValidationError = string.Empty;

        /// <summary>
        /// Aktualnie modyfikowany model widoku pojedynczego członka.
        /// </summary>
        private MemberItemViewModel? _memberBeingEdited;

        /// <summary>
        /// Serwis nawigacyjny do skakania między ekranami.
        /// </summary>
        private readonly INavigationService _navigationService;

        /// <summary>
        /// Reakcja na zamknięcie popupa - pusta implementacja.
        /// </summary>
        protected override void OnPopupClosed()
        {
            
        }

        /// <summary>
        /// Obsługa zaznaczania lub odznaczania wszystkich członków na liście naraz.
        /// </summary>
        /// <param name="value">Czy zaznaczyć wszystko (true), czy odznaczyć (false).</param>
        protected override void OnIsAllSelectedChangedVirtual(bool value)
        {
            if (_isUpdatingSelection) return;

            _isUpdatingSelection = true;

            try
            {
                foreach (var member in Members.Where(m => !m.IsSystemAddTile)) {
                    member.IsSelected = value;
                }
                UpdateSelectionState();
            } finally
            {
                _isUpdatingSelection = false;
            }
        }

        /// <summary>
        /// Konstruktor ViewModelu. Wstrzykuje zależności i asynchronicznie ładuje dane z bazy.
        /// </summary>
        /// <param name="memberRepository">Repozytorium danych o członkach.</param>
        /// <param name="navigationService">Wspólny serwis nawigacyjny.</param>
        public MembersViewModel(IMemberRepository memberRepository, INavigationService navigationService)
        {
            _memberRepository = memberRepository;
            _navigationService = navigationService;
            _ = LoadDataAsync();
        }

        /// <summary>
        /// Aktualizuje licznik zaznaczonych elementów na liście oraz flagę określającą, czy cokolwiek zaznaczono.
        /// </summary>
        private void UpdateSelectionState()
        {
            var selected = Members.Where(m => !m.IsSystemAddTile && m.IsSelected).ToList();
            SelectedCount = selected.Count;
            IsAnySelected = SelectedCount > 0;
        }

        /// <summary>
        /// Wykonuje usunięcie z bazy danych wybranego członka lub wszystkich zaznaczonych.
        /// </summary>
        protected override async Task ExecuteConfirmDeleteAsync()
        {
            try
            {
                if (_memberToDelete != null)
                {
                    await _memberRepository.DeleteSingleMemberAsync(_memberToDelete.MemberId);

                    Members.Remove(_memberToDelete);
                    _allMembers.Remove(_memberToDelete);

                    _memberToDelete = null;
                }
                else
                {
                    var selectedVMs = Members.Where(m => m.IsSelected && !m.IsSystemAddTile).ToList();
                    if (!selectedVMs.Any()) return;

                    var idsToDelete = selectedVMs.Select(m => m.MemberId).ToList();
                    await _memberRepository.DeleteMembersAsync(idsToDelete);

                    foreach (var vm in selectedVMs)
                    {
                        Members.Remove(vm);
                        _allMembers.Remove(vm);
                    }
                }

                IsAllSelected = false;
                UpdateSelectionState();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd bazy: {ex.Message}");
            }
        }

        /// <summary>
        /// Komenda przechodząca do widoku profilu szczegółowego danego członka.
        /// </summary>
        /// <param name="member">Model widoku klikniętego członka.</param>
        [RelayCommand]
        private void OpenProfile(MemberItemViewModel member)
        {
            if (member == null || member.IsSystemAddTile) return;
            var profileVm = new MemberProfileViewModel(
                member.MemberId,
                _navigationService,
                App.Services.GetRequiredService<IMemberRepository>()
            );
            _navigationService.NavigateTo(profileVm);
        }


        /// <summary>
        /// Komenda otwierająca okienko potwierdzenia usunięcia pojedynczej osoby.
        /// </summary>
        /// <param name="member">Model widoku członka przeznaczonego do usunięcia.</param>
        [RelayCommand]
        private void DeleteSingleMember(MemberItemViewModel member)
        {
            if (member == null) return;

            _memberToDelete = member;
            IsPopupVisible = true;
        }

        /// <summary>
        /// Komenda otwierająca okienko edycji danych wybranej osoby i uzupełniająca formularz jej aktualnymi danymi.
        /// </summary>
        /// <param name="member">Model widoku członka, którego chcemy edytować.</param>
        [RelayCommand]
        private void EditMember(MemberItemViewModel member)
        {
            if (member == null || member.IsSystemAddTile) return;

            _memberBeingEdited = member;
            EditFirstName = member.FirstName;
            EditLastName = member.LastName;
            EditEmail = member.Email;
            EditPhoneNumber = member.PhoneNumber;
            EditIndexNumber = member.IndexNumber;
            EditDescription = member.Description;
            EditMajor = member.Major;
            EditAvatar = null;
            EditSelectedRole = AvailableRoles.FirstOrDefault(r => r.Name == member.Role) ?? AvailableRoles.FirstOrDefault(r => r.Name == "Członek");
            EditSelectedDepartment = AvailableDepartments.FirstOrDefault(d => d.Name == member.CollegeDepartment);

            ValidateEditForm();
            IsListEditPopupVisible = true;
        }

        /// <summary>
        /// Komenda anulująca edycję i zamykająca okienko.
        /// </summary>
        [RelayCommand]
        private void CancelEdit()
        {
            IsListEditPopupVisible = false;
        }

        /// <summary>
        /// Komenda asynchroniczna otwierająca systemowy eksplorator plików do wyboru nowego zdjęcia awatara przy edycji.
        /// </summary>
        [RelayCommand]
        private async Task OpenEditAvatarPickerAsync()
        {
            if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var window = desktop.MainWindow;
                if (window != null)
                {
                    var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                    {
                        Title = "Wybierz zdjęcie",
                        AllowMultiple = false,
                        FileTypeFilter = new[] { FilePickerFileTypes.ImageAll }
                    });

                    if (files.Count >= 1)
                    {
                        using var stream = await files[0].OpenReadAsync();
                        using var ms = new MemoryStream();
                        await stream.CopyToAsync(ms);
                        EditAvatar = ms.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// Komenda zapisująca wprowadzone zmiany w danych członka koła w bazie danych.
        /// </summary>
        [RelayCommand]
        private async Task ConfirmEditAsync()
        {
            EditValidationError = string.Empty;

            if (_memberBeingEdited == null)
                return;

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

            int clubId = 1;

            var editMemberDb = new Models.Users.Member
            {
                MemberId = _memberBeingEdited.MemberId,
                FirstName = EditFirstName,
                LastName = EditLastName,
                IndexNumber = EditIndexNumber,
                Major = EditMajor,
                Description = EditDescription,
                MemberAvatar = EditAvatar,
                PhoneNumber = EditPhoneNumber,
                AuthorityRole = EditSelectedRole,
                RoleId = EditSelectedRole?.RoleId ?? 0,
                Account = string.IsNullOrWhiteSpace(EditEmail) ? null : new UserAccount
                {
                    Email = EditEmail,
                    SystemRole = Models.Enums.SystemRole.User
                },
                MemberClubs = new List<MemberClub>
                {
                    new MemberClub { ClubId = clubId, MemberId = _memberBeingEdited.MemberId }
                }
            };

            await _memberRepository.UpdateMemberBasicInfoAsync(editMemberDb, EditSelectedDepartment?.CollegeDepartmentId);

            await LoadDataAsync();

            IsListEditPopupVisible = false;
        }

        /// <summary>
        /// Komenda otwierająca popup dodawania nowego członka i czyszcząca formularz.
        /// </summary>
        [RelayCommand]
        private void AddMember()
        {
            NewFirstName = string.Empty;
            NewLastName = string.Empty;
            NewEmail = string.Empty;
            NewPhoneNumber = string.Empty;
            NewIndexNumber = string.Empty;
            NewDescription = string.Empty;
            NewMajor = string.Empty;
            NewAvatar = null;
            SelectedRole = AvailableRoles.FirstOrDefault(r => r.Name == "Członek");
            SelectedDepartment = AvailableDepartments.FirstOrDefault();

            IsAddFirstNameInvalid = false;
            IsAddLastNameInvalid = false;
            IsAddEmailInvalid = false;
            IsAddIndexNumberInvalid = false;
            IsAddFormValid = false; // Puste pola oznaczają, że na starcie formularz jest niepoprawny

            IsAddPopupVisible = true;
        }

        /// <summary>
        /// Komenda anulująca dodawanie i zamykająca okienko.
        /// </summary>
        [RelayCommand]
        private void CancelAdd()
        {
            IsAddPopupVisible = false;
        }

        /// <summary>
        /// Komenda asynchroniczna otwierająca systemowy eksplorator plików do wyboru zdjęcia awatara dla nowego członka.
        /// </summary>
        [RelayCommand]
        private async Task OpenAvatarPickerAsync()
        {
            if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var window = desktop.MainWindow;
                if (window != null)
                {
                    var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                    {
                        Title = "Wybierz zdjęcie",
                        AllowMultiple = false,
                        FileTypeFilter = new[] { FilePickerFileTypes.ImageAll }
                    });

                    if (files.Count >= 1)
                    {
                        using var stream = await files[0].OpenReadAsync();
                        using var ms = new MemoryStream();
                        await stream.CopyToAsync(ms);
                        NewAvatar = ms.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// Pomocnicza metoda walidująca adres e-mail za pomocą prostego wyrażenia regularnego.
        /// </summary>
        /// <param name="email">Adres e-mail do sprawdzenia.</param>
        /// <returns>Zwraca true, jeśli format jest poprawny lub adres jest pusty, w przeciwnym razie false.</returns>
        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return true; // email jest opcjonalny
            return System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        /// <summary>
        /// Komenda zapisująca nowego członka w bazie danych.
        /// </summary>
        [RelayCommand]
        private async Task ConfirmAddAsync()
        {
            AddValidationError = string.Empty;

            if (string.IsNullOrWhiteSpace(NewFirstName))
            {
                AddValidationError = "Imię jest wymagane.";
                return;
            }

            if (string.IsNullOrWhiteSpace(NewLastName))
            {
                AddValidationError = "Nazwisko jest wymagane.";
                return;
            }

            if (!IsValidEmail(NewEmail))
            {
                AddValidationError = "Niepoprawny format adresu e-mail.";
                return;
            }

            if (!string.IsNullOrWhiteSpace(NewIndexNumber) && !NewIndexNumber.All(char.IsDigit))
            {
                AddValidationError = "Numer indeksu musi składać się wyłącznie z cyfr.";
                return;
            }

            var newMemberDb = new Models.Users.Member
            {
                FirstName = NewFirstName,
                LastName = NewLastName,
                IndexNumber = NewIndexNumber,
                Major = NewMajor,
                Description = NewDescription,
                MemberAvatar = NewAvatar,
                PhoneNumber = NewPhoneNumber,
                IsActive = true,
                JoinDate = System.DateTime.Now,
                AuthorityRole = SelectedRole,
                RoleId = SelectedRole?.RoleId ?? 0,
                Account = string.IsNullOrWhiteSpace(NewEmail)? null: new UserAccount
                            {
                                Email = NewEmail,
                                SystemRole = Models.Enums.SystemRole.User
                            }
            };
            int clubId = 1;

            newMemberDb.MemberClubs = new List<MemberClub>
            {
                new MemberClub { ClubId = clubId }
            };

            await _memberRepository.AddMemberAsync(newMemberDb, SelectedDepartment?.CollegeDepartmentId);

            await LoadDataAsync();

            IsAddPopupVisible = false;
        }

        /// <summary>
        /// Wywoływane po zmianie tekstu wyszukiwania. Resetuje stronę do 1 i filtruje listę członków.
        /// </summary>
        /// <param name="value">Nowa wpisana fraza.</param>
        protected override void OnSearchQueryUpdated(string value)
        {
            CurrentPage = 1;
            ApplyFilter();
        }

        /// <summary>
        /// Reaguje na zmianę zaznaczenia (IsSelected) na pojedynczym elemencie listy członków.
        /// </summary>
        /// <param name="sender">Element, w którym zaszła zmiana.</param>
        /// <param name="e">Argumenty zdarzenia.</param>
        private void OnMemberItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MemberItemViewModel.IsSelected))
            {
                UpdateSelectionState();

                if (!_isUpdatingSelection)
                {
                    _isUpdatingSelection = true;
                    try
                    {
                        var selectableMembers = Members.Where(m => !m.IsSystemAddTile).ToList();
                        if (selectableMembers.Any())
                        {
                            IsAllSelected = selectableMembers.All(m => m.IsSelected);
                        }
                    }
                    finally
                    {
                        _isUpdatingSelection = false;
                    }
                }
            }
        }

        /// <summary>
        /// Odpina zdarzenia od wszystkich modeli widoku członków, chroniąc przed wyciekami pamięci.
        /// </summary>
        private void ClearMemberSubscriptions()
        {
            foreach (var vm in _allMembers)
            {
                vm.PropertyChanged -= OnMemberItemPropertyChanged;
            }
        }

        /// <summary>
        /// Asynchronicznie pobiera z bazy dane o rolach, wydziałach i członkach, po czym aktualizuje listę.
        /// </summary>
        private async Task LoadDataAsync()
        {
            try
            {
                var roles = await _memberRepository.GetAuthorityRolesAsync();
                var departments = await _memberRepository.GetCollegeDepartmentsAsync();
                Dispatcher.UIThread.Post(() =>
                {
                    AvailableRoles.Clear();
                    foreach (var r in roles) AvailableRoles.Add(r);

                    AvailableDepartments.Clear();
                    foreach (var d in departments) AvailableDepartments.Add(d);
                });

                var membersFromDb = await _memberRepository.GetAllMembersAsync();
                Dispatcher.UIThread.Post(() =>
                {
                    ClearMemberSubscriptions();
                    _allMembers.Clear();
                    _allMembers.Add(new MemberItemViewModel(0, Array.Empty<byte>(), "", "", "", "", "", "", "", "", "", true, "", true));

                    if (membersFromDb != null)
                    {
                        foreach (var m in membersFromDb)
                        {
                            var dept = m.MemberClubs?.FirstOrDefault()?.Club?.Department?.Name ?? "Brak wydziału";
                            var vm = new MemberItemViewModel(
                                memberId: m.MemberId,
                                avatar: m.MemberAvatar ?? Array.Empty<byte>(),
                                firstName: m.FirstName ?? string.Empty,
                                lastName: m.LastName ?? string.Empty,
                                role: m.AuthorityRole?.Name ?? "Brak roli",
                                indexNumber: m.IndexNumber ?? string.Empty,
                                email: m.Account?.Email ?? string.Empty,
                                phoneNumber: m.PhoneNumber ?? string.Empty,
                                collegeDepartment: dept,
                                major: m.Major ?? string.Empty,
                                joinDate: m.JoinDate.ToString("dd.MM.yyyy"),
                                isActive: m.IsActive,
                                description: m.Description ?? string.Empty,
                                isSystemAddTile: false
                            );
                            vm.PropertyChanged += OnMemberItemPropertyChanged;
                            _allMembers.Add(vm);
                        }
                    }
                    ApplyFilter();
                });
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
        }

        /// <summary>
        /// Numer aktualnie wyświetlanej strony.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasPreviousPage))]
        [NotifyPropertyChangedFor(nameof(HasNextPage))]
        private int _currentPage = 1;

        /// <summary>
        /// Łączna liczba stron z wynikami.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasPreviousPage))]
        [NotifyPropertyChangedFor(nameof(HasNextPage))]
        private int _totalPages = 1;

        /// <summary>
        /// Czy istnieje poprzednia strona.
        /// </summary>
        public bool HasPreviousPage => CurrentPage > 1;

        /// <summary>
        /// Czy istnieje kolejna strona.
        /// </summary>
        public bool HasNextPage => CurrentPage < TotalPages;

        /// <summary>
        /// Komenda przełączająca na następną stronę wyników.
        /// </summary>
        [RelayCommand]
        private void NextPage()
        {
            if (HasNextPage)
            {
                CurrentPage++;
                ApplyFilter();
            }
        }

        /// <summary>
        /// Komenda przełączająca na poprzednią stronę wyników.
        /// </summary>
        [RelayCommand]
        private void PreviousPage()
        {
            if (HasPreviousPage)
            {
                CurrentPage--;
                ApplyFilter();
            }
        }

        /// <summary>
        /// Filtruje listę członków koła w oparciu o wpisane zapytanie, a także zarządza podziałem na strony.
        /// Pierwsza strona zawiera kafelek szybkiego dodawania nowego członka.
        /// </summary>
        private void ApplyFilter()
        {
            Members.Clear();
            var query = SearchQuery?.ToLower() ?? "";
            
            var filteredActualMembers = _allMembers
                .Where(item => !item.IsSystemAddTile)
                .Where(item => 
                    item.FullName.ToLower().Contains(query) || 
                    item.Role.ToLower().Contains(query) ||
                    item.IndexNumber.ToLower().Contains(query) ||
                    item.Email.ToLower().Contains(query) ||
                    item.CollegeDepartment.ToLower().Contains(query) ||
                    item.Major.ToLower().Contains(query))
                .ToList();

            int pageSize = 9;
            
            if (CurrentPage == 1)
            {
                int totalCount = filteredActualMembers.Count;
                TotalPages = (int)Math.Ceiling((double)(totalCount - 8) / pageSize) + 1;
                if (TotalPages < 1) TotalPages = 1;

                var addTile = _allMembers.FirstOrDefault(item => item.IsSystemAddTile);
                if (addTile != null)
                {
                    Members.Add(addTile);
                }

                var pageMembers = filteredActualMembers.Take(8);
                foreach (var item in pageMembers)
                {
                    Members.Add(item);
                }
            }
            else
            {
                int totalCount = filteredActualMembers.Count;
                TotalPages = (int)Math.Ceiling((double)(totalCount - 8) / pageSize) + 1;
                if (TotalPages < 1) TotalPages = 1;

                int skip = 8 + (CurrentPage - 2) * pageSize;
                var pageMembers = filteredActualMembers.Skip(skip).Take(pageSize);
                foreach (var item in pageMembers)
                {
                    Members.Add(item);
                }
            }
            
            UpdateSelectionState();
        }
    }
}

