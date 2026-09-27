# EF Core Interview Cheat Sheet 🎯

> **Quick reference for your NVIDIA interview!**

---

## 📝 One-Liner Answers

| Question                   | Quick Answer                                                        |
| -------------------------- | ------------------------------------------------------------------- |
| What is ORM?               | Maps objects to database tables, eliminates manual SQL              |
| What is DbContext?         | Unit of Work + Repository, manages entities and database operations |
| What is DbSet?             | Repository for entity type, provides CRUD operations                |
| IQueryable vs IEnumerable? | IQueryable = server-side SQL, IEnumerable = client-side memory      |
| What are migrations?       | Version control for database schema                                 |
| What is change tracking?   | Detects entity modifications for SaveChanges                        |
| AsNoTracking purpose?      | Faster read-only queries, no tracking overhead                      |
| N+1 problem?               | N extra queries in a loop, solve with Include()                     |
| Eager vs Lazy Loading?     | Eager = Include(), Lazy = auto on access (virtual)                  |
| What is ExecuteUpdate?     | Direct SQL update without loading entities (EF 7+)                  |

---

## 🔥 Top Interview Questions & Answers

### 1. How does EF Core translate LINQ to SQL?

```
LINQ → Expression Tree → EF Query Pipeline → SQL
```

Key: Expression trees (not delegates) enable server-side translation.

### 2. Explain Entity States

| State     | SaveChanges |
| --------- | ----------- |
| Detached  | Nothing     |
| Unchanged | Nothing     |
| Added     | INSERT      |
| Modified  | UPDATE      |
| Deleted   | DELETE      |

### 3. Three Loading Strategies

```csharp
// 1. Eager - with main query
.Include(e => e.Department)

// 2. Explicit - on demand
context.Entry(e).Reference(e => e.Department).LoadAsync()

// 3. Lazy - auto on access (needs virtual)
e.Department  // triggers query
```

### 4. IQueryable vs IEnumerable

```csharp
// IQueryable - SQL filter
context.Employees.Where(e => e.Salary > 50000).ToList();
// SELECT * WHERE Salary > 50000

// IEnumerable - C# filter
context.Employees.ToList().Where(e => e.Salary > 50000);
// SELECT * (all rows), then filter in memory
```

### 5. Performance Best Practices

1. **AsNoTracking()** for read-only
2. **Select()** projection over full entities
3. **Include()** to prevent N+1
4. **ExecuteUpdate/Delete** for bulk
5. **AsSplitQuery()** for multiple Includes

---

## 💻 Code Patterns to Know

### Basic CRUD

```csharp
// Create
context.Add(entity);

// Read
context.Employees.Find(id);
context.Employees.FirstOrDefault(e => e.Id == id);
context.Employees.Where(...).ToList();

// Update
var e = context.Find(id);
e.Name = "New";
// or
context.Update(disconnectedEntity);

// Delete
context.Remove(entity);
// or
context.Employees.Where(...).ExecuteDelete();

// Save all
await context.SaveChangesAsync();
```

### Relationship Configuration

```csharp
modelBuilder.Entity<Employee>()
    .HasOne(e => e.Department)     // Employee has ONE Dept
    .WithMany(d => d.Employees)    // Dept has MANY Employees
    .HasForeignKey(e => e.DeptId)  // FK column
    .OnDelete(DeleteBehavior.Restrict);
```

### Querying Related Data

```csharp
// Eager loading
var emps = await context.Employees
    .Include(e => e.Department)
    .ToListAsync();

// Projection (no Include needed)
var dtos = await context.Employees
    .Select(e => new { e.Name, DeptName = e.Department.Name })
    .ToListAsync();

// Filter by related
var engineers = await context.Employees
    .Where(e => e.Department.Name == "Engineering")
    .ToListAsync();
```

---

## 🎯 Common Gotchas

### ❌ Wrong

```csharp
// 1. Client-side filtering
context.Employees.ToList().Where(...)

// 2. Lazy loading in loop (N+1)
foreach (var e in employees)
    Console.Write(e.Department.Name);

// 3. Forgetting SaveChanges
context.Add(entity);
// Missing SaveChangesAsync()!

// 4. Update() for tracked entity
var e = context.Find(1);
e.Name = "New";
context.Update(e);  // Unnecessary!
```

### ✅ Right

```csharp
// 1. Server-side filtering
context.Employees.Where(...).ToList()

// 2. Eager loading
context.Employees.Include(e => e.Department)

// 3. Always save
context.Add(entity);
await context.SaveChangesAsync();

// 4. Just modify and save
var e = context.Find(1);
e.Name = "New";
await context.SaveChangesAsync();  // EF detects change
```

---

## 📊 Quick Comparison Tables

### Find vs FirstOrDefault

| Find()               | FirstOrDefault() |
| -------------------- | ---------------- |
| PK only              | Any predicate    |
| Checks tracker first | Always queries   |
| Faster for PK        | More flexible    |

### Add vs Update vs Attach

| Method   | Entity State | Use When                        |
| -------- | ------------ | ------------------------------- |
| Add()    | Added        | New entity                      |
| Update() | Modified     | Disconnected (updates ALL cols) |
| Attach() | Unchanged    | Manual state control            |

### Delete Behaviors

| Behavior | Parent Deleted | Children  |
| -------- | -------------- | --------- |
| Cascade  | ✓              | Deleted   |
| Restrict | Exception      | Blocked   |
| SetNull  | ✓              | FK = null |

---

## 🔧 Migration Commands

```bash
# Create migration
dotnet ef migrations add <Name>

# Apply to database
dotnet ef database update

# Revert
dotnet ef database update <PreviousMigration>

# Remove last migration
dotnet ef migrations remove

# Generate SQL script
dotnet ef migrations script
```

---

## 🚀 Quick Setup Checklist

```bash
# 1. Create project
dotnet new console -n EFCoreDemoApp

# 2. Add packages
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design

# 3. Create entities, DbContext

# 4. Create migration
dotnet ef migrations add InitialCreate

# 5. Apply
dotnet ef database update
```

---

## 📈 Performance Checklist

- [ ] Using AsNoTracking for read-only?
- [ ] Using Select projection?
- [ ] No N+1 queries (checked Include)?
- [ ] Using bulk operations for mass updates?
- [ ] Split queries for multiple Includes?
- [ ] Proper indexes on query columns?
- [ ] DbContext pooling in high-throughput?

---

## 🎤 Sample Interview Dialogue

**Q: How would you optimize this query?**

```csharp
foreach (var d in context.Departments)
    Console.WriteLine($"{d.Name}: {d.Employees.Count}");
```

**A:** This has the N+1 problem. I'd use either:

```csharp
// Option 1: Eager loading
var depts = context.Departments
    .Include(d => d.Employees)
    .ToList();

// Option 2: Projection (better)
var data = context.Departments
    .Select(d => new { d.Name, Count = d.Employees.Count })
    .ToList();
```

---

## ✅ Final Tips

1. **Always explain WHY** not just what
2. **Mention trade-offs** (eager vs lazy, tracking vs no-tracking)
3. **Use real examples** from your FX Trading experience
4. **Know the SQL generated** by common LINQ operations
5. **Practice live coding** with these patterns

---

**Good luck with your NVIDIA interview! 🚀**
