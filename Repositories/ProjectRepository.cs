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
    /// <summary>
    /// Repozytorium do zarządzania projektami realizowanymi przez koło.
    /// Realizuje operacje bazodanowe na tabeli Projects za pomocą EF Core z użyciem cache'owania.
    /// </summary>
    public class ProjectRepository : IProjectRepository
    {
        private readonly EssetiDbContext _context;
        private readonly ICacheService _cacheService;

        /// <summary>
        /// Konstruktor repozytorium projektów.
        /// Wstrzykuje kontekst bazy danych i serwis do obsługi cache'u.
        /// </summary>
        /// <param name="context">Kontekst bazy danych EF Core.</param>
        /// <param name="cacheService">Serwis cache'u.</param>
        public ProjectRepository(EssetiDbContext context, ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        /// <inheritdoc />
        public async Task<List<Project>> GetAllProjectsAsync()
        {
            return await _cacheService.GetOrLoadAsync("projects_all", () => _context.Projects
                .Include(p => p.PersonInCharge)
                .Where(p => p.IsActive)
                .ToListAsync());
        }

        /// <inheritdoc />
        public async Task<Project?> GetProjectByIdAsync(int id)
        {
            return await _context.Projects
                .Include(p => p.PersonInCharge)
                .Include(p => p.Participants)
                .Where(p => p.IsActive && p.ProjectId == id)
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc />
        public async Task AddProjectAsync(Project project)
        {
            project.IsActive = true;
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
            _cacheService.Invalidate("projects_all");
        }

        /// <inheritdoc />
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
            _cacheService.Invalidate("projects_all");
        }

        /// <inheritdoc />
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
            _cacheService.Invalidate("projects_all");
        }

        /// <inheritdoc />
        public async Task DeleteSingleProjectAsync(int id)
        {
            var project = await _context.Projects.FindAsync(id);

            if (project != null)
            {
                project.IsActive = false;
                await _context.SaveChangesAsync();
                _context.ChangeTracker.Clear();
                _cacheService.Invalidate("projects_all");
            }
        }

        /// <inheritdoc />
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
            _cacheService.Invalidate("projects_all");
        }
    }
}

