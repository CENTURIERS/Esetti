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
                .HasMany(p => p.Participants)
                .WithMany(m => m.Projects)
                .UsingEntity(j => j.ToTable("project_member"));

            modelBuilder.Entity<Activity>()
                .HasMany(a => a.Participants)
                .WithMany(m => m.Activities)
                .UsingEntity(j => j.ToTable("activity_member"));

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

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                if (entity.IsPropertyBag || entity.ClrType == null ||
                   (entity.ClrType.IsGenericType && entity.ClrType.GetGenericTypeDefinition() == typeof(Dictionary<,>)))
                    continue;

                entity.SetTableName(ToSnakeCase(entity.ClrType.Name));

                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(ToSnakeCase(property.Name));
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