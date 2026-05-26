using Esseti.Data;
using Esseti.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models.Activities;
using System.Collections.Generic;
using System.Linq;
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
                .Where(p => p.IsActive)
                .ToListAsync();
        }

        public async Task<Project?> GetProjectByIdAsync(int id)
        {
            return await _context.Projects
                .Include(p => p.PersonInCharge)
                .Include(p => p.Participants)
                .Where(p => p.IsActive && p.ProjectId == id)
                .FirstOrDefaultAsync();
        }

        public async Task AddProjectAsync(Project project)
        {
            project.IsActive = true;
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
        }

        public async Task UpdateProjectAsync(Project project, IEnumerable<int>? participantIds = null)
        {
            var tracked = await _context.Projects
                .Include(p => p.Participants)
                .FirstOrDefaultAsync(p => p.ProjectId == project.ProjectId);

            if (tracked == null) return;

            tracked.Name = project.Name;
            tracked.Description = project.Description;
            tracked.AdditionalInformation = project.AdditionalInformation;
            tracked.Github = project.Github;
            tracked.EstimatedTime = project.EstimatedTime;
            tracked.DateStart = project.DateStart;
            tracked.DateEnd = project.DateEnd;
            tracked.PersonInChargeId = project.PersonInChargeId;

            if (participantIds != null)
            {
                tracked.Participants.Clear();
                var participants = await _context.Members
                    .Where(m => participantIds.Contains(m.MemberId))
                    .ToListAsync();
                foreach (var member in participants)
                {
                    tracked.Participants.Add(member);
                }
            }

            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
        }

        public async Task UpdateProjectParticipantsAsync(int projectId, IEnumerable<int> participantIds)
        {
            var tracked = await _context.Projects
                .Include(p => p.Participants)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);

            if (tracked == null) return;

            tracked.Participants.Clear();
            var participants = await _context.Members
                .Where(m => participantIds.Contains(m.MemberId))
                .ToListAsync();
            foreach (var member in participants)
            {
                tracked.Participants.Add(member);
            }

            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
        }

        public async Task DeleteSingleProjectAsync(int id)
        {
            var project = await _context.Projects.FindAsync(id);

            if (project != null)
            {
                project.IsActive = false;
                await _context.SaveChangesAsync();
                _context.ChangeTracker.Clear();
            }
        }

        public async Task DeleteProjectsAsync(IEnumerable<int> projectsIds)
        {
            var projects = await _context.Projects
                                        .Where(p => projectsIds.Contains(p.ProjectId))
                                        .ToListAsync();

            foreach (var project in projects)
            {
                project.IsActive = false;
            }

            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
        }
    }
}