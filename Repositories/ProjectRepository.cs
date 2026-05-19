using Esseti.Data;
using Esseti.Repositories.Interfaces;
using Models.Activities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Esseti.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        public async Task<List<Project>> GetAllProjectsAsync()
        {
            return new List<Project>
            {

            };
        }
    }
}
