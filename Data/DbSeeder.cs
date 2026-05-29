using Esseti.Data;
using Microsoft.EntityFrameworkCore;
using Models.Activities;
using Models.ClubBase;
using Models.Enums;
using Models.Other;
using Models.University;
using Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Esseti.Data
{
    
    
    
    
    public static class DbSeeder
    {
        public static async Task SeedAsync(EssetiDbContext context)
        {
            // Skip if database already has data
            if (await context.AuthorityRoles.AnyAsync())
                return;

            await SeedAuthorityRolesAsync(context);
            await SeedUniversityStructureAsync(context);
            await SeedClubInfoAsync(context);
            await SeedAccountsAndMembersAsync(context);
            await SeedSectionsAsync(context);
            await SeedProjectsAsync(context);
            await SeedActivitiesAsync(context);
            await SeedTripsAsync(context);
        }

        private static async Task SeedAuthorityRolesAsync(EssetiDbContext context)
        {
            var roles = new List<AuthorityRole>
            {
                new() { RoleId = 1, Name = "ZarzÄ…d", Description = "PeĹ‚ne uprawnienia edycji i zarzÄ…dzania", Permissions = 100 },
                new() { RoleId = 2, Name = "CzĹ‚onek", Description = "Podstawowy dostÄ™p do projektĂłw", Permissions = 10 },
                new() { RoleId = 3, Name = "Skarbnik", Description = "ZarzÄ…dzanie finansami i skĹ‚adkami", Permissions = 80 },
                new() { RoleId = 4, Name = "Sekretarz", Description = "Dokumentacja i protokoĹ‚y", Permissions = 70 },
                new() { RoleId = 5, Name = "Sympatyk", Description = "Osoba biorÄ…ca udziaĹ‚ w eventach, bez gĹ‚osu", Permissions = 5 },
            };

            context.AuthorityRoles.AddRange(roles);
            await context.SaveChangesAsync();
        }

        private static async Task SeedUniversityStructureAsync(EssetiDbContext context)
        {
            var college = new College
            {
                CollegeId = 1,
                Name = "Uniwersytet Rzeszowski",
                NameShort = "UR",
                AddressLine = "al. Rejtana 16c",
                City = "RzeszĂłw",
                PostalCode = "35-959",
                Phone = "17 872 10 00",
                NIP = "8130024724"
            };
            context.Colleges.Add(college);
            await context.SaveChangesAsync();

            var department = new CollegeDepartment
            {
                CollegeDepartmentId = 1,
                CollegeId = 1,
                Name = "WydziaĹ‚ Nauk ĹšcisĹ‚ych i Technicznych",
                AddressLine = "ul. Pigonia 1",
                City = "RzeszĂłw",
                PostalCode = "35-310",
                Phone = "17 872 12 00",
                Email = "wnst@ur.edu.pl"
            };
            context.CollegeDepartments.Add(department);
            await context.SaveChangesAsync();
        }

        private static async Task SeedClubInfoAsync(EssetiDbContext context)
        {
            var club = new ClubInfo
            {
                ClubId = 1,
                Name = "KN .NET & Avalonia",
                DepartmentId = 1,
                ClubRoom = "Budynek F, pokĂłj 102",
                SupervisorName = "dr hab. inĹĽ. Jan Kowalski",
                MeetingsSchedule = "PoniedziaĹ‚ki o 17:00",
                ShortName = "KNA"
            };
            context.Clubs.Add(club);
            await context.SaveChangesAsync();
        }

        private static async Task SeedAccountsAndMembersAsync(EssetiDbContext context)
        {
            // User accounts
            var accounts = new List<UserAccount>
            {
                new() { AccountId = 1, Email = "admin@admin.pl", PasswordHash = "haslo123", SystemRole = SystemRole.SuperAdmin, IsVerified = true, CreatedAt = new DateTime(2026, 4, 1), LastLogin = new DateTime(2026, 5, 28), UpdatedAt = new DateTime(2026, 5, 28) },
                new() { AccountId = 2, Email = "m.kowalski@stud.pl", PasswordHash = "hash1", SystemRole = SystemRole.User, IsVerified = true, CreatedAt = new DateTime(2026, 3, 1), UpdatedAt = new DateTime(2026, 3, 1) },
                new() { AccountId = 3, Email = "j.nowak@stud.pl", PasswordHash = "hash2", SystemRole = SystemRole.User, IsVerified = true, CreatedAt = new DateTime(2026, 3, 2), UpdatedAt = new DateTime(2026, 3, 2) },
                new() { AccountId = 4, Email = "a.wisniewska@stud.pl", PasswordHash = "hash3", SystemRole = SystemRole.User, IsVerified = true, CreatedAt = new DateTime(2026, 3, 3), UpdatedAt = new DateTime(2026, 3, 3) },
                new() { AccountId = 5, Email = "p.wojcik@stud.pl", PasswordHash = "hash4", SystemRole = SystemRole.User, IsVerified = true, CreatedAt = new DateTime(2026, 3, 4), UpdatedAt = new DateTime(2026, 3, 4) },
                new() { AccountId = 6, Email = "k.lewandowska@stud.pl", PasswordHash = "hash5", SystemRole = SystemRole.User, IsVerified = true, CreatedAt = new DateTime(2026, 3, 5), UpdatedAt = new DateTime(2026, 3, 5) },
                new() { AccountId = 7, Email = "m.dabrowski@stud.pl", PasswordHash = "hash6", SystemRole = SystemRole.User, IsVerified = true, CreatedAt = new DateTime(2026, 3, 6), UpdatedAt = new DateTime(2026, 3, 6) },
                new() { AccountId = 8, Email = "k.mazur@stud.pl", PasswordHash = "hash7", SystemRole = SystemRole.User, IsVerified = true, CreatedAt = new DateTime(2026, 3, 7), UpdatedAt = new DateTime(2026, 3, 7) },
                new() { AccountId = 9, Email = "m.krol@stud.pl", PasswordHash = "hash8", SystemRole = SystemRole.User, IsVerified = true, CreatedAt = new DateTime(2026, 3, 8), UpdatedAt = new DateTime(2026, 3, 8) },
                new() { AccountId = 10, Email = "z.wieczorek@stud.pl", PasswordHash = "hash9", SystemRole = SystemRole.User, IsVerified = true, CreatedAt = new DateTime(2026, 3, 9), UpdatedAt = new DateTime(2026, 3, 9) },
                new() { AccountId = 11, Email = "m.adamczyk@stud.pl", PasswordHash = "hash10", SystemRole = SystemRole.User, IsVerified = true, CreatedAt = new DateTime(2026, 3, 10), UpdatedAt = new DateTime(2026, 3, 10) },
                new() { AccountId = 12, Email = "j.dudek@stud.pl", PasswordHash = "hash11", SystemRole = SystemRole.User, IsVerified = true, CreatedAt = new DateTime(2026, 3, 11), UpdatedAt = new DateTime(2026, 3, 11) },
                new() { AccountId = 13, Email = "a.stepien@stud.pl", PasswordHash = "hash12", SystemRole = SystemRole.User, IsVerified = true, CreatedAt = new DateTime(2026, 3, 12), UpdatedAt = new DateTime(2026, 3, 12) },
                new() { AccountId = 14, Email = "m.pawlak@stud.pl", PasswordHash = "hash13", SystemRole = SystemRole.User, IsVerified = true, CreatedAt = new DateTime(2026, 3, 13), UpdatedAt = new DateTime(2026, 3, 13) },
                new() { AccountId = 15, Email = "p.sikora@stud.pl", PasswordHash = "hash14", SystemRole = SystemRole.User, IsVerified = true, CreatedAt = new DateTime(2026, 3, 14), UpdatedAt = new DateTime(2026, 3, 14) },
                new() { AccountId = 16, Email = "m.walczak@stud.pl", PasswordHash = "hash15", SystemRole = SystemRole.User, IsVerified = true, CreatedAt = new DateTime(2026, 3, 15), UpdatedAt = new DateTime(2026, 3, 15) },
            };
            context.UserAccounts.AddRange(accounts);
            await context.SaveChangesAsync();

            // Members
            var members = new List<Member>
            {
                new() { MemberId = 1, AccountId = 1, RoleId = 1, IndexNumber = "123456", FirstName = "Kacper", LastName = "RÄ™czak", Major = "Informatyka Stosowana", PhoneNumber = "555-123-456", Description = "Jestem pasjonatem programowania, C# i frameworka Avalonia. W kole naukowym zajmujÄ™ siÄ™ koordynowaniem najwaĹĽniejszych projektĂłw i budowaniem spoĹ‚ecznoĹ›ci.", IsActive = true, JoinDate = new DateTime(2026, 4, 4) },
                new() { MemberId = 2, AccountId = 2, RoleId = 2, IndexNumber = "100001", FirstName = "Mateusz", LastName = "Kowalski", Major = "Informatyka", PhoneNumber = "555-000-001", Description = "Interesuje siÄ™ tworzeniem gier 2D oraz projektowaniem silnikĂłw graficznych w Unity.", IsActive = true, JoinDate = new DateTime(2026, 3, 1) },
                new() { MemberId = 3, AccountId = 3, RoleId = 2, IndexNumber = "100002", FirstName = "Julia", LastName = "Nowak", Major = "Automatyka", PhoneNumber = "555-000-002", Description = "Pasjonatka mikrokontrolerĂłw, robotyki i automatyzacji procesĂłw przemysĹ‚owych.", IsActive = true, JoinDate = new DateTime(2026, 3, 2) },
                new() { MemberId = 4, AccountId = 4, RoleId = 2, IndexNumber = "100003", FirstName = "Alicja", LastName = "WiĹ›niewska", Major = "Informatyka", PhoneNumber = "555-000-003", Description = "Specjalizuje siÄ™ w technologiach webowych (ASP.NET Core / React).", IsActive = true, JoinDate = new DateTime(2026, 3, 3) },
                new() { MemberId = 5, AccountId = 5, RoleId = 2, IndexNumber = "100004", FirstName = "Piotr", LastName = "WĂłjcik", Major = "Telekomunikacja", PhoneNumber = "555-000-004", Description = "AnalizujÄ™ bezpieczeĹ„stwo systemĂłw operacyjnych i przeprowadzam testy penetracyjne.", IsActive = true, JoinDate = new DateTime(2026, 3, 4) },
                new() { MemberId = 6, AccountId = 6, RoleId = 2, IndexNumber = "100005", FirstName = "Kasia", LastName = "Lewandowska", Major = "Informatyka", PhoneNumber = "555-000-005", Description = "Zajmuje siÄ™ tworzeniem responsywnych interfejsĂłw uĹĽytkownika i optymalizacjÄ… UX.", IsActive = true, JoinDate = new DateTime(2026, 3, 5) },
                new() { MemberId = 7, AccountId = 7, RoleId = 4, IndexNumber = "100006", FirstName = "MichaĹ‚", LastName = "DÄ…browski", Major = "Mechatronika", PhoneNumber = "555-000-006", Description = "KoordynujÄ™ pracÄ™ nad robotami mobilnymi, zajmujÄ™ siÄ™ projektowaniem CAD.", IsActive = true, JoinDate = new DateTime(2026, 3, 6) },
                new() { MemberId = 8, AccountId = 8, RoleId = 3, IndexNumber = "100007", FirstName = "Karolina", LastName = "Mazur", Major = "Informatyka", PhoneNumber = "555-000-007", Description = "Odpowiedzialna za rozliczanie grantĂłw i budĹĽet koĹ‚a naukowego.", IsActive = true, JoinDate = new DateTime(2026, 3, 7) },
            };
            context.Members.AddRange(members);
            await context.SaveChangesAsync();

            // MemberClub associations
            var memberClubs = new List<MemberClub>
            {
                new() { ClubId = 1, MemberId = 1, ClubRole = ClubRole.President },
                new() { ClubId = 1, MemberId = 2, ClubRole = ClubRole.Member },
                new() { ClubId = 1, MemberId = 3, ClubRole = ClubRole.Member },
                new() { ClubId = 1, MemberId = 4, ClubRole = ClubRole.Member },
                new() { ClubId = 1, MemberId = 5, ClubRole = ClubRole.Member },
                new() { ClubId = 1, MemberId = 6, ClubRole = ClubRole.Member },
                new() { ClubId = 1, MemberId = 7, ClubRole = ClubRole.BoardMember },
                new() { ClubId = 1, MemberId = 8, ClubRole = ClubRole.BoardMember },
            };
            context.MemberClubs.AddRange(memberClubs);
            await context.SaveChangesAsync();

            // account_college
            context.Database.ExecuteSqlRaw("INSERT INTO account_college (account_id, college_id) VALUES (1, 1)");
        }

        private static async Task SeedSectionsAsync(EssetiDbContext context)
        {
            var sections = new List<Section>
            {
                new() { SectionId = 1, Name = "Sekcja Gamedev", ShortName = "Unity", Meetings = "PiÄ…tki 20:00, Online", CreatedAt = new DateTime(2026, 2, 1), IsActive = true },
                new() { SectionId = 2, Name = "Sekcja Robotyk", ShortName = "Robo", Meetings = "Ĺšrody 16:00, Lab 5", CreatedAt = new DateTime(2026, 2, 10), IsActive = true },
                new() { SectionId = 3, Name = "Sekcja Cybersec", ShortName = "Sec", Meetings = "PoniedziaĹ‚ki 19:00, Sala 201", CreatedAt = new DateTime(2026, 2, 20), IsActive = true },
            };
            context.Sections.AddRange(sections);
            await context.SaveChangesAsync();

            var sectionMembers = new List<SectionMember>
            {
                new() { SectionId = 3, MemberId = 5, Role = SectionRole.Chairman },
                new() { SectionId = 3, MemberId = 8, Role = SectionRole.Member },
                new() { SectionId = 1, MemberId = 2, Role = SectionRole.Member },
                new() { SectionId = 2, MemberId = 7, Role = SectionRole.Chairman },
            };
            context.SectionMembers.AddRange(sectionMembers);
            await context.SaveChangesAsync();
        }

        private static async Task SeedProjectsAsync(EssetiDbContext context)
        {
            var projects = new List<Project>
            {
                new() { ProjectId = 1, Name = "Gra RPG 2D", Description = "Projekt klasycznej gry fabularnej w Unity", AdditionalInformation = "UĹĽywamy C# i autorskich grafik", PersonInChargeId = 2, Github = "github.com/kni/rpg-game", EstimatedTime = 120, DateStart = new DateTime(2026, 3, 10), DateEnd = new DateTime(2026, 6, 30), IsActive = true },
                new() { ProjectId = 2, Name = "Skaner Sieciowy", Description = "NarzÄ™dzie CLI dla administratorĂłw do audytu bezpieczeĹ„stwa portĂłw", AdditionalInformation = "Wykorzystuje raw sockets w C#", PersonInChargeId = 5, Github = "github.com/kni/net-scanner", EstimatedTime = 80, DateStart = new DateTime(2026, 3, 12), DateEnd = new DateTime(2026, 5, 31), IsActive = true },
                new() { ProjectId = 3, Name = "Robot KroczÄ…cy", Description = "Projekt pajÄ…ka kroczÄ…cego na mikrokontrolerze ESP32", AdditionalInformation = "Wydruk 3D + silniki servo", PersonInChargeId = 7, Github = "github.com/kni/robo-spider", EstimatedTime = 150, DateStart = new DateTime(2026, 3, 20), DateEnd = new DateTime(2026, 9, 30), IsActive = true },
                new() { ProjectId = 4, Name = "Portal Student", Description = "System uĹ‚atwiajÄ…cy wymianÄ™ notatek studenckich i materiaĹ‚Ăłw dydaktycznych", AdditionalInformation = "ASP.NET Core MVC + PostgreSQL", PersonInChargeId = 3, Github = "github.com/kni/student-portal", EstimatedTime = 90, DateStart = new DateTime(2026, 3, 25), DateEnd = new DateTime(2026, 6, 15), IsActive = true },
                new() { ProjectId = 5, Name = "Aplikacja Mobilna Esetti", Description = "Mobilny klient systemu do zarzÄ…dzania koĹ‚ami naukowymi w .NET MAUI", AdditionalInformation = "WspĂłĹ‚pracuje z API Esetti", PersonInChargeId = 1, Github = "github.com/kni/esetti-mobile", EstimatedTime = 100, DateStart = new DateTime(2026, 4, 1), DateEnd = new DateTime(2026, 8, 31), IsActive = true },
                new() { ProjectId = 6, Name = "System Rezerwacji Sal", Description = "ZarzÄ…dzanie rezerwacjami sal na wydziale nauk Ĺ›cisĹ‚ych", AdditionalInformation = "Projekt integracyjny z API uczelni", PersonInChargeId = 1, Github = "github.com/kni/room-booking", EstimatedTime = 60, DateStart = new DateTime(2026, 4, 10), IsActive = true },
                new() { ProjectId = 7, Name = "Sklep Internetowy", Description = "Projekt platformy e-commerce z pĹ‚atnoĹ›ciami online", AdditionalInformation = "UĹĽywa Stripe API do pĹ‚atnoĹ›ci", PersonInChargeId = 2, Github = "github.com/kni/asp-shop", EstimatedTime = 70, DateStart = new DateTime(2026, 4, 15), IsActive = true },
                new() { ProjectId = 8, Name = "Bot na Discorda", Description = "Bot automatyzujÄ…cy procesy na kanale koĹ‚a (np. powiadomienia)", AdditionalInformation = "Napisany w Discord.Net", PersonInChargeId = 1, Github = "github.com/kni/kni-bot", EstimatedTime = 30, DateStart = new DateTime(2026, 4, 20), DateEnd = new DateTime(2026, 5, 10), IsActive = true },
                new() { ProjectId = 9, Name = "Esetti Cloud", Description = "Zasoby chmurowe i hosting dla projektĂłw realizowanych w kole", AdditionalInformation = "Oparty na kontenerach Docker", PersonInChargeId = 5, Github = "github.com/kni/esetti-cloud", EstimatedTime = 200, DateStart = new DateTime(2026, 5, 1), IsActive = true },
            };
            context.Projects.AddRange(projects);
            await context.SaveChangesAsync();

            // project_sections
            context.Database.ExecuteSqlRaw("INSERT INTO project_sections (project_id, section_id) VALUES (1, 1), (2, 3), (3, 2), (5, 1), (9, 3)");

            // project_club â€” all projects belong to club 1
            context.Database.ExecuteSqlRaw("INSERT INTO project_club (club_id, project_id) VALUES (1, 1), (1, 2), (1, 3), (1, 4), (1, 5), (1, 6), (1, 7), (1, 8), (1, 9)");

            // project_member â€” Kacper (id=1) is in all projects
            context.Database.ExecuteSqlRaw("INSERT INTO project_member (project_id, member_id) SELECT project_id, 1 FROM project");
        }

        private static async Task SeedActivitiesAsync(EssetiDbContext context)
        {
            var activities = new List<Activity>
            {
                new() { ActivityId = 1, Name = "Warsztaty C# & Avalonia", AddressLine = "ul. Pigonia 1", City = "RzeszĂłw", PostalCode = "35-310", Date = new DateTime(2026, 5, 15), Time = new TimeSpan(16, 0, 0), PersonInChargeName = "Kacper RÄ™czak", PersonInChargePhone = "555-123-456", PersonInChargeEmail = "admin@admin.pl", AdditionalInformation = "Wymagany zainstalowany .NET SDK 10", IsRepeatable = false, IsActive = true },
                new() { ActivityId = 2, Name = "Spotkanie ZarzÄ…du #1", AddressLine = "ul. Pigonia 1", City = "RzeszĂłw", PostalCode = "35-310", Date = new DateTime(2026, 5, 1), Time = new TimeSpan(18, 0, 0), PersonInChargeName = "Kacper RÄ™czak", PersonInChargePhone = "555-123-456", PersonInChargeEmail = "admin@admin.pl", AdditionalInformation = "OmĂłwienie celĂłw na nowy semestr", IsRepeatable = false, IsActive = true },
                new() { ActivityId = 3, Name = "Spotkanie ZarzÄ…du #2", AddressLine = "ul. Pigonia 1", City = "RzeszĂłw", PostalCode = "35-310", Date = new DateTime(2026, 5, 8), Time = new TimeSpan(18, 0, 0), PersonInChargeName = "Kacper RÄ™czak", PersonInChargePhone = "555-123-456", PersonInChargeEmail = "admin@admin.pl", AdditionalInformation = "BudĹĽet i zakupy na sekcjÄ™ robotycznÄ…", IsRepeatable = false, IsActive = true },
                new() { ActivityId = 4, Name = "Szkolenie z systemu Git", AddressLine = "ul. Pigonia 1", City = "RzeszĂłw", PostalCode = "35-310", Date = new DateTime(2026, 5, 10), Time = new TimeSpan(16, 0, 0), PersonInChargeName = "Julia Nowak", PersonInChargePhone = "555-000-002", PersonInChargeEmail = "j.nowak@stud.pl", AdditionalInformation = "OmĂłwienie Pull Requests i rozwiÄ…zywania konfliktĂłw", IsRepeatable = false, IsActive = true },
                new() { ActivityId = 5, Name = "Hackathon RzeszĂłw", AddressLine = "Hala Podpromie", City = "RzeszĂłw", PostalCode = "35-310", Date = new DateTime(2026, 5, 12), Time = new TimeSpan(9, 0, 0), PersonInChargeName = "Mateusz Kowalski", PersonInChargePhone = "555-000-001", PersonInChargeEmail = "m.kowalski@stud.pl", AdditionalInformation = "24-godzinny Hackathon programistyczny", IsRepeatable = false, IsActive = true },
                new() { ActivityId = 6, Name = "Targi Pracy IT", AddressLine = "al. Rejtana 16c", City = "RzeszĂłw", PostalCode = "35-959", Date = new DateTime(2026, 5, 18), Time = new TimeSpan(10, 0, 0), PersonInChargeName = "Kacper RÄ™czak", PersonInChargePhone = "555-123-456", PersonInChargeEmail = "admin@admin.pl", AdditionalInformation = "Stoisko naszego KoĹ‚a Naukowego", IsRepeatable = false, IsActive = true },
                new() { ActivityId = 7, Name = "Prelekcja AI w C#", AddressLine = "ul. Pigonia 1", City = "RzeszĂłw", PostalCode = "35-310", Date = new DateTime(2026, 5, 20), Time = new TimeSpan(15, 0, 0), PersonInChargeName = "Kacper RÄ™czak", PersonInChargePhone = "555-123-456", PersonInChargeEmail = "admin@admin.pl", AdditionalInformation = "Prelekcja o bibliotekach ML.NET i integracji z modelami LLM", IsRepeatable = false, IsActive = true },
            };
            context.Activities.AddRange(activities);
            await context.SaveChangesAsync();

            // activity_member â€” Kacper in all activities
            context.Database.ExecuteSqlRaw("INSERT INTO activity_member (member_id, activity_id) SELECT 1, activity_id FROM activity");
        }

        private static async Task SeedTripsAsync(EssetiDbContext context)
        {
            var trips = new List<Trip>
            {
                new() { TripId = 1, Name = "Konferencja WrocĹ‚aw", Description = "Wyjazd na OgĂłlnopolskÄ… KonferencjÄ™ .NET DeveloperĂłw we WrocĹ‚awiu.", Date = new DateTime(2026, 6, 10) },
                new() { TripId = 2, Name = "Targi WynalazkĂłw Warszawa", Description = "Prezentacja projektu robota kroczÄ…cego na targach innowacji w Warszawie.", Date = new DateTime(2026, 6, 25) },
                new() { TripId = 3, Name = "Hackathon w Bieszczadach", Description = "Integracyjny wyjazd w Bieszczady poĹ‚Ä…czony z mini-hackathonem na Ĺ›wieĹĽym powietrzu.", Date = new DateTime(2026, 7, 15) },
            };
            context.Trips.AddRange(trips);
            await context.SaveChangesAsync();

            // club_trip
            context.Database.ExecuteSqlRaw("INSERT INTO club_trip (trip_id, club_id) VALUES (1, 1), (2, 1), (3, 1)");
        }
    }
}


