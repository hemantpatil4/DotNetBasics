# Step 7: Create Operations - Adding Data

> **Duration:** 20 minutes  
> **Goal:** Master different ways to insert data with EF Core

---

## 🎯 The CREATE Operation

```
┌─────────────────────────────────────────────────────────────────┐
│                    C# to Database                                │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  var emp = new Employee { ... };   ← Create C# object           │
│  context.Employees.Add(emp);       ← Track as "Added"           │
│  context.SaveChanges();            ← INSERT INTO Employees      │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 💻 Hands-On: Different Ways to Insert

### Update `Program.cs` for Testing

```csharp
using Microsoft.EntityFrameworkCore;
using EFCoreDemoApp.Data;
using EFCoreDemoApp.Entities;

// Create DbContext with connection string
var builder = new DbContextOptionsBuilder<AppDbContext>();

// For Mac (Docker SQL Server)
builder.UseSqlServer("Server=localhost,1433;Database=EFCoreDemoDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;");

// For Windows (LocalDB)
// builder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=EFCoreDemoDB;Trusted_Connection=True;");

using var context = new AppDbContext(builder.Options);

// Ensure database is created
context.Database.EnsureCreated();

// === CREATE OPERATIONS ===
Console.WriteLine("=== CREATE OPERATIONS ===\n");

// Method 1: Add single entity
await AddSingleEmployee(context);

// Method 2: Add multiple entities
await AddMultipleEmployees(context);

// Method 3: Add with relationship
await AddWithRelationship(context);

// Method 4: Add range
await AddRange(context);

Console.WriteLine("\n✅ All create operations completed!");

// ==========================================
// METHOD IMPLEMENTATIONS
// ==========================================

static async Task AddSingleEmployee(AppDbContext context)
{
    Console.WriteLine("--- Method 1: Add Single Entity ---");

    // First, ensure we have a department
    var dept = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Engineering");
    if (dept == null)
    {
        dept = new Department { Name = "Engineering", Budget = 100000 };
        context.Departments.Add(dept);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created Department: {dept.Name} (Id: {dept.DepartmentId})");
    }

    // Create employee
    var employee = new Employee
    {
        FirstName = "John",
        LastName = "Doe",
        Email = "john.doe@company.com",
        Salary = 75000,
        DepartmentId = dept.DepartmentId
    };

    // Track the entity
    context.Employees.Add(employee);

    // EmployeeId is 0 before save
    Console.WriteLine($"Before SaveChanges - EmployeeId: {employee.EmployeeId}");

    // Save to database
    await context.SaveChangesAsync();

    // EmployeeId is populated after save
    Console.WriteLine($"After SaveChanges - EmployeeId: {employee.EmployeeId}");
}

static async Task AddMultipleEmployees(AppDbContext context)
{
    Console.WriteLine("\n--- Method 2: Add Multiple Entities ---");

    var dept = await context.Departments.FirstAsync(d => d.Name == "Engineering");

    var employees = new List<Employee>
    {
        new() { FirstName = "Jane", LastName = "Smith", Email = "jane.smith@company.com", Salary = 80000, DepartmentId = dept.DepartmentId },
        new() { FirstName = "Bob", LastName = "Johnson", Email = "bob.johnson@company.com", Salary = 65000, DepartmentId = dept.DepartmentId }
    };

    // Add all at once
    context.Employees.AddRange(employees);

    // Single SaveChanges for all inserts
    var count = await context.SaveChangesAsync();
    Console.WriteLine($"Inserted {count} employees");
}

static async Task AddWithRelationship(AppDbContext context)
{
    Console.WriteLine("\n--- Method 3: Add with Relationship ---");

    // Create department with employees in one go
    var hrDept = new Department
    {
        Name = "Human Resources",
        Budget = 50000,
        Employees = new List<Employee>
        {
            new() { FirstName = "Alice", LastName = "Brown", Email = "alice.brown@company.com", Salary = 60000 },
            new() { FirstName = "Charlie", LastName = "Wilson", Email = "charlie.wilson@company.com", Salary = 55000 }
        }
    };

    // Only add the parent - children are tracked automatically!
    context.Departments.Add(hrDept);
    await context.SaveChangesAsync();

    Console.WriteLine($"Created {hrDept.Name} with {hrDept.Employees.Count} employees");

    // FK is automatically set!
    foreach (var emp in hrDept.Employees)
    {
        Console.WriteLine($"  - {emp.FullName}: DepartmentId = {emp.DepartmentId}");
    }
}

static async Task AddRange(AppDbContext context)
{
    Console.WriteLine("\n--- Method 4: AddRange vs Add ---");

    var dept = await context.Departments.FirstAsync(d => d.Name == "Engineering");

    // AddRange is more efficient for bulk inserts
    var newEmployees = Enumerable.Range(1, 5)
        .Select(i => new Employee
        {
            FirstName = $"Employee{i}",
            LastName = "Test",
            Email = $"emp{i}@test.com",
            Salary = 50000 + (i * 1000),
            DepartmentId = dept.DepartmentId
        });

    context.Employees.AddRange(newEmployees);
    var count = await context.SaveChangesAsync();

    Console.WriteLine($"Bulk inserted {count} employees");
}
```

---

## 🔍 What Happens During Add & SaveChanges?

### The Change Tracker Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                    Add() + SaveChanges()                         │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  1. context.Add(entity)                                         │
│     └── Entity state = EntityState.Added                        │
│     └── Change Tracker starts tracking                          │
│                                                                  │
│  2. context.SaveChanges()                                       │
│     └── EF Core generates: INSERT INTO ... VALUES (...)         │
│     └── Executes SQL                                            │
│     └── Retrieves generated ID (SCOPE_IDENTITY)                 │
│     └── Updates entity's ID property                            │
│     └── Entity state = EntityState.Unchanged                    │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### Generated SQL Example

```sql
-- What EF Core generates for Add()
SET NOCOUNT ON;
INSERT INTO [Employees] ([DepartmentId], [Email], [FirstName],
                         [IsActive], [JoinDate], [LastName], [Salary])
VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6);
SELECT [EmployeeId]
FROM [Employees]
WHERE @@ROWCOUNT = 1 AND [EmployeeId] = scope_identity();

-- @p0 = 1 (DepartmentId)
-- @p1 = 'john@company.com'
-- ... etc
```

---

## 📊 Add Methods Comparison

| Method                                | Use Case          | Notes                |
| ------------------------------------- | ----------------- | -------------------- |
| `Add(entity)`                         | Single entity     | Marks as Added       |
| `AddRange(entities)`                  | Multiple entities | More efficient batch |
| `AddAsync(entity)`                    | Single + async    | Async operation      |
| `AddRangeAsync(entities)`             | Multiple + async  | Async batch          |
| `context.Entry(entity).State = Added` | Manual control    | Low-level access     |

---

## 🎯 Entity State After Add

```csharp
var emp = new Employee { FirstName = "Test" };

Console.WriteLine(context.Entry(emp).State);  // Detached

context.Employees.Add(emp);
Console.WriteLine(context.Entry(emp).State);  // Added

await context.SaveChangesAsync();
Console.WriteLine(context.Entry(emp).State);  // Unchanged
```

---

## ⚠️ Common Gotchas

### 1. Forgetting SaveChanges

```csharp
// ❌ WRONG - Nothing saved!
context.Employees.Add(employee);
// Missing SaveChanges!

// ✅ CORRECT
context.Employees.Add(employee);
await context.SaveChangesAsync();
```

### 2. Adding Duplicate Entries

```csharp
// ❌ This creates duplicate if called twice
var emp = new Employee { FirstName = "John" };
context.Add(emp);
await context.SaveChangesAsync();

context.Add(emp);  // Creates ANOTHER record!
await context.SaveChangesAsync();

// ✅ Check if exists first
if (!await context.Employees.AnyAsync(e => e.Email == email))
{
    context.Add(employee);
    await context.SaveChangesAsync();
}
```

### 3. ID Set Before Save

```csharp
// ❌ Don't set identity columns manually
var emp = new Employee
{
    EmployeeId = 100,  // EF will ignore this!
    FirstName = "Test"
};

// ✅ Let database generate
var emp = new Employee
{
    FirstName = "Test"
    // EmployeeId auto-generated
};
```

---

## ❓ Interview Questions

### Q1: What does `Add()` do in EF Core?

**Answer:**

> `Add()` does NOT insert data immediately. It:
>
> 1. Starts tracking the entity
> 2. Sets entity state to `EntityState.Added`
> 3. Waits for `SaveChanges()` to generate INSERT SQL
>
> The actual database insert happens only on `SaveChanges()`.

---

### Q2: When is the primary key populated?

**Answer:**

> For identity columns, the ID is populated **after** `SaveChanges()`:
>
> ```csharp
> var emp = new Employee { Name = "John" };
> // emp.EmployeeId = 0
>
> context.Add(emp);
> // emp.EmployeeId = 0 (still)
>
> await context.SaveChangesAsync();
> // emp.EmployeeId = 42 (populated from database)
> ```

---

### Q3: What's the difference between `Add()` and `AddRange()`?

**Answer:**

> | Add()                                       | AddRange()              |
> | ------------------------------------------- | ----------------------- |
> | Single entity                               | Multiple entities       |
> | Multiple calls = multiple change detections | Single detection pass   |
> | Less efficient for bulk                     | More efficient for bulk |
>
> For 100+ entities, `AddRange()` is significantly faster.

---

### Q4: How does EF Core handle relationships on Add?

**Answer:**

> When you add a parent with children:
>
> ```csharp
> var dept = new Department {
>     Name = "HR",
>     Employees = new List<Employee> { new Employee { Name = "John" } }
> };
> context.Add(dept);
> ```
>
> EF Core:
>
> 1. Inserts Department first (gets DepartmentId)
> 2. Sets DepartmentId on all Employees
> 3. Inserts Employees with correct FK

---

### Q5: How to do a "bulk insert" efficiently?

**Answer:**

> For large datasets (1000+ rows):
>
> ```csharp
> // Option 1: AddRange (moderate speed)
> context.AddRange(entities);
> await context.SaveChangesAsync();
>
> // Option 2: Disable tracking (faster)
> context.ChangeTracker.AutoDetectChangesEnabled = false;
> context.AddRange(entities);
> await context.SaveChangesAsync();
>
> // Option 3: Use EF Core.BulkExtensions (fastest)
> await context.BulkInsertAsync(entities);
> ```

---

## ✍️ Exercise: Practice Inserts

### Task 1: Run the Code

```bash
cd EFCoreDemoApp
dotnet run
```

Observe the output showing different insert methods.

### Task 2: Check Database

Use SQL Server Management Studio or Azure Data Studio:

```sql
SELECT * FROM Departments;
SELECT * FROM Employees;
```

### Task 3: Create Your Own

Add a new Department "Finance" with 3 employees in a single `SaveChanges()`.

### Task 4: Debug Challenge

What happens if you:

```csharp
context.Add(employee);
context.Add(employee);  // Same instance twice
await context.SaveChangesAsync();
```

---

## ✅ Checkpoint

After running the program, your database should have:

- Multiple Departments
- Multiple Employees with correct FK relationships

---

**Next Step:** [ReadOperations.md](./ReadOperations.md) - Querying data
