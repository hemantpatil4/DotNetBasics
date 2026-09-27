using System.Runtime.CompilerServices;

namespace LINQPractice1;


public class Program
{
    public static void Main()
    {
        // Console.WriteLine("=== Problem 1: Basic Filtering ===");
        // Solutions.Problem1_BasicFiltering();

        // Console.WriteLine("\n=== Problem 2: Projection ===");
        // Solutions.Problem2_Projection();

        // Console.WriteLine("\n=== Problem 3: Ordering ===");
        // Solutions.Problem3_Ordering();

        // Console.WriteLine("\n=== Problem 4: Grouping ===");
        // Solutions.Problem4_Grouping();

        Console.WriteLine("\n=== Problem 5: Aggregation ===");
        Solutions.Problem5_Aggregation();

        // Console.WriteLine("\n=== Problem 6: Any / All ===");
        // Solutions.Problem6_AnyAll();

        // Console.WriteLine("\n=== Problem 7: Element Access ===");
        // Solutions.Problem7_ElementAccess();

        // Console.WriteLine("\n=== Problem 8: Join ===");
        // Solutions.Problem8_Join();

        // Console.WriteLine("\n=== Problem 9: GroupBy with Aggregation ===");
        // Solutions.Problem9_GroupByAggregation();

        // Console.WriteLine("\n=== Problem 10: Chained Operations ===");
        // Solutions.Problem10_ChainedOperations();

    }
}





public static class Solutions
{
    // ═══════════════════════════════════════════════════════════════
    // Problem 1: Basic Filtering
    // Get all employees whose salary is greater than 70,000
    // ═══════════════════════════════════════════════════════════════
    public static void Problem1_BasicFiltering()
    {
        var employees = TestData.Employees;
        // TODO: Write your solution here
        System.Console.WriteLine(employees);
        var emp1 = employees.Where(x => x.Salary > 70000).ToList();
        foreach (var item in emp1)
        {
            System.Console.WriteLine(item.FullName);
        }


    }

    // ═══════════════════════════════════════════════════════════════
    // Problem 2: Projection (Select)
    // Get FullName and Department for all employees
    // ═══════════════════════════════════════════════════════════════
    public static void Problem2_Projection()
    {
        var employees = TestData.Employees;
        var departments = TestData.Departments;

        // TODO: Write your solution here

        var res = departments.Join(
                employees,
                x => x.Id,
                y => y.DepartmentId,
                (a, b) => new
                {
                    FullName = b.FullName,
                    DepartmentName = string.IsNullOrEmpty(a.Name) ? "No Department" : a.Name
                }
        );
        System.Console.WriteLine(string.Join("\n", res));

    }

    // ═══════════════════════════════════════════════════════════════
    // Problem 3: Ordering
    // Order by Department (asc), then by Salary (desc)
    // ═══════════════════════════════════════════════════════════════
    public static void Problem3_Ordering()
    {
        var employees = TestData.Employees;

        // TODO: Write your solution here
        var res = employees.OrderBy(x => x.DepartmentId)
        .ThenByDescending(x => x.Salary);

    }

    // ═══════════════════════════════════════════════════════════════
    // Problem 4: Grouping
    // Group employees by Department with count
    // ═══════════════════════════════════════════════════════════════
    public static void Problem4_Grouping()
    {
        var employees = TestData.Employees;
        var departments = TestData.Departments;

        // TODO: Write your solution here
        var res = employees.GroupBy(x => x.DepartmentId)
                            .Select(x =>
                                    new
                                    {
                                        DepartmentName = departments.FirstOrDefault(y => y.Id == x.Key, null).Name,
                                        Count = x.Count()
                                    });
        System.Console.WriteLine(string.Join("\n", res));
    }

    // ═══════════════════════════════════════════════════════════════
    // Problem 5: Aggregation
    // Find Total, Average, Max, Min salary
    // ═══════════════════════════════════════════════════════════════
    public static void Problem5_Aggregation()
    {
        var employees = TestData.Employees;

        // TODO: Write your solution here


    }

    // ═══════════════════════════════════════════════════════════════
    // Problem 6: Any / All
    // Check if ANY earns > 100K, ALL in Engineering earn > 60K
    // ═══════════════════════════════════════════════════════════════
    public static void Problem6_AnyAll()
    {
        var employees = TestData.Employees;
        var departments = TestData.Departments;

        // TODO: Write your solution here

    }

    // ═══════════════════════════════════════════════════════════════
    // Problem 7: First / FirstOrDefault / Single
    // Various element retrieval scenarios
    // ═══════════════════════════════════════════════════════════════
    public static void Problem7_ElementAccess()
    {
        var employees = TestData.Employees;
        var departments = TestData.Departments;

        // TODO: Write your solution here

    }

    // ═══════════════════════════════════════════════════════════════
    // Problem 8: Join
    // Join Employees with Departments
    // ═══════════════════════════════════════════════════════════════
    public static void Problem8_Join()
    {
        var employees = TestData.Employees;
        var departments = TestData.Departments;

        // TODO: Write your solution here

    }

    // ═══════════════════════════════════════════════════════════════
    // Problem 9: GroupBy with Aggregation
    // Department summary with multiple aggregates
    // ═══════════════════════════════════════════════════════════════
    public static void Problem9_GroupByAggregation()
    {
        var employees = TestData.Employees;
        var departments = TestData.Departments;

        // TODO: Write your solution here

    }

    // ═══════════════════════════════════════════════════════════════
    // Problem 10: Chained Operations
    // Top 3 highest paid in departments with budget > 150K
    // ═══════════════════════════════════════════════════════════════
    public static void Problem10_ChainedOperations()
    {
        var employees = TestData.Employees;
        var departments = TestData.Departments;

        // TODO: Write your solution here

    }
}


/// <summary>
/// Test data for LINQ practice problems
/// </summary>
public static class TestData
{
    // ═══════════════════════════════════════════════════════════════
    // EMPLOYEE DATA
    // ═══════════════════════════════════════════════════════════════

    public static List<Employee> Employees => new()
    {
        new Employee { Id = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@company.com", Salary = 75000, DepartmentId = 1, JoinDate = new DateTime(2020, 3, 15) },
        new Employee { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane.smith@company.com", Salary = 85000, DepartmentId = 1, JoinDate = new DateTime(2019, 7, 22) },
        new Employee { Id = 3, FirstName = "Bob", LastName = "Johnson", Email = "bob.johnson@company.com", Salary = 65000, DepartmentId = 2, JoinDate = new DateTime(2021, 1, 10) },
        new Employee { Id = 4, FirstName = "Alice", LastName = "Brown", Email = "alice.brown@company.com", Salary = 92000, DepartmentId = 1, JoinDate = new DateTime(2018, 5, 5) },
        new Employee { Id = 5, FirstName = "Charlie", LastName = "Wilson", Email = "charlie.wilson@company.com", Salary = 55000, DepartmentId = 3, JoinDate = new DateTime(2022, 2, 28) },
        new Employee { Id = 6, FirstName = "Diana", LastName = "Taylor", Email = "diana.taylor@company.com", Salary = 78000, DepartmentId = 2, JoinDate = new DateTime(2020, 9, 12) },
        new Employee { Id = 7, FirstName = "Edward", LastName = "Martinez", Email = "edward.martinez@company.com", Salary = 105000, DepartmentId = 1, JoinDate = new DateTime(2017, 4, 1) },
        new Employee { Id = 8, FirstName = "Fiona", LastName = "Garcia", Email = "fiona.garcia@company.com", Salary = 62000, DepartmentId = 3, JoinDate = new DateTime(2021, 8, 18) },
        new Employee { Id = 9, FirstName = "George", LastName = "Lee", Email = "george.lee@company.com", Salary = 71000, DepartmentId = 4, JoinDate = new DateTime(2019, 11, 25) },
        new Employee { Id = 10, FirstName = "Hannah", LastName = "Clark", Email = "hannah.clark@company.com", Salary = 88000, DepartmentId = 4, JoinDate = new DateTime(2018, 12, 3) },
        new Employee { Id = 11, FirstName = "Ivan", LastName = "Rodriguez", Email = "ivan.rodriguez@company.com", Salary = 59000, DepartmentId = 2, JoinDate = new DateTime(2023, 1, 15) },
        new Employee { Id = 12, FirstName = "Julia", LastName = "Anderson", Email = "julia.anderson@company.com", Salary = 95000, DepartmentId = 4, JoinDate = new DateTime(2016, 6, 20) },
    };

    // ═══════════════════════════════════════════════════════════════
    // DEPARTMENT DATA
    // ═══════════════════════════════════════════════════════════════

    public static List<Department> Departments => new()
    {
        new Department { Id = 1, Name = "Engineering", Budget = 500000, Location = "Building A" },
        new Department { Id = 2, Name = "Human Resources", Budget = 150000, Location = "Building B" },
        new Department { Id = 3, Name = "Sales", Budget = 200000, Location = "Building C" },
        new Department { Id = 4, Name = "Finance", Budget = 300000, Location = "Building A" },
    };

    // ═══════════════════════════════════════════════════════════════
    // PRODUCT DATA (for additional practice)
    // ═══════════════════════════════════════════════════════════════

    public static List<Product> Products => new()
    {
        new Product { Id = 1, Name = "Laptop", Category = "Electronics", Price = 999.99m, Stock = 50, IsActive = true },
        new Product { Id = 2, Name = "Mouse", Category = "Electronics", Price = 29.99m, Stock = 200, IsActive = true },
        new Product { Id = 3, Name = "Keyboard", Category = "Electronics", Price = 79.99m, Stock = 150, IsActive = true },
        new Product { Id = 4, Name = "Desk Chair", Category = "Furniture", Price = 249.99m, Stock = 30, IsActive = true },
        new Product { Id = 5, Name = "Monitor", Category = "Electronics", Price = 399.99m, Stock = 75, IsActive = true },
        new Product { Id = 6, Name = "Desk", Category = "Furniture", Price = 199.99m, Stock = 25, IsActive = false },
        new Product { Id = 7, Name = "Headphones", Category = "Electronics", Price = 149.99m, Stock = 100, IsActive = true },
        new Product { Id = 8, Name = "Webcam", Category = "Electronics", Price = 89.99m, Stock = 60, IsActive = true },
        new Product { Id = 9, Name = "Bookshelf", Category = "Furniture", Price = 129.99m, Stock = 15, IsActive = true },
        new Product { Id = 10, Name = "Lamp", Category = "Furniture", Price = 49.99m, Stock = 80, IsActive = true },
    };

    // ═══════════════════════════════════════════════════════════════
    // ORDER DATA (for join practice)
    // ═══════════════════════════════════════════════════════════════

    public static List<Order> Orders => new()
    {
        new Order { Id = 1, CustomerId = 101, ProductId = 1, Quantity = 2, OrderDate = new DateTime(2024, 1, 15), Status = "Delivered" },
        new Order { Id = 2, CustomerId = 102, ProductId = 2, Quantity = 5, OrderDate = new DateTime(2024, 1, 18), Status = "Delivered" },
        new Order { Id = 3, CustomerId = 101, ProductId = 5, Quantity = 1, OrderDate = new DateTime(2024, 2, 1), Status = "Shipped" },
        new Order { Id = 4, CustomerId = 103, ProductId = 3, Quantity = 3, OrderDate = new DateTime(2024, 2, 10), Status = "Processing" },
        new Order { Id = 5, CustomerId = 104, ProductId = 7, Quantity = 2, OrderDate = new DateTime(2024, 2, 15), Status = "Delivered" },
        new Order { Id = 6, CustomerId = 102, ProductId = 4, Quantity = 1, OrderDate = new DateTime(2024, 2, 20), Status = "Shipped" },
        new Order { Id = 7, CustomerId = 105, ProductId = 1, Quantity = 1, OrderDate = new DateTime(2024, 3, 1), Status = "Processing" },
        new Order { Id = 8, CustomerId = 101, ProductId = 8, Quantity = 2, OrderDate = new DateTime(2024, 3, 5), Status = "Pending" },
    };
}

// ═══════════════════════════════════════════════════════════════════
// MODEL CLASSES
// ═══════════════════════════════════════════════════════════════════

public class Employee
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public int DepartmentId { get; set; }
    public DateTime JoinDate { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public string Location { get; set; } = string.Empty;
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
}


