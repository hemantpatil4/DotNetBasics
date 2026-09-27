# Entity Framework Core – Complete Learning Guide

> **Intensive 4-5 Hour Hands-On Session**  
> For Senior .NET Developer Interviews & Production Work

---

## 📚 Learning Path Structure

```
EFCoreDemo/
├── EFCoreOverview.md                    ← You are here (START)
│
├── 01-ORM-Fundamentals/
│   └── WhatIsORM.md                     ✅ Complete
│
├── 02-Setup-Configuration/
│   ├── PackageInstallation.md           ✅ Complete
│   ├── DbContextGuide.md                ✅ Complete
│   └── ConnectionStrings.md             ✅ Complete
│
├── 03-Entities-Migrations/
│   ├── EntityDesign.md                  ✅ Complete
│   └── MigrationsGuide.md               ✅ Complete
│
├── 04-CRUD-Operations/
│   ├── CreateOperations.md              ✅ Complete
│   ├── ReadOperations.md                ✅ Complete
│   ├── UpdateOperations.md              ✅ Complete
│   └── DeleteOperations.md              ✅ Complete
│
├── 05-Querying/
│   ├── IQueryableVsIEnumerable.md       ✅ Complete (Critical!)
│   └── LINQTranslation.md               ✅ Complete
│
├── 06-Change-Tracking/
│   ├── ChangeTrackerGuide.md            ✅ Complete
│   └── SaveChangesInternals.md          ✅ Complete
│
├── 07-Relationships/
│   └── OneToMany.md                     ✅ Complete
│
├── 08-Loading-Strategies/
│   └── LoadingStrategies.md             ✅ Complete (Eager/Lazy/Explicit)
│
├── 09-Performance/
│   └── PerformanceTips.md               ✅ Complete
│
├── 10-Interview-Prep/
│   └── CheatSheet.md                    ✅ Complete (REVIEW BEFORE INTERVIEW!)
│
└── EFCoreDemoApp/                       ✅ Runnable Demo Project
    ├── Entities/
    │   ├── Department.cs
    │   └── Employee.cs
    ├── Data/
    │   └── AppDbContext.cs
    ├── Program.cs                       ← Run this to see demos!
    └── EFCoreDemoApp.csproj
```

---

## 🎯 What You'll Learn

| Module | Topic                                   | Duration |
| ------ | --------------------------------------- | -------- |
| 1      | ORM Fundamentals                        | 15 min   |
| 2      | Setup & Configuration                   | 20 min   |
| 3      | Entities & Migrations                   | 30 min   |
| 4      | CRUD Operations                         | 45 min   |
| 5      | LINQ Queries                            | 30 min   |
| 6      | Change Tracking & SaveChanges Internals | 45 min   |
| 7      | Relationships                           | 30 min   |
| 8      | Loading Strategies                      | 30 min   |
| 9      | Performance Best Practices              | 20 min   |
| 10     | Interview Preparation                   | 30 min   |

**Total: ~4.5 hours**

---

## 🔧 Project Details

| Item          | Value                   |
| ------------- | ----------------------- |
| Framework     | .NET 8                  |
| Project Type  | Console Application     |
| ORM           | Entity Framework Core 8 |
| Database      | SQL Server (LocalDB)    |
| Project Name  | `EFCoreDemoApp`         |
| Database Name | `EFCoreDemoDB`          |

### Entities We'll Build

```
Department (1) ←──────→ (Many) Employee
     │                        │
     ├─ DepartmentId          ├─ EmployeeId
     ├─ Name                  ├─ FirstName
     ├─ Budget                ├─ LastName
     └─ Employees (nav)       ├─ Email
                              ├─ Salary
                              ├─ JoinDate
                              ├─ DepartmentId (FK)
                              └─ Department (nav)
```

---

## 📖 How to Use This Guide

### Learning Approach (for each topic):

```
1. 📖 Read the concept explanation
2. 💻 Run the hands-on code
3. 🔍 Understand internals (what happens behind the scenes)
4. 📊 See the SQL generated
5. ❓ Answer interview questions
6. ✍️ Complete the exercise
```

### Prerequisites

- ✅ C# fundamentals (you have this)
- ✅ LINQ understanding (you have this)
- ✅ SQL basics
- ✅ .NET 8 SDK installed
- ✅ SQL Server LocalDB or Docker SQL Server

---

## 🚀 Quick Start

```bash
# Navigate to project
cd EFCoreDemo/EFCoreDemoApp

# Run the demo
dotnet run
```

---

## 📋 Progress Checklist

- [ ] Step 1: ORM Fundamentals
- [ ] Step 2: Package Installation
- [ ] Step 3: DbContext Creation
- [ ] Step 4: Connection String Configuration
- [ ] Step 5: Entity Design
- [ ] Step 6: Migrations
- [ ] Step 7: Database Creation
- [ ] Step 8: CRUD Operations
- [ ] Step 9: LINQ Queries
- [ ] Step 10: Tracking vs No-Tracking
- [ ] Step 11: SaveChanges Internals
- [ ] Step 12: LINQ to SQL Conversion
- [ ] Step 13: IQueryable vs IEnumerable
- [ ] Step 14: Relationships
- [ ] Step 15: Loading Strategies
- [ ] Interview Preparation Complete

---

**Start with:** [01-ORM-Fundamentals/WhatIsORM.md](./01-ORM-Fundamentals/WhatIsORM.md)
