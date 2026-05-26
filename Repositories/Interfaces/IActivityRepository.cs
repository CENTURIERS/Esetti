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
        Task<Activity?> GetActivityByIdAsync(int id);
        Task AddActivityAsync(Activity activity);
        Task UpdateActivityAsync(Activity activity, IEnumerable<int>? participantIds = null);
        Task UpdateActivityParticipantsAsync(int activityId, IEnumerable<int> participantIds);
        Task DeleteSingleActivityAsync(int id);
        Task DeleteActivitesAsync(IEnumerable<int> activitiesIds);
    }
}
