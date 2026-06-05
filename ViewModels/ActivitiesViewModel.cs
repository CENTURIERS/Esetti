using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Esseti.Repositories.Interfaces;
using Esseti.Services;
using Esseti.ViewModels.Components;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Models.Activities;

namespace Esseti.ViewModels
{
    /// <summary>
    /// Model widoku obsługujący listę wszystkich aktywności w kole/klubie.
    /// Zawiera wyszukiwanie, podział na strony, obsługę popupów do dodawania i edytowania danych oraz usuwanie jednej lub wielu aktywności.
    /// </summary>
    public partial class ActivitiesViewModel : ViewModelBase
    {
        /// <summary>
        /// Tytuł wyświetlany w nagłówku strony.
        /// </summary>
        public override string PageTitle => "Lista Aktywności";

        /// <summary>
        /// Pokazujemy pasek z wyszukiwarką i przyciskami akcji w nagłówku.
        /// </summary>
        public override bool ShowActionHeader => true;

        /// <summary>
        /// Tekst pomocniczy w polu wyszukiwania aktywności.
        /// </summary>
        public override string SearchPlaceholder => "Szukaj aktywności...";

        /// <summary>
        /// Kolekcja aktywności wyświetlana na aktualnej stronie.
        /// </summary>
        public ObservableCollection<ActivityItemViewModel> Activities { get; } = new();

        /// <summary>
        /// Repozytorium do operacji na bazie danych związanych z aktywnościami.
        /// </summary>
        private readonly IActivityRepository _activityRepository;

        /// <summary>
        /// Repozytorium do operacji na bazie danych związanych z członkami koła.
        /// </summary>
        private readonly IMemberRepository _memberRepository;

        /// <summary>
        /// Pełna lista wszystkich aktywności załadowanych na początku, przydatna do filtrowania.
        /// </summary>
        private readonly List<ActivityItemViewModel> _allActivities = new();

        /// <summary>
        /// Wybrana aktywność przeznaczona do usunięcia.
        /// </summary>
        private ActivityItemViewModel? _activityToDelete;

        /// <summary>
        /// Referencja do aktywności aktualnie edytowanej w popupie (null przy dodawaniu).
        /// </summary>
        private ActivityItemViewModel? _editingActivity;

        /// <summary>
        /// Nazwa nowej lub edytowanej aktywności w formularzu.
        /// </summary>
        [ObservableProperty]
        private string _newActivityName = string.Empty;

        /// <summary>
        /// Opis lub szczegóły aktywności w formularzu.
        /// </summary>
        [ObservableProperty]
        private string _newActivityDescription = string.Empty;

        /// <summary>
        /// Data wydarzenia/aktywności jako tekst.
        /// </summary>
        [ObservableProperty]
        private string _newActivityDate = DateTime.Now.ToString("dd.MM.yyyy");

        /// <summary>
        /// Godzina wydarzenia jako tekst.
        /// </summary>
        [ObservableProperty]
        private string _newActivityTime = DateTime.Now.ToString("HH:mm");

        /// <summary>
        /// Miasto, w którym odbywa się aktywność.
        /// </summary>
        [ObservableProperty]
        private string _newActivityCity = string.Empty;

        /// <summary>
        /// Ulica i numer (adres), gdzie odbywa się aktywność.
        /// </summary>
        [ObservableProperty]
        private string _newActivityStreet = string.Empty;

        /// <summary>
        /// Nazwisko lub nazwa osoby odpowiedzialnej za to wydarzenie (jeśli spoza klubu).
        /// </summary>
        [ObservableProperty]
        private string _newActivityPersonInChargeEvent = string.Empty;

        /// <summary>
        /// Telefon kontaktowy do osoby odpowiedzialnej.
        /// </summary>
        [ObservableProperty]
        private string _newActivityPersonInChargePhone = string.Empty;

        /// <summary>
        /// Email kontaktowy do osoby odpowiedzialnej.
        /// </summary>
        [ObservableProperty]
        private string _newActivityPersonInChargeEmail = string.Empty;

        /// <summary>
        /// Czy ta aktywność jest powtarzalna cyklicznie.
        /// </summary>
        [ObservableProperty]
        private bool _newActivityIsRepeatable;

        /// <summary>
        /// Tekstowa lista wszystkich członków koła (sformatowane jako "Imię Nazwisko") do wyboru.
        /// </summary>
        public ObservableCollection<string> ClubMembers { get; } = new();
        
        /// <summary>
        /// Wybrany z klubu członek odpowiedzialny za tę aktywność.
        /// </summary>
        [ObservableProperty]
        private string _newActivityPersonInChargeClub = string.Empty;

        /// <summary>
        /// Lista uczestników przypisanych do tego wydarzenia.
        /// </summary>
        public ObservableCollection<string> EventMembers { get; } = new();

        /// <summary>
        /// Czy popup dodawania/edycji aktywności jest obecnie wyświetlany na ekranie.
        /// </summary>
        [ObservableProperty]
        private bool _isAddEditPopupVisible;

        /// <summary>
        /// Tytuł okienka popup (np. "Nowa aktywność" lub "Edycja aktywności").
        /// </summary>
        [ObservableProperty]
        private string _popupTitle = "Nowa aktywność";

        /// <summary>
        /// Flaga błędu walidacji dla nazwy.
        /// </summary>
        [ObservableProperty]
        private bool _isNameInvalid;

        /// <summary>
        /// Flaga błędu walidacji dla daty.
        /// </summary>
        [ObservableProperty]
        private bool _isDateInvalid;

        /// <summary>
        /// Flaga błędu walidacji dla godziny.
        /// </summary>
        [ObservableProperty]
        private bool _isTimeInvalid;

        /// <summary>
        /// Flaga błędu walidacji dla emaila.
        /// </summary>
        [ObservableProperty]
        private bool _isEmailInvalid;

        /// <summary>
        /// Flaga błędu walidacji dla telefonu.
        /// </summary>
        [ObservableProperty]
        private bool _isPhoneInvalid;

        /// <summary>
        /// Flaga informująca, czy cały formularz aktywności przeszedł walidację pomyślnie.
        /// </summary>
        [ObservableProperty]
        private bool _isFormValid = true;

        /// <summary>
        /// Komunikat błędu z opisem problemu w formularzu.
        /// </summary>
        [ObservableProperty]
        private string _validationError = string.Empty;

        /// <summary>
        /// Przeprowadza walidację pól formularza nowej/edycji aktywności. Ustawia odpowiednie flagi błędów.
        /// </summary>
        private void ValidateForm()
        {
            IsNameInvalid = string.IsNullOrWhiteSpace(NewActivityName);
            IsDateInvalid = string.IsNullOrWhiteSpace(NewActivityDate) || !TryParseDate(NewActivityDate, out _);
            IsTimeInvalid = string.IsNullOrWhiteSpace(NewActivityTime) || !TimeSpan.TryParse(NewActivityTime, out _);
            IsEmailInvalid = !string.IsNullOrWhiteSpace(NewActivityPersonInChargeEmail) && 
                              !System.Text.RegularExpressions.Regex.IsMatch(NewActivityPersonInChargeEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            IsPhoneInvalid = !string.IsNullOrWhiteSpace(NewActivityPersonInChargePhone) && 
                             !System.Text.RegularExpressions.Regex.IsMatch(NewActivityPersonInChargePhone, @"^[+]*[(]{0,1}[0-9]{1,4}[)]{0,1}[-\s\./0-9]*$");

            IsFormValid = !IsNameInvalid && !IsDateInvalid && !IsTimeInvalid && !IsEmailInvalid && !IsPhoneInvalid;

            if (IsNameInvalid) ValidationError = "Nazwa aktywności jest wymagana.";
            else if (IsDateInvalid) ValidationError = "Data aktywności jest wymagana i musi być poprawna.";
            else if (IsTimeInvalid) ValidationError = "Godzina aktywności jest wymagana i musi być poprawna (np. hh:mm).";
            else if (IsEmailInvalid) ValidationError = "Niepoprawny format adresu e-mail osoby odpowiedzialnej.";
            else if (IsPhoneInvalid) ValidationError = "Niepoprawny format numeru telefonu osoby odpowiedzialnej.";
            else ValidationError = string.Empty;
        }

        /// <summary>
        /// Metoda wywoływana przy zmianie obserwowanych właściwości. Uruchamia automatyczną walidację formularza.
        /// </summary>
        /// <param name="e">Argumenty zdarzenia.</param>
        protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(NewActivityName) ||
                e.PropertyName == nameof(NewActivityDate) ||
                e.PropertyName == nameof(NewActivityTime) ||
                e.PropertyName == nameof(NewActivityPersonInChargeEmail) ||
                e.PropertyName == nameof(NewActivityPersonInChargePhone))
            {
                ValidateForm();
            }
        }

        /// <summary>
        /// Serwis służący do przełączania ekranów.
        /// </summary>
        private readonly INavigationService _navigationService;

        /// <summary>
        /// Zwraca napis dla przycisku zaznaczania na podstawie aktualnego stanu zaznaczenia wszystkich kafelków.
        /// </summary>
        public string SelectAllText => IsAllSelected ? "Odznacz wszystko" : "Zaznacz wszystko"; 

        /// <summary>
        /// Czy jakikolwiek aktywność z listy jest w tym momencie zaznaczona checkboxem.
        /// </summary>
        public bool HasSelectedItems => IsAnySelected;

        /// <summary>
        /// Tekst informujący o liczbie zaznaczonych aktywności do usunięcia.
        /// </summary>
        public string SelectedCountText => $"Zaznaczono: {SelectedCount} aktywności";

        /// <summary>
        /// Konstruktor modelu widoku. Wstrzykuje repozytoria i serwis nawigacyjny oraz ładuje asynchronicznie dane z bazy.
        /// </summary>
        /// <param name="activityRepository">Repozytorium aktywności.</param>
        /// <param name="memberRepository">Repozytorium członków.</param>
        /// <param name="navigationService">Wspólny serwis nawigacji.</param>
        public ActivitiesViewModel(IActivityRepository activityRepository, IMemberRepository memberRepository, INavigationService navigationService)
        {
            _activityRepository = activityRepository;
            _memberRepository = memberRepository;
            _navigationService = navigationService;
            _ = LoadDataAsync();
        }

        /// <summary>
        /// Komenda do zaznaczenia lub odznaczenia wszystkich widocznych aktywności.
        /// </summary>
        [RelayCommand]
        private void ToggleSelectAll()
        {
            IsAllSelected = !IsAllSelected;
        }

        /// <summary>
        /// Komenda otwierająca popup tworzenia nowej aktywności z wyczyszczonym formularzem.
        /// </summary>
        [RelayCommand]
        private void OpenAddPopup()
        {
            _editingActivity = null;
            PopupTitle = "Nowa aktywność";
            NewActivityName = string.Empty;
            NewActivityDescription = string.Empty;
            NewActivityDate = DateTime.Now.ToString("dd.MM.yyyy");
            NewActivityTime = DateTime.Now.ToString("HH:mm");
            NewActivityCity = string.Empty;
            NewActivityStreet = string.Empty;
            NewActivityPersonInChargeEvent = string.Empty;
            NewActivityPersonInChargePhone = string.Empty;
            NewActivityPersonInChargeEmail = string.Empty;
            NewActivityIsRepeatable = false;
            
            IsNameInvalid = false;
            IsDateInvalid = false;
            IsTimeInvalid = false;
            IsEmailInvalid = false;
            IsPhoneInvalid = false;
            IsFormValid = false; // Pusta nazwa na start oznacza formularz do poprawy

            IsAddEditPopupVisible = true;
        }

        /// <summary>
        /// Komenda otwierająca popup w trybie edycji danej aktywności i wczytująca jej aktualne dane.
        /// </summary>
        /// <param name="item">Model widoku aktywności wybranej do edycji.</param>
        [RelayCommand]
        private async Task OpenEditPopup(ActivityItemViewModel item)
        {
            if (item == null) return;
            _editingActivity = item;
            PopupTitle = "Edycja aktywności";

            if (int.TryParse(item.ActivityId, out var activityId))
            {
                var fullActivity = await _activityRepository.GetActivityByIdAsync(activityId);
                if (fullActivity != null)
                {
                    NewActivityName = fullActivity.Name;
                    NewActivityDescription = fullActivity.AdditionalInformation ?? string.Empty;
                    NewActivityDate = fullActivity.Date.ToString("dd.MM.yyyy");
                    NewActivityTime = fullActivity.Time?.ToString(@"hh\:mm") ?? string.Empty;
                    NewActivityCity = fullActivity.City ?? string.Empty;
                    NewActivityStreet = fullActivity.AddressLine ?? string.Empty;
                    NewActivityPersonInChargeEvent = fullActivity.PersonInChargeName ?? string.Empty;
                    NewActivityPersonInChargePhone = fullActivity.PersonInChargePhone ?? string.Empty;
                    NewActivityPersonInChargeEmail = fullActivity.PersonInChargeEmail ?? string.Empty;
                    NewActivityIsRepeatable = fullActivity.IsRepeatable;
                }
            }
            ValidateForm();
            IsAddEditPopupVisible = true;
        }

        /// <summary>
        /// Komenda zamykająca popup edycji/dodawania aktywności.
        /// </summary>
        [RelayCommand]
        private void ClosePopup()
        {
            IsAddEditPopupVisible = false;
        }

        /// <summary>
        /// Komenda zapisująca dane aktywności (tworzy nową lub aktualizuje w bazie).
        /// </summary>
        [RelayCommand(CanExecute = nameof(IsFormValid))]
        private async Task SaveActivity()
        {
            TryParseDate(NewActivityDate, out var parsedDate);
            TimeSpan? parsedTime = TimeSpan.TryParse(NewActivityTime, out var t) ? t : null;

            if (_editingActivity == null)
            {
                Activity activityAdd = new Activity
                {
                    Name = NewActivityName,
                    Date = parsedDate,
                    Time = parsedTime,
                    City = NewActivityCity,
                    AddressLine = NewActivityStreet,
                    AdditionalInformation = NewActivityDescription,
                    PersonInChargeName = NewActivityPersonInChargeEvent,
                    PersonInChargePhone = NewActivityPersonInChargePhone,
                    PersonInChargeEmail = NewActivityPersonInChargeEmail,
                    IsRepeatable = NewActivityIsRepeatable
                };
                await _activityRepository.AddActivityAsync(activityAdd);
            }

            if (_editingActivity != null)
            {
                if (int.TryParse(_editingActivity.ActivityId, out var activityId))
                {
                    var activityUpdate = await _activityRepository.GetActivityByIdAsync(activityId);
                    if (activityUpdate != null) {
                        activityUpdate.Name = NewActivityName;
                        activityUpdate.Date = parsedDate;
                        activityUpdate.Time = parsedTime;
                        activityUpdate.City = NewActivityCity;
                        activityUpdate.AddressLine = NewActivityStreet;
                        activityUpdate.AdditionalInformation = NewActivityDescription;
                        activityUpdate.PersonInChargeName = NewActivityPersonInChargeEvent;
                        activityUpdate.PersonInChargePhone = NewActivityPersonInChargePhone;
                        activityUpdate.PersonInChargeEmail = NewActivityPersonInChargeEmail;
                        activityUpdate.IsRepeatable = NewActivityIsRepeatable;
                        
                        await _activityRepository.UpdateActivityAsync(activityUpdate);
                    }
                }
            }

            await LoadDataAsync();
            IsAddEditPopupVisible = false;
        }

        /// <summary>
        /// Komenda otwierająca popup potwierdzenia usunięcia pojedynczej aktywności z bazy.
        /// </summary>
        /// <param name="item">Model widoku aktywności do usunięcia.</param>
        [RelayCommand]
        private void DeleteSingleActivity(ActivityItemViewModel item)
        {
            if (item == null) return;
            _activityToDelete = item;
            IsPopupVisible = true;
        }

        /// <summary>
        /// Wykonuje operację usunięcia wybranej aktywności lub grupy zaznaczonych aktywności z bazy.
        /// </summary>
        protected override async Task ExecuteConfirmDeleteAsync()
        {
            try
            {
                if (_activityToDelete != null)
                {
                    if (int.TryParse(_activityToDelete.ActivityId, out var activityId))
                    {
                        await _activityRepository.DeleteSingleActivityAsync(activityId);
                    }
                    _activityToDelete = null;
                }
                else
                {
                    var selectedVMs = Activities.Where(a => a.IsSelected).ToList();
                    if (!selectedVMs.Any()) return;

                    var idsToDelete = new List<int>();
                    foreach (var a in selectedVMs)
                    {
                        if (int.TryParse(a.ActivityId, out var id))
                        {
                            idsToDelete.Add(id);
                        }
                    }
                    await _activityRepository.DeleteActivitesAsync(idsToDelete);
                }
                IsAllSelected = false;
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd usuwania aktywności: {ex.Message}");
            }
        }

        /// <summary>
        /// Czyszczenie referencji przy zamykaniu popupa.
        /// </summary>
        protected override void OnPopupClosed()
        {
            _activityToDelete = null;
        }

        /// <summary>
        /// Nadpisana metoda obsługująca masowe zaznaczanie/odznaczanie kafelków.
        /// </summary>
        /// <param name="value">Czy zaznaczyć wszystkie (true), czy odznaczyć (false).</param>
        protected override void OnIsAllSelectedChangedVirtual(bool value)
        {
            if (_isUpdatingSelection) return;

            _isUpdatingSelection = true;
            try {
                foreach (var activity in Activities) {
                    activity.IsSelected = value;
                }
                UpdateSelectionState();
            } finally {
                _isUpdatingSelection = false;
            }
        }

        /// <summary>
        /// Przelicza liczbę zaznaczonych aktywności na liście i zgłasza zmianę właściwości.
        /// </summary>
        public void UpdateSelectionState() {
            var selected = Activities.Where(a => a.IsSelected).ToList();
            SelectedCount = selected.Count;
            IsAnySelected = SelectedCount > 0;

            OnPropertyChanged(nameof(SelectAllText));
            OnPropertyChanged(nameof(HasSelectedItems));
            OnPropertyChanged(nameof(SelectedCountText));
        }

        /// <summary>
        /// Numer aktualnie wyświetlanej strony aktywności.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasPreviousPage))]
        [NotifyPropertyChangedFor(nameof(HasNextPage))]
        private int _currentPage = 1;

        /// <summary>
        /// Łączna liczba stron z aktywnościami po przefiltrowaniu.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasPreviousPage))]
        [NotifyPropertyChangedFor(nameof(HasNextPage))]
        private int _totalPages = 1;

        /// <summary>
        /// Zwraca true, jeśli istnieje poprzednia strona z wynikami.
        /// </summary>
        public bool HasPreviousPage => CurrentPage > 1;

        /// <summary>
        /// Zwraca true, jeśli istnieje kolejna strona z wynikami.
        /// </summary>
        public bool HasNextPage => CurrentPage < TotalPages;

        /// <summary>
        /// Przełącza listę na następną stronę.
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
        /// Przełącza listę na poprzednią stronę.
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
        /// Metoda uruchamiana przy wpisaniu tekstu w pole wyszukiwania. Resetuje stronę do pierwszej i filtruje.
        /// </summary>
        /// <param name="value">Wyszukiwana fraza.</param>
        protected override void OnSearchQueryUpdated(string value)
        {
            CurrentPage = 1;
            ApplyFilter();
        }

        /// <summary>
        /// Filtruje listę aktywności i rozdziela je na strony (paginacja).
        /// </summary>
        private void ApplyFilter()
        {
            Activities.Clear();
            var query = SearchQuery?.ToLower() ?? "";

            var filteredList = _allActivities
                .Where(item => (item.Name ?? "").ToLower().Contains(query) || (item.Description ?? "").ToLower().Contains(query))
                .ToList();

            int pageSize = 9;
            int totalCount = filteredList.Count;
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            if (TotalPages < 1) TotalPages = 1;

            if (CurrentPage > TotalPages) CurrentPage = TotalPages;
            if (CurrentPage < 1) CurrentPage = 1;

            var pageItems = filteredList.Skip((CurrentPage - 1) * pageSize).Take(pageSize);
            foreach (var item in pageItems)
            {
                Activities.Add(item);
            }

            UpdateSelectionState();
        }

        /// <summary>
        /// Obsługa zmiany zaznaczenia na pojedynczym kafelku aktywności.
        /// </summary>
        private void OnActivityItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ActivityItemViewModel.IsSelected)) {
                UpdateSelectionState();

                if (!_isUpdatingSelection) {
                    _isUpdatingSelection = true;
                    try {
                        if (Activities.Any()) {
                            IsAllSelected = Activities.All(a => a.IsSelected);
                        }
                    } finally {
                        _isUpdatingSelection = false;
                    }
                }
            }
        }

        /// <summary>
        /// Odczepia subskrypcje zdarzeń od wszystkich kafelków, by nie śmiecić w pamięci.
        /// </summary>
        private void ClearActivitySubscriptions()
        {
            foreach (var vm in _allActivities)
            {
                vm.PropertyChanged -= OnActivityItemPropertyChanged;
            }
        }

        /// <summary>
        /// Asynchronicznie ładuje listę wszystkich aktywności i członków koła z bazy danych do comboboxa.
        /// </summary>
        private async Task LoadDataAsync()
        {
            try 
            {
                var activitiesFromDb = await _activityRepository.GetAllActivitiesAsync();
                var membersFromDb = await _memberRepository.GetAllMembersAsync();

                Dispatcher.UIThread.Post(() => {
                    ClearActivitySubscriptions();
                    Activities.Clear();
                    _allActivities.Clear();
                    
                    if (activitiesFromDb != null) {
                        foreach (var act in activitiesFromDb) {
                            var vm = new ActivityItemViewModel(
                                activityId: act.ActivityId.ToString(),
                                name: act.Name,
                                description: act.AdditionalInformation ?? "Brak opisu",
                                dateString: act.Date.ToString("dd.MM.yyyy"),
                                isSelected: false,
                                isRepeatable: act.IsRepeatable
                            );

                            vm.PropertyChanged += OnActivityItemPropertyChanged;

                            _allActivities.Add(vm);
                        }
                    }

                    ClubMembers.Clear();
                    if (membersFromDb != null) {
                        foreach (var m in membersFromDb) {
                            ClubMembers.Add($"{m.FirstName} {m.LastName}");
                        }
                    }

                    ApplyFilter();
                });
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"Błąd pobierania aktywności: {ex.Message}");
            }
        }

        /// <summary>
        /// Komenda nawigująca użytkownika do pełnego profilu szczegółowego wybranej aktywności.
        /// </summary>
        /// <param name="item">Model widoku aktywności.</param>
        [RelayCommand]
        private void OpenProfile(ActivityItemViewModel item) 
        {
            if (item == null) return;

            if (int.TryParse(item.ActivityId, out var activityId))
            {
                var profileVm = new ActivityProfileViewModel (
                    activityId,
                    _navigationService,
                    _memberRepository,
                    _activityRepository
                );

                _navigationService.NavigateTo(profileVm);
            }
        }
    }
}
