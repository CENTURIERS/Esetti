using Esseti.Data;
using Esseti.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models.Users;
using Models.ClubBase;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Esseti.Repositories
{
    public class MemberRepository : IMemberRepository
    {
        private readonly EssetiDbContext _context;

        public MemberRepository(EssetiDbContext context)
        {
            _context = context;
        }

        public async Task<List<Member>> GetAllMembersAsync()
        {
            return await _context.Members
                .Where(m => m.IsActive)
                .Include(m => m.Account)
                .Include(m => m.AuthorityRole)
                .Include(m => m.MemberClubs)
                    .ThenInclude(mc => mc.Club)
                        .ThenInclude(c => c!.Department)
                .ToListAsync();
        }

        public async Task DeleteSingleMemberAsync(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member != null)
            {
                member.IsActive = false;

                _context.Members.Update(member);

                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteMembersAsync(IEnumerable<int> memberIds)
        {
            var members = await _context.Members.Where(m => memberIds.Contains(m.MemberId)).ToListAsync();
            foreach (var member in members)
            {
                member.IsActive = false;

                _context.Members.Update(member);

            }
            await _context.SaveChangesAsync();
        }

        public async Task AddMemberAsync(Member member)
        {
            _context.Members.Add(member);
            await _context.SaveChangesAsync();
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
            }
        }
    }
}