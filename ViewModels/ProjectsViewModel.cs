using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Esseti.Repositories.Interfaces;
using Esseti.Services;
using Esseti.ViewModels.Components;
using Models.Users;
using Models.Activities;

namespace Esseti.ViewModels
{
    public partial class ProjectsViewModel : ViewModelBase
    {
        public override string PageTitle => "Lista Projektów";
        public override bool ShowActionHeader => true;
        public override string SearchPlaceholder => "Szukaj projektów...";

        public ObservableCollection<ProjectItemViewModel> Projects { get; } = new();
        public ObservableCollection<Models.Users.Member> ClubMembers { get; } = new();

        private readonly List<ProjectItemViewModel> _allProjects = new();

        private readonly IProjectRepository _projectRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly INavigationService _navigationService;

        private ProjectItemViewModel? _projectToDelete;
        private ProjectItemViewModel? _editingProject;

        [ObservableProperty]
        private string _newProjectName = string.Empty;
        [ObservableProperty]
        private string _newProjectDescription = string.Empty;
        [ObservableProperty]
        private string _newProjectAdditionalInfo = string.Empty;
        [ObservableProperty]
        private string _newProjectGithub = string.Empty;
        [ObservableProperty]
        private string _newProjectEstimatedTime = string.Empty;
        [ObservableProperty]
        private string _newProjectDateStart = DateTime.Now.ToString("dd.MM.yyyy");
        [ObservableProperty]
        private string _newProjectDateEnd = DateTime.Now.AddMonths(3).ToString("dd.MM.yyyy");
        [ObservableProperty]
        private Models.Users.Member? _newProjectLeader;
        
        [ObservableProperty]
        private bool _isAddEditPopupVisible;
        [ObservableProperty]
        private string _popupTitle = "Nowy projekt";

        [ObservableProperty]
        private bool _isNameInvalid;
        [ObservableProperty]
        private bool _isEstTimeInvalid;
        [ObservableProperty]
        private bool _isDateStartInvalid;
        [ObservableProperty]
        private bool _isDateEndInvalid;
        [ObservableProperty]
        private bool _isFormValid = true;
        [ObservableProperty]
        private string _validationError = string.Empty;

        private void ValidateForm()
        {
            IsNameInvalid = string.IsNullOrWhiteSpace(NewProjectName);
            IsEstTimeInvalid = string.IsNullOrWhiteSpace(NewProjectEstimatedTime) || !int.TryParse(NewProjectEstimatedTime, out _);
            IsDateStartInvalid = string.IsNullOrWhiteSpace(NewProjectDateStart) || !TryParseDate(NewProjectDateStart, out _);
            IsDateEndInvalid = string.IsNullOrWhiteSpace(NewProjectDateEnd) || !TryParseDate(NewProjectDateEnd, out _);

            IsFormValid = !IsNameInvalid && !IsEstTimeInvalid && !IsDateStartInvalid && !IsDateEndInvalid;

            if (IsNameInvalid) ValidationError = "Nazwa projektu jest wymagana.";
            else if (IsDateStartInvalid) ValidationError = "Data rozpoczęcia jest wymagana i musi być poprawna (np. dd.MM.yyyy).";
            else if (IsDateEndInvalid) ValidationError = "Data zakończenia jest wymagana i musi być poprawna (np. dd.MM.yyyy).";
            else if (IsEstTimeInvalid) ValidationError = "Szacowany czas trwania jest wymagany i musi być liczbą całkowitą.";
            else ValidationError = string.Empty;
        }

        protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(NewProjectName) ||
                e.PropertyName == nameof(NewProjectEstimatedTime) ||
                e.PropertyName == nameof(NewProjectDateStart) ||
                e.PropertyName == nameof(NewProjectDateEnd))
            {
                ValidateForm();
            }
        }

        public string SelectAllText =>  IsAllSelected ? "Odznacz wszystko" : "Zaznacz wszystko";
        public bool HasSelectedItems => IsAnySelected;
        public string SelectedCountText => $"Zaznaczono {SelectedCount} projektów";
        
        public ProjectsViewModel(IProjectRepository projectRepository, IMemberRepository memberRepository, INavigationService navigationService)
        {
            _projectRepository = projectRepository;
            _memberRepository = memberRepository;
            _navigationService = navigationService;

            _ = LoadDataAsync();
        }

        private void OnProjectItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ProjectItemViewModel.IsSelected))
            {
                UpdateSelectionState();

                if (!_isUpdatingSelection)
                {
                    _isUpdatingSelection = true;
                    try
                    {
                        if (Projects.Any())
                            IsAllSelected = Projects.All(p => p.IsSelected);
                    }
                    finally
                    {
                        _isUpdatingSelection = false;
                    }
                }
            }
        }

        private void ClearProjectSubscriptions()
        {
            foreach (var vm in _allProjects)
            {
                vm.PropertyChanged -= OnProjectItemPropertyChanged;
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var projectsFromDb = await _projectRepository.GetAllProjectsAsync();
                var membersFromDb = await _memberRepository.GetAllMembersAsync();

                Dispatcher.UIThread.Post(() =>
                {
                    ClearProjectSubscriptions();
                    Projects.Clear();
                    _allProjects.Clear();

                    if (projectsFromDb != null)
                    {
                        foreach (var project in projectsFromDb)
                        {
                            var leaderName = project.PersonInCharge != null
                                ? $"{project.PersonInCharge.FirstName} {project.PersonInCharge.LastName}"
                                : "Brak Lidera";

                            var vm = new ProjectItemViewModel(
                                projectId: project.ProjectId.ToString(),
                                name: project.Name,
                                description: project.Description ?? "Brak opisu",
                                estimatedTime: project.EstimatedTime != null ? $"{project.EstimatedTime}" : "Niezdefiniowano",
                                leaderName: leaderName,
                                isSelected: false
                            );

                            vm.PropertyChanged += OnProjectItemPropertyChanged;

                            _allProjects.Add(vm);
                        }
                    }

                    ClubMembers.Clear();
                    if (membersFromDb != null)
                    {
                        foreach (var m in membersFromDb)
                            ClubMembers.Add(m);
                    }

                    ApplyFilter();
                });
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd LoadDataAsync: {e}");
            }
        }


        [RelayCommand]
        private void ToggleSelectedAll() => IsAllSelected = !IsAllSelected;

        protected override void OnIsAllSelectedChangedVirtual(bool value)
        {
            if (_isUpdatingSelection) return;

            _isUpdatingSelection = true;
            try
            {
                foreach (var project in Projects)
                {
                    project.IsSelected = value;
                }

                UpdateSelectionState();
            } finally
            {
                _isUpdatingSelection = false;
            }
        }

        public void UpdateSelectionState()
        {
            var selected = Projects.Where(p => p.IsSelected).ToList();
            SelectedCount = selected.Count;
            IsAnySelected = SelectedCount > 0;

            OnPropertyChanged(nameof(SelectAllText));
            OnPropertyChanged(nameof(HasSelectedItems));
            OnPropertyChanged(nameof(SelectedCountText));
        }

        [RelayCommand]
        private void OpenAddPopup()
        {
            _editingProject = null;
            PopupTitle = "Nowy Projekt";

            NewProjectName = string.Empty;
            NewProjectDescription = string.Empty;
            NewProjectAdditionalInfo = string.Empty;
            NewProjectGithub = string.Empty;
            NewProjectEstimatedTime = string.Empty;
            NewProjectDateStart = DateTime.Now.ToString("dd.MM.yyyy");
            NewProjectDateEnd = DateTime.Now.AddMonths(3).ToString("dd.MM.yyyy");
            NewProjectLeader = ClubMembers.FirstOrDefault();

            IsNameInvalid = false;
            IsEstTimeInvalid = false;
            IsDateStartInvalid = false;
            IsDateEndInvalid = false;
            IsFormValid = false; // Name is empty initially

            IsAddEditPopupVisible = true;
        }

        [RelayCommand]
        private async Task OpenEditPopup(ProjectItemViewModel item)
        {
            if (item == null) return;
            _editingProject = item;
            PopupTitle = "Edycja projektu";

            if (int.TryParse(item.ProjectId, out var projectId))
            {
                var fullProject = await _projectRepository.GetProjectByIdAsync(projectId);

                if (fullProject != null)
                {
                    NewProjectName = fullProject.Name;
                    NewProjectDescription = fullProject.Description ?? string.Empty;
                    NewProjectAdditionalInfo = fullProject.AdditionalInformation ?? string.Empty;
                    NewProjectGithub = fullProject.Github ?? string.Empty;
                    NewProjectEstimatedTime = fullProject.EstimatedTime?.ToString() ?? string.Empty;
                    NewProjectDateStart = fullProject.DateStart?.ToString("dd.MM.yyyy") ?? string.Empty;
                    NewProjectDateEnd = fullProject.DateEnd?.ToString("dd.MM.yyyy") ?? string.Empty;
                    NewProjectLeader = ClubMembers.FirstOrDefault(m => m.MemberId == fullProject.PersonInChargeId);
                }
            }
            ValidateForm();
            IsAddEditPopupVisible = true;
        }

        [RelayCommand]
        private void ClosePopup() => IsAddEditPopupVisible = false;

        [RelayCommand(CanExecute = nameof(IsFormValid))]
        private async Task SaveProject()
        {
            try 
            {
                if (string.IsNullOrWhiteSpace(NewProjectName)) return;

                DateTime? start = TryParseDate(NewProjectDateStart, out var ds) ? ds : null;
                DateTime? end = TryParseDate(NewProjectDateEnd, out var de) ? de : null;
                int? estTime = int.TryParse(NewProjectEstimatedTime, out var et) ? et : null;

                if (_editingProject == null)
                {
                    Project projectAdd = new Project
                    {
                        Name = NewProjectName,
                        Description = NewProjectDescription,
                        AdditionalInformation = NewProjectAdditionalInfo,
                        Github = NewProjectGithub,
                        EstimatedTime = estTime,
                        DateStart = start,
                        DateEnd = end,
                        PersonInChargeId = NewProjectLeader?.MemberId,
                        IsActive = true
                    };
                    await _projectRepository.AddProjectAsync(projectAdd);
                } else
                {
                    if (int.TryParse(_editingProject.ProjectId, out var projectId))
                    {
                        var projectUpdate = await _projectRepository.GetProjectByIdAsync(projectId);

                        if (projectUpdate != null)
                        {
                            projectUpdate.Name = NewProjectName;
                            projectUpdate.Description = NewProjectDescription;
                            projectUpdate.AdditionalInformation = NewProjectAdditionalInfo;
                            projectUpdate.Github = NewProjectGithub;
                            projectUpdate.EstimatedTime = estTime;
                            projectUpdate.DateStart = start;
                            projectUpdate.DateEnd = end;
                            projectUpdate.PersonInChargeId = NewProjectLeader?.MemberId;

                            await _projectRepository.UpdateProjectAsync(projectUpdate);
                        }
                    }
                }

                IsAddEditPopupVisible = false;
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving project: {ex}");
            }
        }

        [RelayCommand]
        private void OpenProfile(ProjectItemViewModel item)
        {
            if (item == null) return;

            if (int.TryParse(item.ProjectId, out var projectId))
            {
                var profileVM = new ProjectProfileViewModel(
                    projectId,
                    _navigationService,
                    _projectRepository,
                    _memberRepository
                );

                _navigationService.NavigateTo(profileVM);    
            }
        }

        [RelayCommand]
        private void DeleteSingleProject(ProjectItemViewModel item)
        {
            if (item == null) return;
            _projectToDelete = item;
            IsPopupVisible = true;
        }

        protected override async Task ExecuteConfirmDeleteAsync()
        {
            try
            {
                if (_projectToDelete != null)
                {
                    if (int.TryParse(_projectToDelete.ProjectId, out var projectId))
                    {
                        await _projectRepository.DeleteSingleProjectAsync(projectId);
                    }
                    _projectToDelete = null;
                } else
                {
                    var selectedVMs = Projects.Where(p => p.IsSelected).ToList();
                    if(!selectedVMs.Any()) return;

                    var idsToDelete = new List<int>();
                    foreach (var p in selectedVMs)
                    {
                        if (int.TryParse(p.ProjectId, out var id))
                        {
                            idsToDelete.Add(id);
                        }
                    }
                    await _projectRepository.DeleteProjectsAsync(idsToDelete);
                }

                IsAllSelected = false;
                await LoadDataAsync();
            } catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd usuwania projektu: {ex.Message}");
            }
        }

        protected override void OnPopupClosed()
        {
            _projectToDelete = null;
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasPreviousPage))]
        [NotifyPropertyChangedFor(nameof(HasNextPage))]
        private int _currentPage = 1;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasPreviousPage))]
        [NotifyPropertyChangedFor(nameof(HasNextPage))]
        private int _totalPages = 1;

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        [RelayCommand]
        private void NextPage()
        {
            if (HasNextPage)
            {
                CurrentPage++;
                ApplyFilter();
            }
        }

        [RelayCommand]
        private void PreviousPage()
        {
            if (HasPreviousPage)
            {
                CurrentPage--;
                ApplyFilter();
            }
        }

        protected override void OnSearchQueryUpdated(string value)
        {
            CurrentPage = 1;
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            Projects.Clear();
            var query = SearchQuery?.ToLower() ?? "";

            var filteredList = _allProjects
                .Where(item => (item.Name ?? "").ToLower().Contains(query) || (item.LeaderName ?? "").ToLower().Contains(query))
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
                Projects.Add(item);
            }

            UpdateSelectionState();
        }
    }
}


