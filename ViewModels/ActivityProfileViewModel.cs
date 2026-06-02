using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Esseti.Repositories.Interfaces;
using Esseti.Services;
using Microsoft.Extensions.DependencyInjection;
using Models.Activities;

namespace Esseti.ViewModels
{
    public partial class ActivityProfileViewModel : ViewModelBase
    {
        public override string PageTitle => "Profil aktywności";

        private readonly INavigationService _navigationService;
        private readonly IActivityRepository _activityRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly int _activityId;

        [ObservableProperty]
        private string _activityName = "";
        [ObservableProperty]
        private string _dateText = "";
        [ObservableProperty]
        private string _timeText = "";
        [ObservableProperty]
        private string _city = "";
        [ObservableProperty]
        private string _addressLine = "";
        [ObservableProperty]
        private string _personInChargeName = "";
        [ObservableProperty]
        private string _personInChargePhone = "";
        [ObservableProperty]
        private string _personInChargeEmail = "";
        [ObservableProperty]
        private string _additionalInformation = "";
        [ObservableProperty]
        private string _isRepeatableText = "Nie";
        [ObservableProperty]
        private bool _isLoading = true;

        [ObservableProperty]
        private bool _isActivityEditPopupVisible;

        [ObservableProperty]
        private string _editName = "";
        [ObservableProperty]
        private string _editDate = "";
        [ObservableProperty]
        private string _editTime = "";
        [ObservableProperty]
        private string _editCity = "";
        [ObservableProperty]
        private string _editAddressLine = "";
        [ObservableProperty]
        private string _editPersonInChargeName = "";
        [ObservableProperty]
        private string _editPersonInChargePhone = "";
        [ObservableProperty]
        private string _editPersonInChargeEmail = "";
        [ObservableProperty]
        private string _editAdditionalInfo = "";
        [ObservableProperty]
        private bool _editIsRepeatable;

        public ObservableCollection<Models.Users.Member> Participants { get; } = new();
        public ObservableCollection<Models.Users.Member> ClubMembers { get; } = new();

        [ObservableProperty]
        private Models.Users.Member? _selectedMemberToAdd;

        public ActivityProfileViewModel(int activityId, INavigationService navigationService, IMemberRepository memberRepository, IActivityRepository activityRepository)
        {
            _activityId = activityId;
            _navigationService = navigationService;
            _memberRepository = memberRepository;
            _activityRepository = activityRepository;

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                IsLoading = true;

                var act = await _activityRepository.GetActivityByIdAsync(_activityId);
                var allMembers = await _memberRepository.GetAllMembersAsync();

                if (act == null)
                {
                    IsLoading = false;
                    return;
                }

                Dispatcher.UIThread.Post(() =>
                {
                    ActivityName = act.Name;
                    DateText = act.Date.ToString("dd.MM.yyyy");
                    TimeText = act.Time?.ToString(@"hh\:mm") ?? "Nie określono";
                    City = act.City ?? "Brak danych";
                    AddressLine = act.AddressLine ?? "Brak danych";
                    PersonInChargeName = act.PersonInChargeName ?? "Brak danych";
                    PersonInChargePhone = act.PersonInChargePhone ?? "Brak danych";
                    PersonInChargeEmail = act.PersonInChargeEmail ?? "Brak danych";
                    AdditionalInformation = act.AdditionalInformation ?? "Brak dodatkowych uwag";

                    IsRepeatableText = act.IsRepeatable ? "Tak" : "Nie";

                    Participants.Clear();
                    if (act.Participants != null)
                    {
                        foreach (var m in act.Participants)
                        {
                            Participants.Add(m);
                        }
                    }

                    ClubMembers.Clear();
                    if (allMembers != null)
                    {
                        foreach (var m in allMembers)
                        {
                            ClubMembers.Add(m);
                        }
                    }


                    SelectedMemberToAdd = ClubMembers.FirstOrDefault();
                    IsLoading = false;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd ładowania aktywności: {ex.Message}");
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void GoBack()
        {
            _navigationService.NavigateTo(App.Services.GetRequiredService<ActivitiesViewModel>());
        }

        [RelayCommand]
        private void OpenEdit()
        {
            EditName = ActivityName;
            EditDate = DateText;

            EditTime = TimeText == "Nie określono" ? "" : TimeText;
            EditCity = City == "Brak danych" ? "" : City;
            EditAddressLine = AddressLine == "Brak danych" ? "" : AddressLine;
            EditPersonInChargeName = PersonInChargeName == "Brak danych" ? "" : PersonInChargeName;
            EditPersonInChargePhone = PersonInChargePhone == "Brak danych" ? "" : PersonInChargePhone;
            EditPersonInChargeEmail = PersonInChargeEmail == "Brak danych" ? "" : PersonInChargeEmail;
            EditAdditionalInfo = AdditionalInformation == "Brak dodatkowych uwag" ? "" : AdditionalInformation;
            EditIsRepeatable = IsRepeatableText == "Tak";
            IsActivityEditPopupVisible = true;
        }

        [RelayCommand]
        private void CancelEdit() => IsActivityEditPopupVisible = false;

        [RelayCommand]
        private async Task SaveEditAsync()
        {
            if (string.IsNullOrWhiteSpace(EditName)) return;

            TryParseDate(EditDate, out var date);
            TimeSpan? time = TimeSpan.TryParse(EditTime, out var t) ? t : null;

            var updatedActivity = new Activity
            {
                ActivityId = _activityId,
                Name = EditName,
                Date = date,
                Time = time,
                City = EditCity,
                AddressLine = EditAddressLine,
                PersonInChargeName = EditPersonInChargeName,
                PersonInChargePhone = EditPersonInChargePhone,
                PersonInChargeEmail = EditPersonInChargeEmail,
                AdditionalInformation = EditAdditionalInfo,
                IsRepeatable = EditIsRepeatable,
            };

            var remainingParticipantIds = Participants.Select(p => p.MemberId).ToList();
            await _activityRepository.UpdateActivityAsync(updatedActivity, remainingParticipantIds);

            IsActivityEditPopupVisible = false;
            await LoadAsync();
        }

        [RelayCommand]
        private async Task AddParticipantAsync()
        {
            if (SelectedMemberToAdd == null) return;

            if (Participants.Any(p => p.MemberId == SelectedMemberToAdd.MemberId)) return;

            Participants.Add(SelectedMemberToAdd);

            var participantIds = Participants.Select(p => p.MemberId).ToList();
            await _activityRepository.UpdateActivityParticipantsAsync(_activityId, participantIds);
            await LoadAsync();
        }

        [RelayCommand]
        private async Task RemoveParticipantAsync(Models.Users.Member participant)
        {
            if (participant == null) return;

            Participants.Remove(participant);

            var participantIds = Participants.Select(p => p.MemberId).ToList();
            await _activityRepository.UpdateActivityParticipantsAsync(_activityId, participantIds);
            await LoadAsync();
        }
        

        [RelayCommand]
        private void DeleteThisActivity()
        {
            IsActivityEditPopupVisible = false;
            IsPopupVisible = true;
        }

        protected override async Task ExecuteConfirmDeleteAsync()
        {
            await _activityRepository.DeleteSingleActivityAsync(_activityId);
            _navigationService.NavigateTo(App.Services.GetRequiredService<ActivitiesViewModel>());
        }
    }
}

