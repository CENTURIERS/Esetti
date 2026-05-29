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
        private readonly IClubRepository _clubRepository;

        [ObservableProperty]
        private int _totalActiveMembers;

        [ObservableProperty]
        private int _totalProjects;

        [ObservableProperty]
        private int _totalActivities;

        [ObservableProperty]
        private int _totalTrips;

        public override string PageTitle => "Ustawienia";

        public SettingsViewModel(IClubRepository clubRepository)
        {
            _clubRepository = clubRepository;

            _ = LoadStatsAsync();
        }

        private async Task LoadStatsAsync()
        {
            try
            {
                TotalActiveMembers = await _clubRepository.GetMembersCountAsync();
                TotalProjects = await _clubRepository.GetProjectsCountAsync();
                TotalActivities = await _clubRepository.GetActivitiesCountAsync();
                
                var trips = await _clubRepository.GetTripsAsync();
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
                        Title = "Zapisz kopiÄ™ zapasowÄ… bazy danych",
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

        [RelayCommand]
        private async Task ImportDatabaseAsync()
        {
            if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                var window = desktop.MainWindow;
                if (window != null)
                {
                    var file = await window.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
                    {
                        Title = "Importuj bazÄ™ danych",
                        AllowMultiple = false,
                        FileTypeFilter = new[]
                        {
                            new Avalonia.Platform.Storage.FilePickerFileType("Baza danych SQLite (*.db)")
                            {
                                Patterns = new[] { "*.db" }
                            }
                        }
                    });

                    var selected = file?.FirstOrDefault();
                    if (selected != null)
                    {
                        try
                        {
                            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "esseti.db");
                            // Overwrite existing DB with imported file
                            await using (var source = await selected.OpenReadAsync())
                            await using (var destination = File.Open(dbPath, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                await source.CopyToAsync(destination);
                            }
                            // Reload stats after import
                            await LoadStatsAsync();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to import database: {ex.Message}");
                        }
                    }
                }
            }
        }
    }
}

