# LINQ to SQL Translation

> **Duration:** 15 minutes  
> **Goal:** Understand how LINQ queries become SQL

---

## 🎯 How EF Core Translates LINQ

```
┌─────────────────────────────────────────────────────────────────┐
│                    Translation Pipeline                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  C# LINQ Expression                                             │
│       │                                                         │
│       ▼                                                         │
│  Expression Tree (System.Linq.Expressions)                      │
│       │                                                         │
│       ▼                                                         │
│  EF Core Query Pipeline                                         │
│       │                                                         │
│       ▼                                                         │
│  SQL Command                                                    │
│       │                                                         │
│       ▼                                                         │
│  Database Execution                                             │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📊 LINQ → SQL Translation Examples

### Basic Where Clause

```csharp
// LINQ
context.Employees.Where(e => e.Salary > 50000)

// SQL
SELECT [e].[EmployeeId], [e].[FirstName], ...
FROM [Employees] AS [e]
WHERE [e].[Salary] > 50000
```

### Multiple Conditions

```csharp
// LINQ
context.Employees
    .Where(e => e.Salary > 50000 && e.IsActive)
    .Where(e => e.FirstName.StartsWith("J"))

// SQL
SELECT ...
FROM [Employees] AS [e]
WHERE ([e].[Salary] > 50000 AND [e].[IsActive] = 1)
  AND [e].[FirstName] LIKE N'J%'
```

### OrderBy + Pagination

```csharp
// LINQ
context.Employees
    .OrderByDescending(e => e.Salary)
    .Skip(10)
    .Take(5)

// SQL
SELECT ...
FROM [Employees] AS [e]
ORDER BY [e].[Salary] DESC
OFFSET 10 ROWS FETCH NEXT 5 ROWS ONLY
```

### Select (Projection)

```csharp
// LINQ
context.Employees
    .Select(e => new { e.FirstName, e.Salary })

// SQL
SELECT [e].[FirstName], [e].[Salary]
FROM [Employees] AS [e]
```

### Join via Navigation

```csharp
// LINQ
context.Employees
    .Where(e => e.Department.Name == "Engineering")

// SQL
SELECT [e].*
FROM [Employees] AS [e]
INNER JOIN [Departments] AS [d] ON [e].[DepartmentId] = [d].[DepartmentId]
WHERE [d].[Name] = N'Engineering'
```

### Aggregations

```csharp
// LINQ
context.Employees.CountAsync(e => e.Salary > 50000)

// SQL
SELECT COUNT(*)
FROM [Employees] AS [e]
WHERE [e].[Salary] > 50000
```

```csharp
// LINQ
context.Employees.AverageAsync(e => e.Salary)

// SQL
SELECT AVG([e].[Salary])
FROM [Employees] AS [e]
```

### GroupBy

```csharp
// LINQ
context.Employees
    .GroupBy(e => e.DepartmentId)
    .Select(g => new { DeptId = g.Key, Count = g.Count() })

// SQL
SELECT [e].[DepartmentId] AS [DeptId], COUNT(*) AS [Count]
FROM [Employees] AS [e]
GROUP BY [e].[DepartmentId]
```

---

## 🔍 Enable SQL Logging

```csharp
// Option 1: In DbContext configuration
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder
        .UseSqlServer(connectionString)
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging();  // Shows parameter values
}

// Option 2: In Program.cs
var builder = new DbContextOptionsBuilder<AppDbContext>();
builder.UseSqlServer(connectionString)
    .LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Command.Name });
```

---

## 🎯 Common Translation Mappings

| LINQ                       | SQL                        |
| -------------------------- | -------------------------- |
| `Where(e => e.X == value)` | `WHERE X = @value`         |
| `Where(e => e.X != value)` | `WHERE X <> @value`        |
| `Where(e => e.X > value)`  | `WHERE X > @value`         |
| `FirstOrDefault()`         | `TOP(1)`                   |
| `Take(n)`                  | `TOP(n)` or `FETCH NEXT n` |
| `Skip(n)`                  | `OFFSET n ROWS`            |
| `OrderBy()`                | `ORDER BY`                 |
| `OrderByDescending()`      | `ORDER BY DESC`            |
| `Count()`                  | `COUNT(*)`                 |
| `Sum()`                    | `SUM()`                    |
| `Average()`                | `AVG()`                    |
| `Any()`                    | `EXISTS`                   |
| `Contains()`               | `IN`                       |

### String Methods

| LINQ                     | SQL           |
| ------------------------ | ------------- |
| `e.Name.Contains("x")`   | `LIKE '%x%'`  |
| `e.Name.StartsWith("x")` | `LIKE 'x%'`   |
| `e.Name.EndsWith("x")`   | `LIKE '%x'`   |
| `e.Name.ToLower()`       | `LOWER(Name)` |
| `e.Name.Length`          | `LEN(Name)`   |

---

## ⚠️ Non-Translatable Operations

Some C# operations **cannot** be translated to SQL:

```csharp
// ❌ Custom C# methods
.Where(e => MyCustomValidation(e.Name))

// ❌ Complex string operations
.Where(e => Regex.IsMatch(e.Name, pattern))

// ❌ Some DateTime operations
.Where(e => e.JoinDate.AddDays(30) < DateTime.Now)

// ❌ Arbitrary C# methods
.Where(e => int.Parse(e.StringId) > 100)
```

### Solution: Client-Side Evaluation

```csharp
// Force client-side with AsEnumerable()
var results = context.Employees
    .Where(e => e.IsActive)          // SQL filter (efficient)
    .AsEnumerable()                   // Switch to client
    .Where(e => CustomValidation(e)) // C# method
    .ToList();
```

---

## 🔧 Query Tags for Debugging

```csharp
var employees = await context.Employees
    .TagWith("Getting active engineers for report")
    .Where(e => e.IsActive)
    .Where(e => e.Department.Name == "Engineering")
    .ToListAsync();

// SQL includes the tag as a comment:
// -- Getting active engineers for report
// SELECT * FROM Employees WHERE ...
```

---

## 📊 Raw SQL When Needed

```csharp
// FromSqlRaw for complex queries
var employees = await context.Employees
    .FromSqlRaw("SELECT * FROM Employees WHERE Salary > {0}", 50000)
    .ToListAsync();

// FromSqlInterpolated (safer)
var minSalary = 50000;
var employees = await context.Employees
    .FromSqlInterpolated($"SELECT * FROM Employees WHERE Salary > {minSalary}")
    .ToListAsync();

// Combine with LINQ
var results = await context.Employees
    .FromSqlRaw("SELECT * FROM Employees WHERE Salary > 50000")
    .Where(e => e.IsActive)  // Additional LINQ filtering
    .OrderBy(e => e.Name)
    .ToListAsync();
```

---

## ❓ Interview Questions

### Q1: How does EF Core translate LINQ to SQL?

**Answer:**

> EF Core uses **expression trees**:
>
> 1. LINQ methods build an expression tree (not delegates)
> 2. Expression tree represents the query structure
> 3. Query pipeline analyzes the tree
> 4. SQL generator creates appropriate SQL
> 5. Parameters are passed separately (SQL injection safe)

---

### Q2: What happens if a LINQ expression can't be translated?

**Answer:**

> EF Core either:
>
> 1. **Throws exception** at runtime (QueryClientEvaluationWarning)
> 2. **Falls back to client evaluation** (older versions, if configured)
>
> ```csharp
> // This throws:
> .Where(e => MyCustomMethod(e.Name))
>
> // Solution: Use AsEnumerable() to be explicit
> .AsEnumerable().Where(e => MyCustomMethod(e.Name))
> ```

---

### Q3: How can you see the SQL generated by EF Core?

**Answer:**

> Multiple options:
>
> ```csharp
> // 1. LogTo
> optionsBuilder.LogTo(Console.WriteLine);
>
> // 2. ToQueryString() (EF Core 5+)
> var sql = context.Employees
>     .Where(e => e.Salary > 50000)
>     .ToQueryString();
> Console.WriteLine(sql);
>
> // 3. SQL Server Profiler
> // 4. Azure Data Studio / SSMS Query Profiling
> ```

---

### Q4: How do you use raw SQL with EF Core?

**Answer:**

> ```csharp
> // FromSqlRaw - parameterized
> var results = context.Employees
>     .FromSqlRaw("SELECT * FROM Employees WHERE DeptId = {0}", deptId)
>     .ToList();
>
> // FromSqlInterpolated - string interpolation with safety
> var results = context.Employees
>     .FromSqlInterpolated($"SELECT * FROM Employees WHERE DeptId = {deptId}")
>     .ToList();
>
> // ExecuteSqlRaw for non-query
> await context.Database.ExecuteSqlRawAsync(
>     "UPDATE Employees SET Salary = Salary * 1.1 WHERE DeptId = {0}", deptId);
> ```

---

## ✍️ Exercise

### Task 1: Enable Logging

Add logging to your DbContext and observe the SQL for various queries.

### Task 2: Use ToQueryString()

```csharp
var query = context.Employees
    .Where(e => e.Salary > 50000)
    .OrderBy(e => e.FirstName)
    .Take(10);

Console.WriteLine(query.ToQueryString());
```

### Task 3: Compare Queries

Write these queries and compare generated SQL:

1. Find employees by exact name
2. Find employees by name pattern
3. Get average salary by department

---

**Next Step:** [../06-Change-Tracking/ChangeTrackerGuide.md](../06-Change-Tracking/ChangeTrackerGuide.md) - Understanding Change Tracking
