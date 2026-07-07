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

    }
}