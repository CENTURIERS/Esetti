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
    public partial class ActivitiesViewModel : ViewModelBase
    {
        public override string PageTitle => "Lista Aktywności";
        public override bool ShowActionHeader => true;
        public override string SearchPlaceholder => "Szukaj aktywności...";

        public ObservableCollection<ActivityItemViewModel> Activities { get; } = new();
        private readonly IActivityRepository _activityRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly List<ActivityItemViewModel> _allActivities = new();
        private ActivityItemViewModel? _activityToDelete;
        private ActivityItemViewModel? _editingActivity;

        [ObservableProperty]
        private string _newActivityName = string.Empty;

        [ObservableProperty]
        private string _newActivityDescription = string.Empty;

        [ObservableProperty]
        private string _newActivityDate = DateTime.Now.ToString("dd.MM.yyyy");

        [ObservableProperty]
        private string _newActivityTime = DateTime.Now.ToString("HH:mm");

        [ObservableProperty]
        private string _newActivityCity = string.Empty;

        [ObservableProperty]
        private string _newActivityStreet = string.Empty;

        [ObservableProperty]
        private string _newActivityPersonInChargeEvent = string.Empty;

        [ObservableProperty]
        private string _newActivityPersonInChargePhone = string.Empty;

        [ObservableProperty]
        private string _newActivityPersonInChargeEmail = string.Empty;

        [ObservableProperty]
        private bool _newActivityIsRepeatable;

        public ObservableCollection<string> ClubMembers { get; } = new();
        
        [ObservableProperty]
        private string _newActivityPersonInChargeClub = string.Empty;

        public ObservableCollection<string> EventMembers { get; } = new();

        [ObservableProperty]
        private bool _isAddEditPopupVisible;

        [ObservableProperty]
        private string _popupTitle = "Nowa aktywnoĹ›Ä‡";

        [ObservableProperty]
        private bool _isNameInvalid;
        [ObservableProperty]
        private bool _isDateInvalid;
        [ObservableProperty]
        private bool _isTimeInvalid;
        [ObservableProperty]
        private bool _isEmailInvalid;
        [ObservableProperty]
        private bool _isPhoneInvalid;
        [ObservableProperty]
        private bool _isFormValid = true;
        [ObservableProperty]
        private string _validationError = string.Empty;

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

            if (IsNameInvalid) ValidationError = "Nazwa aktywnoĹ›ci jest wymagana.";
            else if (IsDateInvalid) ValidationError = "Data aktywnoĹ›ci jest wymagana i musi byÄ‡ poprawna.";
            else if (IsTimeInvalid) ValidationError = "Godzina aktywnoĹ›ci jest wymagana i musi byÄ‡ poprawna (np. hh:mm).";
            else if (IsEmailInvalid) ValidationError = "Niepoprawny format adresu e-mail osoby odpowiedzialnej.";
            else if (IsPhoneInvalid) ValidationError = "Niepoprawny format numeru telefonu osoby odpowiedzialnej.";
            else ValidationError = string.Empty;
        }

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

        private readonly INavigationService _navigationService;

        public string SelectAllText => IsAllSelected ? "Odznacz wszystko" : "Zaznacz wszystko"; 
        public bool HasSelectedItems => IsAnySelected;
        public string SelectedCountText => $"Zaznaczono: {SelectedCount} aktywnoĹ›ci";

        public ActivitiesViewModel(IActivityRepository activityRepository, IMemberRepository memberRepository, INavigationService navigationService)
        {
            _activityRepository = activityRepository;
            _memberRepository = memberRepository;
            _navigationService = navigationService;
            _ = LoadDataAsync();
        }

        [RelayCommand]
        private void ToggleSelectAll()
        {
            IsAllSelected = !IsAllSelected;
        }

        [RelayCommand]
        private void OpenAddPopup()
        {
            _editingActivity = null;
            PopupTitle = "Nowa aktywnoĹ›Ä‡";
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
            IsFormValid = false; // Name is empty initially

            IsAddEditPopupVisible = true;
        }

        [RelayCommand]
        private async Task OpenEditPopup(ActivityItemViewModel item)
        {
            if (item == null) return;
            _editingActivity = item;
            PopupTitle = "Edycja aktywnoĹ›ci";

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

        [RelayCommand]
        private void ClosePopup()
        {
            IsAddEditPopupVisible = false;
        }

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

        [RelayCommand]
        private void DeleteSingleActivity(ActivityItemViewModel item)
        {
            if (item == null) return;
            _activityToDelete = item;
            IsPopupVisible = true;
        }

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
                System.Diagnostics.Debug.WriteLine($"BĹ‚Ä…d usuwania aktywnoĹ›ci: {ex.Message}");
            }
        }

        protected override void OnPopupClosed()
        {
            _activityToDelete = null;
        }

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

        public void UpdateSelectionState() {
            var selected = Activities.Where(a => a.IsSelected).ToList();
            SelectedCount = selected.Count;
            IsAnySelected = SelectedCount > 0;

            OnPropertyChanged(nameof(SelectAllText));
            OnPropertyChanged(nameof(HasSelectedItems));
            OnPropertyChanged(nameof(SelectedCountText));
        }

        protected override void OnSearchQueryUpdated(string value) => ApplyFilter();

        private void ApplyFilter()
        {
            Activities.Clear();
            var query = SearchQuery?.ToLower() ?? "";

            foreach (var item in _allActivities)
            {
                var name = item.Name ?? "";
                var desc = item.Description ?? "";
                if (name.ToLower().Contains(query) || desc.ToLower().Contains(query))
                {
                    Activities.Add(item);
                }
            }

            UpdateSelectionState();
        }

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

        private void ClearActivitySubscriptions()
        {
            foreach (var vm in _allActivities)
            {
                vm.PropertyChanged -= OnActivityItemPropertyChanged;
            }
        }

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
                System.Diagnostics.Debug.WriteLine($"BĹ‚Ä…d pobierania aktywnoĹ›ci: {ex.Message}");
            }
        }

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


