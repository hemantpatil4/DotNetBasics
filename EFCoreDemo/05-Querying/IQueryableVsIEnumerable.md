# IQueryable vs IEnumerable - Critical Interview Topic!

> **Duration:** 20 minutes  
> **Goal:** Master the most asked EF Core interview question

---

## 🎯 The Key Difference

```
┌─────────────────────────────────────────────────────────────────┐
│                    Where Processing Happens                      │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  IQueryable<T>                    IEnumerable<T>                │
│  ─────────────                    ───────────────               │
│  Processing: DATABASE             Processing: MEMORY             │
│                                                                  │
│  Filter happens in SQL:           All data loaded first:        │
│  SELECT * FROM Employees          SELECT * FROM Employees       │
│  WHERE Salary > 50000             (all rows to memory)          │
│                                   Then .Where() in C#           │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 💻 Demonstration Code

```csharp
// === IQueryable vs IEnumerable Demo ===
Console.WriteLine("\n=== IQueryable vs IEnumerable ===\n");

// Enable SQL logging
// builder.LogTo(Console.WriteLine, LogLevel.Information);

await CompareQueryExecution(context);

static async Task CompareQueryExecution(AppDbContext context)
{
    Console.WriteLine("--- IQueryable (Server-side) ---");

    // IQueryable - filter happens in SQL Server
    IQueryable<Employee> queryable = context.Employees
        .Where(e => e.Salary > 50000);

    // SQL Generated:
    // SELECT * FROM Employees WHERE Salary > 50000

    Console.WriteLine("Query built, not executed yet...");
    Console.WriteLine($"Type: {queryable.GetType().Name}");

    // Executes HERE
    var results1 = await queryable.ToListAsync();
    Console.WriteLine($"Results: {results1.Count} employees (filtered on server)");

    // -------------------------------------------

    Console.WriteLine("\n--- IEnumerable (Client-side) ---");

    // IEnumerable - ALL data loaded, then filtered in memory
    IEnumerable<Employee> enumerable = context.Employees
        .AsEnumerable()  // Forces client-side evaluation
        .Where(e => e.Salary > 50000);

    // SQL Generated:
    // SELECT * FROM Employees  (NO WHERE!)
    // Then filtering happens in C# memory

    var results2 = enumerable.ToList();
    Console.WriteLine($"Results: {results2.Count} employees (filtered in memory)");
}
```

---

## 📊 Visual Comparison

```
IQueryable (Efficient)
──────────────────────

[C# Code]                          [SQL Server]
   │                                    │
   │  .Where(e => e.Salary > 50000)    │
   │        │                          │
   │        └──── Translated to ───────┤
   │                                    │
   │                            SELECT * FROM Emp
   │                            WHERE Salary > 50000
   │                                    │
   │  ←──── Only matching rows ─────────┤
   │                                    │
   ▼
[10 rows in memory]


IEnumerable (Inefficient)
─────────────────────────

[C# Code]                          [SQL Server]
   │                                    │
   │  .AsEnumerable()                   │
   │        │                          │
   │        └──── SELECT * FROM Emp ───┤
   │                                    │
   │  ←──── ALL 10,000 rows ───────────┤
   │                                    │
   │  .Where(e => e.Salary > 50000)    │
   │        │                          │
   │   [Filter in C# memory]           │
   │                                    │
   ▼
[10 rows after filtering]
[But 10,000 rows were transferred!]
```

---

## 🔍 Key Characteristics

| Feature              | IQueryable<T>          | IEnumerable<T>               |
| -------------------- | ---------------------- | ---------------------------- |
| Namespace            | `System.Linq`          | `System.Collections.Generic` |
| Execution            | Database (SQL)         | Memory (C#)                  |
| Expression Type      | Expression Tree        | Delegate                     |
| Deferred             | Yes                    | Yes                          |
| Best For             | Database queries       | In-memory collections        |
| Can Add More Filters | Yes (before execution) | Yes (but already loaded)     |

---

## 🎯 When Each Type is Used

### IQueryable - Use When:

```csharp
// ✅ Filtering database data
var highEarners = context.Employees
    .Where(e => e.Salary > 50000)  // IQueryable - SQL filter
    .OrderBy(e => e.Name)
    .Take(10);

// ✅ Building dynamic queries
public IQueryable<Employee> GetQuery(decimal? minSalary, string? dept)
{
    var query = context.Employees.AsQueryable();

    if (minSalary.HasValue)
        query = query.Where(e => e.Salary >= minSalary);

    if (!string.IsNullOrEmpty(dept))
        query = query.Where(e => e.Department.Name == dept);

    return query;  // Return IQueryable for further composition
}
```

### IEnumerable - Use When:

```csharp
// ✅ After data is already loaded
var employees = await context.Employees.ToListAsync();

// Now further filtering is IEnumerable (in-memory)
var filtered = employees.Where(e => ComplexMethod(e));

// ✅ Using C# methods not translatable to SQL
var results = context.Employees
    .AsEnumerable()  // Switch to client-side
    .Where(e => CustomValidation(e));  // Can't translate to SQL
```

---

## ⚠️ Common Mistakes

### Mistake 1: Accidentally Switching to Client-Side

```csharp
// ❌ BAD: ToList() forces load, then filter in memory
var results = context.Employees
    .ToList()  // ALL employees loaded!
    .Where(e => e.Salary > 50000);

// ✅ GOOD: Filter before ToList()
var results = context.Employees
    .Where(e => e.Salary > 50000)  // SQL filter
    .ToList();
```

### Mistake 2: Using Non-Translatable Methods

```csharp
// ❌ BAD: This method can't be translated to SQL
var results = context.Employees
    .Where(e => MyCustomMethod(e.Name))  // Runtime exception!
    .ToList();

// ✅ Option 1: Switch to client-side
var results = context.Employees
    .AsEnumerable()  // Now in memory
    .Where(e => MyCustomMethod(e.Name))
    .ToList();

// ✅ Option 2: Rewrite to translatable expression
var results = context.Employees
    .Where(e => e.Name.StartsWith("J"))  // SQL can handle this
    .ToList();
```

### Mistake 3: Returning IEnumerable from Repository

```csharp
// ❌ BAD: Caller can't add more SQL filters
public IEnumerable<Employee> GetEmployees()
{
    return context.Employees.ToList();
}

// Caller wants to filter:
var result = repo.GetEmployees()
    .Where(e => e.Salary > 50000)  // In-memory! All loaded first
    .Take(10);

// ✅ GOOD: Return IQueryable
public IQueryable<Employee> GetEmployees()
{
    return context.Employees;
}

// Caller can add SQL filters:
var result = repo.GetEmployees()
    .Where(e => e.Salary > 50000)  // SQL filter!
    .Take(10)
    .ToList();
```

---

## 🔄 AsQueryable() vs AsEnumerable()

```csharp
// AsEnumerable() - Switch from IQueryable to IEnumerable
// Use when you need client-side evaluation
var results = context.Employees
    .Where(e => e.Salary > 50000)  // SQL
    .AsEnumerable()                 // Switch to client
    .Where(e => CustomCheck(e));    // C# memory

// AsQueryable() - Wrap IEnumerable as IQueryable
// Rarely useful with EF Core
List<Employee> list = new();
IQueryable<Employee> queryable = list.AsQueryable();
// But still executes in memory, not on server!
```

---

## ❓ Interview Questions

### Q1: What is the difference between IQueryable and IEnumerable?

**Answer:**

> | IQueryable                | IEnumerable               |
> | ------------------------- | ------------------------- |
> | Expression tree           | Delegate                  |
> | Server-side execution     | Client-side execution     |
> | SQL translation           | In-memory filtering       |
> | Better for large datasets | Better for in-memory data |
>
> ```csharp
> // IQueryable - SQL WHERE
> ctx.Employees.Where(e => e.Salary > 50000);
>
> // IEnumerable - C# filter
> employees.Where(e => e.Salary > 50000);
> ```

---

### Q2: When would you use AsEnumerable()?

**Answer:**

> Use `AsEnumerable()` when:
>
> 1. Using C# methods that can't translate to SQL
> 2. Need to do client-side processing
> 3. Complex business logic not expressible in SQL
>
> ```csharp
> // Custom C# method requires client-side
> var results = context.Employees
>     .Where(e => e.Salary > 50000)  // SQL filter first
>     .AsEnumerable()                 // Switch to client
>     .Where(e => ValidateEmployee(e)); // C# method
> ```

---

### Q3: What happens if you call ToList() in the middle of a query?

**Answer:**

> `ToList()` **executes the query immediately** and loads all results into memory:
>
> ```csharp
> // All employees loaded, then filtered in memory!
> var results = context.Employees
>     .ToList()                        // Executes: SELECT * FROM Employees
>     .Where(e => e.Salary > 50000)   // Filters in C# memory
>     .Take(10);                      // Takes from already-loaded data
> ```
>
> **Always filter before materializing!**

---

### Q4: Why should repositories return IQueryable instead of IEnumerable?

**Answer:**

> **IQueryable allows:**
>
> - Caller to add more filters (executed in SQL)
> - Deferred execution
> - Query composition
> - Pagination at database level
>
> ```csharp
> // Good: Caller can optimize
> IQueryable<Employee> GetEmployees() => context.Employees;
>
> // Usage - only 10 rows from database
> var result = repo.GetEmployees()
>     .Where(e => e.Salary > 50000)  // SQL
>     .Take(10)                      // SQL
>     .ToListAsync();
> ```

---

### Q5: How can you tell if a LINQ operation will be translated to SQL?

**Answer:**

> **Translated to SQL (IQueryable):**
>
> - Standard LINQ: Where, Select, OrderBy, Take, Skip
> - String methods: Contains, StartsWith, EndsWith
> - Numeric comparisons
> - Navigation properties
>
> **NOT translated (require client-side):**
>
> - Custom C# methods
> - Complex string manipulation
> - Some DateTime operations
> - Regex
>
> **Enable logging to see SQL:**
>
> ```csharp
> options.LogTo(Console.WriteLine);
> ```

---

## 🎯 Quick Reference: Method Chain Impact

```csharp
context.Employees            // IQueryable (dbset)
    .Where(...)              // IQueryable (still building)
    .OrderBy(...)            // IQueryable (still building)
    .Select(...)             // IQueryable (still building)
    .AsEnumerable()          // IEnumerable (switch!)
    .Where(...)              // IEnumerable (in-memory)
    .ToList();               // List<T> (materialized)
```

---

## ✍️ Exercise

### Task 1: Compare SQL Generated

Enable logging and run both:

```csharp
// Version 1
var r1 = context.Employees.Where(e => e.Salary > 50000).ToList();

// Version 2
var r2 = context.Employees.ToList().Where(e => e.Salary > 50000);
```

Compare the SQL generated.

### Task 2: Build Dynamic Query

Create a method that returns `IQueryable<Employee>` with optional filters that can be composed by the caller.

---

## ✅ Key Takeaway

> **Golden Rule:** Filter with IQueryable FIRST, then materialize with ToList().
>
> ```csharp
> // ✅ RIGHT ORDER
> .Where().OrderBy().Take().ToList()
>
> // ❌ WRONG ORDER
> .ToList().Where().OrderBy().Take()
> ```

---

**Next Step:** [LINQTranslation.md](./LINQTranslation.md) - LINQ to SQL translation
