using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Esseti.Repositories.Interfaces;
using Models.Activities;
using Models.ClubBase;
using Models.Other;
using Models.Users;
using Avalonia.Media.Imaging;

namespace Esseti.ViewModels
{
    /// <summary>
    /// ViewModel odpowiedzialny za obsługę widoku informacji o kole naukowym.
    /// Zarządza danymi takimi jak nazwa koła, członkowie zarządu, wyjazdy i sekcje.
    /// </summary>
    public partial class ClubInfoViewModel : ViewModelBase
    {
        /// <summary>
        /// Tytuł strony wyświetlany w nagłówku widoku.
        /// </summary>
        public override string PageTitle => "O kole";

        private readonly IClubRepository _clubRepository;
        private readonly ITripRepository _tripRepository;

        [ObservableProperty]
        private string _clubName = string.Empty;

        [ObservableProperty]
        private string _clubNameShort = string.Empty;

        [ObservableProperty]
        private string _departmentName = string.Empty;

        [ObservableProperty]
        private string _clubRoom = string.Empty;

        [ObservableProperty]
        private string _universityName = string.Empty;

        [ObservableProperty]
        private string _universityShortName = string.Empty;

        [ObservableProperty]
        private string _universityAddress = string.Empty;

        [ObservableProperty]
        private string _supervisorName = string.Empty;

        [ObservableProperty]
        private string _supervisorEmail = string.Empty;

        [ObservableProperty]
        private string _supervisorPhone = string.Empty;

        [ObservableProperty]
        private string _meetingsInfo = string.Empty;

        [ObservableProperty]
        private string _editClubName = string.Empty;

        [ObservableProperty]
        private string _editClubRoom = string.Empty;

        [ObservableProperty]
        private string _editDepartmentName = string.Empty;

        [ObservableProperty]
        private string _editSupervisorName = string.Empty;

        [ObservableProperty]
        private string _editSupervisorEmail = string.Empty;

        [ObservableProperty]
        private string _editSupervisorPhone = string.Empty;

        [ObservableProperty]
        private string _editMeetingsSchedule = string.Empty;

        [ObservableProperty]
        private string _editClubNameShort = string.Empty;

        [ObservableProperty]
        private bool _isEditPopupVisible;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasClubPhoto))]
        private Bitmap? _clubPhotoBitmap;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasEditClubPhoto))]
        private Bitmap? _editClubPhotoBitmap;

        private byte[]? _clubPhotoBytes;
        private byte[]? _editClubPhotoBytes;

        /// <summary>
        /// Zwraca true, jeśli koło ma ustawione zdjęcie (logo).
        /// </summary>
        public bool HasClubPhoto => ClubPhotoBitmap != null;

        /// <summary>
        /// Zwraca true, jeśli w formularzu edycji wybrano zdjęcie koła.
        /// </summary>
        public bool HasEditClubPhoto => EditClubPhotoBitmap != null;

        [ObservableProperty]
        private string _newTripName = string.Empty;

        [ObservableProperty]
        private string _newTripDescription = string.Empty;

        [ObservableProperty]
        private string _newTripDate = string.Empty;

        [ObservableProperty]
        private bool _isAddTripPopupVisible;

        [ObservableProperty]
        private string _editTripName = string.Empty;

        [ObservableProperty]
        private string _editTripDescription = string.Empty;

        [ObservableProperty]
        private string _editTripDate = string.Empty;

        [ObservableProperty]
        private bool _isEditTripPopupVisible;

        [ObservableProperty] private bool _isAddTripNameInvalid;
        [ObservableProperty] private bool _isAddTripDateInvalid;
        [ObservableProperty] private bool _isAddTripFormValid = true;
        [ObservableProperty] private string _addTripValidationError = string.Empty;

        [ObservableProperty] private bool _isEditTripNameInvalid;
        [ObservableProperty] private bool _isEditTripDateInvalid;
        [ObservableProperty] private bool _isEditTripFormValid = true;
        [ObservableProperty] private string _editTripValidationError = string.Empty;

        private Trip? _tripBeingEdited;

        /// <summary>
        /// Zwraca true, jeśli nie ma żadnych członków zarządu do wyświetlenia.
        /// </summary>
        public bool HasNoBoardMembers => !BoardMembers.Any();

        /// <summary>
        /// Zwraca true, jeśli nie ma żadnych wyjazdów do wyświetlenia.
        /// </summary>
        public bool HasNoTrips => !Trips.Any();

        [ObservableProperty]
        private int _membersCount;

        [ObservableProperty]
        private int _projectsCount;

        [ObservableProperty]
        private int _sectionsCount;

        [ObservableProperty]
        private int _activitiesCount;

        [ObservableProperty]
        private string _newSectionName = string.Empty;

        [ObservableProperty]
        private bool _isAddSectionPopupVisible;

        [ObservableProperty]
        private bool _isDeleteTripConfirmVisible;

        [ObservableProperty]
        private bool _isDeleteSectionConfirmVisible;

        private Trip? _tripToDelete;
        private Section? _sectionToDelete;

        /// <summary>
        /// Kolekcja sekcji należących do koła naukowego.
        /// </summary>
        public ObservableCollection<Section> Sections { get; } = new();

        /// <summary>
        /// Kolekcja członków zarządu koła naukowego.
        /// </summary>
        public ObservableCollection<Models.Users.Member> BoardMembers { get; } = new();

        /// <summary>
        /// Kolekcja wyjazdów organizowanych przez koło naukowe.
        /// </summary>
        public ObservableCollection<Trip> Trips { get; } = new();

        /// <summary>
        /// Inicjalizuje ViewModel informacji o kole i uruchamia ładowanie danych z bazy.
        /// </summary>
        /// <param name="clubRepository">Repozytorium danych koła naukowego.</param>
        /// <param name="tripRepository">Repozytorium danych wyjazdów.</param>
        public ClubInfoViewModel(IClubRepository clubRepository, ITripRepository tripRepository)
        {
            _clubRepository = clubRepository;
            _tripRepository = tripRepository;
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                MembersCount = await _clubRepository.GetMembersCountAsync();
                ProjectsCount = await _clubRepository.GetProjectsCountAsync();
                SectionsCount = await _clubRepository.GetSectionsCountAsync();
                ActivitiesCount = await _clubRepository.GetActivitiesCountAsync();

                var club = await _clubRepository.GetClubInfoAsync();
                if (club != null)
                {
                    ClubName = club.Name ?? string.Empty;
                    ClubNameShort = club.ShortName ?? string.Empty;
                    ClubRoom = club.ClubRoom ?? string.Empty;
                    DepartmentName = club.Department?.Name ?? string.Empty;
                    SupervisorName = club.SupervisorName ?? string.Empty;
                    SupervisorEmail = club.SupervisorEmail ?? string.Empty;
                    SupervisorPhone = club.SupervisorPhone ?? string.Empty;
                    MeetingsInfo = club.MeetingsSchedule ?? string.Empty;

                    _clubPhotoBytes = club.ClubPhoto;
                    ClubPhotoBitmap = LoadBitmap(_clubPhotoBytes);

                    var college = club.Department?.College;
                    if (college != null)
                    {
                        UniversityName = college.Name ?? string.Empty;
                        UniversityShortName = college.NameShort ?? string.Empty;

                        var parts = new System.Collections.Generic.List<string>();
                        if (!string.IsNullOrEmpty(college.AddressLine))
                            parts.Add(college.AddressLine);
                        if (!string.IsNullOrEmpty(college.PostalCode) || !string.IsNullOrEmpty(college.City))
                        {
                            var pcCity = $"{college.PostalCode} {college.City}".Trim();
                            if (!string.IsNullOrEmpty(pcCity))
                                parts.Add(pcCity);
                        }

                        var address = string.Join(", ", parts);
                        if (!string.IsNullOrEmpty(college.Phone))
                        {
                            address += $"  \u2022  tel. {college.Phone}";
                        }
                        UniversityAddress = address;
                    }
                }

                var dbSections = await _clubRepository.GetSectionsAsync();
                Sections.Clear();
                foreach (var sec in dbSections)
                {
                    Sections.Add(sec);
                }

                var dbMembers = await _clubRepository.GetBoardMembersAsync();
                BoardMembers.Clear();
                var filteredMembers = dbMembers.Where(m => m.AuthorityRole?.Name != "Członek" && m.AuthorityRole?.Name != "Sympatyk");
                foreach (var member in filteredMembers)
                {
                    BoardMembers.Add(member);
                }

                var dbTrips = await _tripRepository.GetTripsAsync();
                Trips.Clear();
                foreach (var t in dbTrips)
                {
                    Trips.Add(t);
                }
                OnPropertyChanged(nameof(HasNoBoardMembers));
                OnPropertyChanged(nameof(HasNoTrips));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd ładowania danych koła: {ex.Message}");
            }
        }

        private Bitmap? LoadBitmap(byte[]? data)
        {
            if (data == null || data.Length == 0) return null;
            try
            {
                using (var ms = new System.IO.MemoryStream(data))
                {
                    return new Bitmap(ms);
                }
            }
            catch
            {
                return null;
            }
        }

        [RelayCommand]
        private void OpenEdit()
        {
            EditClubName = ClubName;
            EditClubNameShort = ClubNameShort;
            EditClubRoom = ClubRoom;
            EditDepartmentName = DepartmentName;
            EditSupervisorName = SupervisorName;
            EditSupervisorEmail = SupervisorEmail;
            EditSupervisorPhone = SupervisorPhone;
            EditMeetingsSchedule = MeetingsInfo;
            _editClubPhotoBytes = _clubPhotoBytes;
            EditClubPhotoBitmap = LoadBitmap(_editClubPhotoBytes);
            IsEditPopupVisible = true;
        }

        [RelayCommand]
        private async Task SaveEditAsync()
        {
            try
            {
                await _clubRepository.UpdateClubInfoAsync(EditClubName, EditClubRoom, EditDepartmentName, EditSupervisorName, EditSupervisorEmail, EditSupervisorPhone, EditMeetingsSchedule, EditClubNameShort, _editClubPhotoBytes);
                IsEditPopupVisible = false;
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd podczas zapisu: {ex.Message}");
            }
        }

        [RelayCommand]
        private void CancelEdit()
        {
            IsEditPopupVisible = false;
        }

        [RelayCommand]
        private async Task ChooseClubPhotoAsync()
        {
            if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                var window = desktop.MainWindow;
                if (window != null)
                {
                    var files = await window.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
                    {
                        Title = "Wybierz logo koła",
                        FileTypeFilter = new[] { Avalonia.Platform.Storage.FilePickerFileTypes.ImageAll }
                    });
                    if (files.Count > 0)
                    {
                        await using (var stream = await files[0].OpenReadAsync())
                        {
                            using (var memoryStream = new System.IO.MemoryStream())
                            {
                                await stream.CopyToAsync(memoryStream);
                                _editClubPhotoBytes = memoryStream.ToArray();
                                EditClubPhotoBitmap = LoadBitmap(_editClubPhotoBytes);
                            }
                        }
                    }
                }
            }
        }

        private new bool TryParseDate(string dateStr, out DateTime date)
        {
            return DateTime.TryParseExact(dateStr, 
                new[] { "dd.MM.yyyy", "yyyy-MM-dd", "d.M.yyyy", "dd/MM/yyyy" }, 
                System.Globalization.CultureInfo.InvariantCulture, 
                System.Globalization.DateTimeStyles.None, 
                out date) 
                || DateTime.TryParse(dateStr, out date);
        }

        private void ValidateAddTripForm()
        {
            IsAddTripNameInvalid = string.IsNullOrWhiteSpace(NewTripName);
            IsAddTripDateInvalid = string.IsNullOrWhiteSpace(NewTripDate) || !TryParseDate(NewTripDate, out _);
            IsAddTripFormValid = !IsAddTripNameInvalid && !IsAddTripDateInvalid;

            if (IsAddTripNameInvalid) AddTripValidationError = "Nazwa wyjazdu jest wymagana.";
            else if (IsAddTripDateInvalid) AddTripValidationError = "Data wyjazdu jest wymagana i musi być poprawna (np. dd.MM.yyyy).";
            else AddTripValidationError = string.Empty;
        }

        private void ValidateEditTripForm()
        {
            IsEditTripNameInvalid = string.IsNullOrWhiteSpace(EditTripName);
            IsEditTripDateInvalid = string.IsNullOrWhiteSpace(EditTripDate) || !TryParseDate(EditTripDate, out _);
            IsEditTripFormValid = !IsEditTripNameInvalid && !IsEditTripDateInvalid;

            if (IsEditTripNameInvalid) EditTripValidationError = "Nazwa wyjazdu jest wymagana.";
            else if (IsEditTripDateInvalid) EditTripValidationError = "Data wyjazdu jest wymagana i musi być poprawna (np. dd.MM.yyyy).";
            else EditTripValidationError = string.Empty;
        }

        /// <summary>
        /// Reaguje na zmianę właściwości i uruchamia walidację formularzy wyjazdów.
        /// </summary>
        /// <param name="e">Argumenty zdarzenia zmiany właściwości.</param>
        protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(NewTripName) || e.PropertyName == nameof(NewTripDate))
            {
                ValidateAddTripForm();
            }
            if (e.PropertyName == nameof(EditTripName) || e.PropertyName == nameof(EditTripDate))
            {
                ValidateEditTripForm();
            }
        }

        [RelayCommand]
        private void OpenAddTrip()
        {
            NewTripName = string.Empty;
            NewTripDescription = string.Empty;
            NewTripDate = DateTime.Now.ToString("dd.MM.yyyy");
            
            IsAddTripNameInvalid = false;
            IsAddTripDateInvalid = false;
            IsAddTripFormValid = false;
            AddTripValidationError = string.Empty;

            IsAddTripPopupVisible = true;
        }

        [RelayCommand]
        private async Task SaveAddTripAsync()
        {
            try
            {
                if (!IsAddTripFormValid) return;

                DateTime parsedDate = DateTime.Now;
                TryParseDate(NewTripDate, out parsedDate);

                var newTrip = new Trip
                {
                    Name = NewTripName,
                    Description = NewTripDescription,
                    Date = parsedDate
                };

                await _tripRepository.AddTripAsync(newTrip);
                IsAddTripPopupVisible = false;
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd dodawania wycieczki: {ex.Message}");
            }
        }

        [RelayCommand]
        private void CancelAddTrip()
        {
            IsAddTripPopupVisible = false;
        }

        [RelayCommand]
        private void OpenEditTrip(Trip trip)
        {
            if (trip == null) return;
            _tripBeingEdited = trip;
            EditTripName = trip.Name ?? string.Empty;
            EditTripDescription = trip.Description ?? string.Empty;
            EditTripDate = trip.Date.ToString("dd.MM.yyyy");
            
            ValidateEditTripForm();

            IsEditTripPopupVisible = true;
        }

        [RelayCommand]
        private async Task SaveEditTripAsync()
        {
            if (_tripBeingEdited == null || !IsEditTripFormValid)
            {
                return;
            }

            try
            {
                DateTime parseDate = _tripBeingEdited.Date;
                TryParseDate(EditTripDate, out parseDate);

                _tripBeingEdited.Name = EditTripName;
                _tripBeingEdited.Description = EditTripDescription;
                _tripBeingEdited.Date = parseDate;

                await _tripRepository.UpdateTripAsync(_tripBeingEdited);
                IsEditTripPopupVisible = false;
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd podczas edycji wycieczki: {ex.Message}");
            }
        }

        [RelayCommand]
        private void CancelEditTrip()
        {
            IsEditTripPopupVisible = false;
        }

        [RelayCommand]
        private void RequestDeleteTrip(Trip trip)
        {
            if (trip == null) return;
            _tripToDelete = trip;
            IsDeleteTripConfirmVisible = true;
        }
        [RelayCommand]
        private async Task ConfirmDeleteTripAsync()
        {
            if (_tripToDelete == null) return;
            try
            {
                await _tripRepository.DeleteTripAsync(_tripToDelete.TripId);
                IsDeleteTripConfirmVisible = false;
                _tripToDelete = null;
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd usuwania wyjazdu: {ex.Message}");
            }
        }
        [RelayCommand]
        private void CancelDeleteTrip()
        {
            IsDeleteTripConfirmVisible = false;
            _tripToDelete = null;
        }

        [RelayCommand]
        private void OpenAddSection()
        {
            NewSectionName = string.Empty;
            IsAddSectionPopupVisible = true;
        }

        [RelayCommand]
        private async Task SaveAddSection()
        {
            if (string.IsNullOrWhiteSpace(NewSectionName)) return;

            try
            {
                var newSection = new Section
                {
                    Name = NewSectionName
                };

                await _clubRepository.AddSectionAsync(newSection);
                IsAddSectionPopupVisible = false;
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd podczas dodawania sekcji: {ex.Message}");
            }
        }

        [RelayCommand]
        private void CancelAddSection()
        {
            IsAddSectionPopupVisible = false;
        }

        [RelayCommand]
        private void RequestDeleteSection(Section section)
        {
            if (section == null) return;
            _sectionToDelete = section;
            IsDeleteSectionConfirmVisible = true;
        }
        [RelayCommand]
        private async Task ConfirmDeleteSectionAsync()
        {
            if (_sectionToDelete == null) return;
            try
            {
                await _clubRepository.DeleteSectionAsync(_sectionToDelete.SectionId);
                IsDeleteSectionConfirmVisible = false;
                _sectionToDelete = null;
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd usuwania sekcji: {ex.Message}");
            }
        }
        [RelayCommand]
        private void CancelDeleteSection()
        {
            IsDeleteSectionConfirmVisible = false;
            _sectionToDelete = null;
        }
    }
}

