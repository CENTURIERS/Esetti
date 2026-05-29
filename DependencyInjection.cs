using Esseti.Data;
using Esseti.Repositories;
using Esseti.Repositories.Interfaces;
using Esseti.Services;
using Esseti.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Esseti
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddDbContext<EssetiDbContext>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddScoped<IMemberRepository, MemberRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IActivityRepository, ActivityRepository>();
            services.AddScoped<IClubRepository, ClubRepository>();
            services.AddScoped<ITripRepository, TripRepository>();
            return services;
        }

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