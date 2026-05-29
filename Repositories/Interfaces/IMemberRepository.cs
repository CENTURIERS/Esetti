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
        
        Task<List<Models.ClubBase.AuthorityRole>> GetAuthorityRolesAsync();
    
        Task DeleteSingleMemberAsync(int id);

        Task DeleteMembersAsync(IEnumerable<int> memberIds);

        Task AddMemberAsync(Member member, int? departmentId);

        Task<Member?> GetMemberByIdAsync(int id);

        Task UpdateMemberAsync(Member member, List<int> remainingProjectIds, List<int> remainingActivityIds);

        Task UpdateMemberBasicInfoAsync(Member member, int? departmentId);

        Task UpdateMemberAvatarAsync(int memberId, byte[] avatarData);

        Task<List<Models.University.CollegeDepartment>> GetCollegeDepartmentsAsync();
    }
}


