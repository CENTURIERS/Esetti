using Microsoft.EntityFrameworkCore;
using Models.Activities;
using Models.ClubBase;
using Models.Enums;
using Models.Other;
using Models.University;
using Models.Users;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Esseti.Data
{
    /// <summary>
    /// Główny kontekst bazy danych EF Core (Entity Framework).
    /// Mapuje nasze klasy modelowe na tabele w bazie SQLite i zarządza połączeniem.
    /// </summary>
    public class EssetiDbContext : DbContext
    {
        /// <summary>
        /// Tabela z członkami koła naukowego (studenci, zarząd itp.).
        /// </summary>
        public DbSet<Member> Members => Set<Member>();

        /// <summary>
        /// Tabela z projektami realizowanymi w ramach koła.
        /// </summary>
        public DbSet<Project> Projects => Set<Project>();

        /// <summary>
        /// Tabela z aktywnościami (np. warsztaty, spotkania, eventy).
        /// </summary>
        public DbSet<Activity> Activities => Set<Activity>();

        /// <summary>
        /// Tabela z kontami użytkowników (dane logowania, hasła, role).
        /// </summary>
        public DbSet<UserAccount> UserAccounts => Set<UserAccount>();

        /// <summary>
        /// Tabela z rolami i uprawnieniami (zarząd, skarbnik, członek itp.).
        /// </summary>
        public DbSet<AuthorityRole> AuthorityRoles => Set<AuthorityRole>();

        /// <summary>
        /// Tabela z informacjami o kołach naukowych.
        /// </summary>
        public DbSet<ClubInfo> Clubs => Set<ClubInfo>();

        /// <summary>
        /// Tabela z sekcjami tematycznymi koła (np. sekcja gamedev, sekcja AI).
        /// </summary>
        public DbSet<Section> Sections => Set<Section>();

        /// <summary>
        /// Tabela uczelni powiązanych z systemem.
        /// </summary>
        public DbSet<College> Colleges => Set<College>();

        /// <summary>
        /// Tabela z wydziałami danej uczelni.
        /// </summary>
        public DbSet<CollegeDepartment> CollegeDepartments => Set<CollegeDepartment>();

        /// <summary>
        /// Tabela łącząca członków z konkretnymi kołami (relacja wiele-do-wielu z dodatkową rolą).
        /// </summary>
        public DbSet<MemberClub> MemberClubs => Set<MemberClub>();

        /// <summary>
        /// Tabela łącząca członków z konkretnymi sekcjami.
        /// </summary>
        public DbSet<SectionMember> SectionMembers => Set<SectionMember>();

        /// <summary>
        /// Tabela z wyjazdami i delegacjami na konferencje.
        /// </summary>
        public DbSet<Trip> Trips => Set<Trip>();

        /// <summary>
        /// Konfiguracja połączenia z bazą danych. Używamy SQLite i zapisujemy plik bazy w podfolderze Data.
        /// </summary>
        /// <param name="optionsBuilder">Builder opcji konfiguracji kontekstu.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "esseti.db");

                var directory = Path.GetDirectoryName(dbPath);
                if (directory != null && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }

        /// <summary>
        /// Konfiguracja modeli (mapowanie tabel, klucze główne, relacje i konwertery typów wyliczeniowych).
        /// Ustawia też automatyczne nazywanie tabel i kolumn w stylu snake_case.
        /// </summary>
        /// <param name="modelBuilder">Builder do konfiguracji modeli bazy danych.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuthorityRole>().HasKey(e => e.RoleId);
            modelBuilder.Entity<Trip>().HasKey(e => e.TripId);
            modelBuilder.Entity<College>().HasKey(e => e.CollegeId);
            modelBuilder.Entity<CollegeDepartment>().HasKey(e => e.CollegeDepartmentId);
            modelBuilder.Entity<UserAccount>().HasKey(e => e.AccountId);
            modelBuilder.Entity<Member>().HasKey(e => e.MemberId);
            modelBuilder.Entity<ClubInfo>().HasKey(e => e.ClubId);
            modelBuilder.Entity<Section>().HasKey(e => e.SectionId);
            modelBuilder.Entity<Activity>().HasKey(e => e.ActivityId);
            modelBuilder.Entity<Project>().HasKey(e => e.ProjectId);

            modelBuilder.Entity<MemberClub>().HasKey(mc => new { mc.ClubId, mc.MemberId });
            modelBuilder.Entity<SectionMember>().HasKey(sm => new { sm.SectionId, sm.MemberId });
            modelBuilder.Entity<CollegeDepartment>()
                .HasOne(cd => cd.College)
                .WithMany(c => c.Departments)
                .HasForeignKey(cd => cd.CollegeId);

            modelBuilder.Entity<ClubInfo>()
                .HasOne(ci => ci.Department)
                .WithMany()
                .HasForeignKey(ci => ci.DepartmentId);

            modelBuilder.Entity<Member>()
                .HasOne(m => m.Account)
                .WithMany()
                .HasForeignKey(m => m.AccountId);

            modelBuilder.Entity<Member>()
                .HasOne(m => m.AuthorityRole)
                .WithMany()
                .HasForeignKey(m => m.RoleId);

            modelBuilder.Entity<Project>()
                .HasOne(p => p.PersonInCharge)
                .WithMany()
                .HasForeignKey("PersonInChargeId");
            modelBuilder.Entity<MemberClub>()
                .HasOne(mc => mc.Club)
                .WithMany(c => c.MemberClubs)
                .HasForeignKey(mc => mc.ClubId);

            modelBuilder.Entity<MemberClub>()
                .HasOne(mc => mc.Member)
                .WithMany(m => m.MemberClubs)
                .HasForeignKey(mc => mc.MemberId);

            modelBuilder.Entity<SectionMember>()
                .HasOne(sm => sm.Section)
                .WithMany(s => s.SectionMembers)
                .HasForeignKey(sm => sm.SectionId);

            modelBuilder.Entity<SectionMember>()
                .HasOne(sm => sm.Member)
                .WithMany(m => m.SectionMembers)
                .HasForeignKey(sm => sm.MemberId);

            modelBuilder.Entity<Project>()
                .HasMany(p => p.Sections)
                .WithMany(s => s.Projects)
                .UsingEntity<Dictionary<string, object>>(
                    "project_sections",
                    j => j.HasOne<Section>().WithMany().HasForeignKey("section_id"),
                    j => j.HasOne<Project>().WithMany().HasForeignKey("project_id")
                );

            modelBuilder.Entity<Project>()
                .HasMany(p => p.Participants)
                .WithMany(m => m.Projects)
                .UsingEntity<Dictionary<string, object>>(
                    "project_member",
                    j => j.HasOne<Member>().WithMany().HasForeignKey("member_id"),
                    j => j.HasOne<Project>().WithMany().HasForeignKey("project_id")
                );

            modelBuilder.Entity<Activity>()
                .HasMany(a => a.Participants)
                .WithMany(m => m.Activities)
                .UsingEntity<Dictionary<string, object>>(
                    "activity_member",
                    j => j.HasOne<Member>().WithMany().HasForeignKey("member_id"),
                    j => j.HasOne<Activity>().WithMany().HasForeignKey("activity_id")
                );

            modelBuilder.Entity<Project>()
                .HasMany(p => p.Clubs)
                .WithMany(c => c.Projects)
                .UsingEntity<Dictionary<string, object>>(
                    "project_club",
                    j => j.HasOne<ClubInfo>().WithMany().HasForeignKey("club_id"),
                    j => j.HasOne<Project>().WithMany().HasForeignKey("project_id")
                );

            modelBuilder.Entity<Trip>()
                .HasMany(t => t.Clubs)
                .WithMany(c => c.Trips)
                .UsingEntity<Dictionary<string, object>>(
                    "club_trip",
                    j => j.HasOne<ClubInfo>().WithMany().HasForeignKey("club_id"),
                    j => j.HasOne<Trip>().WithMany().HasForeignKey("trip_id")
                );

            modelBuilder.Entity<UserAccount>()
                .HasMany(ua => ua.Colleges)
                .WithMany(c => c.UserAccounts)
                .UsingEntity<Dictionary<string, object>>(
                    "account_college",
                    j => j.HasOne<College>().WithMany().HasForeignKey("college_id"),
                    j => j.HasOne<UserAccount>().WithMany().HasForeignKey("account_id")
                );

            var systemRoleConverter = new ValueConverter<SystemRole, string>(
                v => v == SystemRole.SuperAdmin ? "superadmin" :
                     v == SystemRole.CollegeAdmin ? "college_admin" : "user",
                v => v == "superadmin" ? SystemRole.SuperAdmin :
                     v == "college_admin" ? SystemRole.CollegeAdmin :
                     SystemRole.User
            );

            var clubRoleConverter = new ValueConverter<ClubRole, string>(
                v => v == ClubRole.President ? "president" :
                     v == ClubRole.VicePresident ? "vice_president" :
                     v == ClubRole.BoardMember ? "board_member" :
                     v == ClubRole.Supervisor ? "supervisor" : "member",
                v => v == "president" ? ClubRole.President :
                     v == "vice_president" ? ClubRole.VicePresident :
                     v == "board_member" ? ClubRole.BoardMember :
                     v == "supervisor" ? ClubRole.Supervisor :
                     ClubRole.Member
            );

            var sectionRoleConverter = new ValueConverter<SectionRole, string>(
                v => v == SectionRole.Chairman ? "chairman" :
                     v == SectionRole.Deputy ? "deputy" : "member",
                v => v == "chairman" ? SectionRole.Chairman :
                     v == "deputy" ? SectionRole.Deputy :
                     SectionRole.Member
            );

            modelBuilder.Entity<UserAccount>()
                .Property(u => u.SystemRole)
                .HasConversion(systemRoleConverter);

            modelBuilder.Entity<MemberClub>()
                .Property(mc => mc.ClubRole)
                .HasConversion(clubRoleConverter);

            modelBuilder.Entity<SectionMember>()
                .Property(sm => sm.Role)
                .HasConversion(sectionRoleConverter);

            modelBuilder.Entity<College>()
                .Property(c => c.NIP)
                .HasColumnName("NIP");

            modelBuilder.Entity<UserAccount>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<AuthorityRole>()
                .HasIndex(r => r.Name)
                .IsUnique();

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                if (entity.IsPropertyBag || entity.ClrType == null ||
                   (entity.ClrType.IsGenericType && entity.ClrType.GetGenericTypeDefinition() == typeof(Dictionary<,>)))
                    continue;

                entity.SetTableName(ToSnakeCase(entity.ClrType.Name));

                foreach (var property in entity.GetProperties())
                {
                    if (property.Name != "NIP")
                    {
                        property.SetColumnName(ToSnakeCase(property.Name));
                    }
                }

                foreach (var key in entity.GetKeys())
                {
                    key.SetName(ToSnakeCase(key.GetName()));
                }

                foreach (var foreignKey in entity.GetForeignKeys())
                {
                    foreignKey.SetConstraintName(ToSnakeCase(foreignKey.GetConstraintName()));
                }
            }
        }

        /// <summary>
        /// Prosta funkcja pomocnicza, która zamienia PascalCase (np. UserAccount) na snake_case (np. user_account).
        /// Przydatne pod SQLite, żeby tabele i kolumny były czytelniejsze.
        /// </summary>
        /// <param name="name">Tekst wejściowy w formacie PascalCase.</param>
        /// <returns>Tekst sformatowany do małych liter z podkreśleniami.</returns>
        private static string ToSnakeCase(string? name)
        {
            if (string.IsNullOrEmpty(name)) return name ?? string.Empty;
            return string.Concat(name.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString())).ToLower();
        }
    }
}

