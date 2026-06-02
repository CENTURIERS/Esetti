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

        [ObservableProperty]
        private bool _isStatusPopupVisible;

        [ObservableProperty]
        private string _statusPopupTitle = string.Empty;

        [ObservableProperty]
        private string _statusPopupMessage = string.Empty;

        [ObservableProperty]
        private bool _isStatusError;

        [RelayCommand]
        private void CloseStatusPopup()
        {
            IsStatusPopupVisible = false;
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
                                Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
                                GC.Collect();
                                GC.WaitForPendingFinalizers();
                                await using (var sourceStream = new FileStream(dbPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                await using (var targetStream = await file.OpenWriteAsync())
                                {
                                    await sourceStream.CopyToAsync(targetStream);
                                }
                                StatusPopupTitle = "Sukces";
                                StatusPopupMessage = "Kopia zapasowa bazy danych została pomyślnie utworzona.";
                                IsStatusError = false;
                                IsStatusPopupVisible = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to backup database: {ex.Message}");
                            StatusPopupTitle = "Błąd";
                            StatusPopupMessage = $"Nie udało się utworzyć kopii zapasowej bazy danych: {ex.Message}";
                            IsStatusError = true;
                            IsStatusPopupVisible = true;
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
                        Title = "Importuj bazę danych",
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
                            string dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
                            if (!Directory.Exists(dataDir))
                            {
                                Directory.CreateDirectory(dataDir);
                            }
                            string importPath = Path.Combine(dataDir, "esseti_import.db");
                            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                            await using (var source = await selected.OpenReadAsync())
                            await using (var destination = File.Open(importPath, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                await source.CopyToAsync(destination);
                            }
                            StatusPopupTitle = "Wymagany restart";
                            StatusPopupMessage = "Baza danych została przygotowana do importu. Aby zastosować zmiany i załadować nowe dane, uruchom aplikację ponownie.";
                            IsStatusError = false;
                            IsStatusPopupVisible = true;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to import database: {ex.Message}");
                            StatusPopupTitle = "Błąd";
                            StatusPopupMessage = $"Nie udało się przygotować bazy danych do importu: {ex.Message}";
                            IsStatusError = true;
                            IsStatusPopupVisible = true;
                        }
                    }
                }
            }
        }
    }
}

