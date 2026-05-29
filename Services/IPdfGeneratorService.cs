using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esseti.Services
{
    public interface IPdfGeneratorService
    {
        Task GenerateActivityCertificateAsync(
            string outputPath,
            byte[] logoBytes,
            string clubName,
            string academicYear,
            string fromDateStr,
            string toDateStr,
            List<(Models.Users.Member Member, List<string> Achievements, string Dept)> membersData);

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


