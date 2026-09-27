# LINQ Daily Practice - Day 1

> **Date:** Practice Session 1  
> **Focus:** Core LINQ Patterns  
> **Time:** 45-60 minutes

---

## 📋 Instructions

1. Use the `TestData.cs` file for all sample data
2. Write your solutions in `Solutions.cs`
3. Each problem should be solved using LINQ (method syntax preferred)
4. Test your solutions in `Program.cs`

---

## 🎯 Problems

### Problem 1: Basic Filtering

**Task:** Get all employees whose salary is greater than 70,000.

**Expected Output:** List of employee names with salary > 70K

---

### Problem 2: Projection (Select)

**Task:** Get a list of anonymous objects containing only `FullName` (FirstName + LastName) and `Department` for all employees.

**Expected Output:** Collection of { FullName, Department }

---

### Problem 3: Ordering

**Task:** Get all employees ordered by Department (ascending), then by Salary (descending) within each department.

**Expected Output:** Sorted list of employees

---

### Problem 4: Grouping

**Task:** Group employees by Department and return each department name with the count of employees in it.

**Expected Output:** { Department, EmployeeCount }

---

### Problem 5: Aggregation

**Task:** Find the following for the entire employee list:

- Total salary expense
- Average salary
- Highest salary
- Lowest salary

**Expected Output:** Four numeric values

---

### Problem 6: Any / All

**Task:**

- Check if ANY employee earns more than 100,000
- Check if ALL employees in "Engineering" department earn more than 60,000

**Expected Output:** Two boolean values

---

### Problem 7: First / FirstOrDefault / Single

**Task:**

- Get the FIRST employee in "Sales" department
- Get the FIRST employee named "Zara" (handle if not exists)
- Get the SINGLE employee with Email "john.doe@company.com"

**Expected Output:** Employee objects or null

---

### Problem 8: Join

**Task:** Join Employees with Departments table to get a list showing:

- Employee Name
- Department Name
- Department Budget

**Expected Output:** Collection of { EmployeeName, DepartmentName, Budget }

---

### Problem 9: GroupBy with Aggregation

**Task:** For each department, find:

- Department name
- Number of employees
- Total salary
- Average salary
- Highest paid employee name in that department

**Expected Output:** Collection of department summaries

---

### Problem 10: Chained Operations

**Task:** Get the top 3 highest paid employees who are in departments with budget > 150,000, showing their name, salary, and department.

**Expected Output:** Top 3 employees meeting criteria

---

## ✅ Checklist

| #   | Problem               | Pattern                             | Completed |
| --- | --------------------- | ----------------------------------- | --------- |
| 1   | Basic Filtering       | `Where`                             | ⬜        |
| 2   | Projection            | `Select`                            | ⬜        |
| 3   | Ordering              | `OrderBy`, `ThenBy`                 | ⬜        |
| 4   | Grouping              | `GroupBy`                           | ⬜        |
| 5   | Aggregation           | `Sum`, `Average`, `Max`, `Min`      | ⬜        |
| 6   | Quantifiers           | `Any`, `All`                        | ⬜        |
| 7   | Element Access        | `First`, `FirstOrDefault`, `Single` | ⬜        |
| 8   | Join                  | `Join`                              | ⬜        |
| 9   | GroupBy + Aggregation | `GroupBy` + multiple aggregates     | ⬜        |
| 10  | Chained Operations    | Multiple operators combined         | ⬜        |

---

**Good luck! 🚀**
