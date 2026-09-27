# Step 2: Installing EF Core Packages & Project Setup

> **Duration:** 20 minutes  
> **Goal:** Set up the EFCoreDemoApp project with all required packages

---

## 🎯 What We'll Do

1. Create Console Application
2. Install EF Core packages
3. Understand each package's purpose
4. Configure the project

---

## 📦 Step 2.1: Create the Project

### Terminal Commands

```bash
# Navigate to EFCoreDemo folder
cd /Users/rutujapatil/DotNetInterview/DotNetBasics/EFCoreDemo

# Create console application
dotnet new console -n EFCoreDemoApp

# Navigate into project
cd EFCoreDemoApp
```

---

## 📦 Step 2.2: Install Required Packages

### Core Packages

```bash
# EF Core main package
dotnet add package Microsoft.EntityFrameworkCore

# SQL Server provider
dotnet add package Microsoft.EntityFrameworkCore.SqlServer

# EF Core Tools (for migrations)
dotnet add package Microsoft.EntityFrameworkCore.Tools

# Design package (required for migrations)
dotnet add package Microsoft.EntityFrameworkCore.Design
```

### What Each Package Does

```
┌─────────────────────────────────────────────────────────────────┐
│                    EF Core Package Map                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Microsoft.EntityFrameworkCore                                   │
│  └── Core framework: DbContext, DbSet, Change Tracking          │
│                                                                  │
│  Microsoft.EntityFrameworkCore.SqlServer                         │
│  └── SQL Server database provider                               │
│      └── Translates LINQ → T-SQL                                │
│                                                                  │
│  Microsoft.EntityFrameworkCore.Tools                            │
│  └── PowerShell commands for Package Manager Console            │
│      └── Add-Migration, Update-Database, etc.                   │
│                                                                  │
│  Microsoft.EntityFrameworkCore.Design                           │
│  └── Design-time components                                     │
│      └── Required for dotnet ef commands                        │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📦 Step 2.3: Install EF CLI Tool (Global)

```bash
# Install EF Core CLI globally (one-time setup)
dotnet tool install --global dotnet-ef

# Verify installation
dotnet ef --version
```

### EF CLI vs Package Manager Console

| Method              | When to Use              | Commands                   |
| ------------------- | ------------------------ | -------------------------- |
| **dotnet ef** (CLI) | VS Code, Terminal, CI/CD | `dotnet ef migrations add` |
| **PM Console**      | Visual Studio            | `Add-Migration`            |

We'll use `dotnet ef` since you're in VS Code.

---

## 📁 Step 2.4: Project Structure

Create this folder structure:

```
EFCoreDemoApp/
├── EFCoreDemoApp.csproj
├── Program.cs
├── appsettings.json          ← Connection string
├── Data/
│   └── AppDbContext.cs       ← DbContext
├── Entities/
│   ├── Employee.cs           ← Entity
│   └── Department.cs         ← Entity
└── Migrations/               ← Auto-generated
```

### Create Folders

```bash
# Inside EFCoreDemoApp folder
mkdir Data
mkdir Entities
```

---

## 📄 Step 2.5: Verify .csproj File

Your `EFCoreDemoApp.csproj` should look like:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.*" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.*" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.*">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.*">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>

</Project>
```

---

## 🔍 What Happens Internally

### When You Install Packages

```
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  1. NuGet downloads package from nuget.org                      │
│                              │                                   │
│  2. Adds reference to .csproj                                   │
│                              │                                   │
│  3. Restores dependencies (transitive packages)                 │
│     └── Microsoft.EntityFrameworkCore                           │
│     └── Microsoft.Data.SqlClient                                │
│     └── System.Memory                                           │
│     └── ... many more                                           │
│                              │                                   │
│  4. Assemblies available for your code                          │
└─────────────────────────────────────────────────────────────────┘
```

### Database Providers Available

```
┌─────────────────────────────────────────────────────────────────┐
│                  EF Core Database Providers                      │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  SQL Server  → Microsoft.EntityFrameworkCore.SqlServer          │
│  PostgreSQL  → Npgsql.EntityFrameworkCore.PostgreSQL            │
│  MySQL       → Pomelo.EntityFrameworkCore.MySql                 │
│  SQLite      → Microsoft.EntityFrameworkCore.Sqlite             │
│  In-Memory   → Microsoft.EntityFrameworkCore.InMemory           │
│  Cosmos DB   → Microsoft.EntityFrameworkCore.Cosmos             │
│  Oracle      → Oracle.EntityFrameworkCore                       │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘

Each provider:
├── Translates LINQ to database-specific SQL
├── Handles database-specific types
└── Manages connections using native drivers
```

---

## 📊 Package Dependencies Visualization

```
Your Project
     │
     ├── Microsoft.EntityFrameworkCore.SqlServer (8.0.x)
     │        │
     │        ├── Microsoft.EntityFrameworkCore (8.0.x)
     │        │        │
     │        │        ├── Microsoft.EntityFrameworkCore.Abstractions
     │        │        ├── Microsoft.EntityFrameworkCore.Analyzers
     │        │        └── Microsoft.Extensions.Caching.Memory
     │        │
     │        └── Microsoft.Data.SqlClient (5.x)
     │                 │
     │                 ├── Azure.Identity
     │                 ├── System.Configuration.ConfigurationManager
     │                 └── ... (native SQL Server connectivity)
     │
     └── Microsoft.EntityFrameworkCore.Design (8.0.x)
              │
              └── (Design-time tools and code generation)
```

---

## ❓ Interview Questions

### Q1: What packages are required for EF Core with SQL Server?

**Answer:**

> Minimum required:
>
> 1. `Microsoft.EntityFrameworkCore` - Core framework
> 2. `Microsoft.EntityFrameworkCore.SqlServer` - SQL Server provider
>
> For migrations/tooling: 3. `Microsoft.EntityFrameworkCore.Design` - Design-time services 4. `Microsoft.EntityFrameworkCore.Tools` - CLI/PMC commands

---

### Q2: What is the difference between `Tools` and `Design` packages?

**Answer:**

> - **Tools**: Contains the actual commands (`Add-Migration`, `Update-Database`) for Package Manager Console in Visual Studio
> - **Design**: Contains design-time services that the tools depend on (scaffolding, migration generation)
>
> Both are needed for migrations. The Tools package internally depends on Design.

---

### Q3: Can you use EF Core without SQL Server?

**Answer:**

> Yes! EF Core supports multiple database providers:
>
> - PostgreSQL (Npgsql)
> - MySQL (Pomelo)
> - SQLite
> - In-Memory (for testing)
> - Cosmos DB
>
> Just swap `Microsoft.EntityFrameworkCore.SqlServer` with the appropriate provider package.

---

### Q4: What is the In-Memory provider used for?

**Answer:**

> The In-Memory provider (`Microsoft.EntityFrameworkCore.InMemory`) is used for:
>
> - **Unit testing** without a real database
> - **Prototyping** quick demos
>
> ⚠️ **Warning**: It doesn't enforce relational constraints, so don't use it for integration tests that need to verify database behavior.

---

## ✍️ Exercise: Your Turn!

### Task 1: Create the Project

Run these commands in your terminal:

```bash
# 1. Navigate to EFCoreDemo folder
cd /Users/rutujapatil/DotNetInterview/DotNetBasics/EFCoreDemo

# 2. Create project
dotnet new console -n EFCoreDemoApp

# 3. Navigate into project
cd EFCoreDemoApp

# 4. Install all packages
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.EntityFrameworkCore.Design

# 5. Create folders
mkdir Data
mkdir Entities

# 6. Verify packages installed
dotnet list package

# 7. Install EF CLI (if not already)
dotnet tool install --global dotnet-ef

# 8. Verify EF CLI
dotnet ef --version
```

### Task 2: Verify Setup

After running commands, verify:

- [ ] `EFCoreDemoApp.csproj` exists
- [ ] All 4 packages are listed in csproj
- [ ] `Data/` folder exists
- [ ] `Entities/` folder exists
- [ ] `dotnet ef --version` shows version 8.x

### Task 3: Think About This

Answer these questions:

1. If you wanted to use PostgreSQL instead of SQL Server, which package would you change?
2. Why do we need both `Tools` and `Design` packages?
3. Where are the NuGet packages stored on your machine?

---

## ✅ Checkpoint

After this step, your folder should look like:

```
EFCoreDemo/
├── EFCoreOverview.md
├── 01-ORM-Fundamentals/
│   └── WhatIsORM.md
├── 02-Setup-Configuration/
│   └── PackageInstallation.md    ← You are here
└── EFCoreDemoApp/                ← NEW!
    ├── EFCoreDemoApp.csproj
    ├── Program.cs
    ├── Data/                     ← Empty for now
    └── Entities/                 ← Empty for now
```

---

**Next Step:** [DbContextGuide.md](./DbContextGuide.md) - Creating the DbContext

Let me know when you've completed the setup!
