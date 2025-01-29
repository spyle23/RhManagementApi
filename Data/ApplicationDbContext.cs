using Microsoft.EntityFrameworkCore;
using RhManagementApi.Model;
using FileModel = RhManagementApi.Model.File;

namespace RhManagementApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<RH> RHs { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<Payslip> Payslips { get; set; }
        public DbSet<FileModel> Files { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure inheritance
            modelBuilder.Entity<Admin>().ToTable("Admins");
            modelBuilder.Entity<Employee>().ToTable("Employees");
            modelBuilder.Entity<Manager>().ToTable("Managers");
            modelBuilder.Entity<RH>().ToTable("RHs");

            // Configure relationships
            modelBuilder.Entity<Team>()
                .HasOne(t => t.Manager)
                .WithMany()
                .HasForeignKey(t => t.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Team)
                .WithMany(t => t.Employees)
                .HasForeignKey(e => e.TeamId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            modelBuilder.Entity<Leave>()
            .HasOne(l => l.Employee)
            .WithMany(e => e.Leaves)
            .HasForeignKey(l => l.EmployeeId)
            .IsRequired(false);

            modelBuilder.Entity<EmployeeRecord>()
                .HasOne(er => er.Employee)
                .WithOne()
                .HasForeignKey<EmployeeRecord>(er => er.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}