# Step 3: Creating the DbContext

> **Duration:** 15 minutes  
> **Goal:** Understand DbContext deeply and create AppDbContext

---

## 🎯 What is DbContext?

DbContext is the **primary class** for interacting with the database. Think of it as:

```
DbContext = Session with Database
├── Represents a connection to database
├── Contains DbSet<T> for each table
├── Manages change tracking
├── Handles transactions
└── Configures model mapping
```

### Real-World Analogy

```
DbContext is like a SHOPPING CART at a store:

┌─────────────────────────────────────────────────────────────────┐
│                                                                  │
│   You (Developer)         Cart (DbContext)        Store (DB)    │
│         │                       │                     │          │
│   1. Get cart            ───→  new DbContext()       │          │
│   2. Add items           ───→  context.Add()         │          │
│   3. Remove items        ───→  context.Remove()      │          │
│   4. Modify qty          ───→  entity.Property = x   │          │
│   5. Checkout            ───→  SaveChanges()    ───→ │          │
│         │                       │                     │          │
│   Cart tracks all               │               Database         │
│   your changes!                 │               updated!         │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 💻 Hands-On: Create AppDbContext

### Create `Data/AppDbContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using EFCoreDemoApp.Entities;

namespace EFCoreDemoApp.Data;

/// <summary>
/// Main database context - represents a session with the database
/// </summary>
public class AppDbContext : DbContext
{
    // DbSet<T> = Represents a table in database
    // Each DbSet maps to one table

    public DbSet<Employee> Employees { get; set; }      // → Employees table
    public DbSet<Department> Departments { get; set; }  // → Departments table

    // Configure the database connection
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Connection string for SQL Server LocalDB
        optionsBuilder.UseSqlServer(
            @"Server=(localdb)\MSSQLLocalDB;Database=EFCoreDemoDB;Trusted_Connection=True;"
        );

        // Enable detailed logging (for learning - disable in production!)
        optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
        optionsBuilder.EnableSensitiveDataLogging();  // Shows parameter values
    }

    // Configure entity mappings (Fluent API)
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // We'll add configurations here later
        base.OnModelCreating(modelBuilder);
    }
}
```

---

## 🔍 DbContext Internals Deep Dive

### What's Inside DbContext?

```csharp
public class DbContext : IDisposable, IAsyncDisposable
{
    // Internal components
    private DbContextOptions _options;           // Configuration
    private IStateManager _stateManager;         // Change tracking
    private IDbContextServices _contextServices; // DI services
    private IModel _model;                       // Entity metadata

    // Key properties you use
    public ChangeTracker ChangeTracker { get; }  // Access to tracked entities
    public DatabaseFacade Database { get; }       // Low-level DB operations

    // Key methods
    public DbSet<TEntity> Set<TEntity>();        // Get DbSet for entity
    public EntityEntry<TEntity> Entry<TEntity>(); // Get tracking info
    public int SaveChanges();                     // Persist changes
    public void Dispose();                        // Clean up
}
```

### DbContext Lifecycle

```
┌─────────────────────────────────────────────────────────────────┐
│                    DbContext Lifecycle                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  1. CREATION                                                     │
│     using var context = new AppDbContext();                     │
│     └── Connection NOT opened yet                               │
│     └── Model is built/cached                                   │
│                                                                  │
│  2. QUERYING                                                     │
│     var emps = context.Employees.ToList();                      │
│     └── Connection opened                                       │
│     └── SQL executed                                            │
│     └── Results materialized to objects                         │
│     └── Objects tracked (unless AsNoTracking)                   │
│     └── Connection closed (returned to pool)                    │
│                                                                  │
│  3. TRACKING CHANGES                                             │
│     emp.Salary = 60000;                                         │
│     └── Change detected by ChangeTracker                        │
│     └── Entity marked as Modified                               │
│                                                                  │
│  4. SAVING                                                       │
│     context.SaveChanges();                                      │
│     └── Connection opened                                       │
│     └── Transaction started                                     │
│     └── SQL generated for changes                               │
│     └── SQL executed                                            │
│     └── Transaction committed                                   │
│     └── Connection closed                                       │
│                                                                  │
│  5. DISPOSAL                                                     │
│     context.Dispose();                                          │
│     └── All tracked entities cleared                            │
│     └── Resources released                                      │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📊 DbSet<T> Explained

```csharp
public DbSet<Employee> Employees { get; set; }
```

### What DbSet Provides

```
DbSet<Employee>
├── IQueryable<Employee>  → LINQ queries translated to SQL
├── Add(entity)           → Mark for INSERT
├── Remove(entity)        → Mark for DELETE
├── Find(key)             → Find by primary key (checks cache first!)
├── Local                 → Access tracked entities
└── Attach(entity)        → Start tracking existing entity
```

### DbSet is NOT a Collection

```csharp
// WRONG mental model:
// DbSet is NOT like List<Employee> in memory

// CORRECT mental model:
// DbSet is a QUERY BUILDER that talks to database

// This builds a query, doesn't load all data:
IQueryable<Employee> query = context.Employees.Where(e => e.Salary > 50000);

// Query only executes when you materialize:
List<Employee> results = query.ToList();  // NOW SQL runs!
```

---

## 🔧 Alternative: Dependency Injection Style

In real applications (ASP.NET Core), you'd use DI:

```csharp
// AppDbContext.cs - Production style
public class AppDbContext : DbContext
{
    // Constructor injection of options
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees { get; set; }
    public DbSet<Department> Departments { get; set; }
}

// Program.cs / Startup.cs - Register in DI
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
);

// Controller/Service - Inject context
public class EmployeeService
{
    private readonly AppDbContext _context;

    public EmployeeService(AppDbContext context)
    {
        _context = context;  // DI provides scoped instance
    }
}
```

For our console app, we'll use the simpler `OnConfiguring` approach.

---

## ❓ Interview Questions

### Q1: What is DbContext and what is its purpose?

**Answer:**

> DbContext is the primary class for interacting with a database in EF Core. It serves as:
>
> 1. **Unit of Work** - Groups multiple operations into a single transaction
> 2. **Repository** - Provides DbSet<T> for each entity type
> 3. **Change Tracker** - Tracks modifications to entities
> 4. **Query Manager** - Translates LINQ to SQL
> 5. **Configuration Point** - Defines model mappings

---

### Q2: What is the recommended lifetime for DbContext?

**Answer:**

> DbContext should be **short-lived** (scoped per request in web apps):
>
> ```csharp
> // GOOD - Short lived
> using (var context = new AppDbContext())
> {
>     // Do work
>     context.SaveChanges();
> } // Disposed immediately
>
> // BAD - Long lived (singleton)
> public class BadService
> {
>     private static AppDbContext _context = new();  // NEVER do this!
> }
> ```
>
> **Why short-lived?**
>
> - Change tracker accumulates entities → memory leak
> - Stale data in cache
> - Connection pool exhaustion
> - Concurrency issues

---

### Q3: What is DbSet<T>?

**Answer:**

> DbSet<T> represents a collection of entities that maps to a database table. It provides:
>
> - LINQ query capabilities (via IQueryable<T>)
> - CRUD operations (Add, Remove, Find)
> - Access to local cache of tracked entities
>
> Key insight: DbSet is **NOT** an in-memory collection. It's a **query builder** that generates SQL.

---

### Q4: What happens when DbContext is disposed?

**Answer:**

> When disposed:
>
> 1. All tracked entities are cleared from ChangeTracker
> 2. Database connections are returned to the pool
> 3. Internal services are released
>
> ⚠️ Any tracked entities become "detached" and won't save changes if you try to use them with a new context.

---

### Q5: Explain OnConfiguring vs Constructor injection

**Answer:**

> **OnConfiguring**:
>
> - Override method in DbContext
> - Hardcoded configuration
> - Good for simple apps, testing
>
> **Constructor injection**:
>
> - Options passed via DI container
> - Configuration externalized
> - Required for ASP.NET Core apps
> - Better for testing (can mock/substitute)
>
> Production apps should use constructor injection for flexibility.

---

## ✍️ Exercise: Your Turn!

### Task 1: Create the DbContext

Create `Data/AppDbContext.cs` with this exact code:

```csharp
using Microsoft.EntityFrameworkCore;
using EFCoreDemoApp.Entities;

namespace EFCoreDemoApp.Data;

public class AppDbContext : DbContext
{
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Department> Departments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            @"Server=(localdb)\MSSQLLocalDB;Database=EFCoreDemoDB;Trusted_Connection=True;"
        );

        // Enable logging to see SQL
        optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
        optionsBuilder.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
```

### Task 2: Think About These

1. Why do we inherit from `DbContext`?
2. What would happen if we forgot to add `DbSet<Employee>`?
3. Why is `LogTo(Console.WriteLine)` useful for learning?

### Task 3: Predict

Before we create entities, predict:

- What SQL table name will `DbSet<Employee> Employees` create?
- What if we named it `DbSet<Employee> Staff`?

---

## ✅ Checkpoint

After this step:

```
EFCoreDemoApp/
├── Data/
│   └── AppDbContext.cs    ← NEW!
├── Entities/              ← Still empty
├── Program.cs
└── EFCoreDemoApp.csproj
```

**Note:** The code won't compile yet because `Employee` and `Department` classes don't exist. That's next!

---

**Next Step:** [ConnectionStrings.md](./ConnectionStrings.md) - Understanding connection strings

Then: [../03-Entities-Migrations/EntityDesign.md](../03-Entities-Migrations/EntityDesign.md) - Creating entities
