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
    }
}
