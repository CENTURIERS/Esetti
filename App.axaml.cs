using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Esseti.ViewModels;
using Esseti.Views;
using System;
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
            AvaloniaXamlLoader.Load(this);
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            ConfigureServices();
            InitializeDatabase();
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
                System.Diagnostics.Debug.WriteLine($"BĹ‚Ä…d podczas inicjalizacji bazy danych: {ex.Message}");
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

