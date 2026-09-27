using Microsoft.EntityFrameworkCore;
using EFCoreDemoApp.Data;
using EFCoreDemoApp.Entities;

Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
Console.WriteLine("║           EF Core Demo - Interview Practice                   ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
Console.WriteLine();

// Configure DbContext
var builder = new DbContextOptionsBuilder<AppDbContext>();

// CONNECTION STRING OPTIONS:
// ═══════════════════════════════════════════════════════════════════

// Option 1: Mac with Docker SQL Server
// First run: docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
builder.UseSqlServer("Server=localhost,1433;Database=EFCoreDemoDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;");

// Option 2: Windows with LocalDB
// builder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=EFCoreDemoDB;Trusted_Connection=True;");

// Enable SQL logging to see generated queries
builder.LogTo(Console.WriteLine, new[] { Microsoft.EntityFrameworkCore.Diagnostics.DbLoggerCategory.Database.Command.Name }, Microsoft.Extensions.Logging.LogLevel.Information);

using var context = new AppDbContext(builder.Options);

// Ensure database is created with seed data
await context.Database.EnsureCreatedAsync();

Console.WriteLine("✅ Database ready!\n");

// ═══════════════════════════════════════════════════════════════════
// DEMO SECTIONS - Uncomment to run each demo
// ═══════════════════════════════════════════════════════════════════

await RunCreateDemo(context);
await RunReadDemo(context);
await RunUpdateDemo(context);
await RunQueryDemo(context);
await RunLoadingDemo(context);

Console.WriteLine("\n✅ All demos completed!");

// ═══════════════════════════════════════════════════════════════════
// CREATE OPERATIONS
// ═══════════════════════════════════════════════════════════════════
static async Task RunCreateDemo(AppDbContext context)
{
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("                    CREATE OPERATIONS");
    Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

    // Check entity state before add
    var newEmployee = new Employee
    {
        FirstName = "Alice",
        LastName = "Brown",
        Email = $"alice.brown{DateTime.Now.Ticks}@company.com",
        Salary = 75000,
        DepartmentId = 1
    };

    Console.WriteLine($"Before Add - EmployeeId: {newEmployee.EmployeeId}");
    Console.WriteLine($"Before Add - State: {context.Entry(newEmployee).State}");

    context.Employees.Add(newEmployee);
    Console.WriteLine($"After Add - State: {context.Entry(newEmployee).State}");

    await context.SaveChangesAsync();
    Console.WriteLine($"After SaveChanges - EmployeeId: {newEmployee.EmployeeId}");
    Console.WriteLine($"After SaveChanges - State: {context.Entry(newEmployee).State}");

    Console.WriteLine($"\n✅ Created employee: {newEmployee.FullName} (ID: {newEmployee.EmployeeId})\n");
}

// ═══════════════════════════════════════════════════════════════════
// READ OPERATIONS
// ═══════════════════════════════════════════════════════════════════
static async Task RunReadDemo(AppDbContext context)
{
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("                    READ OPERATIONS");
    Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

    // Find by PK
    Console.WriteLine("--- FindAsync (by primary key) ---");
    var emp = await context.Employees.FindAsync(1);
    Console.WriteLine($"Found: {emp?.FullName ?? "null"}");

    // FirstOrDefault
    Console.WriteLine("\n--- FirstOrDefaultAsync ---");
    var firstEmp = await context.Employees
        .FirstOrDefaultAsync(e => e.Salary > 80000);
    Console.WriteLine($"First with salary > 80K: {firstEmp?.FullName ?? "none"}");

    // Where + ToList
    Console.WriteLine("\n--- Where + ToList ---");
    var engineers = await context.Employees
        .Where(e => e.DepartmentId == 1)
        .ToListAsync();
    Console.WriteLine($"Engineers: {string.Join(", ", engineers.Select(e => e.FullName))}");

    // Count
    Console.WriteLine("\n--- Aggregations ---");
    var count = await context.Employees.CountAsync();
    var avgSalary = await context.Employees.AverageAsync(e => e.Salary);
    Console.WriteLine($"Total employees: {count}");
    Console.WriteLine($"Average salary: ${avgSalary:N2}");

    Console.WriteLine();
}

// ═══════════════════════════════════════════════════════════════════
// UPDATE OPERATIONS
// ═══════════════════════════════════════════════════════════════════
static async Task RunUpdateDemo(AppDbContext context)
{
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("                    UPDATE OPERATIONS");
    Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

    // Connected update
    var employee = await context.Employees.FirstAsync();
    var originalSalary = employee.Salary;

    Console.WriteLine($"Before: {employee.FullName}, Salary: ${employee.Salary:N2}");
    Console.WriteLine($"State: {context.Entry(employee).State}");

    employee.Salary += 5000;

    Console.WriteLine($"After modification - State: {context.Entry(employee).State}");

    await context.SaveChangesAsync();

    Console.WriteLine($"After SaveChanges: {employee.FullName}, Salary: ${employee.Salary:N2}");
    Console.WriteLine($"State: {context.Entry(employee).State}");

    // Restore original value
    employee.Salary = originalSalary;
    await context.SaveChangesAsync();

    Console.WriteLine();
}

// ═══════════════════════════════════════════════════════════════════
// ADVANCED QUERYING
// ═══════════════════════════════════════════════════════════════════
static async Task RunQueryDemo(AppDbContext context)
{
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("                    ADVANCED QUERYING");
    Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

    // Projection
    Console.WriteLine("--- Projection (Select) ---");
    var projectedData = await context.Employees
        .Select(e => new
        {
            e.FullName,
            e.Salary,
            DeptName = e.Department.Name  // No Include needed!
        })
        .ToListAsync();

    foreach (var item in projectedData)
    {
        Console.WriteLine($"  {item.FullName} - {item.DeptName} - ${item.Salary:N2}");
    }

    // Group By
    Console.WriteLine("\n--- Group By Department ---");
    var byDept = await context.Employees
        .GroupBy(e => e.Department.Name)
        .Select(g => new
        {
            Department = g.Key,
            Count = g.Count(),
            TotalSalary = g.Sum(e => e.Salary)
        })
        .ToListAsync();

    foreach (var dept in byDept)
    {
        Console.WriteLine($"  {dept.Department}: {dept.Count} employees, ${dept.TotalSalary:N2} total");
    }

    // AsNoTracking demo
    Console.WriteLine("\n--- AsNoTracking Performance ---");
    var trackedCount = context.ChangeTracker.Entries().Count();
    Console.WriteLine($"Tracked entities before: {trackedCount}");

    var untracked = await context.Employees
        .AsNoTracking()
        .ToListAsync();

    trackedCount = context.ChangeTracker.Entries().Count();
    Console.WriteLine($"Tracked entities after AsNoTracking query: {trackedCount}");
    Console.WriteLine("(AsNoTracking queries don't add to change tracker)");

    Console.WriteLine();
}

// ═══════════════════════════════════════════════════════════════════
// LOADING STRATEGIES
// ═══════════════════════════════════════════════════════════════════
static async Task RunLoadingDemo(AppDbContext context)
{
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("                    LOADING STRATEGIES");
    Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

    // Eager Loading
    Console.WriteLine("--- Eager Loading (Include) ---");
    var empWithDept = await context.Employees
        .Include(e => e.Department)
        .FirstAsync();
    Console.WriteLine($"{empWithDept.FullName} works in {empWithDept.Department.Name}");

    // Load department with employees
    Console.WriteLine("\n--- Department with Employees ---");
    var dept = await context.Departments
        .Include(d => d.Employees)
        .FirstAsync(d => d.Name == "Engineering");
    Console.WriteLine($"{dept.Name} department has {dept.Employees.Count} employees:");
    foreach (var e in dept.Employees)
    {
        Console.WriteLine($"  - {e.FullName}");
    }

    // Explicit Loading
    Console.WriteLine("\n--- Explicit Loading ---");
    var department = await context.Departments
        .FirstAsync(d => d.Name == "Human Resources");
    Console.WriteLine($"Before Load - Employees loaded: {department.Employees?.Count ?? 0}");

    await context.Entry(department)
        .Collection(d => d.Employees)
        .LoadAsync();
    Console.WriteLine($"After Load - Employees loaded: {department.Employees.Count}");

    Console.WriteLine();
}
