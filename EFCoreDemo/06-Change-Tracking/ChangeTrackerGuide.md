# Change Tracking Deep Dive

> **Duration:** 25 minutes  
> **Goal:** Master EF Core's change tracking mechanism - critical for interviews!

---

## 🎯 What is Change Tracking?

Change Tracking is EF Core's mechanism to detect what changed since entities were loaded:

```
┌─────────────────────────────────────────────────────────────────┐
│                    Change Tracker                                │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Query: var emp = ctx.Employees.Find(1)                         │
│           │                                                     │
│           ▼                                                     │
│  ┌─────────────────────────────────────┐                       │
│  │  Change Tracker stores:              │                       │
│  │  ├── Current values (emp.Salary)     │                       │
│  │  ├── Original values (from DB)       │                       │
│  │  └── Entity state (Unchanged)        │                       │
│  └─────────────────────────────────────┘                       │
│                                                                  │
│  Modify: emp.Salary = 80000                                     │
│           │                                                     │
│           ▼                                                     │
│  SaveChanges():                                                 │
│  ├── DetectChanges() compares current vs original              │
│  ├── Finds Salary: 70000 → 80000                               │
│  └── Generates: UPDATE SET Salary = 80000                      │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📊 Entity States

| State       | Meaning             | On SaveChanges |
| ----------- | ------------------- | -------------- |
| `Detached`  | Not tracked         | Nothing        |
| `Unchanged` | Tracked, no changes | Nothing        |
| `Added`     | New entity          | INSERT         |
| `Modified`  | Existing, changed   | UPDATE         |
| `Deleted`   | Marked for deletion | DELETE         |

```csharp
// Checking and manipulating states
var emp = new Employee { FirstName = "Test" };
Console.WriteLine(context.Entry(emp).State);  // Detached

context.Employees.Add(emp);
Console.WriteLine(context.Entry(emp).State);  // Added

await context.SaveChangesAsync();
Console.WriteLine(context.Entry(emp).State);  // Unchanged

emp.Salary = 80000;
context.ChangeTracker.DetectChanges();
Console.WriteLine(context.Entry(emp).State);  // Modified

context.Employees.Remove(emp);
Console.WriteLine(context.Entry(emp).State);  // Deleted
```

---

## 💻 Hands-On: Exploring Change Tracker

```csharp
// === CHANGE TRACKING DEMO ===
Console.WriteLine("\n=== CHANGE TRACKING DEMO ===\n");

await ExploreChangeTracker(context);

static async Task ExploreChangeTracker(AppDbContext context)
{
    // Get a fresh context to start clean
    Console.WriteLine("--- Viewing Tracked Entities ---");

    // Query some employees (now tracked)
    var employees = await context.Employees.Take(2).ToListAsync();

    // See what's being tracked
    Console.WriteLine($"Tracked entities: {context.ChangeTracker.Entries().Count()}");

    foreach (var entry in context.ChangeTracker.Entries<Employee>())
    {
        Console.WriteLine($"  {entry.Entity.FullName}: {entry.State}");
    }

    // Modify one employee
    Console.WriteLine("\n--- After Modification ---");
    employees[0].Salary += 1000;

    // DetectChanges is called automatically on SaveChanges
    // But we can call it explicitly
    context.ChangeTracker.DetectChanges();

    foreach (var entry in context.ChangeTracker.Entries<Employee>())
    {
        Console.WriteLine($"  {entry.Entity.FullName}: {entry.State}");

        if (entry.State == EntityState.Modified)
        {
            // See what properties changed
            foreach (var prop in entry.Properties.Where(p => p.IsModified))
            {
                Console.WriteLine($"    {prop.Metadata.Name}: {prop.OriginalValue} → {prop.CurrentValue}");
            }
        }
    }

    // Save and observe state change
    Console.WriteLine("\n--- After SaveChanges ---");
    await context.SaveChangesAsync();

    foreach (var entry in context.ChangeTracker.Entries<Employee>())
    {
        Console.WriteLine($"  {entry.Entity.FullName}: {entry.State}");
    }
}
```

---

## 🔍 Original vs Current Values

```csharp
static async Task CompareValues(AppDbContext context)
{
    var emp = await context.Employees.FirstAsync();
    var entry = context.Entry(emp);

    // Before modification
    Console.WriteLine("Before modification:");
    Console.WriteLine($"  Original Salary: {entry.OriginalValues["Salary"]}");
    Console.WriteLine($"  Current Salary: {entry.CurrentValues["Salary"]}");

    // Modify
    emp.Salary = 100000;

    Console.WriteLine("\nAfter modification:");
    Console.WriteLine($"  Original Salary: {entry.OriginalValues["Salary"]}");  // Still old value
    Console.WriteLine($"  Current Salary: {entry.CurrentValues["Salary"]}");    // New value

    // You can reset changes
    entry.CurrentValues.SetValues(entry.OriginalValues);
    Console.WriteLine($"\nAfter reset: {emp.Salary}");  // Back to original
}
```

---

## 🎯 AsNoTracking() - Performance Optimization

```csharp
// With tracking (default)
var employees = await context.Employees.ToListAsync();
// - Stores original values
// - Creates identity map
// - Uses more memory
// - Slower for read-only scenarios

// Without tracking
var employees = await context.Employees
    .AsNoTracking()
    .ToListAsync();
// - No original values stored
// - No identity map
// - Less memory
// - Faster for read-only scenarios
// - Can't SaveChanges() these entities
```

### When to Use AsNoTracking

```csharp
// ✅ Good for: Read-only API endpoints
[HttpGet]
public async Task<IActionResult> GetEmployees()
{
    var employees = await _context.Employees
        .AsNoTracking()
        .Select(e => new EmployeeDto { ... })
        .ToListAsync();
    return Ok(employees);
}

// ✅ Good for: Reports
var reportData = await context.Employees
    .AsNoTracking()
    .GroupBy(e => e.DepartmentId)
    .Select(g => new { DeptId = g.Key, Count = g.Count() })
    .ToListAsync();

// ❌ Don't use when: You need to update entities
var emp = await context.Employees.AsNoTracking().FirstAsync();
emp.Salary = 80000;
await context.SaveChangesAsync();  // Won't work! Not tracked
```

---

## 🔄 DetectChanges()

```csharp
// DetectChanges is called automatically before:
// - SaveChanges()
// - Querying ChangeTracker.Entries()

// But you might need to call it manually:
var emp = await context.Employees.FirstAsync();
emp.Salary = 80000;

// State might still show "Unchanged" until DetectChanges
Console.WriteLine(context.Entry(emp).State);  // Might be Unchanged

context.ChangeTracker.DetectChanges();
Console.WriteLine(context.Entry(emp).State);  // Now Modified
```

### AutoDetectChanges Configuration

```csharp
// Disable auto-detection for bulk operations (performance)
context.ChangeTracker.AutoDetectChangesEnabled = false;

foreach (var emp in employees)
{
    context.Employees.Add(emp);
}

// Must manually call before Save
context.ChangeTracker.DetectChanges();
await context.SaveChangesAsync();

// Re-enable
context.ChangeTracker.AutoDetectChangesEnabled = true;
```

---

## 📊 Identity Resolution

When tracking is enabled, EF Core ensures **one instance per entity**:

```csharp
// With tracking (default)
var emp1 = await context.Employees.FirstAsync(e => e.EmployeeId == 1);
var emp2 = await context.Employees.FirstAsync(e => e.EmployeeId == 1);

Console.WriteLine(ReferenceEquals(emp1, emp2));  // True! Same instance

// Without tracking
var emp3 = await context.Employees.AsNoTracking().FirstAsync(e => e.EmployeeId == 1);
var emp4 = await context.Employees.AsNoTracking().FirstAsync(e => e.EmployeeId == 1);

Console.WriteLine(ReferenceEquals(emp3, emp4));  // False! Different instances
```

---

## ❓ Interview Questions

### Q1: What is Change Tracking in EF Core?

**Answer:**

> Change Tracking is EF Core's mechanism to track modifications to entities:
>
> - Stores **original values** when entities are loaded
> - Compares with **current values** during SaveChanges
> - Generates appropriate SQL (INSERT/UPDATE/DELETE)
> - Manages entity states (Detached, Unchanged, Added, Modified, Deleted)

---

### Q2: What are the different entity states?

**Answer:**

> | State     | Description                       |
> | --------- | --------------------------------- |
> | Detached  | Not tracked by context            |
> | Unchanged | Tracked, same as database         |
> | Added     | New entity, will INSERT           |
> | Modified  | Changed since loaded, will UPDATE |
> | Deleted   | Marked for deletion, will DELETE  |

---

### Q3: What is AsNoTracking and when should you use it?

**Answer:**

> `AsNoTracking()` disables change tracking for a query:
>
> **Benefits:**
>
> - Better performance (no snapshot storage)
> - Less memory usage
> - No identity resolution overhead
>
> **Use when:**
>
> - Read-only scenarios (API GET endpoints)
> - Reports and analytics
> - Data you won't modify
>
> **Don't use when:**
>
> - You need to update the entities later

---

### Q4: How does EF Core detect changes?

**Answer:**

> EF Core uses **snapshot-based detection**:
>
> 1. When entity is queried, original values are stored in a snapshot
> 2. `DetectChanges()` compares current property values vs snapshot
> 3. Modified properties are marked in the entity entry
> 4. SaveChanges generates SQL only for modified properties
>
> ```csharp
> var emp = ctx.Find(1);        // Snapshot stored
> emp.Salary = 80000;           // Property changed
> ctx.SaveChanges();            // DetectChanges() runs, finds diff
> ```

---

### Q5: What is Identity Resolution?

**Answer:**

> Identity Resolution ensures **one instance per entity key** in a context:
>
> ```csharp
> var emp1 = ctx.Employees.Find(1);
> var emp2 = ctx.Employees.Find(1);
> // emp1 and emp2 are the SAME object reference
> ```
>
> This prevents:
>
> - Conflicting changes to same entity
> - Redundant database queries (returns cached instance)
> - Inconsistent state within a context

---

### Q6: What is the difference between Entry().State and Entry().Property().IsModified?

**Answer:**

> - `Entry().State` - Overall entity state (Added, Modified, etc.)
> - `Entry().Property().IsModified` - Whether specific property changed
>
> ```csharp
> emp.FirstName = "New";  // Only FirstName changed
> emp.Salary = emp.Salary;  // Same value
>
> var entry = ctx.Entry(emp);
> entry.State;  // Modified (entity has changes)
>
> entry.Property(e => e.FirstName).IsModified;  // True
> entry.Property(e => e.Salary).IsModified;     // False
> ```
>
> EF only generates UPDATE for properties where `IsModified = true`.

---

## ✍️ Exercise

### Task 1: Explore States

Write code to transition an entity through all 5 states and print each state.

### Task 2: Compare Performance

Create a benchmark comparing:

- Query with tracking
- Query without tracking

For 1000 entities.

### Task 3: Manual State Management

Load an entity, modify it, then use `Entry().CurrentValues.SetValues(Entry().OriginalValues)` to undo changes.

---

## ✅ Key Takeaways

1. Change tracking enables automatic change detection
2. Use `AsNoTracking()` for read-only scenarios
3. Entity states determine what SQL is generated
4. Identity resolution ensures consistency
5. Original values are stored in snapshots

---

**Next Step:** [SaveChangesInternals.md](./SaveChangesInternals.md) - Deep dive into SaveChanges
