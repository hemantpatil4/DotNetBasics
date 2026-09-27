# Step 4-5: Creating Entities

> **Duration:** 20 minutes  
> **Goal:** Design Employee and Department entities with proper relationships

---

## 🎯 What are Entities?

Entities are **POCO (Plain Old CLR Objects)** classes that map to database tables:

```
C# Class (Entity)          →    Database Table
─────────────────────────────────────────────────
public class Employee      →    Employees table
{
    public int EmployeeId  →    EmployeeId column (PK)
    public string Name     →    Name column
    public decimal Salary  →    Salary column
}
```

---

## 💻 Hands-On: Create Entities

### Create `Entities/Department.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCoreDemoApp.Entities;

/// <summary>
/// Department entity - One Department has Many Employees
/// </summary>
public class Department
{
    // Primary Key - EF Core convention: <EntityName>Id or Id
    public int DepartmentId { get; set; }

    // Required field with max length
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    // Optional field with precision
    [Column(TypeName = "decimal(18,2)")]
    public decimal Budget { get; set; }

    // Navigation property - Collection of related Employees
    // This creates the "One" side of One-to-Many
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
```

### Create `Entities/Employee.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCoreDemoApp.Entities;

/// <summary>
/// Employee entity - Many Employees belong to One Department
/// </summary>
public class Employee
{
    // Primary Key
    public int EmployeeId { get; set; }

    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Salary { get; set; }

    public DateTime JoinDate { get; set; } = DateTime.Now;

    public bool IsActive { get; set; } = true;

    // Foreign Key - explicitly defined
    public int DepartmentId { get; set; }

    // Navigation property - Reference to parent Department
    // This creates the "Many" side of One-to-Many
    public Department Department { get; set; } = null!;

    // Computed property (not mapped to database)
    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";
}
```

---

## 🔍 Entity Configuration Deep Dive

### EF Core Conventions (Automatic)

```
┌─────────────────────────────────────────────────────────────────┐
│                    EF Core Conventions                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Property Convention              →    Database Result           │
│  ─────────────────────────────────────────────────────────────  │
│  int EmployeeId                   →    PK, Identity              │
│  int Id                           →    PK, Identity              │
│  string Name                      →    nvarchar(max)             │
│  int DepartmentId (with nav)      →    FK to Departments         │
│  DateTime JoinDate                →    datetime2                 │
│  decimal Salary                   →    decimal(18,2)             │
│  bool IsActive                    →    bit                       │
│  ICollection<Employee>            →    One-to-Many nav           │
│                                                                  │
│  DbSet<Employee> Employees        →    Table: Employees          │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### Data Annotations

```csharp
// Validation + Database Schema
[Required]                    // NOT NULL
[MaxLength(100)]              // nvarchar(100)
[StringLength(100)]           // nvarchar(100)
[EmailAddress]                // Validation only

// Schema Control
[Table("Emps")]               // Custom table name
[Column("EmpName")]           // Custom column name
[Column(TypeName = "decimal(18,2)")]  // Specific SQL type

// Key Configuration
[Key]                         // Primary key (if not following convention)
[DatabaseGenerated(...)]      // Identity, Computed, None

// Not Mapped
[NotMapped]                   // Exclude from database
```

### Fluent API (Alternative to Annotations)

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Employee configuration
    modelBuilder.Entity<Employee>(entity =>
    {
        entity.HasKey(e => e.EmployeeId);

        entity.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        entity.Property(e => e.Salary)
            .HasColumnType("decimal(18,2)");

        // Relationship
        entity.HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    });

    // Department configuration
    modelBuilder.Entity<Department>(entity =>
    {
        entity.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(100);
    });
}
```

---

## 📊 Relationship Configuration

### One-to-Many Relationship

```
Department (1) ─────────────────→ (*) Employee
    │                                    │
    │  DepartmentId (PK)                 │  EmployeeId (PK)
    │  Name                              │  FirstName
    │  Budget                            │  DepartmentId (FK) ──┐
    │                                    │  Department (Nav)    │
    │                                    │                      │
    └── Employees (Nav Collection) ←─────┴──────────────────────┘
```

### How EF Core Detects Relationships

```csharp
// EF Core sees:
public class Employee
{
    public int DepartmentId { get; set; }        // FK property
    public Department Department { get; set; }    // Navigation
}

public class Department
{
    public ICollection<Employee> Employees { get; set; }  // Inverse nav
}

// EF Core automatically:
// 1. Creates FK constraint
// 2. Links navigation properties
// 3. Enables Include() for eager loading
```

---

## 🔧 C# to SQL Type Mapping

| C# Type                       | SQL Server Type    | Notes              |
| ----------------------------- | ------------------ | ------------------ |
| `int`                         | `int`              | 4 bytes            |
| `long`                        | `bigint`           | 8 bytes            |
| `string`                      | `nvarchar(max)`    | Unicode, unlimited |
| `string` + `[MaxLength(100)]` | `nvarchar(100)`    | Unicode, limited   |
| `decimal`                     | `decimal(18,2)`    | Default precision  |
| `DateTime`                    | `datetime2(7)`     | High precision     |
| `bool`                        | `bit`              | 0 or 1             |
| `Guid`                        | `uniqueidentifier` | 16 bytes           |
| `byte[]`                      | `varbinary(max)`   | Binary data        |
| `enum`                        | `int`              | Stored as number   |

---

## ❓ Interview Questions

### Q1: What are navigation properties?

**Answer:**

> Navigation properties are properties that reference related entities:
>
> - **Reference navigation**: Points to single entity (`Employee.Department`)
> - **Collection navigation**: Points to multiple entities (`Department.Employees`)
>
> They enable:
>
> - Eager loading with `Include()`
> - Lazy loading (if configured)
> - Relationship traversal in LINQ

---

### Q2: What is the difference between Data Annotations and Fluent API?

**Answer:**

> | Data Annotations         | Fluent API              |
> | ------------------------ | ----------------------- |
> | Attributes on properties | Code in OnModelCreating |
> | Limited features         | Full control            |
> | Pollutes entity class    | Keeps entity clean      |
> | Simple configurations    | Complex configurations  |
>
> **Best Practice**: Use Data Annotations for simple stuff, Fluent API for complex relationships and configurations.

---

### Q3: Why use `ICollection<T>` instead of `List<T>` for navigation?

**Answer:**

> `ICollection<T>` is the interface that EF Core requires for collection navigation properties. Benefits:
>
> - More flexible (EF can use any implementation)
> - Enables lazy loading proxies
> - Convention over implementation
>
> `List<T>` works too, but `ICollection<T>` is the recommended convention.

---

### Q4: What does `[NotMapped]` do?

**Answer:**

> `[NotMapped]` excludes a property from the database model:
>
> ```csharp
> [NotMapped]
> public string FullName => $"{FirstName} {LastName}";
> ```
>
> Use for:
>
> - Computed properties
> - Temporary data
> - Properties that shouldn't be persisted

---

### Q5: How does EF Core determine the primary key?

**Answer:**

> By convention, EF Core looks for:
>
> 1. Property named `Id`
> 2. Property named `<EntityName>Id` (e.g., `EmployeeId`)
>
> You can override with:
>
> ```csharp
> [Key]
> public int MyCustomId { get; set; }
> ```
>
> Or Fluent API:
>
> ```csharp
> entity.HasKey(e => e.MyCustomId);
> ```

---

## ✍️ Exercise: Your Turn!

### Task 1: Create Entity Files

Create both files exactly as shown:

- `Entities/Department.cs`
- `Entities/Employee.cs`

### Task 2: Verify Build

```bash
cd EFCoreDemoApp
dotnet build
```

Should compile with no errors.

### Task 3: Add a New Property

Add a `Phone` property to Employee:

- Should be optional (nullable)
- Max length 20

### Task 4: Think About This

1. What SQL type will `Salary` be?
2. What happens if you don't initialize `Employees` collection?
3. Why is `Department` navigation property marked `null!`?

---

## ✅ Checkpoint

Your project should now have:

```
EFCoreDemoApp/
├── Data/
│   └── AppDbContext.cs
├── Entities/
│   ├── Department.cs     ← NEW!
│   └── Employee.cs       ← NEW!
├── Program.cs
└── EFCoreDemoApp.csproj
```

Run `dotnet build` - it should succeed!

---

**Next Step:** [MigrationsGuide.md](./MigrationsGuide.md) - Creating and running migrations
