# SaveChanges Internals

> **Duration:** 15 minutes  
> **Goal:** Understand what happens when SaveChanges() is called

---

## 🎯 SaveChanges Pipeline

```
┌─────────────────────────────────────────────────────────────────┐
│                    SaveChanges() Flow                            │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  1. DetectChanges()                                             │
│     └── Compares current vs original values                     │
│     └── Updates entity states                                   │
│                                                                  │
│  2. Validate Changes                                            │
│     └── Check required fields                                   │
│     └── Validate relationships                                  │
│                                                                  │
│  3. Generate SQL Commands                                       │
│     ├── Added → INSERT                                          │
│     ├── Modified → UPDATE (only changed columns)                │
│     └── Deleted → DELETE                                        │
│                                                                  │
│  4. Begin Transaction (implicit)                                │
│                                                                  │
│  5. Execute Commands                                            │
│     └── Send SQL to database                                    │
│     └── Retrieve generated values (IDs, timestamps)             │
│                                                                  │
│  6. Commit Transaction                                          │
│                                                                  │
│  7. Update Entity States → Unchanged                            │
│                                                                  │
│  8. Return number of affected rows                              │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 💻 Hands-On: Observing SaveChanges

```csharp
// === SaveChanges Internals Demo ===
Console.WriteLine("\n=== SAVECHANGES INTERNALS ===\n");

await SaveChangesDemo(context);

static async Task SaveChangesDemo(AppDbContext context)
{
    Console.WriteLine("--- Before Any Changes ---");
    PrintTrackedEntities(context);

    // Make various changes
    var existingEmp = await context.Employees.FirstAsync();
    existingEmp.Salary += 500;  // Modified

    var newEmp = new Employee
    {
        FirstName = "New",
        LastName = "Employee",
        Email = $"new{DateTime.Now.Ticks}@test.com",
        Salary = 50000,
        DepartmentId = existingEmp.DepartmentId
    };
    context.Employees.Add(newEmp);  // Added

    Console.WriteLine("\n--- After Changes, Before SaveChanges ---");
    PrintTrackedEntities(context);

    // SaveChanges
    Console.WriteLine("\n--- Calling SaveChangesAsync ---");
    var rowsAffected = await context.SaveChangesAsync();

    Console.WriteLine($"\nRows affected: {rowsAffected}");
    Console.WriteLine($"New employee ID: {newEmp.EmployeeId}");  // Generated ID populated

    Console.WriteLine("\n--- After SaveChanges ---");
    PrintTrackedEntities(context);

    // Cleanup
    context.Employees.Remove(newEmp);
    await context.SaveChangesAsync();
}

static void PrintTrackedEntities(AppDbContext context)
{
    foreach (var entry in context.ChangeTracker.Entries<Employee>())
    {
        Console.WriteLine($"  {entry.Entity.FullName}: {entry.State}");
    }
    if (!context.ChangeTracker.Entries().Any())
    {
        Console.WriteLine("  (no tracked entities)");
    }
}
```

---

## 🔄 Transaction Behavior

### Default: Implicit Transaction

```csharp
// SaveChanges wraps all changes in a single transaction
context.Employees.Add(emp1);
context.Employees.Add(emp2);
context.Departments.Add(dept);

await context.SaveChangesAsync();
// All succeed or all fail together
```

### Explicit Transaction

```csharp
using var transaction = await context.Database.BeginTransactionAsync();

try
{
    // First operation
    var dept = new Department { Name = "New Dept" };
    context.Departments.Add(dept);
    await context.SaveChangesAsync();

    // Second operation (uses dept.DepartmentId)
    var emp = new Employee
    {
        FirstName = "Test",
        DepartmentId = dept.DepartmentId  // FK from first save
    };
    context.Employees.Add(emp);
    await context.SaveChangesAsync();

    // Commit both
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

---

## 📊 Return Value

```csharp
// SaveChanges returns count of rows affected
var emp1 = new Employee { ... };
var emp2 = new Employee { ... };
context.Employees.AddRange(emp1, emp2);

var existingEmp = await context.Employees.FirstAsync();
existingEmp.Salary += 1000;

var rowsAffected = await context.SaveChangesAsync();
Console.WriteLine(rowsAffected);  // 3 (2 inserts + 1 update)
```

---

## 🎯 Execution Order

EF Core executes changes in this order:

1. **INSERTs** (parents before children due to FK constraints)
2. **UPDATEs**
3. **DELETEs** (children before parents)

```csharp
// EF Core figures out correct order
context.Employees.Add(new Employee { DepartmentId = 1 });  // Needs Dept first
context.Departments.Add(new Department { });               // Will be inserted first

await context.SaveChangesAsync();
// Inserts Department, gets ID, then inserts Employee with FK
```

---

## 🔧 Generated Values After Save

```csharp
var emp = new Employee { FirstName = "Test" };
Console.WriteLine(emp.EmployeeId);  // 0 (not set)

context.Employees.Add(emp);
Console.WriteLine(emp.EmployeeId);  // Still 0

await context.SaveChangesAsync();
Console.WriteLine(emp.EmployeeId);  // 42 (from database)

// Same for computed columns, timestamps, etc.
```

---

## ⚠️ Common Issues

### 1. DbUpdateConcurrencyException

```csharp
// Two users editing same record
try
{
    await context.SaveChangesAsync();
}
catch (DbUpdateConcurrencyException ex)
{
    // Another user modified/deleted this record
    var entry = ex.Entries.Single();

    // Option 1: Reload and retry
    await entry.ReloadAsync();

    // Option 2: Get database values
    var dbValues = await entry.GetDatabaseValuesAsync();
}
```

### 2. DbUpdateException

```csharp
try
{
    await context.SaveChangesAsync();
}
catch (DbUpdateException ex)
{
    // Constraint violation, connection error, etc.
    var sqlException = ex.InnerException;
    // Handle specific error
}
```

### 3. Validation Errors

```csharp
// Required field missing
var emp = new Employee { FirstName = null };  // FirstName is [Required]
context.Employees.Add(emp);
await context.SaveChangesAsync();  // Throws DbUpdateException
```

---

## 🔄 Interceptors (Advanced)

```csharp
// Customize SaveChanges behavior
public class AuditInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        var context = eventData.Context!;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                // Set CreatedAt
                if (entry.Entity is IHasCreatedAt entity)
                    entity.CreatedAt = DateTime.UtcNow;
            }
            if (entry.State == EntityState.Modified)
            {
                // Set ModifiedAt
                if (entry.Entity is IHasModifiedAt entity)
                    entity.ModifiedAt = DateTime.UtcNow;
            }
        }

        return base.SavingChanges(eventData, result);
    }
}

// Register interceptor
optionsBuilder.AddInterceptors(new AuditInterceptor());
```

---

## ❓ Interview Questions

### Q1: What does SaveChanges() do internally?

**Answer:**

> 1. **DetectChanges()** - Compares current vs original values
> 2. **Validation** - Checks constraints
> 3. **SQL Generation** - Creates INSERT/UPDATE/DELETE
> 4. **Transaction** - Wraps in implicit transaction
> 5. **Execution** - Sends SQL to database
> 6. **Update State** - All entities become Unchanged
> 7. **Return** - Number of affected rows

---

### Q2: Is SaveChanges transactional by default?

**Answer:**

> **Yes!** SaveChanges wraps all changes in an implicit transaction:
>
> ```csharp
> context.Employees.Add(emp1);
> context.Employees.Add(emp2);
> await context.SaveChangesAsync();
> // Either both are saved, or neither
> ```
>
> For multiple SaveChanges calls to be in one transaction, use explicit transaction:
>
> ```csharp
> using var tx = await ctx.Database.BeginTransactionAsync();
> // multiple SaveChanges...
> await tx.CommitAsync();
> ```

---

### Q3: What is the return value of SaveChanges?

**Answer:**

> Returns the **number of state entries written to the database**:
>
> ```csharp
> context.Add(emp1);    // +1
> context.Add(emp2);    // +1
> emp3.Salary = 80000;  // +1 (Modified)
> context.Remove(emp4); // +1
>
> var count = await context.SaveChangesAsync();
> // count = 4
> ```

---

### Q4: When are generated values (like IDs) populated?

**Answer:**

> Generated values are populated **after SaveChanges completes**:
>
> ```csharp
> var emp = new Employee { Name = "Test" };
> // emp.EmployeeId = 0
>
> context.Add(emp);
> // emp.EmployeeId = 0 (still)
>
> await context.SaveChangesAsync();
> // emp.EmployeeId = 42 (populated from DB)
> ```
>
> EF Core retrieves the ID using `SCOPE_IDENTITY()` or `OUTPUT` clause.

---

### Q5: How do you handle SaveChanges exceptions?

**Answer:**

> Common exceptions and handling:
>
> ```csharp
> try
> {
>     await context.SaveChangesAsync();
> }
> catch (DbUpdateConcurrencyException)
> {
>     // Optimistic concurrency conflict
>     // Reload and retry or notify user
> }
> catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx)
> {
>     if (sqlEx.Number == 2627)  // Unique constraint
>         // Handle duplicate
>     else if (sqlEx.Number == 547)  // FK constraint
>         // Handle invalid reference
> }
> ```

---

## ✍️ Exercise

### Task 1: Transaction Demo

Create a scenario where you need two SaveChanges calls in one transaction (e.g., create Department, then Employee referencing it).

### Task 2: Error Handling

Intentionally cause a constraint violation and handle the exception gracefully.

### Task 3: Observe Generated SQL

Enable logging and observe the SQL generated during SaveChanges for:

- One INSERT
- One UPDATE
- One DELETE

---

## ✅ Key Takeaways

1. SaveChanges is transactional by default
2. Changes are ordered correctly (FK dependencies)
3. Generated values are populated after save
4. Handle concurrency and constraint exceptions
5. Use interceptors for cross-cutting concerns

---

**Next Step:** [../07-Relationships/OneToMany.md](../07-Relationships/OneToMany.md) - Relationship configuration
