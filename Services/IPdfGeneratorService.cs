using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esseti.Services
{
    /// <summary>
    /// Interfejs dla serwisu generującego dokumenty PDF.
    /// </summary>
    public interface IPdfGeneratorService
    {
        /// <summary>
        /// Generuje certyfikat/zaświadczenie o działalności dla członków koła.
        /// </summary>
        /// <param name="outputPath">Ścieżka do zapisu pliku PDF.</param>
        /// <param name="logoBytes">Logo koła lub uczelni w formie bajtów.</param>
        /// <param name="clubName">Nazwa koła naukowego.</param>
        /// <param name="academicYear">Rok akademicki, np. 2025/2026.</param>
        /// <param name="fromDateStr">Data początkowa działalności.</param>
        /// <param name="toDateStr">Data końcowa działalności.</param>
        /// <param name="membersData">Lista z danymi członków, ich osiągnięciami i wydziałem.</param>
        Task GenerateActivityCertificateAsync(
            string outputPath,
            byte[] logoBytes,
            string clubName,
            string academicYear,
            string fromDateStr,
            string toDateStr,
            List<(Models.Users.Member Member, List<string> Achievements, string Dept)> membersData);

        /// <summary>
        /// Generuje oficjalną listę członków koła naukowego w formacie PDF.
        /// </summary>
        /// <param name="outputPath">Ścieżka do zapisu pliku PDF.</param>
        /// <param name="logoBytes">Logo koła lub uczelni.</param>
        /// <param name="clubName">Nazwa koła naukowego.</param>
        /// <param name="supervisor">Imię i nazwisko opiekuna koła.</param>
        /// <param name="supervisorEmail">Email opiekuna.</param>
        /// <param name="supervisorPhone">Telefon opiekuna.</param>
        /// <param name="zarzadList">Członkowie wchodzący w skład zarządu.</param>
        /// <param name="zwykliCzlonkowie">Pozostali członkowie koła.</param>
        Task GenerateMembersListAsync(
            string outputPath,
            byte[] logoBytes,
            string clubName,
            string supervisor,
            string supervisorEmail,
            string supervisorPhone,
            List<Models.Users.Member> zarzadList,
            List<Models.Users.Member> zwykliCzlonkowie);
    }
}


