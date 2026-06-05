using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Esseti.Repositories.Interfaces;
using Esseti.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Models.Users;

namespace Esseti.ViewModels
{
    /// <summary>
    /// Model widoku dla podstrony profilu (szczegółów) wybranego projektu.
    /// Zarządza wyświetlaniem lidera, listy uczestników, edycją danych projektu, a także dodawaniem i usuwaniem osób do/z projektu.
    /// </summary>
    public partial class ProjectProfileViewModel : ViewModelBase
    {
        /// <summary>
        /// Tytuł wyświetlany w nagłówku strony.
        /// </summary>
        public override string PageTitle => "Profil projektu";

        /// <summary>
        /// Serwis nawigacyjny do skakania między ekranami.
        /// </summary>
        private readonly INavigationService _navigationService;

        /// <summary>
        /// Repozytorium obsługujące zapytania bazodanowe dla projektów.
        /// </summary>
        private readonly IProjectRepository _projectRepository;

        /// <summary>
        /// Repozytorium obsługujące zapytania bazodanowe dla członków koła.
        /// </summary>
        private readonly IMemberRepository _memberRepository;

        /// <summary>
        /// Unikalne ID wyświetlanego projektu.
        /// </summary>
        private readonly int _projectId;

        /// <summary>
        /// Nazwa projektu.
        /// </summary>
        [ObservableProperty]
        private string _projectName = "";

        /// <summary>
        /// Opis projektu.
        /// </summary>
        [ObservableProperty]
        private string _description = "";

        /// <summary>
        /// Dodatkowe informacje o projekcie.
        /// </summary>
        [ObservableProperty]
        private string _additionalInformation = "";

        /// <summary>
        /// Link do repozytorium GitHub projektu.
        /// </summary>
        [ObservableProperty]
        private string _github = "";

        /// <summary>
        /// Sformatowany czas trwania projektu (np. "120 godzin").
        /// </summary>
        [ObservableProperty]
        private string _estimatedTimeText = "";

        /// <summary>
        /// Sformatowana data rozpoczęcia projektu.
        /// </summary>
        [ObservableProperty]
        private string _dateStartText = "";

        /// <summary>
        /// Sformatowana data zakończenia projektu.
        /// </summary>
        [ObservableProperty]
        private string _dateEndText = "";

        /// <summary>
        /// Imię i nazwisko lidera projektu.
        /// </summary>
        [ObservableProperty]
        private string _leaderName = "";

        /// <summary>
        /// E-mail lidera projektu.
        /// </summary>
        [ObservableProperty]
        private string _leaderEmail = "";

        /// <summary>
        /// Flaga określająca, czy dane profilu są w trakcie ładowania (pokazuje animację na widoku).
        /// </summary>
        [ObservableProperty]
        private bool _isLoading = true;

        /// <summary>
        /// Czy popup edycji właściwości projektu jest widoczny.
        /// </summary>
        [ObservableProperty]
        private bool _isProjectEditPopupVisible;

        /// <summary>
        /// Nazwa projektu wpisana w formularzu edycji.
        /// </summary>
        [ObservableProperty]
        private string _editName = "";

        /// <summary>
        /// Opis projektu wpisany w formularzu edycji.
        /// </summary>
        [ObservableProperty]
        private string _editDescription = "";

        /// <summary>
        /// Dodatkowe info o projekcie wpisane w formularzu edycji.
        /// </summary>
        [ObservableProperty]
        private string _editAdditionalInfo = "";

        /// <summary>
        /// Link do GitHuba wpisany w formularzu edycji.
        /// </summary>
        [ObservableProperty]
        private string _editGithub = "";

        /// <summary>
        /// Czas trwania projektu wpisany w formularzu edycji.
        /// </summary>
        [ObservableProperty]
        private string _editEstimatedTime = "";

        /// <summary>
        /// Data rozpoczęcia projektu wpisana w formularzu edycji.
        /// </summary>
        [ObservableProperty]
        private string _editDateStart = "";

        /// <summary>
        /// Data zakończenia projektu wpisana w formularzu edycji.
        /// </summary>
        [ObservableProperty]
        private string _editDateEnd = "";

        /// <summary>
        /// Wybrany lider projektu w formularzu edycji.
        /// </summary>
        [ObservableProperty]
        private Models.Users.Member? _editLeader;

        /// <summary>
        /// Kolekcja uczestników aktualnie przypisanych do tego projektu (wyświetlana w tabeli).
        /// </summary>
        public ObservableCollection<Models.Users.Member> Participants { get; } = new();

        /// <summary>
        /// Lista wszystkich członków klubu/koła, z których można wybrać uczestnika do dodania.
        /// </summary>
        public ObservableCollection<Models.Users.Member> ClubMembers { get; } = new();

        /// <summary>
        /// Model członka wybranego z ComboBoxa, którego chcemy dopisać do projektu.
        /// </summary>
        [ObservableProperty]
        private Models.Users.Member? _selectedMemberToAdd;

        /// <summary>
        /// Konstruktor profilu projektu. Inicjalizuje pola i asynchronicznie ładuje dane z bazy.
        /// </summary>
        /// <param name="projectId">ID projektu.</param>
        /// <param name="navigationService">Serwis do skakania po ekranach.</param>
        /// <param name="projectRepository">Repozytorium projektów.</param>
        /// <param name="memberRepository">Repozytorium członków.</param>
        public ProjectProfileViewModel(int projectId, INavigationService navigationService, IProjectRepository projectRepository, IMemberRepository memberRepository)
        {
            _projectId = projectId;
            _navigationService = navigationService;
            _projectRepository = projectRepository;
            _memberRepository = memberRepository;

            _ = LoadAsync();
        }

        /// <summary>
        /// Asynchronicznie wczytuje szczegółowe dane projektu z bazy i uzupełnia właściwości bindowane w widoku.
        /// </summary>
        private async Task LoadAsync()
        {
            try
            {
                IsLoading = true;

                var proj = await _projectRepository.GetProjectByIdAsync(_projectId);
                var allMembers = await _memberRepository.GetAllMembersAsync();

                if (proj == null)
                {
                    IsLoading = false;
                    return;
                }

                Dispatcher.UIThread.Post(() =>
                {
                    ProjectName = proj.Name;
                    Description = proj.Description ?? "Brak Opisu";
                    AdditionalInformation = proj.AdditionalInformation ?? "Brak dodatkowych informacji";
                    Github = proj.Github ?? "Brak linku do github";

                    EstimatedTimeText = proj.EstimatedTime != null ? $"{proj.EstimatedTime} godzin" : "Nie określono";
                    DateStartText = proj.DateStart?.ToString("dd.MM.yyyy") ?? "Nie określono";
                    DateEndText = proj.DateEnd?.ToString("dd.MM.yyyy") ?? "Nie określono";

                    if (proj.PersonInCharge != null)
                    {
                        LeaderName = $"{proj.PersonInCharge.FirstName} {proj.PersonInCharge.LastName}";
                        LeaderEmail = proj.PersonInCharge.Account?.Email ?? "Brak emaila";
                    }
                    else
                    {
                        LeaderName = "Brak lidera";
                        LeaderEmail = string.Empty;
                    }

                    Participants.Clear();
                    if (proj.Participants != null)
                    {
                        foreach (var m in proj.Participants)
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
                System.Diagnostics.Debug.WriteLine($"Błąd ładowania profilu projektu: {ex.Message}");
                IsLoading = false;
            }
        }

        /// <summary>
        /// Komenda powrotu do ekranu z listą wszystkich projektów.
        /// </summary>
        [RelayCommand]
        private void GoBack()
        {
            _navigationService.NavigateTo(App.Services.GetRequiredService<ProjectsViewModel>());
        }

        /// <summary>
        /// Komenda otwierająca popup edycji projektu i ładująca do niego aktualne wartości właściwości.
        /// </summary>
        [RelayCommand]
        private void OpenEdit()
        {
            EditName = ProjectName;

            EditDescription = Description == "Brak Opisu" ? "" : Description;
            EditAdditionalInfo = AdditionalInformation == "Brak dodatkowych informacji" ? "" : AdditionalInformation;
            EditGithub = Github == "Brak linku do github" ? "" : Github;

            EditEstimatedTime = EstimatedTimeText.Replace(" godzin", "").Replace("Nie określono", "");
            EditDateStart = DateStartText == "Nie określono" ? "" : DateStartText;
            EditDateEnd = DateEndText == "Nie określono" ? "" : DateEndText;

            EditLeader = ClubMembers.FirstOrDefault(m => $"{m.FirstName} {m.LastName}" == LeaderName);

            IsProjectEditPopupVisible = true;
        }

        /// <summary>
        /// Komenda zamykająca okienko edycji projektu.
        /// </summary>
        [RelayCommand]
        private void CancelEdit() => IsProjectEditPopupVisible = false;

        /// <summary>
        /// Komenda asynchroniczna zapisująca wprowadzone zmiany w danych projektu w bazie.
        /// </summary>
        [RelayCommand]
        private async Task SaveEditAsync()
        {
            if (string.IsNullOrWhiteSpace(EditName)) return;

            DateTime? start = TryParseDate(EditDateStart, out var ds) ? ds : null;
            DateTime? end = TryParseDate(EditDateEnd, out var de) ? de : null;
            int? estTime = int.TryParse(EditEstimatedTime, out var et) ? et : null;

            var updateProject = new Models.Activities.Project
            {
                ProjectId = _projectId,
                Name = EditName,
                Description = EditDescription,
                AdditionalInformation = EditAdditionalInfo,
                Github = EditGithub,
                EstimatedTime = estTime,
                DateStart = start,
                DateEnd = end,
                PersonInChargeId = EditLeader?.MemberId,
                IsActive = true
            };

            var remainingParticipantsIds = Participants.Select(p => p.MemberId).ToList();

            await _projectRepository.UpdateProjectAsync(updateProject, remainingParticipantsIds);
            IsProjectEditPopupVisible = false;
            await LoadAsync();
        }

        /// <summary>
        /// Komenda asynchroniczna dopisująca wybranego członka koła jako uczestnika tego projektu.
        /// </summary>
        [RelayCommand]
        private async Task AddParticipantAsync()
        {
            if (SelectedMemberToAdd == null) return;

            if (Participants.Any(p => p.MemberId == SelectedMemberToAdd.MemberId)) return;

            Participants.Add(SelectedMemberToAdd);

            var participantIds = Participants.Select(p => p.MemberId).ToList();
            await _projectRepository.UpdateProjectParticipantsAsync(_projectId, participantIds);
            await LoadAsync();
        }

        /// <summary>
        /// Komenda asynchroniczna usuwająca wskazaną osobę z listy uczestników tego projektu.
        /// </summary>
        /// <param name="participant">Model członka do usunięcia.</param>
        [RelayCommand]
        private async Task RemoveParticipantAsync(Models.Users.Member participant)
        {
            if (participant == null) return;

            Participants.Remove(participant);

            var participantIds = Participants.Select(p => p.MemberId).ToList();
            await _projectRepository.UpdateProjectParticipantsAsync(_projectId, participantIds);
            await LoadAsync();
        }

        /// <summary>
        /// Komenda otwierająca popup potwierdzenia chęci usunięcia tego projektu z bazy.
        /// </summary>
        [RelayCommand]
        private void DeleteThisProject()
        {
            IsProjectEditPopupVisible = false;
            IsPopupVisible = true;
        }

        /// <summary>
        /// Wykonuje usunięcie tego projektu z bazy i powraca do ekranu listy projektów.
        /// </summary>
        protected override async Task ExecuteConfirmDeleteAsync()
        {
            await _projectRepository.DeleteSingleProjectAsync(_projectId);

            _navigationService.NavigateTo(App.Services.GetRequiredService<ProjectsViewModel>());
        }
    }
}

