using Esseti.Data;
using Esseti.Repositories.Interfaces;
using Esseti.Services;
using Microsoft.EntityFrameworkCore;
using Models.Users;
using Models.ClubBase;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace Esseti.Repositories
{
    public class MemberRepository : IMemberRepository
    {
        private readonly EssetiDbContext _context;
        private readonly ICacheService _cacheService;

        public MemberRepository(EssetiDbContext context, ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<List<Member>> GetAllMembersAsync()
        {
            return await _cacheService.GetOrLoadAsync("members_all", () => _context.Members
                .Where(m => m.IsActive)
                .Include(m => m.Account)
                .Include(m => m.AuthorityRole)
                .Include(m => m.MemberClubs)
                    .ThenInclude(mc => mc.Club)
                        .ThenInclude(c => c!.Department)
                .ToListAsync());
        }

        public async Task<List<AuthorityRole>> GetAuthorityRolesAsync()
        {
            return await _cacheService.GetOrLoadAsync("authority_roles", () => _context.AuthorityRoles.ToListAsync());
        }

        public async Task DeleteSingleMemberAsync(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member != null)
            {
                member.IsActive = false;
                await _context.SaveChangesAsync();
                _cacheService.Invalidate("members_all");
                _cacheService.Invalidate("board_members");
            }
        }

        public async Task DeleteMembersAsync(IEnumerable<int> memberIds)
        {
            var members = await _context.Members.Where(m => memberIds.Contains(m.MemberId)).ToListAsync();
            foreach (var member in members)
            {
                member.IsActive = false;
            }
            await _context.SaveChangesAsync();
            _cacheService.Invalidate("members_all");
            _cacheService.Invalidate("board_members");
        }

        public async Task AddMemberAsync(Member member, int? departmentId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                Member? existingMember = null;
                if (!string.IsNullOrWhiteSpace(member.IndexNumber))
                {
                    existingMember = await _context.Members
                        .Include(m => m.Account)
                        .Include(m => m.MemberClubs)
                        .FirstOrDefaultAsync(m => m.IndexNumber == member.IndexNumber);
                }

                if (existingMember != null)
                {
                    existingMember.IsActive = true;
                    existingMember.FirstName = member.FirstName;
                    existingMember.LastName = member.LastName;
                    existingMember.PhoneNumber = member.PhoneNumber;
                    existingMember.Major = member.Major;
                    existingMember.Description = member.Description;
                    existingMember.MemberAvatar = member.MemberAvatar;
                    existingMember.RoleId = member.RoleId;
                    existingMember.JoinDate = DateTime.Now;

                    if (member.Account != null)
                    {
                        if (existingMember.Account != null)
                        {
                            existingMember.Account.Email = member.Account.Email;
                        }
                        else
                        {
                            existingMember.Account = new UserAccount
                            {
                                Email = member.Account.Email,
                                SystemRole = Models.Enums.SystemRole.User
                            };
                        }
                    }

                    if (departmentId.HasValue)
                    {
                        var club = await _context.Clubs.FirstOrDefaultAsync(c => c.DepartmentId == departmentId.Value);
                        if (club == null)
                        {
                            var dept = await _context.CollegeDepartments.FindAsync(departmentId.Value);
                            if (dept != null)
                            {
                                club = new ClubInfo
                                {
                                    Name = $"Koło Naukowe - {dept.Name}",
                                    DepartmentId = dept.CollegeDepartmentId,
                                    ShortName = dept.Name.Split(' ').LastOrDefault() ?? "KN"
                                };
                                _context.Clubs.Add(club);
                                await _context.SaveChangesAsync();
                            }
                        }
                        if (club != null)
                        {
                            if (existingMember.MemberClubs == null)
                            {
                                existingMember.MemberClubs = new List<MemberClub>();
                            }
                            if (!existingMember.MemberClubs.Any(mc => mc.ClubId == club.ClubId))
                            {
                                existingMember.MemberClubs.Add(new MemberClub { ClubId = club.ClubId, MemberId = existingMember.MemberId });
                            }
                        }
                    }
                }
                else
                {
                    if (departmentId.HasValue)
                    {
                        var club = await _context.Clubs.FirstOrDefaultAsync(c => c.DepartmentId == departmentId.Value);
                        if (club == null)
                        {
                            var dept = await _context.CollegeDepartments.FindAsync(departmentId.Value);
                            if (dept != null)
                            {
                                club = new ClubInfo
                                {
                                    Name = $"Koło Naukowe - {dept.Name}",
                                    DepartmentId = dept.CollegeDepartmentId,
                                    ShortName = dept.Name.Split(' ').LastOrDefault() ?? "KN"
                                };
                                _context.Clubs.Add(club);
                                await _context.SaveChangesAsync();
                            }
                        }
                        if (club != null)
                        {
                            member.MemberClubs = new List<MemberClub>
                            {
                                new MemberClub { ClubId = club.ClubId }
                            };
                        }
                    }
                    _context.Members.Add(member);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _cacheService.Invalidate("members_all");
                _cacheService.Invalidate("board_members");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task<Member?> GetMemberByIdAsync(int id)
        {
            return await _context.Members
                .Where(m => m.MemberId == id)
                .Include(m => m.Account)
                .Include(m => m.AuthorityRole)
                .Include(m => m.MemberClubs)
                    .ThenInclude(mc => mc.Club)
                        .ThenInclude(c => c!.Department)
                            .ThenInclude(d => d!.College)
                .Include(m => m.Activities)
                .Include(m => m.Projects)
                    .ThenInclude(p => p.PersonInCharge)
                .Include(m => m.Projects)
                    .ThenInclude(p => p.Participants)
                .Include(m => m.Projects)
                    .ThenInclude(p => p.Sections)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateMemberAsync(Member member, List<int> remainingProjectIds, List<int> remainingActivityIds) 
        {
            var dbMember = await _context.Members
                .Include(m => m.Account)
                .Include(m => m.AuthorityRole)
                .Include(m => m.Projects)   
                .Include(m => m.Activities) 
                .FirstOrDefaultAsync(m => m.MemberId == member.MemberId);

            if (dbMember != null)
            {
                dbMember.FirstName = member.FirstName;
                dbMember.LastName = member.LastName;
                dbMember.PhoneNumber = member.PhoneNumber;
                dbMember.IndexNumber = member.IndexNumber;
                dbMember.Major = member.Major;
                dbMember.Description = member.Description;

                if (member.Account != null)
                {
                    if (dbMember.Account != null)
                    {
                        dbMember.Account.Email = member.Account.Email;
                    }
                    else
                    {
                        dbMember.Account = new UserAccount
                        {
                            Email = member.Account.Email,
                            SystemRole = Models.Enums.SystemRole.User
                        };
                    }
                }
                else
                {
                    dbMember.Account = null;
                }

                if (member.AuthorityRole != null)
                {
                    var roleName = member.AuthorityRole.Name;
                    var dbRole = await _context.AuthorityRoles.FirstOrDefaultAsync(r => r.Name.ToLower() == roleName.ToLower());
                    if (dbRole != null)
                    {
                        dbMember.RoleId = dbRole.RoleId;
                        dbMember.AuthorityRole = dbRole;
                    }
                    else
                    {
                        var newRole = new AuthorityRole { Name = roleName };
                        _context.AuthorityRoles.Add(newRole);
                        dbMember.AuthorityRole = newRole;
                    }
                }

                if (dbMember.Projects != null)
                {
                    var projectsToRemove = dbMember.Projects
                        .Where(p => !remainingProjectIds.Contains(p.ProjectId))
                        .ToList();
                    foreach (var p in projectsToRemove)
                    {
                        dbMember.Projects.Remove(p); 
                    }
                } 
                
                if (dbMember.Activities != null)
                {
                    var activitiesToRemove = dbMember.Activities
                        .Where(a => !remainingActivityIds.Contains(a.ActivityId))
                        .ToList();
                    foreach (var a in activitiesToRemove)
                    {
                        dbMember.Activities.Remove(a); 
                    }
                }

                await _context.SaveChangesAsync();
                _cacheService.Invalidate("members_all");
                _cacheService.Invalidate("board_members");
            }
        }

        public async Task UpdateMemberBasicInfoAsync(Member member, int? departmentId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var dbMember = await _context.Members
                    .Include(m => m.Account)
                    .Include(m => m.AuthorityRole)
                    .Include(m => m.MemberClubs)
                    .FirstOrDefaultAsync(m => m.MemberId == member.MemberId);

                if (dbMember != null)
                {
                    dbMember.FirstName = member.FirstName;
                    dbMember.LastName = member.LastName;
                    dbMember.PhoneNumber = member.PhoneNumber;
                    dbMember.IndexNumber = member.IndexNumber;
                    dbMember.Major = member.Major;
                    dbMember.Description = member.Description;
                    
                    if (member.MemberAvatar != null)
                        dbMember.MemberAvatar = member.MemberAvatar;

                    if (member.Account != null)
                    {
                        if (dbMember.Account != null)
                        {
                            dbMember.Account.Email = member.Account.Email;
                        }
                        else
                        {
                            dbMember.Account = new UserAccount
                            {
                                Email = member.Account.Email,
                                SystemRole = Models.Enums.SystemRole.User
                            };
                        }
                    }
                    else
                    {
                        dbMember.Account = null;
                    }

                    if (member.AuthorityRole != null)
                    {
                        var roleName = member.AuthorityRole.Name;
                        var dbRole = await _context.AuthorityRoles.FirstOrDefaultAsync(r => r.Name.ToLower() == roleName.ToLower());
                        if (dbRole != null)
                        {
                            dbMember.RoleId = dbRole.RoleId;
                            dbMember.AuthorityRole = dbRole;
                        }
                    }

                    if (departmentId.HasValue)
                    {
                        var club = await _context.Clubs.FirstOrDefaultAsync(c => c.DepartmentId == departmentId.Value);
                        if (club == null)
                        {
                            var dept = await _context.CollegeDepartments.FindAsync(departmentId.Value);
                            if (dept != null)
                            {
                                club = new ClubInfo
                                {
                                    Name = $"Koło Naukowe - {dept.Name}",
                                    DepartmentId = dept.CollegeDepartmentId,
                                    ShortName = dept.Name.Split(' ').LastOrDefault() ?? "KN"
                                };
                                _context.Clubs.Add(club);
                                await _context.SaveChangesAsync();
                            }
                        }
                        if (club != null)
                        {
                            dbMember.MemberClubs ??= new List<MemberClub>();
                            dbMember.MemberClubs.Clear();
                            dbMember.MemberClubs.Add(new MemberClub { ClubId = club.ClubId, MemberId = dbMember.MemberId });
                        }
                    }
                    else if (member.MemberClubs != null && member.MemberClubs.Any())
                    {
                        var targetClubId = member.MemberClubs.First().ClubId;
                        dbMember.MemberClubs ??= new List<MemberClub>();
                        dbMember.MemberClubs.Clear();
                        dbMember.MemberClubs.Add(new MemberClub { ClubId = targetClubId, MemberId = dbMember.MemberId });
                    }

                    await _context.SaveChangesAsync();
                }
                await transaction.CommitAsync();
                _cacheService.Invalidate("members_all");
                _cacheService.Invalidate("board_members");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateMemberAvatarAsync(int memberId, byte[] avatarData)
        {
            var member = await _context.Members.FindAsync(memberId);
            if (member != null)
            {
                member.MemberAvatar = avatarData;
                await _context.SaveChangesAsync();
                _cacheService.Invalidate("members_all");
                _cacheService.Invalidate("board_members");
            }
        }

        public async Task<List<Models.University.CollegeDepartment>> GetCollegeDepartmentsAsync()
        {
            return await _cacheService.GetOrLoadAsync("departments", () => _context.CollegeDepartments.ToListAsync());
        }
    }
}

