using Esseti.Repositories;
using Esseti.Repositories.Interfaces;
using Esseti.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Esseti
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddSingleton<IMemberRepository, MemberRepository>();
            services.AddSingleton<IProjectRepository, ProjectRepository>();
            services.AddSingleton<IActivityRepository, ActivityRepository>();

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
