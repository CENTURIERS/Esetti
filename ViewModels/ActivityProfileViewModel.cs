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
    /// <summary>
    /// Model widoku dla profilu szczegółowego danej aktywności (wydarzenia).
    /// Pokazuje szczegóły aktywności, pozwala na edycję jej parametrów, dodawanie i usuwanie uczestników oraz jej całkowite kasowanie.
    /// </summary>
    public partial class ActivityProfileViewModel : ViewModelBase
    {
        /// <summary>
        /// Tytuł wyświetlany na górze strony szczegółów aktywności.
        /// </summary>
        public override string PageTitle => "Profil aktywności";

        /// <summary>
        /// Wspólny serwis nawigacyjny do skakania po widokach.
        /// </summary>
        private readonly INavigationService _navigationService;

        /// <summary>
        /// Repozytorium do operowania na aktywnościach w bazie.
        /// </summary>
        private readonly IActivityRepository _activityRepository;

        /// <summary>
        /// Repozytorium do operowania na członkach koła w bazie.
        /// </summary>
        private readonly IMemberRepository _memberRepository;

        /// <summary>
        /// Unikalne ID przeglądanej aktywności.
        /// </summary>
        private readonly int _activityId;

        /// <summary>
        /// Nazwa aktywności.
        /// </summary>
        [ObservableProperty]
        private string _activityName = "";

        /// <summary>
        /// Sformatowana data wydarzenia.
        /// </summary>
        [ObservableProperty]
        private string _dateText = "";

        /// <summary>
        /// Sformatowana godzina wydarzenia.
        /// </summary>
        [ObservableProperty]
        private string _timeText = "";

        /// <summary>
        /// Miasto, w którym odbywa się wydarzenie.
        /// </summary>
        [ObservableProperty]
        private string _city = "";

        /// <summary>
        /// Adres (np. ulica i numer sali), gdzie odbywa się aktywność.
        /// </summary>
        [ObservableProperty]
        private string _addressLine = "";

        /// <summary>
        /// Imię i nazwisko osoby odpowiedzialnej za wydarzenie.
        /// </summary>
        [ObservableProperty]
        private string _personInChargeName = "";

        /// <summary>
        /// Numer telefonu kontaktowego do osoby odpowiedzialnej.
        /// </summary>
        [ObservableProperty]
        private string _personInChargePhone = "";

        /// <summary>
        /// Adres e-mail do osoby odpowiedzialnej.
        /// </summary>
        [ObservableProperty]
        private string _personInChargeEmail = "";

        /// <summary>
        /// Dodatkowe notatki lub uwagi na temat aktywności.
        /// </summary>
        [ObservableProperty]
        private string _additionalInformation = "";

        /// <summary>
        /// Tekstowa informacja, czy aktywność jest cykliczna ("Tak" lub "Nie").
        /// </summary>
        [ObservableProperty]
        private string _isRepeatableText = "Nie";

        /// <summary>
        /// Flaga informująca o trwającym ładowaniu danych z bazy (pokazuje loader na ekranie).
        /// </summary>
        [ObservableProperty]
        private bool _isLoading = true;

        /// <summary>
        /// Czy popup edycji danych aktywności jest obecnie widoczny.
        /// </summary>
        [ObservableProperty]
        private bool _isActivityEditPopupVisible;

        /// <summary>
        /// Nazwa aktywności w formularzu edycji.
        /// </summary>
        [ObservableProperty]
        private string _editName = "";

        /// <summary>
        /// Data aktywności w formularzu edycji.
        /// </summary>
        [ObservableProperty]
        private string _editDate = "";

        /// <summary>
        /// Godzina aktywności w formularzu edycji.
        /// </summary>
        [ObservableProperty]
        private string _editTime = "";

        /// <summary>
        /// Miasto w formularzu edycji.
        /// </summary>
        [ObservableProperty]
        private string _editCity = "";

        /// <summary>
        /// Ulica/sala w formularzu edycji.
        /// </summary>
        [ObservableProperty]
        private string _editAddressLine = "";

        /// <summary>
        /// Nazwisko odpowiedzialnego w formularzu edycji.
        /// </summary>
        [ObservableProperty]
        private string _editPersonInChargeName = "";

        /// <summary>
        /// Telefon odpowiedzialnego w formularzu edycji.
        /// </summary>
        [ObservableProperty]
        private string _editPersonInChargePhone = "";

        /// <summary>
        /// E-mail odpowiedzialnego w formularzu edycji.
        /// </summary>
        [ObservableProperty]
        private string _editPersonInChargeEmail = "";

        /// <summary>
        /// Dodatkowe informacje w formularzu edycji.
        /// </summary>
        [ObservableProperty]
        private string _editAdditionalInfo = "";

        /// <summary>
        /// Flaga cykliczności w formularzu edycji.
        /// </summary>
        [ObservableProperty]
        private bool _editIsRepeatable;

        /// <summary>
        /// Lista uczestników aktualnie przypisanych do tego wydarzenia.
        /// </summary>
        public ObservableCollection<Models.Users.Member> Participants { get; } = new();

        /// <summary>
        /// Lista wszystkich członków koła do wyboru przy dodawaniu uczestnika.
        /// </summary>
        public ObservableCollection<Models.Users.Member> ClubMembers { get; } = new();

        /// <summary>
        /// Wybrany z listy członek koła, którego chcemy dopisać jako uczestnika.
        /// </summary>
        [ObservableProperty]
        private Models.Users.Member? _selectedMemberToAdd;

        /// <summary>
        /// Konstruktor modelu widoku profilu aktywności. Uruchamia asynchroniczne pobieranie danych.
        /// </summary>
        /// <param name="activityId">Unikalny identyfikator aktywności.</param>
        /// <param name="navigationService">Serwis nawigacyjny.</param>
        /// <param name="memberRepository">Repozytorium członków koła.</param>
        /// <param name="activityRepository">Repozytorium aktywności.</param>
        public ActivityProfileViewModel(int activityId, INavigationService navigationService, IMemberRepository memberRepository, IActivityRepository activityRepository)
        {
            _activityId = activityId;
            _navigationService = navigationService;
            _memberRepository = memberRepository;
            _activityRepository = activityRepository;

            _ = LoadAsync();
        }

        /// <summary>
        /// Asynchroniczne pobieranie danych szczegółowych o aktywności oraz listy wszystkich członków koła z bazy.
        /// </summary>
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

        /// <summary>
        /// Komenda powrotu do podstrony z listą wszystkich aktywności.
        /// </summary>
        [RelayCommand]
        private void GoBack()
        {
            _navigationService.NavigateTo(App.Services.GetRequiredService<ActivitiesViewModel>());
        }

        /// <summary>
        /// Komenda otwierająca popup edycji i wypełniająca go aktualnymi szczegółami aktywności.
        /// </summary>
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

        /// <summary>
        /// Komenda anulująca edycję aktywności i zamykająca popup.
        /// </summary>
        [RelayCommand]
        private void CancelEdit() => IsActivityEditPopupVisible = false;

        /// <summary>
        /// Komenda asynchroniczna zapisująca naniesione w formularzu edycji poprawki w bazie danych.
        /// </summary>
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

        /// <summary>
        /// Komenda asynchroniczna dopisująca wybranego członka koła jako uczestnika tego wydarzenia.
        /// </summary>
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

        /// <summary>
        /// Komenda asynchroniczna wypisująca danego członka z listy uczestników tej aktywności.
        /// </summary>
        /// <param name="participant">Model członka koła do usunięcia.</param>
        [RelayCommand]
        private async Task RemoveParticipantAsync(Models.Users.Member participant)
        {
            if (participant == null) return;

            Participants.Remove(participant);

            var participantIds = Participants.Select(p => p.MemberId).ToList();
            await _activityRepository.UpdateActivityParticipantsAsync(_activityId, participantIds);
            await LoadAsync();
        }
        

        /// <summary>
        /// Komenda otwierająca popup potwierdzenia usunięcia tej aktywności.
        /// </summary>
        [RelayCommand]
        private void DeleteThisActivity()
        {
            IsActivityEditPopupVisible = false;
            IsPopupVisible = true;
        }

        /// <summary>
        /// Potwierdzenie usunięcia aktywności - kasuje ją z bazy i nawiguje do listy aktywności.
        /// </summary>
        protected override async Task ExecuteConfirmDeleteAsync()
        {
            await _activityRepository.DeleteSingleActivityAsync(_activityId);
            _navigationService.NavigateTo(App.Services.GetRequiredService<ActivitiesViewModel>());
        }
    }
}
