using System.Collections.Generic;
using System.Threading.Tasks;
using Models.ClubBase;
using Models.Users;
using Models.Activities;
using Models.Other;

namespace Esseti.Repositories.Interfaces
{
    /// <summary>
    /// Interfejs repozytorium do zarządzania ogólnymi informacjami o kole naukowym (członkowie, sekcje, projekty itp.).
    /// </summary>
    public interface IClubRepository
    {
        /// <summary>
        /// Pobiera szczegółowe informacje o kole naukowym.
        /// </summary>
        Task<ClubInfo?> GetClubInfoAsync();

        /// <summary>
        /// Pobiera liczbę członków przypisanych do koła.
        /// </summary>
        Task<int> GetMembersCountAsync();

        /// <summary>
        /// Pobiera całkowitą liczbę projektów.
        /// </summary>
        Task<int> GetProjectsCountAsync();

        /// <summary>
        /// Pobiera całkowitą liczbę sekcji.
        /// </summary>
        Task<int> GetSectionsCountAsync();

        /// <summary>
        /// Pobiera liczbę powiązanych aktywności.
        /// </summary>
        Task<int> GetActivitiesCountAsync();

        /// <summary>
        /// Pobiera listę wszystkich sekcji naukowych.
        /// </summary>
        Task<List<Section>> GetSectionsAsync();

        /// <summary>
        /// Pobiera listę członków zarządu koła.
        /// </summary>
        Task<List<Member>> GetBoardMembersAsync();

        /// <summary>
        /// Pobiera listę wyjazdów.
        /// </summary>
        Task<List<Trip>> GetTripsAsync();

        /// <summary>
        /// Aktualizuje podstawowe dane koła naukowego oraz opcjonalnie logo.
        /// </summary>
        Task UpdateClubInfoAsync(string clubName, string clubRoom, string departmentName, string supervisorName, string supervisorEmail, string supervisorPhone, string meetingsSchedule, string shortName, byte[]? clubPhoto);

        /// <summary>
        /// Dodaje nową sekcję naukową.
        /// </summary>
        Task AddSectionAsync(Section section);

        /// <summary>
        /// Usuwa sekcję naukową na podstawie ID.
        /// </summary>
        Task DeleteSectionAsync(int sectionId);
    }
}


