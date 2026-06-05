using Esseti.Data;
using Esseti.Repositories;
using Esseti.Repositories.Interfaces;
using Esseti.Services;
using Esseti.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Esseti
{
    /// <summary>
    /// Klasa konfigurująca nasz kontener DI (Dependency Injection).
    /// Tutaj rejestrujemy serwisy, repozytoria i view modele, żeby framework wiedział, jak je wstrzykiwać.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Rejestruje bazę danych, serwisy pomocnicze oraz wszystkie repozytoria w kontenerze DI.
        /// </summary>
        /// <param name="services">Kolekcja serwisów, do której dorzucamy nasze klasy.</param>
        /// <returns>Zwraca zmodyfikowaną kolekcję serwisów (pozwala na chaining metod).</returns>
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddDbContext<EssetiDbContext>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<ICacheService, MemoryCacheService>();
            services.AddScoped<IMemberRepository, MemberRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IActivityRepository, ActivityRepository>();
            services.AddScoped<IClubRepository, ClubRepository>();
            services.AddScoped<ITripRepository, TripRepository>();
            services.AddTransient<IPdfGeneratorService, PdfGeneratorService>();
            return services;
        }

        /// <summary>
        /// Rejestruje ViewModele używane w aplikacji. 
        /// Większość jest Transient (tworzone na żądanie za każdym razem), oprócz MainWindowViewModel, który jest Singletonem.
        /// </summary>
        /// <param name="services">Kolekcja serwisów.</param>
        /// <returns>Zwraca tę samą kolekcję serwisów z dodanymi ViewModelami.</returns>
        public static IServiceCollection AddViewModels(this IServiceCollection services)
        {
            services.AddTransient<MembersViewModel>();
            services.AddTransient<ProjectsViewModel>();
            services.AddTransient<ActivitiesViewModel>();
            services.AddTransient<ClubInfoViewModel>();
            services.AddTransient<DocumentsViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddSingleton<MainWindowViewModel>();
            return services;
        }
    }
}

