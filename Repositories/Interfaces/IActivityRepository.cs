using Models.Activities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Esseti.Repositories.Interfaces
{
    public interface IActivityRepository
    {
        Task<List<Activity>> GetAllActivitiesAsync();
    }
}
