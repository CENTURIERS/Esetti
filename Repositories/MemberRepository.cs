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
                .Include(m => m.Account)
                .Include(m => m.AuthorityRole)
                .Include(m => m.MemberClubs)
                    .ThenInclude(mc => mc.Club)
                        .ThenInclude(c => c.Department)
                .ToListAsync();
        }

        public async Task DeleteSingleMemberAsync(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member != null)
            {
                member.IsActive = false;
                await _context.SaveChangesAsync();
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
        }
    }
}