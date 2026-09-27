# One-to-Many Relationships

> **Duration:** 20 minutes  
> **Goal:** Master relationship configuration in EF Core

---

## 🎯 Our Relationship: Department → Employees

```
┌─────────────────────────────────────────────────────────────────┐
│                    One-to-Many Relationship                      │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Department (Principal/Parent)     Employee (Dependent/Child)   │
│  ─────────────────────────────     ──────────────────────────  │
│  │ DepartmentId (PK)              │ EmployeeId (PK)            │
│  │ Name                           │ FirstName                   │
│  │ Budget                         │ LastName                    │
│  │                                │ DepartmentId (FK) ←─────┐  │
│  │ Employees (Nav Collection)     │ Department (Nav Ref) ──┘  │
│  │        │                       │                            │
│  │        └── HasMany ────────────┤                            │
│  │                                                              │
│  └─────────────────────────────────────────────────────────────┘
│                                                                  │
│  One Department ──has──► Many Employees                         │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 💻 Configuration Options

### Option 1: Convention (Automatic)

```csharp
// Just having these properties enables EF to figure out the relationship

public class Department
{
    public int DepartmentId { get; set; }
    public string Name { get; set; }

    // Collection navigation
    public ICollection<Employee> Employees { get; set; }
}

public class Employee
{
    public int EmployeeId { get; set; }
    public string FirstName { get; set; }

    // FK property (convention: <NavigationProperty>Id)
    public int DepartmentId { get; set; }

    // Reference navigation
    public Department Department { get; set; }
}

// EF Core detects:
// - Department.Employees is collection nav
// - Employee.Department is reference nav
// - Employee.DepartmentId is FK (naming convention)
```

### Option 2: Fluent API (Explicit & Recommended)

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Employee>()
        .HasOne(e => e.Department)           // Employee has ONE Department
        .WithMany(d => d.Employees)          // Department has MANY Employees
        .HasForeignKey(e => e.DepartmentId)  // FK is DepartmentId
        .OnDelete(DeleteBehavior.Restrict);  // Delete behavior
}
```

### Option 3: Data Annotations

```csharp
public class Employee
{
    public int EmployeeId { get; set; }

    [ForeignKey(nameof(Department))]  // Explicit FK
    public int DeptId { get; set; }   // Custom FK name

    public Department Department { get; set; }
}
```

---

## 🔍 Relationship Components

```
┌─────────────────────────────────────────────────────────────────┐
│                    Relationship Parts                            │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Principal Entity (Parent)                                      │
│  └── The "one" side: Department                                 │
│  └── Has the principal key (DepartmentId)                       │
│                                                                  │
│  Dependent Entity (Child)                                       │
│  └── The "many" side: Employee                                  │
│  └── Contains the foreign key (DepartmentId)                    │
│                                                                  │
│  Navigation Properties                                          │
│  └── Collection: Department.Employees (ICollection<Employee>)   │
│  └── Reference: Employee.Department                             │
│                                                                  │
│  Foreign Key                                                    │
│  └── Employee.DepartmentId → Department.DepartmentId            │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📊 Required vs Optional Relationships

### Required Relationship (Non-nullable FK)

```csharp
public class Employee
{
    // Required FK - employee MUST have a department
    public int DepartmentId { get; set; }  // Non-nullable
    public Department Department { get; set; }
}

// Database: DepartmentId INT NOT NULL
// Deleting Department with employees:
// - Cascade: Deletes employees too
// - Restrict: Throws exception
```

### Optional Relationship (Nullable FK)

```csharp
public class Employee
{
    // Optional FK - employee CAN be without department
    public int? DepartmentId { get; set; }  // Nullable
    public Department? Department { get; set; }
}

// Database: DepartmentId INT NULL
// Deleting Department:
// - SetNull: Sets employee's DepartmentId to NULL
```

---

## 🔄 Delete Behaviors

| Behavior        | When Delete Principal | Effect on Dependents         |
| --------------- | --------------------- | ---------------------------- |
| `Cascade`       | Delete department     | Employees deleted too        |
| `Restrict`      | Delete department     | Exception if employees exist |
| `SetNull`       | Delete department     | Employee.DeptId = NULL       |
| `ClientSetNull` | Delete department     | SetNull for tracked only     |
| `NoAction`      | Delete department     | Database decides             |

```csharp
// Configure in Fluent API
modelBuilder.Entity<Employee>()
    .HasOne(e => e.Department)
    .WithMany(d => d.Employees)
    .OnDelete(DeleteBehavior.Restrict);  // Choose your behavior
```

---

## 💻 Working with Relationships

### Adding Child with Relationship

```csharp
// Option 1: Set FK property
var emp = new Employee
{
    FirstName = "John",
    DepartmentId = 1  // Set FK directly
};
context.Employees.Add(emp);

// Option 2: Set navigation property
var dept = await context.Departments.FindAsync(1);
var emp = new Employee
{
    FirstName = "John",
    Department = dept  // Set navigation (FK set automatically)
};
context.Employees.Add(emp);

// Option 3: Add to collection
var dept = await context.Departments
    .Include(d => d.Employees)
    .FirstAsync(d => d.DepartmentId == 1);

dept.Employees.Add(new Employee { FirstName = "John" });
// FK automatically set on SaveChanges
```

### Creating Parent with Children

```csharp
var dept = new Department
{
    Name = "Engineering",
    Employees = new List<Employee>
    {
        new Employee { FirstName = "John" },
        new Employee { FirstName = "Jane" }
    }
};

context.Departments.Add(dept);
await context.SaveChangesAsync();

// All employees now have DepartmentId set
```

### Querying with Relationships

```csharp
// Include related data
var deptWithEmployees = await context.Departments
    .Include(d => d.Employees)
    .FirstAsync(d => d.Name == "Engineering");

// Filter by related entity
var engineers = await context.Employees
    .Where(e => e.Department.Name == "Engineering")
    .ToListAsync();

// Project related data
var summary = await context.Departments
    .Select(d => new
    {
        d.Name,
        EmployeeCount = d.Employees.Count,
        TotalSalary = d.Employees.Sum(e => e.Salary)
    })
    .ToListAsync();
```

---

## ❓ Interview Questions

### Q1: What is the difference between principal and dependent entity?

**Answer:**

> | Principal (Parent)      | Dependent (Child)        |
> | ----------------------- | ------------------------ |
> | The "one" side          | The "many" side          |
> | Has primary key         | Has foreign key          |
> | Department              | Employee                 |
> | Can exist independently | References the principal |
>
> The FK lives in the dependent entity pointing to the principal.

---

### Q2: How do you configure a One-to-Many relationship?

**Answer:**

> Three options:
>
> **1. Convention** (automatic with correct naming):
>
> ```csharp
> public int DepartmentId { get; set; }  // FK property
> public Department Department { get; set; }  // Navigation
> ```
>
> **2. Fluent API** (recommended for explicit control):
>
> ```csharp
> modelBuilder.Entity<Employee>()
>     .HasOne(e => e.Department)
>     .WithMany(d => d.Employees)
>     .HasForeignKey(e => e.DepartmentId);
> ```
>
> **3. Data Annotations**:
>
> ```csharp
> [ForeignKey(nameof(Department))]
> public int DeptId { get; set; }
> ```

---

### Q3: What delete behaviors are available and when to use each?

**Answer:**

> | Behavior   | When to Use                                                         |
> | ---------- | ------------------------------------------------------------------- |
> | `Cascade`  | Children meaningless without parent (OrderItems when Order deleted) |
> | `Restrict` | Prevent accidental data loss                                        |
> | `SetNull`  | Children can exist independently                                    |
>
> **Default**: Cascade for required FK, ClientSetNull for optional FK.

---

### Q4: How does EF Core automatically set FKs?

**Answer:**

> When you add a child to parent's collection:
>
> ```csharp
> department.Employees.Add(employee);
> await context.SaveChangesAsync();
> ```
>
> EF Core:
>
> 1. Detects employee is added to Employees collection
> 2. Relationship fixup sets `employee.Department = department`
> 3. On SaveChanges, generates `employee.DepartmentId = department.DepartmentId`

---

### Q5: What is relationship fixup?

**Answer:**

> Relationship fixup is EF Core's automatic synchronization of navigation properties:
>
> ```csharp
> var emp = new Employee { DepartmentId = 1 };
> context.Employees.Add(emp);
>
> // After fixup, emp.Department points to Department with Id=1
> // (if it's tracked)
>
> // And that Department's Employees collection includes emp
> ```
>
> Works both ways: setting FK updates navigation, setting navigation updates FK.

---

## ✍️ Exercise

### Task 1: Create Relationship

Create a new entity `Project` with One-to-Many relationship to Employee (an employee can work on multiple projects, a project has multiple employees... wait, that's Many-to-Many!).

Instead: Create `Address` entity where one Employee has one Address (One-to-One).

### Task 2: Test Delete Behaviors

1. Create Department with Employees
2. Try to delete Department with `Restrict`
3. Change to `Cascade` and delete again

### Task 3: Query Relationships

Write queries to:

1. Get all departments with their employee count
2. Get employees in departments with budget > 100000
3. Get departments that have no employees

---

## ✅ Key Takeaways

1. Principal = "one" side, Dependent = "many" side
2. FK goes in the dependent entity
3. Use Fluent API for explicit configuration
4. Choose appropriate delete behavior
5. EF Core auto-fixes relationships (fixup)

---

**Next Step:** [../08-Loading-Strategies/LoadingStrategies.md](../08-Loading-Strategies/LoadingStrategies.md) - Loading related data
