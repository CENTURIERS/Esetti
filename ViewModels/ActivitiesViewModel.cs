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
        private string _popupTitle = "Nowa aktywność";

        private readonly INavigationService _navigationService;

        public string SelectAllText => IsAllSelected ? "Odznacz wszystko" : "Zaznacz wszystko"; 
        public bool HasSelectedItems => IsAnySelected;
        public string SelectedCountText => $"Zaznaczono: {SelectedCount} aktywności";

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
            IsAddEditPopupVisible = true;
        }

        [RelayCommand]
        private async Task OpenEditPopup(ActivityItemViewModel item)
        {
            if (item == null) return;
            _editingActivity = item;
            PopupTitle = "Edycja aktywności";

            var fullActivity = await _activityRepository.GetActivityByIdAsync(int.Parse(item.ActivityId));
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
            IsAddEditPopupVisible = true;
        }

        [RelayCommand]
        private void ClosePopup()
        {
            IsAddEditPopupVisible = false;
        }

        [RelayCommand]
        private async Task SaveActivity()
        {
            DateTime parsedDate = DateTime.TryParse(NewActivityDate, out var d) ? d : DateTime.Now;
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
                var activityUpdate = await _activityRepository.GetActivityByIdAsync(int.Parse(_editingActivity.ActivityId));
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
                    await _activityRepository.DeleteSingleActivityAsync(int.Parse(_activityToDelete.ActivityId));
                    _activityToDelete = null;
                }
                else
                {
                    var selectedVMs = Activities.Where(a => a.IsSelected).ToList();
                    if (!selectedVMs.Any()) return;

                    var idsToDelete = selectedVMs.Select(a => int.Parse(a.ActivityId)).ToList();
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

        private async Task LoadDataAsync()
        {
            try 
            {
                var activitiesFromDb = await _activityRepository.GetAllActivitiesAsync();
                var membersFromDb = await _memberRepository.GetAllMembersAsync();

                Dispatcher.UIThread.Post(() => {
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

                            vm.PropertyChanged += (s, e) => {
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
                            };

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

        [RelayCommand]
        private void OpenProfile(ActivityItemViewModel item) 
        {
            if (item == null) return;

            var profileVm = new ActivityProfileViewModel (
                int.Parse(item.ActivityId),
                _navigationService,
                _memberRepository,
                _activityRepository
            );

            _navigationService.NavigateTo(profileVm);
        }
    }
}
