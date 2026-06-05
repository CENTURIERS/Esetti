using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Models.Activities;
using Models.ClubBase;
using Models.Enums;
using Models.Other;
using Models.University;
using Models.Users;

namespace Esseti.Data
{
    /// <summary>
    /// Klasa służąca do automatycznego napełniania (seedowania) bazy danych testowymi rekordami.
    /// Przydatna sprawa, żeby po każdym usunięciu bazy nie klepać danych z palca.
    /// </summary>
    public static class DbSeeder
    {
        /// <summary>
        /// Odpala cały proces siania danych. Najpierw usuwa starą bazę, tworzy nową czystą strukturę,
        /// a potem po kolei ładuje testowe dane (role, uczelnię, koło naukowe, studentów, projekty itd.).
        /// </summary>
        /// <param name="context">Instancja kontekstu naszej bazy danych (EssetiDbContext).</param>
        public static async Task SeedAsync(EssetiDbContext context)
        {
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            await SeedAuthorityRolesAsync(context);
            await SeedUniversityStructureAsync(context);
            await SeedClubInfoAsync(context);
            await SeedAccountsAndMembersAsync(context);
            await SeedSectionsAsync(context);
            await SeedProjectsAsync(context);
            await SeedActivitiesAsync(context);
            await SeedTripsAsync(context);
        }

        /// <summary>
        /// Sypie do bazy podstawowe role w kołach naukowych (np. Zarząd, Członek, Skarbnik).
        /// </summary>
        private static async Task SeedAuthorityRolesAsync(EssetiDbContext context)
        {
            var roles = new List<AuthorityRole>
            {
                new() { RoleId = 1, Name = "Zarząd", Description = "Pełne uprawnienia edycji i zarządzania", Permissions = 100 },
                new() { RoleId = 2, Name = "Członek", Description = "Podstawowy dostęp do projektów", Permissions = 10 },
                new() { RoleId = 3, Name = "Skarbnik", Description = "Zarządzanie finansami i składkami", Permissions = 80 },
                new() { RoleId = 4, Name = "Sekretarz", Description = "Dokumentacja i protokoły", Permissions = 70 },
                new() { RoleId = 5, Name = "Sympatyk", Description = "Osoba biorąca udział w eventach, bez głosu", Permissions = 5 }
            };

            context.AuthorityRoles.AddRange(roles);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Wrzuca do bazy strukturę uczelni (Uniwersytet Rzeszowski i Wydział).
        /// </summary>
        private static async Task SeedUniversityStructureAsync(EssetiDbContext context)
        {
            var college = new College
            {
                CollegeId = 1,
                Name = "Uniwersytet Rzeszowski",
                NameShort = "UR",
                AddressLine = "al. Rejtana 16c",
                City = "Rzeszów",
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
                Name = "Wydział Nauk Ścisłych i Technicznych",
                AddressLine = "ul. Pigonia 1",
                City = "Rzeszów",
                PostalCode = "35-310",
                Phone = "17 872 12 00",
                Email = "wnst@ur.edu.pl"
            };
            context.CollegeDepartments.Add(department);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Wrzuca podstawowe informacje o samym kole naukowym (nazwa, pokój, opiekun).
        /// </summary>
        private static async Task SeedClubInfoAsync(EssetiDbContext context)
        {
            var club = new ClubInfo
            {
                ClubId = 1,
                Name = "KN .NET & Avalonia",
                DepartmentId = 1,
                ClubRoom = "Budynek F, pokój 102",
                SupervisorName = "dr hab. inż. Jan Kowalski",
                MeetingsSchedule = "Poniedziałki o 17:00",
                ShortName = "KNA"
            };
            context.Clubs.Add(club);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Generuje pętlą 50 kont i profili członków koła (studentów) na potrzeby testów.
        /// Pierwszy to prezes Kacper Ręczak.
        /// </summary>
        private static async Task SeedAccountsAndMembersAsync(EssetiDbContext context)
        {
            var college = await context.Colleges.FirstAsync(c => c.CollegeId == 1);

            var accounts = new List<UserAccount>();
            var members = new List<Member>();
            var memberClubs = new List<MemberClub>();

            accounts.Add(new UserAccount 
            { 
                AccountId = 1, 
                Email = "admin@admin.pl", 
                PasswordHash = "haslo123", 
                SystemRole = SystemRole.SuperAdmin, 
                IsVerified = true, 
                CreatedAt = new DateTime(2026, 4, 1), 
                UpdatedAt = DateTime.Now,
                Colleges = new List<College> { college }
            });

            members.Add(new Member 
            { 
                MemberId = 1, 
                AccountId = 1, 
                RoleId = 1, 
                IndexNumber = "123456", 
                FirstName = "Kacper", 
                LastName = "Ręczak", 
                Major = "Informatyka Stosowana", 
                PhoneNumber = "555-123-456", 
                Description = "Prezes, król Avaloni, główny mózg operacji. Zrobi to w MAUI.", 
                IsActive = true, 
                JoinDate = new DateTime(2026, 4, 4) 
            });

            memberClubs.Add(new MemberClub { ClubId = 1, MemberId = 1, ClubRole = ClubRole.President });

            string[] firstNames = { "Mateusz", "Julia", "Alicja", "Piotr", "Kasia", "Michał", "Karolina", "Marek", "Zofia", "Adam", "Ewa", "Tomasz", "Anna", "Jan", "Paweł", "Magda", "Krzysztof", "Artur", "Dorota", "Dawid" };
            string[] lastNames = { "Kowalski", "Nowak", "Wiśniewska", "Wójcik", "Lewandowska", "Dąbrowski", "Mazur", "Król", "Wieczorek", "Mickiewicz", "Lis", "Grodzka", "Kukiz", "Gessler", "Szpilka", "Rabczewska", "Pazura", "Węgiel", "Mentzen", "Tusk" };
            string[] majors = { "Informatyka", "Automatyka", "Telekomunikacja", "Mechatronika", "Zarządzanie", "Matematyka", "Fizyka", "Chemia" };

            for (int i = 2; i <= 50; i++)
            {
                accounts.Add(new UserAccount 
                { 
                    AccountId = i, 
                    Email = $"u{i}@stud.pl", 
                    PasswordHash = "hash123", 
                    SystemRole = SystemRole.User, 
                    IsVerified = true, 
                    CreatedAt = DateTime.Now.AddDays(-i), 
                    UpdatedAt = DateTime.Now,
                    Colleges = new List<College> { college }
                });
                
                members.Add(new Member 
                { 
                    MemberId = i, 
                    AccountId = i, 
                    RoleId = (i % 7 == 0) ? 3 : 2,
                    IndexNumber = $"1000{i:D2}", 
                    FirstName = firstNames[i % firstNames.Length], 
                    LastName = lastNames[i % lastNames.Length], 
                    Major = majors[i % majors.Length], 
                    PhoneNumber = $"555-{i:D3}", 
                    Description = "Aktywny członek koła.", 
                    IsActive = true, 
                    JoinDate = DateTime.Now.AddDays(-i * 2) 
                });

                memberClubs.Add(new MemberClub { ClubId = 1, MemberId = i, ClubRole = (i % 5 == 0) ? ClubRole.BoardMember : ClubRole.Member });
            }

            await context.UserAccounts.AddRangeAsync(accounts);
            await context.Members.AddRangeAsync(members);
            await context.MemberClubs.AddRangeAsync(memberClubs);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Wrzuca sekcje tematyczne działające w kole (np. Gamedev, Robotyk, AI) i przypisuje do nich ludzi.
        /// </summary>
        private static async Task SeedSectionsAsync(EssetiDbContext context)
        {
            var members = await context.Members.ToListAsync();

            var sections = new List<Section>
            {
                new() { SectionId = 1, Name = "Sekcja Gamedev", ShortName = "Unity", Meetings = "Piątki 20:00", CreatedAt = DateTime.Now.AddMonths(-5), IsActive = true },
                new() { SectionId = 2, Name = "Sekcja Robotyk", ShortName = "Robo", Meetings = "Środy 16:00", CreatedAt = DateTime.Now.AddMonths(-4), IsActive = true },
                new() { SectionId = 3, Name = "Sekcja Cybersec", ShortName = "Sec", Meetings = "Poniedziałki 19:00", CreatedAt = DateTime.Now.AddMonths(-3), IsActive = true },
                new() { SectionId = 4, Name = "Sekcja .NET & Web", ShortName = "WebDev", Meetings = "Wtorki 17:30", CreatedAt = DateTime.Now.AddMonths(-2), IsActive = true },
                new() { SectionId = 5, Name = "Sekcja AI & ML", ShortName = "AI", Meetings = "Czwartki 18:00", CreatedAt = DateTime.Now.AddMonths(-1), IsActive = true },
                new() { SectionId = 6, Name = "Sekcja Mobile", ShortName = "Mobile", Meetings = "Środy 18:00", CreatedAt = DateTime.Now, IsActive = true },
                new() { SectionId = 7, Name = "Sekcja Cloud", ShortName = "Cloud", Meetings = "Soboty 10:00", CreatedAt = DateTime.Now, IsActive = true },
                new() { SectionId = 8, Name = "Sekcja UI/UX Design", ShortName = "Design", Meetings = "Piątki 16:00", CreatedAt = DateTime.Now, IsActive = true },
                new() { SectionId = 9, Name = "Sekcja QA", ShortName = "Testy", Meetings = "Wtorki 19:00", CreatedAt = DateTime.Now, IsActive = true },
                new() { SectionId = 10, Name = "Sekcja Algorytmiczna", ShortName = "Algo", Meetings = "Czwartki 16:00", CreatedAt = DateTime.Now, IsActive = true }
            };
            await context.Sections.AddRangeAsync(sections);

            var sectionMembers = new List<SectionMember>();
            foreach (var s in sections)
            {
                foreach (var m in members)
                {
                    if ((s.SectionId * m.MemberId) % 4 == 0 || m.MemberId == 1)
                    {
                        var role = ((s.SectionId + m.MemberId) % 11 == 0) ? SectionRole.Chairman : SectionRole.Member;
                        sectionMembers.Add(new SectionMember { SectionId = s.SectionId, MemberId = m.MemberId, Role = role });
                    }
                }
            }
            await context.SectionMembers.AddRangeAsync(sectionMembers);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Tworzy 50 fikcyjnych projektów studenckich, przypisuje im sekcje, szefów oraz uczestników.
        /// </summary>
        private static async Task SeedProjectsAsync(EssetiDbContext context)
        {
            var members = await context.Members.ToListAsync();
            var sections = await context.Sections.ToListAsync();
            var club = await context.Clubs.FirstAsync(c => c.ClubId == 1);

            string[] projectNames = { "Gra RPG 2D", "Skaner Sieciowy", "Robot Kroczący", "Portal Student", "Aplikacja Esetti", "System Rezerwacji", "Sklep Internetowy", "Bot Discord", "Esetti Cloud", "Smart Lustro", "Wykrywacz Twarzy", "Analiza Giełd", "Kalkulator Kalorii", "Gra Tower Defense", "Klon Spotify" };
            var projects = new List<Project>();

            for (int i = 1; i <= 50; i++)
            {
                var p = new Project
                {
                    ProjectId = i,
                    Name = i <= projectNames.Length ? projectNames[i - 1] : $"Projekt Badawczy KNA #{i}",
                    Description = $"Opis dla niesamowitego projektu nr {i}.",
                    AdditionalInformation = "Wygenerowano automatycznie w MAUI.",
                    PersonInCharge = members.First(m => m.MemberId == ((i % 10) + 1)),
                    Github = "github.com/kni/esetti",
                    EstimatedTime = 100 + (i * 10),
                    DateStart = DateTime.Now.AddDays(-i * 5),
                    DateEnd = DateTime.Now.AddDays(100),
                    IsActive = true,
                    Clubs = new List<ClubInfo> { club },
                    Sections = new List<Section>(),
                    Participants = new List<Member>()
                };

                int targetSectionId = (i % 10) + 1;
                p.Sections.Add(sections.First(s => s.SectionId == targetSectionId));

                p.Participants.Add(members.First(m => m.MemberId == 1));

                for (int mId = 2; mId <= 50; mId++)
                {
                    if ((i * mId) % 13 == 0 || (i + mId) % 17 == 0)
                    {
                        p.Participants.Add(members.First(m => m.MemberId == mId));
                    }
                }
                
                projects.Add(p);
            }

            await context.Projects.AddRangeAsync(projects);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Tworzy testowe wydarzenia koła (np. warsztaty, spotkania) i przypisuje uczestników.
        /// </summary>
        private static async Task SeedActivitiesAsync(EssetiDbContext context)
        {
            var members = await context.Members.ToListAsync();
            var activities = new List<Activity>();

            for (int i = 1; i <= 30; i++)
            {
                var act = new Activity
                {
                    ActivityId = i,
                    Name = $"Wydarzenie Koła #{i} - " + (i % 2 == 0 ? "Warsztaty" : "Spotkanie Zarządu"),
                    AddressLine = "ul. Pigonia 1",
                    City = "Rzeszów",
                    PostalCode = "35-310",
                    Date = DateTime.Now.AddDays(i * 2),
                    Time = new TimeSpan(16, 0, 0),
                    PersonInChargeName = "Kacper Ręczak",
                    AdditionalInformation = "Brak",
                    IsRepeatable = false,
                    IsActive = true,
                    Participants = new List<Member>()
                };

                act.Participants.Add(members.First(m => m.MemberId == 1));
                
                for (int mId = 2; mId <= 50; mId++)
                {
                    if ((mId * i) % 9 == 0 || (mId + i) % 11 == 0)
                    {
                        act.Participants.Add(members.First(m => m.MemberId == mId));
                    }
                }
                activities.Add(act);
            }
            
            await context.Activities.AddRangeAsync(activities);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Dodaje do bazy wyjazdy/konferencje powiązane z kołem naukowym.
        /// </summary>
        private static async Task SeedTripsAsync(EssetiDbContext context)
        {
            var club = await context.Clubs.FirstAsync(c => c.ClubId == 1);
            var trips = new List<Trip>();
            string[] tripNames = { "Konferencja .NET Wrocław", "Targi Innowacji Warszawa", "Hackathon Bieszczady", "Azure Bootcamp Kraków", "CyberSec Katowice", "PGA Poznań", "Festiwal Nauki Rzeszów", "Devoxx Kraków", "Microsoft Warszawa", "Gamedev Night Lublin" };
            
            for (int i = 1; i <= 15; i++)
            {
                trips.Add(new Trip
                {
                    TripId = i,
                    Name = i <= tripNames.Length ? tripNames[i - 1] : $"Wyjazd integracyjny #{i}",
                    Description = "Oficjalny wyjazd koła.",
                    Date = DateTime.Now.AddMonths(i),
                    Clubs = new List<ClubInfo> { club }
                });
            }

            await context.Trips.AddRangeAsync(trips);
            await context.SaveChangesAsync();
        }
    }
}