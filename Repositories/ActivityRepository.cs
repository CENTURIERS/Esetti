using Esseti.Repositories.Interfaces;
using Models.Activities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Esseti.Repositories
{
    public class ActivityRepository : IActivityRepository
    {
        public async Task<List<Activity>> GetAllActivitiesAsync()
        {
            return new List<Activity>
            {

            };
        }
    }
}
