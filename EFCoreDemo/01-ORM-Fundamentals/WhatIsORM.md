# Step 1: What is ORM and Why Entity Framework Exists

> **Duration:** 15 minutes  
> **Goal:** Understand the problem ORM solves and why EF Core is the go-to choice

---

## 🎯 The Problem: Impedance Mismatch

### Without ORM - The Pain

```csharp
// Traditional ADO.NET approach - LOTS of boilerplate!
public Employee GetEmployee(int id)
{
    Employee employee = null;

    using (SqlConnection conn = new SqlConnection(connectionString))
    {
        conn.Open();

        string sql = "SELECT EmployeeId, FirstName, LastName, Email, Salary FROM Employees WHERE EmployeeId = @Id";

        using (SqlCommand cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@Id", id);

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    employee = new Employee
                    {
                        EmployeeId = reader.GetInt32(0),
                        FirstName = reader.GetString(1),
                        LastName = reader.GetString(2),
                        Email = reader.GetString(3),
                        Salary = reader.GetDecimal(4)
                    };
                }
            }
        }
    }

    return employee;
}

// For INSERT - even more code!
public void InsertEmployee(Employee emp)
{
    using (SqlConnection conn = new SqlConnection(connectionString))
    {
        conn.Open();
        string sql = @"INSERT INTO Employees (FirstName, LastName, Email, Salary, DepartmentId)
                       VALUES (@FirstName, @LastName, @Email, @Salary, @DeptId)";

        using (SqlCommand cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@FirstName", emp.FirstName);
            cmd.Parameters.AddWithValue("@LastName", emp.LastName);
            cmd.Parameters.AddWithValue("@Email", emp.Email);
            cmd.Parameters.AddWithValue("@Salary", emp.Salary);
            cmd.Parameters.AddWithValue("@DeptId", emp.DepartmentId);
            cmd.ExecuteNonQuery();
        }
    }
}
```

### Problems with Raw ADO.NET:

| Problem                | Impact                                           |
| ---------------------- | ------------------------------------------------ |
| **Boilerplate code**   | Write same connection/command pattern everywhere |
| **Manual mapping**     | Map each column to property manually             |
| **Type safety**        | SQL is a string - no compile-time checking       |
| **SQL Injection risk** | Easy to forget parameterization                  |
| **No change tracking** | Must track what changed manually                 |
| **Database coupling**  | SQL syntax varies between databases              |

---

## 🔧 What is ORM?

**ORM = Object-Relational Mapper**

```
┌─────────────────┐         ┌─────────────────┐
│   C# Objects    │  ←ORM→  │   Database      │
│                 │         │   Tables        │
│  Employee emp   │         │  Employees      │
│  emp.FirstName  │   ↔     │  FirstName col  │
│  emp.Salary     │         │  Salary col     │
└─────────────────┘         └─────────────────┘
```

### Real-World Analogy

Think of ORM as a **translator** between two people who speak different languages:

```
You (C# Developer)     ORM (Translator)     Database (SQL World)
      │                      │                     │
      │  "Get employee 5"    │                     │
      │ ──────────────────→  │                     │
      │                      │  SELECT * FROM      │
      │                      │  Employees          │
      │                      │  WHERE Id = 5       │
      │                      │ ──────────────────→ │
      │                      │                     │
      │                      │  ← Returns rows     │
      │                      │ ←────────────────── │
      │  ← Employee object   │                     │
      │ ←────────────────────│                     │
```

---

## ✨ With EF Core - Clean Code

```csharp
// Same operations with EF Core - CLEAN!
public class EmployeeService
{
    private readonly AppDbContext _context;

    public EmployeeService(AppDbContext context)
    {
        _context = context;
    }

    // GET - One line!
    public Employee GetEmployee(int id)
    {
        return _context.Employees.Find(id);
    }

    // INSERT - Just add and save!
    public void InsertEmployee(Employee emp)
    {
        _context.Employees.Add(emp);
        _context.SaveChanges();
    }

    // UPDATE - Modify and save!
    public void UpdateSalary(int id, decimal newSalary)
    {
        var emp = _context.Employees.Find(id);
        emp.Salary = newSalary;
        _context.SaveChanges();  // EF tracks the change!
    }

    // DELETE
    public void DeleteEmployee(int id)
    {
        var emp = _context.Employees.Find(id);
        _context.Employees.Remove(emp);
        _context.SaveChanges();
    }

    // QUERY with LINQ - Type-safe!
    public List<Employee> GetHighEarners(decimal minSalary)
    {
        return _context.Employees
            .Where(e => e.Salary > minSalary)
            .OrderByDescending(e => e.Salary)
            .ToList();
    }
}
```

---

## 🏗️ What EF Core Does Internally

```
┌─────────────────────────────────────────────────────────────────┐
│                        EF Core Pipeline                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│   1. LINQ Query                                                  │
│      _context.Employees.Where(e => e.Salary > 50000)            │
│                           │                                      │
│                           ▼                                      │
│   2. Expression Tree     (IQueryable builds expression tree)     │
│                           │                                      │
│                           ▼                                      │
│   3. Query Translation   (EF Core translates to SQL)            │
│                           │                                      │
│                           ▼                                      │
│   4. SQL Generation      SELECT * FROM Employees                │
│                          WHERE Salary > 50000                    │
│                           │                                      │
│                           ▼                                      │
│   5. Database Execution  (ADO.NET executes SQL)                 │
│                           │                                      │
│                           ▼                                      │
│   6. Materialization     (Results mapped to C# objects)         │
│                           │                                      │
│                           ▼                                      │
│   7. Change Tracking     (Objects tracked for changes)          │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📊 EF Core vs Other ORMs

| Feature               | EF Core     | Dapper    | NHibernate |
| --------------------- | ----------- | --------- | ---------- |
| **Type**              | Full ORM    | Micro ORM | Full ORM   |
| **Learning Curve**    | Medium      | Low       | High       |
| **Performance**       | Good        | Excellent | Good       |
| **Change Tracking**   | ✅ Yes      | ❌ No     | ✅ Yes     |
| **LINQ Support**      | ✅ Full     | ❌ No     | ✅ Partial |
| **Migrations**        | ✅ Built-in | ❌ No     | ✅ Yes     |
| **Code First**        | ✅ Yes      | ❌ No     | ✅ Yes     |
| **Microsoft Support** | ✅ Official | Community | Community  |

### When to Use What?

```
Use EF Core when:
├── Building new applications
├── Need change tracking
├── Want LINQ queries
├── Need migrations
└── Standard CRUD operations

Use Dapper when:
├── Need maximum performance
├── Complex stored procedures
├── Read-heavy applications
└── Simple mapping needs
```

---

## 🔍 Key EF Core Components

```
┌─────────────────────────────────────────────────────────────────┐
│                     EF Core Architecture                         │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐       │
│  │   DbContext  │    │   DbSet<T>   │    │   Entities   │       │
│  │              │    │              │    │              │       │
│  │ - Entry point│    │ - Table rep  │    │ - POCO class │       │
│  │ - Unit of    │    │ - Query      │    │ - Maps to    │       │
│  │   Work       │    │ - CRUD ops   │    │   table      │       │
│  └──────────────┘    └──────────────┘    └──────────────┘       │
│         │                   │                   │                │
│         └───────────────────┴───────────────────┘                │
│                             │                                    │
│                    ┌────────▼────────┐                          │
│                    │ Change Tracker  │                          │
│                    │ - Tracks state  │                          │
│                    │ - Detects changes│                         │
│                    └────────┬────────┘                          │
│                             │                                    │
│                    ┌────────▼────────┐                          │
│                    │  Database       │                          │
│                    │  Provider       │                          │
│                    │  (SQL Server)   │                          │
│                    └─────────────────┘                          │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## ❓ Interview Questions & Answers

### Q1: What is ORM and why do we need it?

**Answer:**

> ORM (Object-Relational Mapper) is a technique that lets you query and manipulate data from a database using an object-oriented paradigm. It acts as a bridge between the object-oriented world of C# and the relational world of databases.
>
> We need it because:
>
> 1. **Reduces boilerplate** - No manual connection/command management
> 2. **Type safety** - LINQ provides compile-time checking
> 3. **Productivity** - Write less code, focus on business logic
> 4. **Database abstraction** - Can switch databases easier
> 5. **Change tracking** - Automatically detects what changed

---

### Q2: What is the difference between EF Core and EF 6?

**Answer:**

> EF Core is a complete rewrite of Entity Framework, not an upgrade:
>
> | Aspect           | EF 6                | EF Core                             |
> | ---------------- | ------------------- | ----------------------------------- |
> | Platform         | .NET Framework only | Cross-platform (.NET Core, .NET 5+) |
> | Performance      | Good                | Better (lighter, faster)            |
> | Features         | Full-featured       | Started minimal, now mature         |
> | EDMX Support     | Yes                 | No (Code-First only)                |
> | Lazy Loading     | Built-in            | Optional package                    |
> | Batch Operations | No                  | Yes                                 |

---

### Q3: What problems does EF Core solve?

**Answer:**

> 1. **Impedance Mismatch** - Bridges object-oriented C# with relational tables
> 2. **Boilerplate Code** - Eliminates repetitive ADO.NET code
> 3. **SQL Injection** - Parameterizes queries automatically
> 4. **Change Detection** - Tracks entity state changes
> 5. **Database Migrations** - Version control for database schema
> 6. **LINQ Integration** - Type-safe queries with IntelliSense

---

### Q4: When would you NOT use EF Core?

**Answer:**

> - **High-performance scenarios** where every millisecond counts (use Dapper)
> - **Complex stored procedures** that return multiple result sets
> - **Legacy databases** with non-standard schemas
> - **Bulk operations** on millions of records (use SqlBulkCopy)
> - **Simple read-only queries** where change tracking overhead isn't needed

---

## ✍️ Exercise: Your Turn!

### Task 1: Analyze the Problem

Look at this ADO.NET code and list all the problems:

```csharp
public List<Employee> GetEmployeesByDepartment(int deptId)
{
    List<Employee> employees = new List<Employee>();
    SqlConnection conn = new SqlConnection(connString);
    conn.Open();

    string sql = "SELECT * FROM Employees WHERE DepartmentId = " + deptId;  // Problem?
    SqlCommand cmd = new SqlCommand(sql, conn);
    SqlDataReader reader = cmd.ExecuteReader();

    while (reader.Read())
    {
        employees.Add(new Employee
        {
            EmployeeId = (int)reader["EmployeeId"],
            FirstName = (string)reader["FirstName"],
            // ... more mapping
        });
    }

    return employees;
}
```

**Find these problems:**

1. SQL Injection vulnerability - where?
2. Resource leak - what's not disposed?
3. Manual mapping - how many lines?
4. Type safety - what could go wrong?

### Task 2: Think About This

Write answers to these questions (just think, no code yet):

1. If you have 20 tables, how many similar methods would you write with ADO.NET?
2. What happens if you rename a column in the database?
3. How would you track which employees were modified?

---

## ✅ Key Takeaways

```
╔═══════════════════════════════════════════════════════════════╗
║                    Step 1 Summary                              ║
╠═══════════════════════════════════════════════════════════════╣
║                                                                ║
║  ORM = Object-Relational Mapper                               ║
║  ├── Bridges C# objects ↔ Database tables                     ║
║  ├── Eliminates boilerplate ADO.NET code                      ║
║  └── Provides LINQ, Change Tracking, Migrations               ║
║                                                                ║
║  EF Core Benefits:                                            ║
║  ├── Type-safe LINQ queries                                   ║
║  ├── Automatic change tracking                                ║
║  ├── Database migrations                                      ║
║  ├── Cross-platform support                                   ║
║  └── Active Microsoft development                             ║
║                                                                ║
║  Remember: EF Core is NOT always the answer                   ║
║  └── Use Dapper for raw performance needs                     ║
║                                                                ║
╚═══════════════════════════════════════════════════════════════╝
```

---

**Next Step:** [../02-Setup-Configuration/PackageInstallation.md](../02-Setup-Configuration/PackageInstallation.md)

Ready? Let me know to continue to Step 2: Installing EF Core Packages!
