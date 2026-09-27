using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCoreDemoApp.Entities;

/// <summary>
/// Employee entity - Many Employees belong to One Department (Dependent/Child)
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
