using Microsoft.EntityFrameworkCore;

namespace VacaYAY.Data;

using VacaYAY.Domain.Entities;

public class VacaYAYDbContext : DbContext
{
    public VacaYAYDbContext(DbContextOptions<VacaYAYDbContext> options) : base(options)
    {

    }

    public DbSet<User> Users => Set<User>();
    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<LegacyEmployee> LegacyEmployees => Set<LegacyEmployee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.Role)
                  .HasConversion<string>()
                .HasMaxLength(20);

            //email is unique
            entity.HasIndex(u => u.Email).IsUnique();

            //optimizacija upita za soft deleted usere
            entity.HasQueryFilter(u => !u.IsDeleted);

        });

        modelBuilder.Entity<LeaveType>(entity =>
        {
            entity.Property(t => t.Name)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(t => t.Color)
                .HasConversion<string>()
                .HasMaxLength(20);

            //one type per name 
            entity.HasIndex(t => t.Name).IsUnique();

            //soft delete filter
            entity.HasQueryFilter(t => !t.IsDeleted);
        });

        modelBuilder.Entity<LeaveRequest>(entity =>
        {

            entity.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasQueryFilter(r => !r.Employee!.IsDeleted);

            //*-1
            //without cascade delete
            entity.HasOne(r => r.Employee)
                 .WithMany(u => u.LeaveRequests)
                 .HasForeignKey(r => r.EmployeeId)
                 .OnDelete(DeleteBehavior.Restrict);

            //*-1
            entity.HasOne(r => r.LeaveType)
                .WithMany(t => t.LeaveRequests)
                .HasForeignKey(r => r.LeaveTypeId)
                .OnDelete(DeleteBehavior.Restrict);


        }
        );

        modelBuilder.Entity<LegacyEmployee>(entity =>
        {
            // The old system owns this table — it is created and populated outside the app, so EF
            // maps it but never generates migrations for it.
            entity.ToTable("legacy_employees", t => t.ExcludeFromMigrations());

            entity.HasKey(e => e.LegacyId);
            entity.Property(e => e.LegacyId).HasColumnName("legacy_id").ValueGeneratedNever();
            entity.Property(e => e.FirstName).HasColumnName("first_name").HasMaxLength(100);
            entity.Property(e => e.LastName).HasColumnName("last_name").HasMaxLength(100);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(256);
            entity.Property(e => e.Department).HasColumnName("department").HasMaxLength(100);
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(100);
            entity.Property(e => e.HiredOn).HasColumnName("hired_on").HasColumnType("date");
            entity.Property(e => e.ContractEnd).HasColumnName("contract_end").HasColumnType("date");
            entity.Property(e => e.DaysOff).HasColumnName("days_off");

            entity.HasIndex(e => e.Email).IsUnique();
        });

    }
}