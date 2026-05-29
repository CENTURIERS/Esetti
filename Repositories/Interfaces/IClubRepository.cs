using System.Collections.Generic;
using System.Threading.Tasks;
using Models.ClubBase;
using Models.Users;
using Models.Activities;
using Models.Other;

namespace Esseti.Repositories.Interfaces
{
    public interface IClubRepository
    {
        Task<ClubInfo?> GetClubInfoAsync();
        Task<int> GetMembersCountAsync();
        Task<int> GetProjectsCountAsync();
        Task<int> GetSectionsCountAsync();
        Task<int> GetActivitiesCountAsync();
        Task<List<Section>> GetSectionsAsync();
        Task<List<Member>> GetBoardMembersAsync();
        Task<List<Trip>> GetTripsAsync();
        Task UpdateClubInfoAsync(string clubName, string clubRoom, string departmentName, string supervisorName, string meetingsSchedule, string shortName, byte[]? clubPhoto);
        Task AddSectionAsync(Section section);
        Task DeleteSectionAsync(int sectionId);
    }
}
