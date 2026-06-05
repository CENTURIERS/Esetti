using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Esseti.ViewModels;
using Esseti.Views;
using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Esseti;
using Esseti.Data;

namespace Esseti
{
    /// <summary>
    /// Główna klasa aplikacji Esseti — odpowiada za inicjalizację, konfigurację DI i uruchomienie okna głównego.
    /// </summary>
    public partial class App : Application
    {

        /// <summary>
        /// Globalny kontener usług (DI) dostępny w całej aplikacji.
        /// </summary>
        public static IServiceProvider Services { get; private set; } = null!;

        /// <summary>
        /// Inicjalizuje aplikację — ładuje XAML, konfiguruje licencję QuestPDF, rejestruje usługi i przygotowuje bazę danych.
        /// </summary>
        public override void Initialize()
        {
            CheckAndSwapDatabase();
            AvaloniaXamlLoader.Load(this);
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            ConfigureServices();
            InitializeDatabase();
        }

        /// <summary>
        /// Sprawdza, czy istnieje plik importu bazy danych i jeśli tak, podmienia nim aktualną bazę.
        /// </summary>
        private void CheckAndSwapDatabase()
        {
            try
            {
                string dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
                string importPath = Path.Combine(dataDir, "esseti_import.db");
                string dbPath = Path.Combine(dataDir, "esseti.db");

                if (File.Exists(importPath))
                {
                    string walPath = Path.Combine(dataDir, "esseti.db-wal");
                    string shmPath = Path.Combine(dataDir, "esseti.db-shm");

                    Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    if (File.Exists(dbPath))
                    {
                        File.Delete(dbPath);
                    }
                    if (File.Exists(walPath))
                    {
                        File.Delete(walPath);
                    }
                    if (File.Exists(shmPath))
                    {
                        File.Delete(shmPath);
                    }

                    File.Move(importPath, dbPath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd podczas podmieniania bazy danych: {ex.Message}");
            }
        }

        /// <summary>
        /// Uruchamia migracje EF Core i seeduje bazę danych początkowymi danymi, jeśli jest pusta.
        /// </summary>
        private void InitializeDatabase()
        {
            try
            {
                using var scope = Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<EssetiDbContext>();

                // Apply pending EF Core migrations (creates DB if not exists)
                context.Database.Migrate();

                // Seed initial data if database is empty
                DbSeeder.SeedAsync(context).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd podczas inicjalizacji bazy danych: {ex.Message}");
            }
        }

        /// <summary>
        /// Konfiguruje kontener Dependency Injection — rejestruje repozytoria i ViewModele.
        /// </summary>
        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddRepositories().AddViewModels();

            Services = services.BuildServiceProvider();
        }

        /// <summary>
        /// Wywoływane po zakończeniu inicjalizacji frameworka — tworzy i wyświetla główne okno aplikacji.
        /// </summary>
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = Services.GetRequiredService<MainWindowViewModel>(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

    }
}

