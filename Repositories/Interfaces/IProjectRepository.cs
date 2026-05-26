using Models.Activities;
using Models.Users;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Esseti.Repositories.Interfaces
{
    public interface IProjectRepository
    {
        Task<List<Project>> GetAllProjectsAsync();
        Task<Project?> GetProjectByIdAsync(int id);
        Task AddProjectAsync(Project project);
        Task UpdateProjectAsync(Project project, IEnumerable<int>? participantIds = null);
        Task UpdateProjectParticipantsAsync(int projectId, IEnumerable<int> participantIds);
        Task DeleteSingleProjectAsync(int id);
        Task DeleteProjectsAsync(IEnumerable<int> projectIds);
    }
}
