# EF Core Performance Tips

> **Duration:** 20 minutes  
> **Goal:** Learn key optimizations for production EF Core applications

---

## 🎯 Top Performance Considerations

```
┌─────────────────────────────────────────────────────────────────┐
│                    Performance Pyramid                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│                    ┌──────────┐                                 │
│                    │ Indexes  │  ← Database level               │
│                 ┌──┴──────────┴──┐                              │
│                 │  Query Design   │  ← Right queries            │
│              ┌──┴────────────────┴──┐                           │
│              │   Loading Strategy    │  ← Avoid N+1             │
│           ┌──┴──────────────────────┴──┐                        │
│           │    AsNoTracking / Projection │  ← Memory             │
│        ┌──┴────────────────────────────┴──┐                     │
│        │     DbContext Lifetime / Pooling  │  ← Resource mgmt   │
│        └───────────────────────────────────┘                    │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 1️⃣ Use AsNoTracking for Read-Only Queries

```csharp
// ❌ Default: With tracking (slower for read-only)
var employees = await context.Employees.ToListAsync();

// ✅ For read-only: Without tracking (faster)
var employees = await context.Employees
    .AsNoTracking()
    .ToListAsync();

// Benchmark difference for 10,000 records:
// With tracking: ~150ms, 50MB memory
// No tracking:   ~80ms,  25MB memory
```

### When to Use

```csharp
// ✅ Use AsNoTracking for:
// - API GET endpoints
// - Reports
// - Read-only queries
// - Data you won't modify

// ❌ Don't use when:
// - You need to update entities
// - You need identity resolution
// - You're within a unit of work that saves
```

---

## 2️⃣ Use Projection (Select) Instead of Full Entities

```csharp
// ❌ Loading full entities (more memory, slower)
var employees = await context.Employees
    .Include(e => e.Department)
    .ToListAsync();

// ✅ Project only what you need
var employees = await context.Employees
    .Select(e => new EmployeeDto
    {
        Id = e.EmployeeId,
        Name = e.FirstName + " " + e.LastName,
        DeptName = e.Department.Name  // No Include needed!
    })
    .ToListAsync();

// Benefits:
// - Less data transferred from database
// - No tracking overhead
// - Automatic AsNoTracking
// - Related data without explicit Include
```

---

## 3️⃣ Avoid N+1 Queries

```csharp
// ❌ N+1 Problem: 1 + N queries
var depts = await context.Departments.ToListAsync();
foreach (var dept in depts)
{
    Console.WriteLine($"{dept.Name}: {dept.Employees.Count}");  // Query per dept!
}

// ✅ Single query with Include
var depts = await context.Departments
    .Include(d => d.Employees)
    .ToListAsync();

// ✅ Or use projection
var deptSummary = await context.Departments
    .Select(d => new { d.Name, EmpCount = d.Employees.Count })
    .ToListAsync();
```

---

## 4️⃣ Use Compiled Queries for Hot Paths

```csharp
// Compiled query - expression tree parsed once
private static readonly Func<AppDbContext, int, Task<Employee?>> GetEmployeeById =
    EF.CompileAsyncQuery((AppDbContext ctx, int id) =>
        ctx.Employees.FirstOrDefault(e => e.EmployeeId == id));

// Usage
var employee = await GetEmployeeById(context, 42);

// Benefits:
// - Expression tree compiled once
// - ~30% faster for repeated queries
// - Best for frequently executed queries
```

---

## 5️⃣ Use Split Queries for Multiple Includes

```csharp
// ❌ Cartesian explosion with multiple Includes
var data = await context.Departments
    .Include(d => d.Employees)
    .Include(d => d.Projects)  // Each combination creates a row!
    .ToListAsync();
// 10 depts × 50 employees × 20 projects = 10,000 rows!

// ✅ Split into separate queries
var data = await context.Departments
    .Include(d => d.Employees)
    .Include(d => d.Projects)
    .AsSplitQuery()  // Three separate queries instead of one big join
    .ToListAsync();
```

---

## 6️⃣ Bulk Operations (EF Core 7+)

```csharp
// ❌ Slow: Load, modify, save
var employees = await context.Employees
    .Where(e => e.DepartmentId == 1)
    .ToListAsync();
foreach (var emp in employees)
{
    emp.Salary *= 1.1m;
}
await context.SaveChangesAsync();  // Multiple UPDATE statements

// ✅ Fast: ExecuteUpdate (single SQL)
await context.Employees
    .Where(e => e.DepartmentId == 1)
    .ExecuteUpdateAsync(s => s.SetProperty(e => e.Salary, e => e.Salary * 1.1m));
// Single: UPDATE Employees SET Salary = Salary * 1.1 WHERE DeptId = 1

// Same for delete
await context.Employees
    .Where(e => e.IsActive == false)
    .ExecuteDeleteAsync();
```

---

## 7️⃣ DbContext Pooling

```csharp
// In Program.cs / Startup.cs
builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseSqlServer(connectionString),
    poolSize: 128);  // Reuse DbContext instances

// Benefits:
// - Reduces allocation overhead
// - Better for high-throughput scenarios
// - DbContext initialization is expensive
```

---

## 8️⃣ Proper Index Usage

```csharp
// Fluent API - add indexes
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Employee>()
        .HasIndex(e => e.Email)
        .IsUnique();

    modelBuilder.Entity<Employee>()
        .HasIndex(e => e.DepartmentId);  // FK index

    // Composite index
    modelBuilder.Entity<Employee>()
        .HasIndex(e => new { e.LastName, e.FirstName });
}
```

---

## 📊 Quick Reference: Do's and Don'ts

| ❌ Don't                   | ✅ Do                          |
| -------------------------- | ------------------------------ |
| `ToList().Where()`         | `Where().ToList()`             |
| `Include` for read-only    | `Select` projection            |
| Load all, filter in C#     | Filter in SQL with Where       |
| Lazy loading in loops      | Eager loading with Include     |
| Many SaveChanges calls     | Batch changes, one SaveChanges |
| Default tracking for reads | AsNoTracking for read-only     |
| Multiple single Includes   | AsSplitQuery                   |
| Load, modify, save in loop | ExecuteUpdate                  |

---

## 🔧 Monitoring Performance

```csharp
// 1. Enable logging
optionsBuilder
    .LogTo(Console.WriteLine, LogLevel.Information)
    .EnableSensitiveDataLogging();  // Shows parameter values

// 2. Simple query timer
var sw = Stopwatch.StartNew();
var results = await context.Employees.ToListAsync();
Console.WriteLine($"Query took: {sw.ElapsedMilliseconds}ms");

// 3. Use MiniProfiler or Application Insights in production
```

---

## ❓ Interview Questions

### Q1: What is the most common EF Core performance issue?

**Answer:**

> **N+1 Query Problem**: Executing N additional queries when iterating over N entities.
>
> Solution: Use `Include()` for eager loading or `Select()` for projection.

---

### Q2: When should you use AsNoTracking?

**Answer:**

> Use for **read-only queries** where you won't modify entities:
>
> - API GET endpoints returning DTOs
> - Reports and analytics
> - Displaying data without edit capability
>
> Benefits: 30-50% faster, less memory usage.

---

### Q3: What's the benefit of projection (Select) over loading entities?

**Answer:**

> | Full Entity        | Projection             |
> | ------------------ | ---------------------- |
> | All columns        | Only needed columns    |
> | Tracked by default | Not tracked            |
> | Requires Include   | Related data automatic |
> | More memory        | Less memory            |
> | Slower             | Faster                 |

---

### Q4: What are compiled queries?

**Answer:**

> Compiled queries cache the expression tree parsing:
>
> ```csharp
> private static readonly Func<AppDbContext, int, Task<Employee?>>
>     GetById = EF.CompileAsyncQuery(
>         (AppDbContext ctx, int id) =>
>             ctx.Employees.FirstOrDefault(e => e.EmployeeId == id));
> ```
>
> ~30% faster for frequently executed queries.

---

### Q5: What is AsSplitQuery and when to use it?

**Answer:**

> `AsSplitQuery()` splits multiple Includes into separate SQL queries instead of one big JOIN.
>
> **Use when**: Multiple collection Includes cause cartesian explosion:
>
> ```csharp
> .Include(d => d.Employees)
> .Include(d => d.Projects)
> .AsSplitQuery()  // 3 queries instead of 1 huge join
> ```

---

## ✍️ Exercise

### Task 1: Benchmark AsNoTracking

Compare query time with and without AsNoTracking for 1000 employees.

### Task 2: Optimize This Query

```csharp
// Optimize this:
var data = await context.Departments.ToListAsync();
foreach (var d in data)
{
    Console.WriteLine($"{d.Name}: {d.Employees.Count}");
}
```

### Task 3: Add an Index

Add an index on the `Email` column and observe query plan change.

---

## ✅ Key Takeaways

1. **AsNoTracking** for read-only scenarios
2. **Select** projection over full entities
3. **Include** to avoid N+1
4. **ExecuteUpdate/Delete** for bulk operations
5. **AsSplitQuery** for multiple collection Includes
6. **DbContext Pooling** for high throughput
7. **Proper indexes** on frequently queried columns

---

**Next Step:** [../10-Interview-Prep/CheatSheet.md](../10-Interview-Prep/CheatSheet.md) - Final review and cheat sheet
