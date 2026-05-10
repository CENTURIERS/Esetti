using Models.Users;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Esseti.Repositories.Interfaces
{
    public interface IMemberRepository
    {
        Task<List<Member>> GetAllMembersAsync();
    
        Task DeleteSingleMemberAsync(int id);

        Task DeleteMembersAsync(IEnumerable<int> memberIds);

        Task AddMemberAsync(Member member);
    }
}
