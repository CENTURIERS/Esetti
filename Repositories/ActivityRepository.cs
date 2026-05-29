using Esseti.Data;
using Esseti.Repositories.Interfaces;
using Esseti.Services;
using Microsoft.EntityFrameworkCore;
using Models.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Esseti.Repositories
{
    public class ActivityRepository : IActivityRepository
    {
        private readonly EssetiDbContext _context;
        private readonly ICacheService _cacheService;

        public ActivityRepository(EssetiDbContext context, ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<List<Activity>> GetAllActivitiesAsync()
        {
            return await _cacheService.GetOrLoadAsync("activities_all", () => _context.Activities
                .Where(a => a.IsActive)
                .Include(a => a.Participants)
                .ToListAsync());
        }

        public async Task<Activity?> GetActivityByIdAsync(int id)
        {
            return await _context.Activities
                        .Include(a => a.Participants)
                        .FirstOrDefaultAsync(a => a.ActivityId == id && a.IsActive);
        }

        public async Task AddActivityAsync(Activity activity)
        {
            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();
            _cacheService.Invalidate("activities_all");
        }

        public async Task UpdateActivityAsync(Activity activity, IEnumerable<int>? participantIds = null)
        {
            if (participantIds == null)
            {
                _context.Activities.Update(activity);
                await _context.SaveChangesAsync();
                _cacheService.Invalidate("activities_all");
                return;
            }

            var dbActivity = await _context.Activities
                                .Include(a => a.Participants)
                                .FirstOrDefaultAsync(a => a.ActivityId == activity.ActivityId); 

            if (dbActivity != null)
            {
                dbActivity.Name = activity.Name;
                dbActivity.Date = activity.Date;
                dbActivity.Time = activity.Time;
                dbActivity.City = activity.City;
                dbActivity.AddressLine = activity.AddressLine;
                dbActivity.PostalCode = activity.PostalCode;
                dbActivity.PersonInChargeName = activity.PersonInChargeName;
                dbActivity.PersonInChargePhone = activity.PersonInChargePhone;
                dbActivity.PersonInChargeEmail = activity.PersonInChargeEmail ;
                dbActivity.AdditionalInformation = activity.AdditionalInformation;
                dbActivity.IsRepeatable = activity.IsRepeatable;

                dbActivity.Participants.Clear();

                var participants = await _context.Members
                                    .Where(m => participantIds.Contains(m.MemberId))
                                    .ToListAsync();

                foreach (var member in participants)
                {
                    dbActivity.Participants.Add(member);
                }

                await _context.SaveChangesAsync();
                _cacheService.Invalidate("activities_all");
            }
        }

        public async Task UpdateActivityParticipantsAsync(int activityId, IEnumerable<int> participantIds)
        {
            var dbActivity = await _context.Activities
                                .Include(a => a.Participants)
                                .FirstOrDefaultAsync(a => a.ActivityId == activityId);

            if (dbActivity != null)
            {
                dbActivity.Participants.Clear();
                var participants = await _context.Members
                                    .Where(m => participantIds.Contains(m.MemberId))
                                    .ToListAsync();

                foreach (var member in participants)
                {
                    dbActivity.Participants.Add(member);
                }

                await _context.SaveChangesAsync();
                _context.ChangeTracker.Clear();
                _cacheService.Invalidate("activities_all");
            }
        }

        public async Task DeleteSingleActivityAsync(int id)
        {
            var activity = await _context.Activities.FindAsync(id);

            if (activity != null)
            {
                activity.IsActive = false;
                await _context.SaveChangesAsync();
                _cacheService.Invalidate("activities_all");
            }
        }

        public async Task DeleteActivitesAsync(IEnumerable<int> activitiesIds)
        {
            List<Activity> activitiesToDelete = await _context.Activities.Where(a => activitiesIds.Contains(a.ActivityId)).ToListAsync();

            foreach (var activity in activitiesToDelete)
            {
                activity.IsActive = false;
            }

            await _context.SaveChangesAsync();
            _cacheService.Invalidate("activities_all");
        }
    }
}

