# Step 9: Update Operations - Modifying Data

> **Duration:** 20 minutes  
> **Goal:** Learn different patterns for updating entities

---

## 🎯 The UPDATE Operation

```
┌─────────────────────────────────────────────────────────────────┐
│                    Update Flow                                   │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  1. Retrieve entity (now tracked)                               │
│  2. Modify properties                                           │
│  3. SaveChanges() detects changes                               │
│  4. EF generates UPDATE statement                               │
│                                                                  │
│  var emp = await ctx.Employees.FindAsync(1);                    │
│  emp.Salary = 80000;            ← Change Tracker detects this   │
│  await ctx.SaveChangesAsync();  ← UPDATE Employees SET...       │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 💻 Hands-On: Different Update Patterns

```csharp
// === UPDATE OPERATIONS ===
Console.WriteLine("\n=== UPDATE OPERATIONS ===\n");

// Method 1: Connected scenario (fetch, modify, save)
await UpdateConnected(context);

// Method 2: Disconnected scenario (Update method)
await UpdateDisconnected(context);

// Method 3: Update specific properties only
await UpdateSpecificProperties(context);

// Method 4: Bulk update (EF Core 7+)
await BulkUpdate(context);

// ==========================================
// UPDATE METHOD IMPLEMENTATIONS
// ==========================================

static async Task UpdateConnected(AppDbContext context)
{
    Console.WriteLine("--- Method 1: Connected Update ---");

    // Step 1: Retrieve (entity is now tracked)
    var employee = await context.Employees.FirstOrDefaultAsync();

    if (employee == null)
    {
        Console.WriteLine("No employee found");
        return;
    }

    Console.WriteLine($"Before: {employee.FullName}, Salary: ${employee.Salary:N0}");

    // Step 2: Modify (Change Tracker detects this)
    var oldSalary = employee.Salary;
    employee.Salary = employee.Salary * 1.10m; // 10% raise

    // Check entity state
    var state = context.Entry(employee).State;
    Console.WriteLine($"Entity state: {state}"); // Modified

    // Step 3: Save (generates UPDATE statement)
    await context.SaveChangesAsync();

    Console.WriteLine($"After: {employee.FullName}, Salary: ${employee.Salary:N0}");

    // Restore for demo
    employee.Salary = oldSalary;
    await context.SaveChangesAsync();
}

static async Task UpdateDisconnected(AppDbContext context)
{
    Console.WriteLine("\n--- Method 2: Disconnected Update ---");

    // Simulate receiving entity from API/UI (not tracked)
    var employeeFromClient = new Employee
    {
        EmployeeId = 1,  // Must have the ID
        FirstName = "John",
        LastName = "Doe",
        Email = "john.doe@company.com",
        Salary = 85000,
        DepartmentId = 1,
        IsActive = true,
        JoinDate = DateTime.Now
    };

    // Option A: Update (marks ALL properties as modified)
    context.Employees.Update(employeeFromClient);

    var state = context.Entry(employeeFromClient).State;
    Console.WriteLine($"Entity state after Update(): {state}"); // Modified

    // This generates UPDATE for ALL columns
    await context.SaveChangesAsync();

    Console.WriteLine("Updated via Update() method");
}

static async Task UpdateSpecificProperties(AppDbContext context)
{
    Console.WriteLine("\n--- Method 3: Update Specific Properties ---");

    // When you only want to update certain columns
    var employeeId = 1;

    // Attach (starts tracking without loading)
    var employee = new Employee { EmployeeId = employeeId };
    context.Employees.Attach(employee);

    // Mark specific property as modified
    employee.Salary = 90000;
    context.Entry(employee).Property(e => e.Salary).IsModified = true;

    // Only Salary column will be in UPDATE statement
    await context.SaveChangesAsync();

    Console.WriteLine("Updated only Salary property");

    // Verify
    var updated = await context.Employees.FindAsync(employeeId);
    Console.WriteLine($"New salary: ${updated?.Salary:N0}");
}

static async Task BulkUpdate(AppDbContext context)
{
    Console.WriteLine("\n--- Method 4: Bulk Update (EF Core 7+) ---");

    // ExecuteUpdateAsync - Updates directly in database
    // No entities loaded into memory!

    // Give 5% raise to all employees in Engineering
    var rowsAffected = await context.Employees
        .Where(e => e.Department.Name == "Engineering")
        .ExecuteUpdateAsync(setters => setters
            .SetProperty(e => e.Salary, e => e.Salary * 1.05m));

    Console.WriteLine($"Bulk updated {rowsAffected} employees");

    // Multiple properties
    await context.Employees
        .Where(e => e.IsActive == false)
        .ExecuteUpdateAsync(setters => setters
            .SetProperty(e => e.Salary, 0)
            .SetProperty(e => e.Email, e => "archived_" + e.Email));
}
```

---

## 🔍 How Change Tracking Detects Updates

```
┌─────────────────────────────────────────────────────────────────┐
│                    Change Detection                              │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  When you query: var emp = ctx.Employees.Find(1)                │
│  ├── EF stores "original values" snapshot                       │
│  └── Entity state = Unchanged                                   │
│                                                                  │
│  When you modify: emp.Salary = 80000                            │
│  └── Entity state still "Unchanged" (not detected yet)          │
│                                                                  │
│  When SaveChanges() called:                                     │
│  ├── DetectChanges() compares current vs original               │
│  ├── Finds Salary changed: 75000 → 80000                        │
│  ├── Entity state = Modified                                    │
│  ├── Generates: UPDATE SET Salary = 80000 WHERE Id = 1          │
│  └── After save: Entity state = Unchanged                       │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📊 Update Methods Comparison

| Method                  | Use Case           | What It Does                   |
| ----------------------- | ------------------ | ------------------------------ |
| Modify tracked entity   | Connected scenario | Only changed columns in UPDATE |
| `Update(entity)`        | Disconnected       | ALL columns in UPDATE          |
| `Attach()` + IsModified | Specific columns   | Only marked columns in UPDATE  |
| `ExecuteUpdateAsync()`  | Bulk updates       | Direct SQL, no tracking        |

---

## 🎯 Entity States During Update

```csharp
// Connected scenario
var emp = await ctx.Employees.FindAsync(1);
// State: Unchanged

emp.Salary = 80000;
// State: Unchanged (not detected yet)

ctx.ChangeTracker.DetectChanges(); // or SaveChanges
// State: Modified

await ctx.SaveChangesAsync();
// State: Unchanged (after save)
```

```csharp
// Disconnected scenario
var emp = new Employee { EmployeeId = 1, ... };
// State: Detached

ctx.Update(emp);
// State: Modified (ALL properties marked modified)
```

---

## ⚠️ Common Update Gotchas

### 1. Update() Updates ALL Columns

```csharp
// ❌ PROBLEM: Updates all columns even if only one changed
var emp = new Employee { EmployeeId = 1, Salary = 80000 };
context.Update(emp);  // ALL columns will be SET, others become default!

// ✅ SOLUTION: Attach and mark specific property
var emp = new Employee { EmployeeId = 1 };
context.Attach(emp);
emp.Salary = 80000;
context.Entry(emp).Property(e => e.Salary).IsModified = true;
```

### 2. Forgetting SaveChanges

```csharp
// ❌ Changes lost!
employee.Salary = 80000;
// Missing SaveChanges()

// ✅ Correct
employee.Salary = 80000;
await context.SaveChangesAsync();
```

### 3. Concurrency Without Detection

```csharp
// User A reads employee
var empA = await ctxA.Employees.FindAsync(1); // Salary = 70000

// User B reads same employee
var empB = await ctxB.Employees.FindAsync(1); // Salary = 70000

// User A updates
empA.Salary = 75000;
await ctxA.SaveChangesAsync();  // Saved: 75000

// User B updates (overwrites User A's change!)
empB.Salary = 72000;
await ctxB.SaveChangesAsync();  // Saved: 72000 ← User A's raise lost!
```

---

## 🔐 Optimistic Concurrency

Add RowVersion for concurrency checking:

```csharp
public class Employee
{
    // ... other properties

    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;
}
```

```csharp
// Now concurrent updates throw DbUpdateConcurrencyException
try
{
    await context.SaveChangesAsync();
}
catch (DbUpdateConcurrencyException ex)
{
    // Handle conflict - reload, merge, or inform user
    var entry = ex.Entries.Single();
    var databaseValues = await entry.GetDatabaseValuesAsync();
    // Decide how to handle the conflict
}
```

---

## ❓ Interview Questions

### Q1: How does EF Core detect changes?

**Answer:**

> EF Core uses **snapshot-based change tracking**:
>
> 1. When entity is queried, original values are stored
> 2. `DetectChanges()` compares current vs original values
> 3. Changed properties are marked as modified
> 4. `SaveChanges()` generates UPDATE for only modified columns
>
> This happens automatically before SaveChanges().

---

### Q2: What's the difference between `Update()` and modifying a tracked entity?

**Answer:**

> | Modifying Tracked Entity       | Using Update()             |
> | ------------------------------ | -------------------------- |
> | Only changed columns in UPDATE | ALL columns in UPDATE      |
> | Requires loading entity first  | Works with detached entity |
> | Uses change detection          | Marks everything modified  |
> | More efficient SQL             | Less efficient SQL         |
>
> ```csharp
> // Tracked: UPDATE SET Salary = 80000
> var emp = await ctx.FindAsync(1);
> emp.Salary = 80000;
>
> // Update(): UPDATE SET FirstName=, LastName=, Salary=, ...
> ctx.Update(detachedEmployee);
> ```

---

### Q3: How do you update only specific columns in a disconnected scenario?

**Answer:**

> ```csharp
> // Option 1: Attach and mark modified
> var emp = new Employee { EmployeeId = 1 };
> context.Attach(emp);
> emp.Salary = 80000;
> context.Entry(emp).Property(e => e.Salary).IsModified = true;
>
> // Option 2: ExecuteUpdateAsync (EF Core 7+)
> await context.Employees
>     .Where(e => e.EmployeeId == 1)
>     .ExecuteUpdateAsync(s => s.SetProperty(e => e.Salary, 80000));
> ```

---

### Q4: What is `ExecuteUpdateAsync()` and when should you use it?

**Answer:**

> `ExecuteUpdateAsync()` (EF Core 7+) executes UPDATE directly in the database:
>
> **Benefits:**
>
> - No entities loaded into memory
> - Single SQL statement for bulk updates
> - Better performance for mass updates
>
> **Drawbacks:**
>
> - Bypasses change tracker
> - No entity events fired
> - Can't use navigation properties in setter
>
> ```csharp
> // Updates 1000 employees with ONE SQL statement
> await ctx.Employees
>     .Where(e => e.DepartmentId == 1)
>     .ExecuteUpdateAsync(s => s.SetProperty(e => e.Salary, e => e.Salary * 1.1m));
> ```

---

### Q5: How do you handle concurrency conflicts?

**Answer:**

> Add `[Timestamp]` or `[ConcurrencyCheck]`:
>
> ```csharp
> [Timestamp]
> public byte[] RowVersion { get; set; }
> ```
>
> EF adds `WHERE RowVersion = @originalVersion` to UPDATE.
>
> If another user changed the row, `DbUpdateConcurrencyException` is thrown:
>
> ```csharp
> catch (DbUpdateConcurrencyException ex)
> {
>     var entry = ex.Entries.Single();
>
>     // Option 1: Database wins (reload)
>     await entry.ReloadAsync();
>
>     // Option 2: Client wins (override)
>     entry.OriginalValues.SetValues(await entry.GetDatabaseValuesAsync());
>     await ctx.SaveChangesAsync();
>
>     // Option 3: Merge or inform user
> }
> ```

---

## ✍️ Exercise: Update Practice

### Task 1: Connected Update

Give employee with ID 1 a $5000 raise using connected pattern.

### Task 2: Disconnected Update

Update only the email for employee ID 2 without loading the full entity.

### Task 3: Bulk Update

Deactivate all employees who haven't logged in (use `IsActive = false` for all in "HR" department).

### Task 4: Compare SQL

Enable logging and compare the SQL generated by:

- Connected update
- `Update()` method
- `ExecuteUpdateAsync()`

---

## ✅ Checkpoint

You should now understand:

- Connected vs disconnected update patterns
- How change tracking works
- `Update()` vs tracked entity modification
- Bulk updates with `ExecuteUpdateAsync()`
- Concurrency handling

---

**Next Step:** [DeleteOperations.md](./DeleteOperations.md) - Removing data
