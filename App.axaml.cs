using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Esseti.ViewModels;
using Esseti.Views;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Esseti;

namespace Esseti
{
    public partial class App : Application
    {

        public static IServiceProvider Services { get; private set; } = null!;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            ConfigureServices();
            MigrateDatabase();
        }

        private void MigrateDatabase()
        {
            try
            {
                using (var scope = Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<Esseti.Data.EssetiDbContext>();
                    try
                    {
                        context.Database.ExecuteSqlRaw("ALTER TABLE club_info ADD COLUMN supervisor_email TEXT;");
                    }
                    catch { }
                    try
                    {
                        context.Database.ExecuteSqlRaw("ALTER TABLE club_info ADD COLUMN supervisor_phone TEXT;");
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd podczas migracji bazy danych: {ex.Message}");
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