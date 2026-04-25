using Microsoft.EntityFrameworkCore;
using Models.Activities;
using Models.ClubBase;
using Models.Other;
using Models.University;
using Models.Users;
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
            optionsBuilder.UseSqlite("Data Source=Data/esseti.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(ToSnakeCase(entity.GetTableName()));
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(ToSnakeCase(property.Name));
                }
            }

            modelBuilder.Entity<MemberClub>()
                .HasKey(mc => new { mc.ClubId, mc.MemberId });

            modelBuilder.Entity<SectionMember>()
                .HasKey(sm => new { sm.SectionId, sm.MemberId });

            modelBuilder.Entity<Member>()
                .HasOne(m => m.Account)
                .WithMany()
                .HasForeignKey(m => m.AccountId);

            modelBuilder.Entity<Member>()
                .HasOne(m => m.AuthorityRole)
                .WithMany()
                .HasForeignKey(m => m.RoleId);

            modelBuilder.Entity<Member>()
                .HasOne(m => m.Department)
                .WithMany()
                .HasForeignKey("DepartmentId");

            modelBuilder.Entity<MemberClub>()
                .HasOne(mc => mc.Club)
                .WithMany(c => c.MemberClubs)
                .HasForeignKey(mc => mc.ClubId);

            modelBuilder.Entity<MemberClub>()
                .HasOne(mc => mc.Member)
                .WithMany(m => m.MemberClubs)
                .HasForeignKey(mc => mc.MemberId);
        }

        private static string ToSnakeCase(string? name)
        {
            if (string.IsNullOrEmpty(name)) return name ?? string.Empty;
            return string.Concat(name.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString())).ToLower();
        }
    }
}