using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Esseti.Repositories.Interfaces;

namespace Esseti.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
        private readonly IMemberRepository _memberRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IActivityRepository _activityRepository;
        private readonly ITripRepository _tripRepository;

        [ObservableProperty]
        private int _totalActiveMembers;

        [ObservableProperty]
        private int _totalProjects;

        [ObservableProperty]
        private int _totalActivities;

        [ObservableProperty]
        private int _totalTrips;

        public override string PageTitle => "Ustawienia";

        public SettingsViewModel(
            IMemberRepository memberRepository,
            IProjectRepository projectRepository,
            IActivityRepository activityRepository,
            ITripRepository tripRepository)
        {
            _memberRepository = memberRepository;
            _projectRepository = projectRepository;
            _activityRepository = activityRepository;
            _tripRepository = tripRepository;

            _ = LoadStatsAsync();
        }

        private async Task LoadStatsAsync()
        {
            try
            {
                var members = await _memberRepository.GetAllMembersAsync();
                TotalActiveMembers = members.Count(m => m.IsActive);

                var projects = await _projectRepository.GetAllProjectsAsync();
                TotalProjects = projects.Count;

                var activities = await _activityRepository.GetAllActivitiesAsync();
                TotalActivities = activities.Count;

                var trips = await _tripRepository.GetTripsAsync();
                TotalTrips = trips.Count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading statistics: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task BackupDatabaseAsync()
        {
            if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                var window = desktop.MainWindow;
                if (window != null)
                {
                    var file = await window.StorageProvider.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions
                    {
                        Title = "Zapisz kopię zapasową bazy danych",
                        DefaultExtension = "db",
                        SuggestedFileName = $"esseti_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db",
                        FileTypeChoices = new[]
                        {
                            new Avalonia.Platform.Storage.FilePickerFileType("Baza danych SQLite (*.db)")
                            {
                                Patterns = new[] { "*.db" }
                            }
                        }
                    });

                    if (file != null)
                    {
                        try
                        {
                            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "esseti.db");
                            if (File.Exists(dbPath))
                            {
                                await using (var sourceStream = File.OpenRead(dbPath))
                                await using (var targetStream = await file.OpenWriteAsync())
                                {
                                    await sourceStream.CopyToAsync(targetStream);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to backup database: {ex.Message}");
                        }
                    }
                }
            }
        }
    }
}
