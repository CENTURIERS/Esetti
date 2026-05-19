using Esseti.Data;
using Esseti.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models.Users;
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
    }
}