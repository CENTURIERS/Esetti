using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Esseti.ViewModels;
using Esseti.Views;
using System;
using Microsoft.Extensions.DependencyInjection;
using Esseti;

namespace Esseti
{
    public partial class App : Application
    {

        public static IServiceProvider Services { get; private set; } = null!;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            ConfigureServices();
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