# Step 8: Read Operations - Querying Data

> **Duration:** 30 minutes  
> **Goal:** Master LINQ queries with EF Core

---

## 🎯 The READ Operation

```
┌─────────────────────────────────────────────────────────────────┐
│                    LINQ → SQL Translation                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  C# LINQ:                                                       │
│  context.Employees.Where(e => e.Salary > 50000)                 │
│                                                                  │
│  SQL Generated:                                                 │
│  SELECT * FROM Employees WHERE Salary > 50000                   │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 💻 Hands-On: Reading Data

Add to `Program.cs`:

```csharp
// === READ OPERATIONS ===
Console.WriteLine("\n=== READ OPERATIONS ===\n");

// 1. Get all records
await GetAllEmployees(context);

// 2. Find by primary key
await FindById(context);

// 3. Filter with Where
await FilterEmployees(context);

// 4. Single vs First vs FirstOrDefault
await DifferentRetrieval(context);

// 5. Projection with Select
await ProjectData(context);

// 6. Ordering
await OrderEmployees(context);

// 7. Pagination
await PaginateResults(context);

// 8. Aggregations
await AggregateData(context);

// ==========================================
// READ METHOD IMPLEMENTATIONS
// ==========================================

static async Task GetAllEmployees(AppDbContext context)
{
    Console.WriteLine("--- 1. Get All Employees ---");

    // ToListAsync() executes the query
    var employees = await context.Employees.ToListAsync();

    Console.WriteLine($"Total employees: {employees.Count}");
    foreach (var emp in employees.Take(3))
    {
        Console.WriteLine($"  {emp.EmployeeId}: {emp.FullName}");
    }
}

static async Task FindById(AppDbContext context)
{
    Console.WriteLine("\n--- 2. Find by Primary Key ---");

    // FindAsync is optimized for PK lookup
    // First checks if already tracked, then queries
    var employee = await context.Employees.FindAsync(1);

    if (employee != null)
    {
        Console.WriteLine($"Found: {employee.FullName}");
    }

    // Alternative: FirstOrDefaultAsync
    var emp2 = await context.Employees
        .FirstOrDefaultAsync(e => e.EmployeeId == 1);
}

static async Task FilterEmployees(AppDbContext context)
{
    Console.WriteLine("\n--- 3. Filter with Where ---");

    // Simple filter
    var highEarners = await context.Employees
        .Where(e => e.Salary > 60000)
        .ToListAsync();

    Console.WriteLine($"High earners (>60K): {highEarners.Count}");

    // Multiple conditions
    var filtered = await context.Employees
        .Where(e => e.Salary > 50000 && e.IsActive)
        .Where(e => e.FirstName.StartsWith("J"))
        .ToListAsync();

    Console.WriteLine($"Active Js earning >50K: {filtered.Count}");

    // Contains (IN clause)
    var names = new[] { "Engineering", "HR" };
    var deptEmployees = await context.Employees
        .Where(e => names.Contains(e.Department.Name))
        .ToListAsync();
}

static async Task DifferentRetrieval(AppDbContext context)
{
    Console.WriteLine("\n--- 4. Single vs First vs FirstOrDefault ---");

    // FirstOrDefaultAsync - Returns first match or default(null)
    var first = await context.Employees
        .FirstOrDefaultAsync(e => e.Salary > 50000);
    Console.WriteLine($"FirstOrDefault: {first?.FullName ?? "null"}");

    // FirstAsync - Throws if no match
    try
    {
        var mustExist = await context.Employees
            .FirstAsync(e => e.Salary > 50000);
        Console.WriteLine($"First: {mustExist.FullName}");
    }
    catch (InvalidOperationException)
    {
        Console.WriteLine("First: No match found!");
    }

    // SingleOrDefaultAsync - Expects 0 or 1 match
    var unique = await context.Employees
        .SingleOrDefaultAsync(e => e.Email == "john.doe@company.com");
    Console.WriteLine($"SingleOrDefault: {unique?.FullName ?? "null"}");

    // SingleAsync - Throws if 0 or multiple matches
    // Use when you expect EXACTLY one result
}

static async Task ProjectData(AppDbContext context)
{
    Console.WriteLine("\n--- 5. Projection with Select ---");

    // Anonymous type projection
    var names = await context.Employees
        .Select(e => new { e.FirstName, e.LastName, e.Salary })
        .ToListAsync();

    foreach (var n in names.Take(3))
    {
        Console.WriteLine($"  {n.FirstName} {n.LastName}: ${n.Salary:N0}");
    }

    // DTO projection
    var dtos = await context.Employees
        .Select(e => new EmployeeDto
        {
            Id = e.EmployeeId,
            FullName = e.FirstName + " " + e.LastName,
            DepartmentName = e.Department.Name
        })
        .ToListAsync();
}

static async Task OrderEmployees(AppDbContext context)
{
    Console.WriteLine("\n--- 6. Ordering ---");

    // Order by salary descending
    var topPaid = await context.Employees
        .OrderByDescending(e => e.Salary)
        .Take(3)
        .ToListAsync();

    Console.WriteLine("Top 3 paid:");
    foreach (var emp in topPaid)
    {
        Console.WriteLine($"  {emp.FullName}: ${emp.Salary:N0}");
    }

    // Multiple ordering
    var ordered = await context.Employees
        .OrderBy(e => e.Department.Name)
        .ThenByDescending(e => e.Salary)
        .ToListAsync();
}

static async Task PaginateResults(AppDbContext context)
{
    Console.WriteLine("\n--- 7. Pagination ---");

    int pageSize = 5;
    int pageNumber = 1; // 0-based would use (pageNumber * pageSize)

    var page = await context.Employees
        .OrderBy(e => e.EmployeeId)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    Console.WriteLine($"Page {pageNumber} ({page.Count} items):");
    foreach (var emp in page)
    {
        Console.WriteLine($"  {emp.EmployeeId}: {emp.FullName}");
    }

    // Total count for pagination info
    var totalCount = await context.Employees.CountAsync();
    var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    Console.WriteLine($"Total: {totalCount} items, {totalPages} pages");
}

static async Task AggregateData(AppDbContext context)
{
    Console.WriteLine("\n--- 8. Aggregations ---");

    // Count
    var count = await context.Employees.CountAsync();
    Console.WriteLine($"Total employees: {count}");

    // Count with condition
    var activeCount = await context.Employees
        .CountAsync(e => e.IsActive);
    Console.WriteLine($"Active employees: {activeCount}");

    // Sum
    var totalSalary = await context.Employees.SumAsync(e => e.Salary);
    Console.WriteLine($"Total salary: ${totalSalary:N0}");

    // Average
    var avgSalary = await context.Employees.AverageAsync(e => e.Salary);
    Console.WriteLine($"Average salary: ${avgSalary:N0}");

    // Min/Max
    var maxSalary = await context.Employees.MaxAsync(e => e.Salary);
    var minSalary = await context.Employees.MinAsync(e => e.Salary);
    Console.WriteLine($"Salary range: ${minSalary:N0} - ${maxSalary:N0}");

    // Group By
    var byDept = await context.Employees
        .GroupBy(e => e.Department.Name)
        .Select(g => new
        {
            Department = g.Key,
            Count = g.Count(),
            AvgSalary = g.Average(e => e.Salary)
        })
        .ToListAsync();

    Console.WriteLine("\nBy Department:");
    foreach (var d in byDept)
    {
        Console.WriteLine($"  {d.Department}: {d.Count} employees, ${d.AvgSalary:N0} avg");
    }
}

// DTO class
public class EmployeeDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
}
```

---

## 📊 Query Execution Comparison

| Method      | When Executed | Returns              |
| ----------- | ------------- | -------------------- |
| `ToList()`  | Immediately   | List<T>              |
| `ToArray()` | Immediately   | T[]                  |
| `First()`   | Immediately   | Single T             |
| `Single()`  | Immediately   | Single T             |
| `Count()`   | Immediately   | int                  |
| `Any()`     | Immediately   | bool                 |
| `Where()`   | **Deferred**  | IQueryable<T>        |
| `Select()`  | **Deferred**  | IQueryable<T>        |
| `OrderBy()` | **Deferred**  | IOrderedQueryable<T> |

---

## 🔍 Deferred vs Immediate Execution

```csharp
// Deferred - Query NOT executed yet
var query = context.Employees
    .Where(e => e.Salary > 50000);  // Just building expression

// Query executes HERE
var results = await query.ToListAsync();

// Each enumeration executes query AGAIN!
foreach (var emp in query) { }  // Executes!
foreach (var emp in query) { }  // Executes AGAIN!
```

```
┌─────────────────────────────────────────────────────────────────┐
│                    Deferred Execution                            │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  var query = ctx.Employees.Where(...)  ← NO SQL executed        │
│                    │                                            │
│                    ▼                                            │
│  [More LINQ methods can be chained]                             │
│                    │                                            │
│                    ▼                                            │
│  .ToListAsync()                        ← SQL executed NOW       │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🎯 Single vs First vs Find

```
┌─────────────────────────────────────────────────────────────────┐
│                    Retrieval Methods                             │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  FindAsync(key)                                                 │
│  ├── Checks Change Tracker first                                │
│  ├── Only works with primary key                                │
│  └── Most efficient for PK lookups                              │
│                                                                  │
│  FirstOrDefaultAsync(predicate)                                 │
│  ├── Always queries database                                    │
│  ├── Returns first match or null                                │
│  └── Use when expecting 0+ results                              │
│                                                                  │
│  SingleOrDefaultAsync(predicate)                                │
│  ├── Verifies only 0 or 1 result                               │
│  ├── Throws if multiple matches                                 │
│  └── Use when expecting unique result                           │
│                                                                  │
│  FirstAsync(predicate)                                          │
│  └── Throws InvalidOperationException if no match               │
│                                                                  │
│  SingleAsync(predicate)                                         │
│  └── Throws if 0 or 2+ matches                                  │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## ❓ Interview Questions

### Q1: What is deferred execution in EF Core?

**Answer:**

> Deferred execution means the query doesn't run until you **enumerate** the results:
>
> ```csharp
> var query = ctx.Employees.Where(e => e.Salary > 50000); // No SQL
> var list = query.ToList();  // SQL executes HERE
> ```
>
> Benefits:
>
> - Chain multiple operations before executing
> - Optimize query composition
> - Server-side filtering (not client-side)

---

### Q2: What's the difference between `Find()` and `FirstOrDefault()`?

**Answer:**

> | Find()                      | FirstOrDefault()      |
> | --------------------------- | --------------------- |
> | Only works with PK          | Any predicate         |
> | Checks Change Tracker first | Always queries DB     |
> | Returns tracked entity      | May return new entity |
> | Can't include navigation    | Can use Include()     |
>
> ```csharp
> // Find checks tracker first - no DB call if already loaded
> var emp = await ctx.Employees.FindAsync(1);
>
> // FirstOrDefault always queries
> var emp = await ctx.Employees.FirstOrDefaultAsync(e => e.EmployeeId == 1);
> ```

---

### Q3: When should you use `Single()` vs `First()`?

**Answer:**

> **Use Single when:**
>
> - You expect EXACTLY one result
> - Multiple results indicate a bug
> - Looking up by unique constraint
>
> **Use First when:**
>
> - You want the first match of potentially many
> - ORDER BY is important
> - Example: "Get the highest paid employee"
>
> ```csharp
> // Single - throws if 0 or 2+ matches
> var user = await ctx.Users.SingleAsync(u => u.Email == email);
>
> // First - gets first of potentially many
> var topPaid = await ctx.Employees
>     .OrderByDescending(e => e.Salary)
>     .FirstAsync();
> ```

---

### Q4: How do you implement pagination in EF Core?

**Answer:**

> ```csharp
> public async Task<PagedResult<T>> GetPagedAsync(
>     int page, int pageSize)
> {
>     var query = context.Employees.AsQueryable();
>
>     var totalCount = await query.CountAsync();
>
>     var items = await query
>         .OrderBy(e => e.EmployeeId)  // Required for consistent paging
>         .Skip((page - 1) * pageSize)
>         .Take(pageSize)
>         .ToListAsync();
>
>     return new PagedResult<T> {
>         Items = items,
>         TotalCount = totalCount,
>         Page = page,
>         PageSize = pageSize
>     };
> }
> ```

---

### Q5: What's the difference between `Any()` and `Count() > 0`?

**Answer:**

> `Any()` is more efficient:
>
> ```sql
> -- Any() generates:
> SELECT CASE WHEN EXISTS (
>     SELECT 1 FROM Employees WHERE Salary > 50000
> ) THEN 1 ELSE 0 END
>
> -- Count() > 0 generates:
> SELECT COUNT(*) FROM Employees WHERE Salary > 50000
> ```
>
> `Any()` stops at first match, `Count()` scans all rows.

---

### Q6: How does `AsNoTracking()` improve read performance?

**Answer:**

> ```csharp
> // With tracking (default)
> var employees = await ctx.Employees.ToListAsync();
> // - Entities are tracked in Change Tracker
> // - Identity resolution (same entity = same instance)
> // - More memory usage
>
> // Without tracking
> var employees = await ctx.Employees
>     .AsNoTracking()
>     .ToListAsync();
> // - Faster query execution
> // - Less memory
> // - Can't call SaveChanges() on these entities
> ```
>
> **Use for read-only scenarios** (reports, APIs returning DTOs).

---

## ✍️ Exercise: Query Practice

### Task 1: Run and Observe

```bash
cd EFCoreDemoApp
dotnet run
```

### Task 2: Write These Queries

1. Get all employees in the "Engineering" department
2. Find the employee with the highest salary
3. Count employees who joined this year
4. Get average salary by department
5. Search employees by name (case-insensitive)

### Task 3: Compare SQL Output

Add to Program.cs to see generated SQL:

```csharp
// In DbContextOptionsBuilder setup
builder.LogTo(Console.WriteLine, LogLevel.Information);
```

---

## ✅ Checkpoint

You should now understand:

- Different query methods (ToList, First, Single, Find)
- Deferred vs immediate execution
- Filtering, ordering, pagination
- Aggregations and grouping

---

**Next Step:** [UpdateOperations.md](./UpdateOperations.md) - Modifying data
