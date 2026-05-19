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

        [ObservableProperty]
        private string _newActivityName = string.Empty;

        [ObservableProperty]
        private string _newActivityDescription = string.Empty;

        [ObservableProperty]
        private string _newActivityDate = DateTime.Now.ToString("dd.MM.yyyy 12:00");

        [ObservableProperty]
        private string _newActivityCity = string.Empty;

        [ObservableProperty]
        private string _newActivityStreet = string.Empty;

        [ObservableProperty]
        private string _newActivityAdditionalInfo = string.Empty;

        public ObservableCollection<string> ClubMembers { get; } = new();
        
        [ObservableProperty]
        private string _newActivityPersonInChargeClub = string.Empty;

        public ObservableCollection<string> EventMembers { get; } = new();

        [ObservableProperty]
        private string _newActivityPersonInChargeEvent = string.Empty;

        [ObservableProperty]
        private bool _isAddEditPopupVisible;

        [ObservableProperty]
        private string _popupTitle = "Nowa aktywność";

        [ObservableProperty]
        private bool _isConfirmDeletePopupVisible;

        [ObservableProperty]
        private string _deleteConfirmMessage = string.Empty;

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
            PopupTitle = "Nowa aktywność";
            NewActivityName = string.Empty;
            NewActivityDescription = string.Empty;
            NewActivityDate = DateTime.Now.ToString("dd.MM.yyyy 12:00");
            NewActivityCity = string.Empty;
            NewActivityStreet = string.Empty;
            NewActivityAdditionalInfo = string.Empty;
            IsAddEditPopupVisible = true;
        }

        [RelayCommand]
        private void OpenEditPopup(ActivityItemViewModel item)
        {
            if (item == null) return;
            PopupTitle = "Edycja aktywności";
            NewActivityName = item.Name;
            NewActivityDescription = item.Description;
            NewActivityDate = item.DateString;
            IsAddEditPopupVisible = true;
        }

        [RelayCommand]
        private void ClosePopup()
        {
            IsAddEditPopupVisible = false;
        }

        [RelayCommand]
        private void SaveActivity()
        {
            IsAddEditPopupVisible = false;
        }

        [RelayCommand]
        private void DeleteSingleActivity(ActivityItemViewModel item)
        {
            if (item == null) return;
            _activityToDelete = item;
            DeleteConfirmMessage = $"Czy na pewno chcesz usunąć aktywność \"{item.Name}\"?";
            IsConfirmDeletePopupVisible = true;
        }

        [RelayCommand]
        private void CancelDeleteActivity()
        {
            IsConfirmDeletePopupVisible = false;
            _activityToDelete = null;
        }

        [RelayCommand]
        private void ConfirmDeleteActivity()
        {
            if (_activityToDelete != null)
            {
                Activities.Remove(_activityToDelete);
                _allActivities.Remove(_activityToDelete);
                _activityToDelete = null;
            }
            IsConfirmDeletePopupVisible = false;
            UpdateSelectionState();
        }

        [RelayCommand]
        private void ExportSelected()
        {
        }

        [RelayCommand]
        private void RequestBulkDelete()
        {
            DeleteConfirmMessage = $"Czy na pewno chcesz usunąć {SelectedCount} zaznaczone aktywności?";
            IsConfirmDeletePopupVisible = true;
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
                                isSelected: false
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
                            Activities.Add(vm);
                        }
                    }

                    ClubMembers.Clear();
                    if (membersFromDb != null) {
                        foreach (var m in membersFromDb) {
                            ClubMembers.Add($"{m.FirstName} {m.LastName}");
                        }
                    }

                    UpdateSelectionState();
                });
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"Błąd pobierania aktywności: {ex.Message}");
            }
        }
    }
}
