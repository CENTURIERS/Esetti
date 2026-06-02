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
    public partial class App : Application
    {

        public static IServiceProvider Services { get; private set; } = null!;

        public override void Initialize()
        {
            CheckAndSwapDatabase();
            AvaloniaXamlLoader.Load(this);
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            ConfigureServices();
            InitializeDatabase();
        }

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

        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddRepositories().AddViewModels();

            Services = services.BuildServiceProvider();
        }

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

