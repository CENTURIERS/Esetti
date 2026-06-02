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
    public partial class ProjectProfileViewModel : ViewModelBase
    {
        public override string PageTitle => "Profil projektu";

        private readonly INavigationService _navigationService;
        private readonly IProjectRepository _projectRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly int _projectId;

        [ObservableProperty]
        private string _projectName = "";
        [ObservableProperty]
        private string _description = "";
        [ObservableProperty]
        private string _additionalInformation = "";
        [ObservableProperty]
        private string _github = "";
        [ObservableProperty]
        private string _estimatedTimeText = "";
        [ObservableProperty]
        private string _dateStartText = "";
        [ObservableProperty]
        private string _dateEndText = "";
        [ObservableProperty]
        private string _leaderName = "";
        [ObservableProperty]
        private string _leaderEmail = "";
        [ObservableProperty]
        private bool _isLoading = true;

        [ObservableProperty]
        private bool _isProjectEditPopupVisible;

        [ObservableProperty]
        private string _editName = "";
        [ObservableProperty]
        private string _editDescription = "";
        [ObservableProperty]
        private string _editAdditionalInfo = "";
        [ObservableProperty]
        private string _editGithub = "";
        [ObservableProperty]
        private string _editEstimatedTime = "";
        [ObservableProperty]
        private string _editDateStart = "";
        [ObservableProperty]
        private string _editDateEnd = "";
        [ObservableProperty]
        private Models.Users.Member? _editLeader;

        public ObservableCollection<Models.Users.Member> Participants { get; } = new();

        public ObservableCollection<Models.Users.Member> ClubMembers { get; } = new();

        [ObservableProperty]
        private Models.Users.Member? _selectedMemberToAdd;

        public ProjectProfileViewModel(int projectId, INavigationService navigationService, IProjectRepository projectRepository, IMemberRepository memberRepository)
        {
            _projectId = projectId;
            _navigationService = navigationService;
            _projectRepository = projectRepository;
            _memberRepository = memberRepository;

            _ = LoadAsync();
        }

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

        [RelayCommand]
        private void GoBack()
        {
            _navigationService.NavigateTo(App.Services.GetRequiredService<ProjectsViewModel>());
        }

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

        [RelayCommand]
        private void CancelEdit() => IsProjectEditPopupVisible = false;

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

        [RelayCommand]
        private async Task RemoveParticipantAsync(Models.Users.Member participant)
        {
            if (participant == null) return;

            Participants.Remove(participant);

            var participantIds = Participants.Select(p => p.MemberId).ToList();
            await _projectRepository.UpdateProjectParticipantsAsync(_projectId, participantIds);
            await LoadAsync();
        }

        [RelayCommand]
        private void DeleteThisProject()
        {
            IsProjectEditPopupVisible = false;
            IsPopupVisible = true;
        }

        protected override async Task ExecuteConfirmDeleteAsync()
        {
            await _projectRepository.DeleteSingleProjectAsync(_projectId);

            _navigationService.NavigateTo(App.Services.GetRequiredService<ProjectsViewModel>());
        }
    }
}

