using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Esseti.Data;
using Esseti.Repositories.Interfaces;
using Esseti.Services;
using Models.ClubBase;
using Models.Users;
using Models.Activities;
using Models.Other;
using Models.University;

namespace Esseti.Repositories
{
    public class ClubRepository : IClubRepository
    {
        private readonly EssetiDbContext _context;
        private readonly ICacheService _cacheService;

        public ClubRepository(EssetiDbContext context, ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<ClubInfo?> GetClubInfoAsync()
        {
            return await _cacheService.GetOrLoadAsync("club_info", async () =>
            {
                var club = await _context.Clubs
                    .Include(c => c.Department)
                        .ThenInclude(d => d!.College)
                    .FirstOrDefaultAsync();
                if (club == null)
                {
                    var college = await _context.Colleges.FirstOrDefaultAsync();
                    if (college == null)
                    {
                        college = new College
                        {
                            Name = "Uniwersytet Rzeszowski",
                            NameShort = "UR",
                            AddressLine = "Al. Tadeusza Rejtana 16C",
                            PostalCode = "35-959",
                            City = "RzeszĂłw",
                            Phone = "+48 17 872 10 00"
                        };
                        _context.Colleges.Add(college);
                        await _context.SaveChangesAsync();
                    }
                    else if (string.IsNullOrEmpty(college.AddressLine))
                    {
                        college.AddressLine = "Al. Tadeusza Rejtana 16C";
                        college.PostalCode = "35-959";
                        college.City = "RzeszĂłw";
                        college.Phone = "+48 17 872 10 00";
                        await _context.SaveChangesAsync();
                    }

                    var dept = new CollegeDepartment
                    {
                        Name = "WydziaĹ‚ Nauk ĹšcisĹ‚ych i Technicznych",
                        CollegeId = college.CollegeId
                    };
                    _context.CollegeDepartments.Add(dept);
                    await _context.SaveChangesAsync();

                    club = new ClubInfo
                    {
                        Name = "KoĹ‚o Naukowe InformatykĂłw KNI",
                        ClubRoom = "Sala 333 B2, ul. Pigonia 1",
                        DepartmentId = dept.CollegeDepartmentId,
                        SupervisorName = "dr inĹĽ. Marcin Ochab",
                        MeetingsSchedule = "PoniedziaĹ‚ki w tygodniu B, godzina 18:00, sala 333 B2",
                        ShortName = "KNI"
                    };
                    _context.Clubs.Add(club);
                    await _context.SaveChangesAsync();
                }
                if (club != null)
                {
                    bool changed = false;
                    if (club.Department?.College != null && string.IsNullOrEmpty(club.Department.College.AddressLine))
                    {
                        club.Department.College.AddressLine = "Al. Tadeusza Rejtana 16C";
                        club.Department.College.PostalCode = "35-959";
                        club.Department.College.City = "RzeszĂłw";
                        club.Department.College.Phone = "+48 17 872 10 00";
                        changed = true;
                    }
                    if (string.IsNullOrEmpty(club.SupervisorName))
                    {
                        club.SupervisorName = "dr inĹĽ. Marcin Ochab";
                        changed = true;
                    }
                    if (string.IsNullOrEmpty(club.MeetingsSchedule))
                    {
                        club.MeetingsSchedule = "PoniedziaĹ‚ki w tygodniu B, godzina 18:00, sala 333 B2";
                        changed = true;
                    }
                    if (string.IsNullOrEmpty(club.ShortName))
                    {
                        club.ShortName = "KNI";
                        changed = true;
                    }
                    if (changed)
                    {
                        await _context.SaveChangesAsync();
                    }
                }
                return club;
            });
        }

        public async Task<int> GetMembersCountAsync()
        {
            return await _context.Members.CountAsync();
        }

        public async Task<int> GetProjectsCountAsync()
        {
            return await _context.Projects.CountAsync();
        }

        public async Task<int> GetSectionsCountAsync()
        {
            return await _context.Sections.CountAsync();
        }

        public async Task<int> GetActivitiesCountAsync()
        {
            return await _context.Activities.CountAsync();
        }

        public async Task<List<Section>> GetSectionsAsync()
        {
            return await _cacheService.GetOrLoadAsync("sections_all", () => _context.Sections.ToListAsync());
        }
 
        public async Task<List<Member>> GetBoardMembersAsync()
        {
            return await _cacheService.GetOrLoadAsync("board_members", () => _context.Members
                .Include(m => m.AuthorityRole)
                .Where(m => m.AuthorityRole != null && m.AuthorityRole.Name != "CzĹ‚onek" && m.AuthorityRole.Name != "czĹ‚onek")
                .ToListAsync());
        }
 
        public async Task<List<Trip>> GetTripsAsync()
        {
            return await _cacheService.GetOrLoadAsync("trips_all", () => _context.Trips.ToListAsync());
        }

        public async Task UpdateClubInfoAsync(string clubName, string clubRoom, string departmentName, string supervisorName, string meetingsSchedule, string shortName, byte[]? clubPhoto)
        {
            var club = await GetClubInfoAsync();
            if (club != null)
            {
                club.Name = clubName;
                club.ClubRoom = clubRoom;
                club.SupervisorName = supervisorName;
                club.MeetingsSchedule = meetingsSchedule;
                club.ShortName = shortName;
                club.ClubPhoto = clubPhoto;
                if (club.Department != null)
                {
                    club.Department.Name = departmentName;
                }
                await _context.SaveChangesAsync();
                _cacheService.Invalidate("club_info");
                _cacheService.Invalidate("sections_all");
            }
        }
        public async Task AddSectionAsync(Section section)
        {
            _context.Sections.Add(section);
            await _context.SaveChangesAsync();
            _cacheService.Invalidate("club_info");
            _cacheService.Invalidate("sections_all");
        }
 
        public async Task DeleteSectionAsync(int sectionId)
        {
            var section = await _context.Sections.FindAsync(sectionId);
            if (section != null)
            {
                _context.Sections.Remove(section);
                await _context.SaveChangesAsync();
                _cacheService.Invalidate("club_info");
                _cacheService.Invalidate("sections_all");
            }
        }
    }
}


