using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Esseti.Repositories.Interfaces;

namespace Esseti.ViewModels
{
    /// <summary>
    /// Model widoku obsługujący ekran Ustawień.
    /// Zapewnia podgląd statystyk koła (liczba studentów, projektów itp.) oraz opcje backupu i importu bazy danych.
    /// </summary>
    public partial class SettingsViewModel : ViewModelBase
    {
        /// <summary>
        /// Repozytorium klubu służące do pobierania statystyk.
        /// </summary>
        private readonly IClubRepository _clubRepository;

        /// <summary>
        /// Łączna liczba aktywnych członków koła.
        /// </summary>
        [ObservableProperty]
        private int _totalActiveMembers;

        /// <summary>
        /// Łączna liczba zarejestrowanych projektów.
        /// </summary>
        [ObservableProperty]
        private int _totalProjects;

        /// <summary>
        /// Łączna liczba zarejestrowanych aktywności.
        /// </summary>
        [ObservableProperty]
        private int _totalActivities;

        /// <summary>
        /// Łączna liczba zorganizowanych wyjazdów/wycieczek.
        /// </summary>
        [ObservableProperty]
        private int _totalTrips;

        /// <summary>
        /// Tytuł wyświetlany w nagłówku strony.
        /// </summary>
        public override string PageTitle => "Ustawienia";

        /// <summary>
        /// Konstruktor modelu widoku ustawień. Pobiera asynchronicznie statystyki z bazy.
        /// </summary>
        /// <param name="clubRepository">Repozytorium klubu/koła.</param>
        public SettingsViewModel(IClubRepository clubRepository)
        {
            _clubRepository = clubRepository;

            _ = LoadStatsAsync();
        }

        /// <summary>
        /// Asynchroniczne ładowanie i przeliczanie statystyk koła naukowego do wyświetlenia na widoku.
        /// </summary>
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

        /// <summary>
        /// Flaga sterująca widocznością wyskakującego okienka (popup) ze statusem operacji na bazie.
        /// </summary>
        [ObservableProperty]
        private bool _isStatusPopupVisible;

        /// <summary>
        /// Tytuł wyskakującego okienka statusu.
        /// </summary>
        [ObservableProperty]
        private string _statusPopupTitle = string.Empty;

        /// <summary>
        /// Treść wiadomości w wyskakującym okienku statusu.
        /// </summary>
        [ObservableProperty]
        private string _statusPopupMessage = string.Empty;

        /// <summary>
        /// Czy status w popupie dotyczy błędu (zmienia np. ikonę/kolor tekstu w widoku).
        /// </summary>
        [ObservableProperty]
        private bool _isStatusError;

        /// <summary>
        /// Komenda zamykająca okienko statusu.
        /// </summary>
        [RelayCommand]
        private void CloseStatusPopup()
        {
            IsStatusPopupVisible = false;
        }

        /// <summary>
        /// Komenda asynchroniczna tworząca kopię zapasową pliku bazy danych (SQLite).
        /// Otwiera systemowe okno zapisu pliku i kopiuje tam aktualną bazę.
        /// </summary>
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

        /// <summary>
        /// Komenda asynchroniczna importująca bazę danych z zewnętrznego pliku SQLite.
        /// Kopiuje wskazany plik do folderu aplikacji jako plik przygotowany do podmiany po restarcie.
        /// </summary>
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
