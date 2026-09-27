# Connection Strings in EF Core

> Understanding connection strings and configuration options

---

## 🔗 Connection String Anatomy

### SQL Server Connection String

```
Server=(localdb)\MSSQLLocalDB;Database=EFCoreDemoDB;Trusted_Connection=True;
  │              │                    │                      │
  │              │                    │                      └── Windows Auth
  │              │                    └── Database name
  │              └── Instance name
  └── Server type
```

### Common Connection String Formats

```csharp
// 1. LocalDB (Development) - Windows only
@"Server=(localdb)\MSSQLLocalDB;Database=EFCoreDemoDB;Trusted_Connection=True;"

// 2. SQL Server with Windows Auth
@"Server=localhost;Database=EFCoreDemoDB;Trusted_Connection=True;TrustServerCertificate=True;"

// 3. SQL Server with SQL Auth
@"Server=localhost;Database=EFCoreDemoDB;User Id=sa;Password=YourPassword123;TrustServerCertificate=True;"

// 4. Docker SQL Server (common for Mac/Linux)
@"Server=localhost,1433;Database=EFCoreDemoDB;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"

// 5. Azure SQL
@"Server=yourserver.database.windows.net;Database=EFCoreDemoDB;User Id=admin;Password=pass;Encrypt=True;"
```

---

## 🍎 For Mac Users (Docker SQL Server)

Since LocalDB doesn't work on Mac, use Docker:

```bash
# Pull SQL Server image
docker pull mcr.microsoft.com/mssql/server:2022-latest

# Run SQL Server container
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong!Passw0rd" \
  -p 1433:1433 --name sql_server \
  -d mcr.microsoft.com/mssql/server:2022-latest

# Verify it's running
docker ps
```

### Connection String for Docker

```csharp
@"Server=localhost,1433;Database=EFCoreDemoDB;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"
```

---

## 📁 Best Practice: Use appsettings.json

### Create `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=EFCoreDemoDB;Trusted_Connection=True;",
    "DockerConnection": "Server=localhost,1433;Database=EFCoreDemoDB;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

### Read Configuration in DbContext

```csharp
using Microsoft.Extensions.Configuration;

public class AppDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        // Read connection string
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        optionsBuilder.UseSqlServer(connectionString);
    }
}
```

### Add NuGet Package for Configuration

```bash
dotnet add package Microsoft.Extensions.Configuration.Json
```

---

## 🔧 Connection String Parameters

| Parameter                  | Purpose                | Example                               |
| -------------------------- | ---------------------- | ------------------------------------- |
| `Server`                   | Database server        | `localhost`, `(localdb)\MSSQLLocalDB` |
| `Database`                 | Database name          | `EFCoreDemoDB`                        |
| `Trusted_Connection`       | Windows authentication | `True`                                |
| `User Id`                  | SQL login username     | `sa`                                  |
| `Password`                 | SQL login password     | `YourPassword`                        |
| `TrustServerCertificate`   | Skip SSL validation    | `True` (dev only)                     |
| `MultipleActiveResultSets` | Allow multiple queries | `True`                                |
| `Connection Timeout`       | Seconds to wait        | `30`                                  |

---

## ⚠️ Security Best Practices

```
╔═══════════════════════════════════════════════════════════════╗
║                 Connection String Security                     ║
╠═══════════════════════════════════════════════════════════════╣
║                                                                ║
║  ❌ NEVER do this:                                             ║
║     - Hardcode passwords in source code                       ║
║     - Commit appsettings.json with passwords to Git           ║
║     - Use 'sa' account in production                          ║
║                                                                ║
║  ✅ DO this:                                                   ║
║     - Use User Secrets for development                        ║
║     - Use Environment Variables in production                 ║
║     - Use Azure Key Vault for cloud apps                      ║
║     - Use Windows Auth when possible                          ║
║                                                                ║
╚═══════════════════════════════════════════════════════════════╝
```

### Using User Secrets (Development)

```bash
# Initialize user secrets
dotnet user-secrets init

# Set connection string
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Password=secret"
```

---

## 📝 Updated AppDbContext

Here's the complete `AppDbContext.cs` that works for both Windows and Mac:

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
        // Choose the right connection string for your environment:

        // Option 1: Windows with LocalDB
        // optionsBuilder.UseSqlServer(
        //     @"Server=(localdb)\MSSQLLocalDB;Database=EFCoreDemoDB;Trusted_Connection=True;"
        // );

        // Option 2: Mac/Linux with Docker SQL Server
        optionsBuilder.UseSqlServer(
            @"Server=localhost,1433;Database=EFCoreDemoDB;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"
        );

        // Enable logging to see generated SQL
        optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
        optionsBuilder.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
```

---

## ✍️ Exercise: Your Turn!

### Task 1: Set Up Your Database

**For Mac (Docker):**

```bash
# Start Docker SQL Server
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong!Passw0rd" \
  -p 1433:1433 --name sql_server \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

**For Windows (LocalDB):**
LocalDB is included with Visual Studio. No setup needed.

### Task 2: Update Your AppDbContext

Use the correct connection string for your OS.

### Task 3: Test Connection (After creating entities)

```csharp
// In Program.cs (we'll do this soon)
using var context = new AppDbContext();
Console.WriteLine($"Can connect: {context.Database.CanConnect()}");
```

---

**Next Step:** [../03-Entities-Migrations/EntityDesign.md](../03-Entities-Migrations/EntityDesign.md) - Creating Entity classes
