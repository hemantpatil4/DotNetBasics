# Loading Strategies: Eager, Lazy, and Explicit Loading

> **Duration:** 25 minutes  
> **Goal:** Master the three ways to load related data - Critical interview topic!

---

## 🎯 The Loading Problem

```csharp
var employee = await context.Employees.FirstAsync();
Console.WriteLine(employee.Department.Name);  // 💥 NullReferenceException!
// Why? Department was not loaded!
```

EF Core doesn't automatically load related entities. You must choose a loading strategy.

---

## 📊 Three Loading Strategies

```
┌─────────────────────────────────────────────────────────────────┐
│                    Loading Strategies                            │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  1. EAGER LOADING (Include)                                     │
│     └── Load related data WITH the main query                   │
│     └── Single database call                                    │
│     └── Best for: Known navigation needs                        │
│                                                                  │
│  2. EXPLICIT LOADING (Load)                                     │
│     └── Load related data AFTER main query                      │
│     └── Additional database call when needed                    │
│     └── Best for: Conditional loading                           │
│                                                                  │
│  3. LAZY LOADING (Automatic)                                    │
│     └── Load related data ON ACCESS                             │
│     └── Database call when property accessed                    │
│     └── Best for: Exploration/prototyping                       │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 1️⃣ Eager Loading (Include)

Load related entities **in the same query**:

```csharp
// === EAGER LOADING ===
Console.WriteLine("\n=== EAGER LOADING ===\n");

// Include single navigation
var employeesWithDept = await context.Employees
    .Include(e => e.Department)
    .ToListAsync();

// SQL Generated:
// SELECT e.*, d.*
// FROM Employees e
// INNER JOIN Departments d ON e.DepartmentId = d.DepartmentId

foreach (var emp in employeesWithDept.Take(3))
{
    Console.WriteLine($"{emp.FullName} - {emp.Department.Name}");  // ✅ Works!
}

// Include collection
var deptWithEmployees = await context.Departments
    .Include(d => d.Employees)
    .ToListAsync();

// Multiple includes
var fullData = await context.Employees
    .Include(e => e.Department)
    // .Include(e => e.Projects)  // If had more relations
    .ToListAsync();

// Nested ThenInclude
var deepLoad = await context.Departments
    .Include(d => d.Employees)
        .ThenInclude(e => e.Department)  // Circular - usually not needed
    .ToListAsync();

// Filtered Include (EF Core 5+)
var activeOnly = await context.Departments
    .Include(d => d.Employees.Where(e => e.IsActive))
    .ToListAsync();
```

### Eager Loading SQL

```sql
-- Include generates JOIN
SELECT [e].[EmployeeId], [e].[FirstName], ...,
       [d].[DepartmentId], [d].[Name], [d].[Budget]
FROM [Employees] AS [e]
INNER JOIN [Departments] AS [d] ON [e].[DepartmentId] = [d].[DepartmentId]
```

---

## 2️⃣ Explicit Loading

Load related entities **separately, on demand**:

```csharp
// === EXPLICIT LOADING ===
Console.WriteLine("\n=== EXPLICIT LOADING ===\n");

// Load employee WITHOUT department
var employee = await context.Employees.FirstAsync();
Console.WriteLine($"Loaded: {employee.FullName}");
Console.WriteLine($"Department: {employee.Department?.Name ?? "Not loaded"}");

// Explicitly load the department
await context.Entry(employee)
    .Reference(e => e.Department)
    .LoadAsync();

Console.WriteLine($"After Load: {employee.Department.Name}");  // ✅ Now loaded

// Load collection
var dept = await context.Departments.FirstAsync();
Console.WriteLine($"Employees before load: {dept.Employees?.Count ?? 0}");

await context.Entry(dept)
    .Collection(d => d.Employees)
    .LoadAsync();

Console.WriteLine($"Employees after load: {dept.Employees.Count}");

// Query instead of Load (more control)
var highEarners = await context.Entry(dept)
    .Collection(d => d.Employees)
    .Query()
    .Where(e => e.Salary > 60000)
    .CountAsync();
Console.WriteLine($"High earners in {dept.Name}: {highEarners}");
```

### Explicit Loading - Two Queries

```sql
-- Query 1: Main entity
SELECT TOP(1) * FROM Employees

-- Query 2: After calling Load()
SELECT * FROM Departments WHERE DepartmentId = @id
```

---

## 3️⃣ Lazy Loading

Load related entities **automatically when accessed**:

### Setup Required

```csharp
// Step 1: Install package
// dotnet add package Microsoft.EntityFrameworkCore.Proxies

// Step 2: Enable in DbContext
optionsBuilder
    .UseLazyLoadingProxies()  // Enable lazy loading
    .UseSqlServer(connectionString);

// Step 3: Make navigation properties virtual
public class Employee
{
    public int EmployeeId { get; set; }
    public string FirstName { get; set; }

    public int DepartmentId { get; set; }
    public virtual Department Department { get; set; }  // MUST be virtual!
}

public class Department
{
    public int DepartmentId { get; set; }
    public string Name { get; set; }

    public virtual ICollection<Employee> Employees { get; set; }  // MUST be virtual!
}
```

### Usage

```csharp
// === LAZY LOADING ===
Console.WriteLine("\n=== LAZY LOADING ===\n");

// Query without Include
var employee = await context.Employees.FirstAsync();
// No SQL for Department yet

// Access navigation - triggers automatic load
Console.WriteLine(employee.Department.Name);  // SQL query happens HERE!

// ⚠️ WARNING: N+1 Problem
foreach (var emp in await context.Employees.ToListAsync())
{
    // Each iteration triggers a NEW query!
    Console.WriteLine($"{emp.FullName} - {emp.Department.Name}");
}
// With 100 employees = 1 + 100 = 101 queries! 😱
```

---

## ⚠️ The N+1 Problem

```
┌─────────────────────────────────────────────────────────────────┐
│                    N+1 Query Problem                             │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  // BAD: Lazy loading in a loop                                 │
│  var employees = ctx.Employees.ToList();    // Query 1          │
│  foreach (var e in employees)                                   │
│  {                                                              │
│      Console.Write(e.Department.Name);      // Query 2, 3, 4... │
│  }                                                              │
│  // 100 employees = 101 queries!                                │
│                                                                  │
│  // GOOD: Eager loading                                         │
│  var employees = ctx.Employees                                  │
│      .Include(e => e.Department)            // Query 1 (with JOIN)
│      .ToList();                                                 │
│  foreach (var e in employees)                                   │
│  {                                                              │
│      Console.Write(e.Department.Name);      // No query         │
│  }                                                              │
│  // 100 employees = 1 query!                                    │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📊 Comparison Table

| Feature      | Eager           | Explicit       | Lazy               |
| ------------ | --------------- | -------------- | ------------------ |
| When loaded  | With main query | On demand      | On property access |
| Queries      | 1 (with JOIN)   | 1 + 1 per load | 1 + 1 per access   |
| Control      | High            | High           | Low                |
| N+1 risk     | No              | Manageable     | High               |
| Setup needed | None            | None           | Proxies package    |
| Best for     | Known needs     | Conditional    | Prototyping        |

---

## 🎯 Best Practices

### ✅ DO

```csharp
// Use Include when you KNOW you need related data
var report = await context.Employees
    .Include(e => e.Department)
    .Select(e => new ReportDto {
        Name = e.FullName,
        DeptName = e.Department.Name
    })
    .ToListAsync();

// Use Explicit when loading is conditional
var emp = await context.Employees.FindAsync(id);
if (needDepartmentDetails)
{
    await context.Entry(emp).Reference(e => e.Department).LoadAsync();
}

// Use projection to avoid loading entire entities
var summary = await context.Departments
    .Select(d => new {
        d.Name,
        EmployeeCount = d.Employees.Count  // No full Employee load
    })
    .ToListAsync();
```

### ❌ DON'T

```csharp
// Don't use lazy loading in production without careful thought
foreach (var dept in depts)
{
    foreach (var emp in dept.Employees)  // 💥 N+1!
    {
        // ...
    }
}

// Don't over-include
var result = await context.Employees
    .Include(e => e.Department)
    .Include(e => e.Projects)        // Do you need all these?
        .ThenInclude(p => p.Tasks)
        .ThenInclude(t => t.Assignee)
    .ToListAsync();
```

---

## ❓ Interview Questions

### Q1: What are the three loading strategies in EF Core?

**Answer:**

> | Strategy     | How                | When                     |
> | ------------ | ------------------ | ------------------------ |
> | **Eager**    | `Include()`        | Main query (JOIN)        |
> | **Explicit** | `Entry().Load()`   | Separate query on demand |
> | **Lazy**     | Virtual properties | Auto on access           |
>
> Eager is most common in production. Lazy requires proxy setup.

---

### Q2: What is the N+1 problem and how do you avoid it?

**Answer:**

> **N+1 Problem**: Executing N additional queries for N entities in a loop.
>
> ```csharp
> // BAD: 1 + N queries
> foreach (var emp in context.Employees.ToList())
>     Console.WriteLine(emp.Department.Name);  // Query per iteration!
>
> // GOOD: 1 query
> foreach (var emp in context.Employees.Include(e => e.Department).ToList())
>     Console.WriteLine(emp.Department.Name);  // No additional queries
> ```
>
> **Avoid by**: Using `Include()`, projection with `Select()`, or being aware of lazy loading.

---

### Q3: When would you use Explicit Loading over Eager Loading?

**Answer:**

> Use Explicit Loading when:
>
> 1. **Conditional loading**: Only load if user expands a section
> 2. **Already have entity**: Need related data for already-loaded entity
> 3. **Filtered loading**: Need to query/filter the related data
>
> ```csharp
> var emp = await context.Employees.FindAsync(id);
>
> if (showFullDetails)
> {
>     await context.Entry(emp)
>         .Reference(e => e.Department)
>         .LoadAsync();
> }
> ```

---

### Q4: What setup is required for Lazy Loading?

**Answer:**

> Two requirements:
>
> 1. **Package**: `Microsoft.EntityFrameworkCore.Proxies`
> 2. **Virtual navigation properties**
>
> ```csharp
> // DbContext
> options.UseLazyLoadingProxies();
>
> // Entity
> public virtual Department Department { get; set; }
> public virtual ICollection<Employee> Employees { get; set; }
> ```
>
> EF creates proxy classes that override virtual properties to add loading logic.

---

### Q5: Why is Lazy Loading generally not recommended for production?

**Answer:**

> Reasons:
>
> 1. **N+1 queries**: Easy to accidentally trigger many queries
> 2. **Hidden database calls**: Hard to track/optimize
> 3. **Serialization issues**: Can trigger loads during JSON serialization
> 4. **Performance**: Harder to predict and profile
>
> **Better alternatives**: Eager loading with Include, or projection with Select.

---

## ✍️ Exercise

### Task 1: Compare Query Counts

1. Load 10 employees with eager loading (Include)
2. Load 10 employees, then access Department in a loop (without Include)
3. Compare the number of SQL queries in logs

### Task 2: Filtered Include

Use filtered Include to load only active employees per department.

### Task 3: Explicit Loading with Query

Load a department, then explicitly load only employees with salary > 70000.

---

## ✅ Key Takeaways

1. **Eager (Include)**: Best for known requirements, prevents N+1
2. **Explicit (Load)**: Best for conditional loading
3. **Lazy (Virtual)**: Convenient but dangerous, avoid in production
4. **Always** consider N+1 problem
5. **Projection** (`Select`) often better than full entity loading

---

**Next Step:** [../09-Performance/PerformanceTips.md](../09-Performance/PerformanceTips.md) - Performance optimization
