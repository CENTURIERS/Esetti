using Microsoft.EntityFrameworkCore;
using Models.Activities;
using Models.ClubBase;
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

        public EssetiDbContext()
        {
            Database.EnsureCreated();
            EnsureSchemaUpToDate();
        }

        private void EnsureSchemaUpToDate()
        {
            try
            {
                using (var command = Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "PRAGMA table_info(club_info);";
                    var connection = command.Connection;
                    if (connection != null)
                    {
                        if (connection.State != System.Data.ConnectionState.Open)
                        {
                            connection.Open();
                        }
                        var columns = new List<string>();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                columns.Add(reader["name"].ToString() ?? "");
                            }
                        }
                        if (!columns.Contains("supervisor_name"))
                        {
                            Database.ExecuteSqlRaw("ALTER TABLE club_info ADD COLUMN supervisor_name TEXT;");
                        }
                        if (!columns.Contains("meetings_schedule"))
                        {
                            Database.ExecuteSqlRaw("ALTER TABLE club_info ADD COLUMN meetings_schedule TEXT;");
                        }
                        if (!columns.Contains("short_name"))
                        {
                            Database.ExecuteSqlRaw("ALTER TABLE club_info ADD COLUMN short_name TEXT;");
                        }
                        if (!columns.Contains("club_photo"))
                        {
                            Database.ExecuteSqlRaw("ALTER TABLE club_info ADD COLUMN club_photo BLOB;");
                        }
                    }
                }
                using (var command = Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "PRAGMA table_info(trip);";
                    var connection = command.Connection;
                    if (connection != null)
                    {
                        if (connection.State != System.Data.ConnectionState.Open)
                        {
                            connection.Open();
                        }
                        var columns = new List<string>();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                columns.Add(reader["name"].ToString() ?? "");
                            }
                        }
                        if (!columns.Contains("name"))
                        {
                            Database.ExecuteSqlRaw("ALTER TABLE trip ADD COLUMN name TEXT;");
                        }
                    }
                }
                using (var command = Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "PRAGMA table_info(activity);";
                    var connection = command.Connection;
                    if (connection != null)
                    {
                        if (connection.State != System.Data.ConnectionState.Open)
                        {
                            connection.Open();
                        }
                        var columns = new List<string>();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                columns.Add(reader["name"].ToString() ?? "");
                            }
                        }
                        if (!columns.Contains("is_active"))
                        {
                            Database.ExecuteSqlRaw("ALTER TABLE activity ADD COLUMN is_active INTEGER NOT NULL DEFAULT 1;");
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "esseti.db");

            var directory = Path.GetDirectoryName(dbPath);
            if (directory != null && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

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
                .HasOne(p => p.PersonInCharge)
                .WithMany()
                .HasForeignKey("PersonInChargeId");

            modelBuilder.Entity<Member>()
                .HasOne(m => m.Account)
                .WithMany()
                .HasForeignKey(m => m.AccountId);

            modelBuilder.Entity<Member>()
                .HasOne(m => m.AuthorityRole)
                .WithMany()
                .HasForeignKey(m => m.RoleId);

            var systemRoleConverter = new ValueConverter<Models.Enums.SystemRole, string>(
                v => v == Models.Enums.SystemRole.SuperAdmin ? "superadmin" :
                     v == Models.Enums.SystemRole.CollegeAdmin ? "college_admin" : "user",
                v => v == "superadmin" ? Models.Enums.SystemRole.SuperAdmin :
                     v == "college_admin" ? Models.Enums.SystemRole.CollegeAdmin :
                     Models.Enums.SystemRole.User
            );

            modelBuilder.Entity<UserAccount>()
                .Property(u => u.SystemRole)
                .HasConversion(systemRoleConverter);

            modelBuilder.Entity<College>()
                .Property(c => c.NIP)
                .HasColumnName("NIP");

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