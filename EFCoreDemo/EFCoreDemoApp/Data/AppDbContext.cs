using Microsoft.EntityFrameworkCore;
using EFCoreDemoApp.Entities;

namespace EFCoreDemoApp.Data;

/// <summary>
/// AppDbContext - Unit of Work + Repository pattern implementation
/// Manages database connections, entity tracking, and transactions
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // DbSets - These become database tables
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Department> Departments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure Employee-Department relationship
        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed initial data
        modelBuilder.Entity<Department>().HasData(
            new Department { DepartmentId = 1, Name = "Engineering", Budget = 500000 },
            new Department { DepartmentId = 2, Name = "Human Resources", Budget = 150000 },
            new Department { DepartmentId = 3, Name = "Finance", Budget = 300000 }
        );

        modelBuilder.Entity<Employee>().HasData(
            new Employee
            {
                EmployeeId = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@company.com",
                Salary = 85000,
                DepartmentId = 1,
                JoinDate = new DateTime(2022, 1, 15)
            },
            new Employee
            {
                EmployeeId = 2,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@company.com",
                Salary = 92000,
                DepartmentId = 1,
                JoinDate = new DateTime(2021, 6, 1)
            },
            new Employee
            {
                EmployeeId = 3,
                FirstName = "Bob",
                LastName = "Johnson",
                Email = "bob.johnson@company.com",
                Salary = 65000,
                DepartmentId = 2,
                JoinDate = new DateTime(2023, 3, 20)
            }
        );
    }
}
