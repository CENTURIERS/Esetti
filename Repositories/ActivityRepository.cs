using Esseti.Data;
using Esseti.Repositories.Interfaces;
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

        public ActivityRepository(EssetiDbContext context)
        {
            _context = context;
        }

        public async Task<List<Activity>> GetAllActivitiesAsync()
        {
            return await _context.Activities
                .Include(a => a.Participants)
                .ToListAsync();
        }

        public async Task<Activity?> GetActivityByIdAsync(int id)
        {
            return await _context.Activities
                        .Include(a => a.Participants)
                        .FirstOrDefaultAsync(a => a.ActivityId == id);
        }

        public async Task AddActivityAsync(Activity activity)
        {
            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateActivityAsync(Activity activity, IEnumerable<int>? participantIds = null)
        {
            if (participantIds == null)
            {
                _context.Activities.Update(activity);
                await _context.SaveChangesAsync();
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
            }
        }

        public async Task DeleteSingleActivityAsync(int id)
        {
            var activity = await _context.Activities.FindAsync(id);

            if (activity != null)
            {
                _context.Activities.Remove(activity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteActivitesAsync(IEnumerable<int> activitiesIds)
        {
            List<Activity> activitiesToDelete = await _context.Activities.Where(a => activitiesIds.Contains(a.ActivityId)).ToListAsync();

            _context.Activities.RemoveRange(activitiesToDelete);  

            await _context.SaveChangesAsync();                                                  
        }
    }
}