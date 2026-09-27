# Step 10: Delete Operations - Removing Data

> **Duration:** 15 minutes  
> **Goal:** Understand hard delete, soft delete, and cascade behaviors

---

## 🎯 The DELETE Operation

```
┌─────────────────────────────────────────────────────────────────┐
│                    Delete Flow                                   │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  var emp = await ctx.Employees.FindAsync(1);                    │
│  ctx.Employees.Remove(emp);     ← Mark as Deleted               │
│  await ctx.SaveChangesAsync();  ← DELETE FROM Employees...      │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 💻 Hands-On: Different Delete Patterns

```csharp
// === DELETE OPERATIONS ===
Console.WriteLine("\n=== DELETE OPERATIONS ===\n");

// Method 1: Remove tracked entity
await DeleteTracked(context);

// Method 2: Remove by ID (disconnected)
await DeleteById(context);

// Method 3: Soft delete pattern
await SoftDelete(context);

// Method 4: Bulk delete (EF Core 7+)
await BulkDelete(context);

// Method 5: Cascade delete
await CascadeDeleteDemo(context);

// ==========================================
// DELETE METHOD IMPLEMENTATIONS
// ==========================================

static async Task DeleteTracked(AppDbContext context)
{
    Console.WriteLine("--- Method 1: Delete Tracked Entity ---");

    // Create a test employee to delete
    var testEmp = new Employee
    {
        FirstName = "ToDelete",
        LastName = "Test",
        Email = "delete.me@test.com",
        Salary = 50000,
        DepartmentId = 1
    };
    context.Employees.Add(testEmp);
    await context.SaveChangesAsync();
    Console.WriteLine($"Created test employee ID: {testEmp.EmployeeId}");

    // Standard delete - load then remove
    var employee = await context.Employees.FindAsync(testEmp.EmployeeId);

    if (employee != null)
    {
        // Mark for deletion
        context.Employees.Remove(employee);

        // Check state
        var state = context.Entry(employee).State;
        Console.WriteLine($"Entity state after Remove(): {state}"); // Deleted

        // Execute delete
        await context.SaveChangesAsync();
        Console.WriteLine("Employee deleted");
    }
}

static async Task DeleteById(AppDbContext context)
{
    Console.WriteLine("\n--- Method 2: Delete by ID (No Load) ---");

    // Create test employee
    var testEmp = new Employee
    {
        FirstName = "ToDelete2",
        LastName = "Test",
        Email = "delete2.me@test.com",
        Salary = 50000,
        DepartmentId = 1
    };
    context.Employees.Add(testEmp);
    await context.SaveChangesAsync();
    var idToDelete = testEmp.EmployeeId;

    // Detach to simulate disconnected
    context.Entry(testEmp).State = EntityState.Detached;

    // Delete without loading full entity
    var stub = new Employee { EmployeeId = idToDelete };
    context.Employees.Remove(stub);
    await context.SaveChangesAsync();

    Console.WriteLine($"Deleted employee ID {idToDelete} without loading");
}

static async Task SoftDelete(AppDbContext context)
{
    Console.WriteLine("\n--- Method 3: Soft Delete Pattern ---");

    // Soft delete = set IsActive = false instead of deleting
    var employee = await context.Employees
        .FirstOrDefaultAsync(e => e.IsActive);

    if (employee != null)
    {
        // "Delete" by deactivating
        employee.IsActive = false;
        await context.SaveChangesAsync();

        Console.WriteLine($"Soft deleted {employee.FullName} (set IsActive = false)");

        // Restore for demo
        employee.IsActive = true;
        await context.SaveChangesAsync();
    }
    else
    {
        Console.WriteLine("No active employee found");
    }
}

static async Task BulkDelete(AppDbContext context)
{
    Console.WriteLine("\n--- Method 4: Bulk Delete (EF Core 7+) ---");

    // Create some test data to delete
    var testEmps = Enumerable.Range(1, 5).Select(i => new Employee
    {
        FirstName = $"BulkDelete{i}",
        LastName = "Test",
        Email = $"bulk{i}@test.com",
        Salary = 30000,
        DepartmentId = 1
    });
    context.Employees.AddRange(testEmps);
    await context.SaveChangesAsync();

    // ExecuteDeleteAsync - deletes directly in database
    // No entities loaded!
    var rowsDeleted = await context.Employees
        .Where(e => e.FirstName.StartsWith("BulkDelete"))
        .ExecuteDeleteAsync();

    Console.WriteLine($"Bulk deleted {rowsDeleted} employees");
}

static async Task CascadeDeleteDemo(AppDbContext context)
{
    Console.WriteLine("\n--- Method 5: Cascade Delete Behavior ---");

    // Create a department with employees
    var tempDept = new Department
    {
        Name = "Temporary",
        Budget = 10000,
        Employees = new List<Employee>
        {
            new() { FirstName = "Temp1", LastName = "Worker", Email = "temp1@test.com", Salary = 40000 },
            new() { FirstName = "Temp2", LastName = "Worker", Email = "temp2@test.com", Salary = 40000 }
        }
    };
    context.Departments.Add(tempDept);
    await context.SaveChangesAsync();

    Console.WriteLine($"Created department '{tempDept.Name}' with {tempDept.Employees.Count} employees");

    // Depending on OnDelete behavior:
    // - Cascade: Deletes employees automatically
    // - Restrict: Throws exception if employees exist
    // - SetNull: Sets FK to NULL (requires nullable FK)

    // Our setup uses Restrict, so we must delete employees first
    // Or change relationship configuration

    Console.WriteLine("Note: With Restrict, must delete children first or change config");

    // Clean up - delete employees first
    context.Employees.RemoveRange(tempDept.Employees);
    context.Departments.Remove(tempDept);
    await context.SaveChangesAsync();

    Console.WriteLine("Cleaned up temporary department");
}
```

---

## 📊 Delete Methods Comparison

| Method                 | Use Case              | SQL Generated            |
| ---------------------- | --------------------- | ------------------------ |
| `Remove(entity)`       | Single tracked entity | `DELETE WHERE Id = @id`  |
| `Remove(stub)`         | By ID, no load        | `DELETE WHERE Id = @id`  |
| `RemoveRange(list)`    | Multiple entities     | Multiple DELETEs         |
| `ExecuteDeleteAsync()` | Bulk delete           | Single DELETE with WHERE |

---

## 🔄 Cascade Delete Behaviors

```
┌─────────────────────────────────────────────────────────────────┐
│                    OnDelete Behaviors                            │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Cascade (default for required relationships)                   │
│  ├── Delete parent → Children deleted automatically             │
│  └── DELETE FROM Departments → Employees also deleted           │
│                                                                  │
│  Restrict                                                        │
│  ├── Delete parent with children → Exception thrown             │
│  └── Must delete children first                                 │
│                                                                  │
│  SetNull (only for optional relationships)                      │
│  ├── Delete parent → Children's FK set to NULL                  │
│  └── DELETE FROM Departments → Employees.DeptId = NULL          │
│                                                                  │
│  ClientSetNull                                                   │
│  └── SetNull but only for tracked entities                      │
│                                                                  │
│  NoAction                                                        │
│  └── Database handles it (depends on DB constraints)            │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### Configure in Fluent API:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Employee>()
        .HasOne(e => e.Department)
        .WithMany(d => d.Employees)
        .HasForeignKey(e => e.DepartmentId)
        .OnDelete(DeleteBehavior.Restrict);  // or Cascade, SetNull, etc.
}
```

---

## 🛡️ Soft Delete Pattern (Best Practice)

Instead of actually deleting records, mark them as inactive:

```csharp
// Entity with soft delete support
public class Employee
{
    // ... other properties
    public bool IsActive { get; set; } = true;
    public DateTime? DeletedAt { get; set; }
}

// Global query filter (always exclude soft-deleted)
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Employee>()
        .HasQueryFilter(e => e.IsActive);
}

// "Delete" = set inactive
public async Task SoftDeleteEmployee(int id)
{
    var emp = await _context.Employees.FindAsync(id);
    if (emp != null)
    {
        emp.IsActive = false;
        emp.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}

// Query includes filter automatically
var activeEmployees = await _context.Employees.ToListAsync();
// Only active employees returned

// To include deleted:
var allEmployees = await _context.Employees
    .IgnoreQueryFilters()
    .ToListAsync();
```

---

## ❓ Interview Questions

### Q1: What's the difference between `Remove()` and `ExecuteDeleteAsync()`?

**Answer:**

> | Remove()                          | ExecuteDeleteAsync()       |
> | --------------------------------- | -------------------------- |
> | Requires entity (tracked or stub) | Just needs LINQ expression |
> | Goes through change tracker       | Direct SQL execution       |
> | Triggers entity events            | Bypasses events            |
> | One DELETE per entity             | Single DELETE statement    |
> | Good for single deletes           | Good for bulk deletes      |
>
> ```csharp
> // Remove - loads entity, tracks, deletes
> ctx.Remove(employee);
>
> // ExecuteDelete - direct SQL
> await ctx.Employees.Where(e => e.DeptId == 1).ExecuteDeleteAsync();
> ```

---

### Q2: What is cascade delete and how do you configure it?

**Answer:**

> Cascade delete automatically deletes child records when parent is deleted.
>
> **Configure in Fluent API:**
>
> ```csharp
> modelBuilder.Entity<Employee>()
>     .HasOne(e => e.Department)
>     .WithMany(d => d.Employees)
>     .OnDelete(DeleteBehavior.Cascade); // or Restrict, SetNull
> ```
>
> **Behaviors:**
>
> - `Cascade`: Delete children with parent
> - `Restrict`: Prevent delete if children exist
> - `SetNull`: Set FK to null on children

---

### Q3: What is soft delete and why use it?

**Answer:**

> Soft delete marks records as "deleted" without removing them:
>
> **Benefits:**
>
> - Data recovery possible
> - Audit trail preserved
> - Referential integrity maintained
> - Historical reporting still works
>
> **Implementation:**
>
> ```csharp
> // Flag instead of delete
> employee.IsActive = false;
> employee.DeletedAt = DateTime.UtcNow;
>
> // Global filter to hide soft-deleted
> modelBuilder.Entity<Employee>()
>     .HasQueryFilter(e => e.IsActive);
> ```

---

### Q4: How do you delete by ID without loading the entity?

**Answer:**

> Create a "stub" entity with just the ID:
>
> ```csharp
> // Option 1: Stub entity
> var stub = new Employee { EmployeeId = idToDelete };
> context.Employees.Remove(stub);
> await context.SaveChangesAsync();
>
> // Option 2: ExecuteDeleteAsync (EF Core 7+)
> await context.Employees
>     .Where(e => e.EmployeeId == idToDelete)
>     .ExecuteDeleteAsync();
> ```
>
> Both generate: `DELETE FROM Employees WHERE EmployeeId = @id`

---

### Q5: What happens if you delete a parent with children and cascade is disabled?

**Answer:**

> You get a **foreign key constraint violation** exception:
>
> ```csharp
> // With Restrict/NoAction
> try
> {
>     context.Departments.Remove(deptWithEmployees);
>     await context.SaveChangesAsync();
> }
> catch (DbUpdateException ex)
> {
>     // SqlException: REFERENCE constraint conflict
>     // Cannot delete because Employees reference this Department
> }
> ```
>
> **Solutions:**
>
> - Delete children first
> - Change to `DeleteBehavior.Cascade`
> - Change to `DeleteBehavior.SetNull` (if FK nullable)

---

## ✍️ Exercise: Delete Practice

### Task 1: Standard Delete

Delete an employee by loading and removing.

### Task 2: Delete by ID

Delete an employee without loading the full entity.

### Task 3: Implement Soft Delete

Add `IsDeleted` flag and implement soft delete for Department.

### Task 4: Test Cascade

1. Create a Department with Employees
2. Try to delete the Department
3. Observe the behavior based on your OnDelete configuration

### Task 5: Bulk Delete

Delete all employees with salary < 40000 using `ExecuteDeleteAsync()`.

---

## ✅ Checkpoint

You should now understand:

- Different delete methods
- Cascade delete behaviors
- Soft delete pattern
- Bulk delete with `ExecuteDeleteAsync()`

---

**Next Step:** [../05-Querying/IQueryableVsIEnumerable.md](../05-Querying/IQueryableVsIEnumerable.md) - Deep dive into querying
