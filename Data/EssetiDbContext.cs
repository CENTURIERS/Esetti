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
    public class EssetiDbContext : DbContext
    {
        public DbSet<Member> Members => Set<Member>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<Activity> Activities => Set<Activity>();
        public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
        public DbSet<AuthorityRole> AuthorityRoles => Set<AuthorityRole>();
        public DbSet<ClubInfo> Clubs => Set<ClubInfo>();
        public DbSet<Section> Sections => Set<Section>();
        public DbSet<College> Colleges => Set<College>();
        public DbSet<CollegeDepartment> CollegeDepartments => Set<CollegeDepartment>();
        public DbSet<MemberClub> MemberClubs => Set<MemberClub>();
        public DbSet<SectionMember> SectionMembers => Set<SectionMember>();
        public DbSet<Trip> Trips => Set<Trip>();

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            // Primary Keys
            // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
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

            // Composite keys for join entities
            modelBuilder.Entity<MemberClub>().HasKey(mc => new { mc.ClubId, mc.MemberId });
            modelBuilder.Entity<SectionMember>().HasKey(sm => new { sm.SectionId, sm.MemberId });

            // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            // One-to-Many Relationships
            // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

            // CollegeDepartment â†’ College
            modelBuilder.Entity<CollegeDepartment>()
                .HasOne(cd => cd.College)
                .WithMany(c => c.Departments)
                .HasForeignKey(cd => cd.CollegeId);

            // ClubInfo â†’ CollegeDepartment
            modelBuilder.Entity<ClubInfo>()
                .HasOne(ci => ci.Department)
                .WithMany()
                .HasForeignKey(ci => ci.DepartmentId);

            // Member â†’ UserAccount (one-to-one via FK)
            modelBuilder.Entity<Member>()
                .HasOne(m => m.Account)
                .WithMany()
                .HasForeignKey(m => m.AccountId);

            // Member â†’ AuthorityRole
            modelBuilder.Entity<Member>()
                .HasOne(m => m.AuthorityRole)
                .WithMany()
                .HasForeignKey(m => m.RoleId);

            // Project â†’ Member (PersonInCharge)
            modelBuilder.Entity<Project>()
                .HasOne(p => p.PersonInCharge)
                .WithMany()
                .HasForeignKey("PersonInChargeId");

            // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            // Join Entity Relationships (explicit)
            // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

            // MemberClub â†’ ClubInfo & Member
            modelBuilder.Entity<MemberClub>()
                .HasOne(mc => mc.Club)
                .WithMany(c => c.MemberClubs)
                .HasForeignKey(mc => mc.ClubId);

            modelBuilder.Entity<MemberClub>()
                .HasOne(mc => mc.Member)
                .WithMany(m => m.MemberClubs)
                .HasForeignKey(mc => mc.MemberId);

            // SectionMember â†’ Section & Member
            modelBuilder.Entity<SectionMember>()
                .HasOne(sm => sm.Section)
                .WithMany(s => s.SectionMembers)
                .HasForeignKey(sm => sm.SectionId);

            modelBuilder.Entity<SectionMember>()
                .HasOne(sm => sm.Member)
                .WithMany(m => m.SectionMembers)
                .HasForeignKey(sm => sm.MemberId);

            // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            // Many-to-Many (implicit join tables)
            // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

            // Project â†” Section (project_sections)
            modelBuilder.Entity<Project>()
                .HasMany(p => p.Sections)
                .WithMany(s => s.Projects)
                .UsingEntity<Dictionary<string, object>>(
                    "project_sections",
                    j => j.HasOne<Section>().WithMany().HasForeignKey("section_id"),
                    j => j.HasOne<Project>().WithMany().HasForeignKey("project_id")
                );

            // Project â†” Member (project_member)
            modelBuilder.Entity<Project>()
                .HasMany(p => p.Participants)
                .WithMany(m => m.Projects)
                .UsingEntity<Dictionary<string, object>>(
                    "project_member",
                    j => j.HasOne<Member>().WithMany().HasForeignKey("member_id"),
                    j => j.HasOne<Project>().WithMany().HasForeignKey("project_id")
                );

            // Activity â†” Member (activity_member)
            modelBuilder.Entity<Activity>()
                .HasMany(a => a.Participants)
                .WithMany(m => m.Activities)
                .UsingEntity<Dictionary<string, object>>(
                    "activity_member",
                    j => j.HasOne<Member>().WithMany().HasForeignKey("member_id"),
                    j => j.HasOne<Activity>().WithMany().HasForeignKey("activity_id")
                );

            // Project â†” ClubInfo (project_club)
            modelBuilder.Entity<Project>()
                .HasMany(p => p.Clubs)
                .WithMany(c => c.Projects)
                .UsingEntity<Dictionary<string, object>>(
                    "project_club",
                    j => j.HasOne<ClubInfo>().WithMany().HasForeignKey("club_id"),
                    j => j.HasOne<Project>().WithMany().HasForeignKey("project_id")
                );

            // Trip â†” ClubInfo (club_trip)
            modelBuilder.Entity<Trip>()
                .HasMany(t => t.Clubs)
                .WithMany(c => c.Trips)
                .UsingEntity<Dictionary<string, object>>(
                    "club_trip",
                    j => j.HasOne<ClubInfo>().WithMany().HasForeignKey("club_id"),
                    j => j.HasOne<Trip>().WithMany().HasForeignKey("trip_id")
                );

            // UserAccount â†” College (account_college)
            modelBuilder.Entity<UserAccount>()
                .HasMany(ua => ua.Colleges)
                .WithMany(c => c.UserAccounts)
                .UsingEntity<Dictionary<string, object>>(
                    "account_college",
                    j => j.HasOne<College>().WithMany().HasForeignKey("college_id"),
                    j => j.HasOne<UserAccount>().WithMany().HasForeignKey("account_id")
                );

            // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            // Value Converters (enum â†’ TEXT in SQLite)
            // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

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

            // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            // Column Name Overrides
            // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            modelBuilder.Entity<College>()
                .Property(c => c.NIP)
                .HasColumnName("NIP");

            // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            // Indexes
            // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            modelBuilder.Entity<UserAccount>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<AuthorityRole>()
                .HasIndex(r => r.Name)
                .IsUnique();

            // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            // Snake_case naming convention
            // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
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

        private static string ToSnakeCase(string? name)
        {
            if (string.IsNullOrEmpty(name)) return name ?? string.Empty;
            return string.Concat(name.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString())).ToLower();
        }
    }
}

