using Esseti.Data;
using Esseti.Repositories;
using Esseti.Repositories.Interfaces;
using Esseti.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Esseti
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddDbContext<EssetiDbContext>();

            services.AddScoped<IMemberRepository, MemberRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IActivityRepository, ActivityRepository>();

            return services;
        }

        public static IServiceCollection AddViewModels(this IServiceCollection services)
        {
            services.AddTransient<MembersViewModel>();
            services.AddTransient<ProjectsViewModel>();
            services.AddTransient<ActivitiesViewModel>();
            services.AddSingleton<MainWindowViewModel>();

            return services;
        }
    }
}