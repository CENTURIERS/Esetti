using Esseti.Data;
using Esseti.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models.Users;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    }
}