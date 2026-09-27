using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCoreDemoApp.Entities;

/// <summary>
/// Department entity - One Department has Many Employees (Principal/Parent)
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
