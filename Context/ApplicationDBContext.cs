using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VinhUni_Educator_API.Entities;

namespace VinhUni_Educator_API.Context
{
    public class ApplicationDBContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // SeedData(modelBuilder);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
            base.OnConfiguring(optionsBuilder);
        }

        public void SeedData(ModelBuilder modelBuilder)
        {
            ApplicationUser ExampleUser = new ApplicationUser
            {
                UserName = "admin",
                NormalizedUserName = "admin".ToUpper(),
                Email = "tomtepfa@gmail.com",
                NormalizedEmail = "TOMTEPFA@GMAIL.COM".ToUpper(),
                EmailConfirmed = true,
                Gender = 1,
                FirstName = "Nguyễn Ngọc Anh",
                LastName = "Tuấn",
                Address = "TP.Vinh, Nghệ An",
                USmartId = 78592,
                CreatedAt = DateTime.UtcNow,
                DateOfBirth = new DateOnly(2002, 07, 02),
                PhoneNumber = "0123456789"
            };
            PasswordHasher<ApplicationUser> passwordHasher = new PasswordHasher<ApplicationUser>();
            ExampleUser.PasswordHash = passwordHasher.HashPassword(ExampleUser, "Admin@123");

            IdentityRole ExampleRole = new IdentityRole
            {
                Name = "Quản trị viên",
                NormalizedName = "QUẢN TRỊ VIÊN"
            };
            IdentityUserRole<string> UserRole = new IdentityUserRole<string>
            {
                RoleId = ExampleRole.Id,
                UserId = ExampleUser.Id
            };
            modelBuilder.Entity<ApplicationUser>().HasData(ExampleUser);
            modelBuilder.Entity<IdentityRole>().HasData(ExampleRole);
            modelBuilder.Entity<IdentityUserRole<string>>().HasData(UserRole);
        }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<USmartToken> USmartTokens { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<SyncAction> SyncActions { get; set; }
        public DbSet<Major> Majors { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<TrainingProgram> TrainingPrograms { get; set; }
        public DbSet<PrimaryClass> PrimaryClasses { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<SchoolYear> SchoolYears { get; set; }
        public DbSet<Semester> Semesters { get; set; }
        public DbSet<Module> Modules { get; set; }
    }
}