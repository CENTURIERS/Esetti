using Esseti.Data;
using Esseti.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models.Activities;
using System.Collections.Generic;
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
    }
}