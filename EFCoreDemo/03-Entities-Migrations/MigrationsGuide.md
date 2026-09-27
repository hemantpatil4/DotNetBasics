# Step 6: Migrations - Your Database Version Control

> **Duration:** 25 minutes  
> **Goal:** Master migrations for database schema management

---

## 🎯 What are Migrations?

Migrations are **versioned snapshots** of your database schema:

```
┌─────────────────────────────────────────────────────────────────┐
│                     EF Core Migrations                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Your C# Entities  →  Migration  →  Database Schema              │
│                                                                  │
│  Employee.cs        →  Add-Migration  →  CREATE TABLE Employees  │
│  Department.cs      →  Initial        →  CREATE TABLE Departments│
│                                                                  │
│  [change Employee]  →  Add-Migration  →  ALTER TABLE Employees   │
│                     →  AddPhone        →  ADD Phone nvarchar(20) │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🔄 Migration Workflow

```
┌───────────────┐     ┌───────────────┐     ┌───────────────┐
│   Modify      │     │    Create     │     │    Apply      │
│   Entities    │ ──→ │   Migration   │ ──→ │   to DB       │
└───────────────┘     └───────────────┘     └───────────────┘
                            │
                            ▼
                   Migrations/
                   ├── 20240115_Initial.cs
                   ├── 20240115_AddPhone.cs
                   └── ModelSnapshot.cs
```

---

## 💻 Hands-On: First Migration

### Step 1: Update DbContext with DbSets

Make sure your `Data/AppDbContext.cs` has DbSets:

```csharp
using Microsoft.EntityFrameworkCore;
using EFCoreDemoApp.Entities;

namespace EFCoreDemoApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    // DbSets - These become tables
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Department> Departments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure relationship
        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed initial data (optional)
        modelBuilder.Entity<Department>().HasData(
            new Department { DepartmentId = 1, Name = "Engineering", Budget = 100000 },
            new Department { DepartmentId = 2, Name = "HR", Budget = 50000 }
        );
    }
}
```

### Step 2: Create Initial Migration

```bash
cd EFCoreDemoApp

# Create migration
dotnet ef migrations add InitialCreate

# What this creates:
# Migrations/
# ├── YYYYMMDD_InitialCreate.cs          ← Migration code
# ├── YYYYMMDD_InitialCreate.Designer.cs  ← Snapshot reference
# └── AppDbContextModelSnapshot.cs        ← Current model state
```

### Step 3: Examine Migration File

```csharp
// Migrations/YYYYMMDD_InitialCreate.cs
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // CREATE TABLE Departments
        migrationBuilder.CreateTable(
            name: "Departments",
            columns: table => new
            {
                DepartmentId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Budget = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Departments", x => x.DepartmentId);
            });

        // CREATE TABLE Employees
        migrationBuilder.CreateTable(
            name: "Employees",
            columns: table => new
            {
                EmployeeId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                // ... other columns
                DepartmentId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Employees", x => x.EmployeeId);
                table.ForeignKey(
                    name: "FK_Employees_Departments_DepartmentId",
                    column: x => x.DepartmentId,
                    principalTable: "Departments",
                    principalColumn: "DepartmentId",
                    onDelete: ReferentialAction.Restrict);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Reverse operations
        migrationBuilder.DropTable(name: "Employees");
        migrationBuilder.DropTable(name: "Departments");
    }
}
```

### Step 4: Apply Migration to Database

```bash
# Apply all pending migrations
dotnet ef database update

# Output:
# Applying migration '20240115123456_InitialCreate'.
# Done.
```

---

## 📋 Essential Migration Commands

| Command                                 | Purpose                                |
| --------------------------------------- | -------------------------------------- |
| `dotnet ef migrations add <Name>`       | Create new migration                   |
| `dotnet ef database update`             | Apply pending migrations               |
| `dotnet ef database update <Migration>` | Update to specific migration           |
| `dotnet ef migrations remove`           | Remove last migration (if not applied) |
| `dotnet ef migrations list`             | List all migrations                    |
| `dotnet ef database drop`               | Delete database                        |
| `dotnet ef migrations script`           | Generate SQL script                    |

---

## 🔧 Common Migration Scenarios

### Scenario 1: Add New Column

```csharp
// Add to Employee.cs
[MaxLength(20)]
public string? Phone { get; set; }
```

```bash
dotnet ef migrations add AddEmployeePhone
dotnet ef database update
```

### Scenario 2: Change Column Type

```csharp
// Change MaxLength from 50 to 100
[MaxLength(100)]  // was 50
public string FirstName { get; set; }
```

```bash
dotnet ef migrations add ChangeFirstNameLength
dotnet ef database update
```

### Scenario 3: Add New Entity

```csharp
// Create new Project.cs entity
public class Project { ... }

// Add to DbContext
public DbSet<Project> Projects { get; set; }
```

```bash
dotnet ef migrations add AddProjectEntity
dotnet ef database update
```

### Scenario 4: Remove Migration (Not Applied)

```bash
# Oops, made a mistake
dotnet ef migrations remove  # Removes last unapplied migration
```

### Scenario 5: Rollback to Previous Migration

```bash
# Rollback to InitialCreate
dotnet ef database update InitialCreate

# Then remove the migration file
dotnet ef migrations remove
```

---

## 📁 Migration Files Explained

```
Migrations/
├── 20240115123456_InitialCreate.cs           ← Up() and Down() methods
├── 20240115123456_InitialCreate.Designer.cs  ← Snapshot for this migration
├── 20240115124000_AddPhone.cs                ← Second migration
├── 20240115124000_AddPhone.Designer.cs
└── AppDbContextModelSnapshot.cs              ← Current model state
```

### What's in Each File?

```
┌────────────────────────────────────────────────────────────────┐
│  Migration.cs (Main File)                                       │
├────────────────────────────────────────────────────────────────┤
│  Up()   - What to do when applying migration                   │
│  Down() - What to do when reverting migration                  │
│                                                                │
│  Example:                                                      │
│  Up():   CREATE TABLE Employees                                │
│  Down(): DROP TABLE Employees                                  │
└────────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────────┐
│  Migration.Designer.cs                                          │
├────────────────────────────────────────────────────────────────┤
│  Snapshot of the model at this point in time                   │
│  Used to calculate changes for next migration                  │
└────────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────────┐
│  ModelSnapshot.cs                                               │
├────────────────────────────────────────────────────────────────┤
│  Current state of the entire model                             │
│  Updated with each new migration                               │
│  EF compares this to your entities to detect changes           │
└────────────────────────────────────────────────────────────────┘
```

---

## 🔄 \_\_EFMigrationsHistory Table

EF Core creates this table to track applied migrations:

```sql
SELECT * FROM __EFMigrationsHistory

-- Result:
-- MigrationId                         | ProductVersion
-- ────────────────────────────────────│───────────────
-- 20240115123456_InitialCreate        | 8.0.0
-- 20240115124000_AddPhone             | 8.0.0
```

---

## 🏭 Production Best Practices

### Generate SQL Script for DBA

```bash
# Generate full script
dotnet ef migrations script -o migration.sql

# Generate from specific migration
dotnet ef migrations script InitialCreate AddPhone -o update.sql

# Generate idempotent script (safe to run multiple times)
dotnet ef migrations script --idempotent -o migration.sql
```

### Review Generated SQL

```sql
-- Example output
IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory]
              WHERE [MigrationId] = N'20240115_InitialCreate')
BEGIN
    CREATE TABLE [Departments] (
        [DepartmentId] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Budget] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_Departments] PRIMARY KEY ([DepartmentId])
    );
END;
```

---

## ❓ Interview Questions

### Q1: What is the purpose of migrations in EF Core?

**Answer:**

> Migrations provide **version control for your database schema**:
>
> - Track schema changes over time
> - Enable team collaboration
> - Support deployment pipelines
> - Allow rollbacks to previous states
>
> They generate SQL scripts from C# entity changes.

---

### Q2: What is the difference between `Add-Migration` and `Update-Database`?

**Answer:**

> | Add-Migration              | Update-Database                 |
> | -------------------------- | ------------------------------- |
> | Creates migration files    | Applies migrations to database  |
> | Compares model to snapshot | Executes Up() method            |
> | Generates Up/Down code     | Updates \_\_EFMigrationsHistory |
> | Does NOT touch database    | Actually modifies database      |

---

### Q3: How do you rollback a migration?

**Answer:**

> **If migration is NOT applied:**
>
> ```bash
> dotnet ef migrations remove
> ```
>
> **If migration IS applied:**
>
> ```bash
> # Revert to previous migration
> dotnet ef database update PreviousMigrationName
>
> # Then remove migration file
> dotnet ef migrations remove
> ```

---

### Q4: What is stored in \_\_EFMigrationsHistory?

**Answer:**

> This system table tracks:
>
> - **MigrationId**: Unique identifier (timestamp + name)
> - **ProductVersion**: EF Core version used
>
> EF Core checks this table to know which migrations are already applied.

---

### Q5: How do migrations work in a team environment?

**Answer:**

> Best practices:
>
> - Commit migration files to source control
> - Never edit applied migrations
> - Coordinate migration creation (one at a time)
> - Use `dotnet ef migrations script` for production deployments
> - Resolve conflicts by removing and recreating

---

### Q6: What is the ModelSnapshot file?

**Answer:**

> `ModelSnapshot.cs` represents the **current state** of your model.
>
> When you run `Add-Migration`:
>
> 1. EF compares your entities to ModelSnapshot
> 2. Detects what changed
> 3. Generates migration code
> 4. Updates ModelSnapshot
>
> **Never manually edit this file!**

---

## ✍️ Exercise: Practice Migrations

### Task 1: Create Initial Migration

```bash
cd EFCoreDemoApp
dotnet ef migrations add InitialCreate
```

Examine the generated files.

### Task 2: Apply to Database

```bash
dotnet ef database update
```

Verify tables exist in SQL Server.

### Task 3: Add New Column

Add to `Employee.cs`:

```csharp
[MaxLength(200)]
public string? Address { get; set; }
```

Create and apply migration:

```bash
dotnet ef migrations add AddEmployeeAddress
dotnet ef database update
```

### Task 4: View Migration Script

```bash
dotnet ef migrations script -o script.sql
```

Open and review `script.sql`.

### Task 5: Rollback Practice

```bash
# Rollback to initial
dotnet ef database update InitialCreate

# Remove AddEmployeeAddress migration
dotnet ef migrations remove
```

---

## ✅ Checkpoint

After completing migrations:

```
EFCoreDemoApp/
├── Migrations/                          ← NEW!
│   ├── YYYYMMDD_InitialCreate.cs
│   ├── YYYYMMDD_InitialCreate.Designer.cs
│   └── AppDbContextModelSnapshot.cs
├── Data/
│   └── AppDbContext.cs
├── Entities/
│   ├── Department.cs
│   └── Employee.cs
└── ...
```

Database should have:

- `Departments` table
- `Employees` table
- `__EFMigrationsHistory` table

---

**Next Step:** [../04-CRUD-Operations/CreateOperations.md](../04-CRUD-Operations/CreateOperations.md) - Insert data into database
