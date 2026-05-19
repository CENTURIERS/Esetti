using Esseti.Data;
using Esseti.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models.Activities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esseti.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly EssetiDbContext _context;

        public ProjectRepository(EssetiDbContext context)
        {
            _context = context;
        }

        public async Task<List<Project>> GetAllProjectsAsync()
        {
            return await _context.Projects
                .Include(p => p.PersonInCharge)
                .Include(p => p.Participants)
                .Include(p => p.Sections)
                .Include(p => p.Clubs)
                .ToListAsync();
        }
    }
}