# Expression Trees in C# – Complete Guide

> **From Basics to Advanced** – Understanding code as data  
> Essential for LINQ providers, ORM internals, and dynamic code generation

---

## Table of Contents

1. [What are Expression Trees?](#1-what-are-expression-trees)
2. [Expression Trees vs Delegates](#2-expression-trees-vs-delegates)
3. [How Expression Trees Work](#3-how-expression-trees-work)
4. [Creating Expression Trees](#4-creating-expression-trees)
5. [Analyzing Expression Trees](#5-analyzing-expression-trees)
6. [Expression Tree in LINQ](#6-expression-tree-in-linq)
7. [Building Dynamic Queries](#7-building-dynamic-queries)
8. [Compiling Expression Trees](#8-compiling-expression-trees)
9. [Real-World Use Cases](#9-real-world-use-cases)
10. [Interview Questions](#10-interview-questions)

---

## 1. What are Expression Trees?

### 1.1 Definition

An **Expression Tree** is a **data structure that represents code as a tree** where each node is an expression (operation, method call, constant, etc.).

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    CODE vs EXPRESSION TREE                              │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   Lambda (Code):          x => x * 2 + 1                                │
│                                                                         │
│   Expression Tree (Data):                                               │
│                                                                         │
│                              [Add]                                      │
│                             /     \                                     │
│                        [Multiply]  [Constant: 1]                        │
│                        /      \                                         │
│               [Parameter: x]  [Constant: 2]                             │
│                                                                         │
│   Key Insight: Expression tree is CODE REPRESENTED AS DATA              │
│                that can be inspected, modified, and executed            │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 1.2 Why Do We Need Expression Trees?

| Use Case               | Description                               |
| ---------------------- | ----------------------------------------- |
| **LINQ to SQL/EF**     | Convert C# code to SQL queries            |
| **Dynamic Queries**    | Build queries at runtime                  |
| **Code Analysis**      | Inspect what code does without running it |
| **Serialization**      | Send code across boundaries               |
| **ORM Implementation** | Map C# expressions to database operations |

### 1.3 The Key Difference

```csharp
// This is EXECUTABLE CODE (compiled to IL)
Func<int, int> func = x => x * 2;

// This is DATA STRUCTURE (tree representation of code)
Expression<Func<int, int>> expr = x => x * 2;
```

**Critical Point:**

- `Func<>` is **compiled** → Can only execute
- `Expression<Func<>>` is **not compiled** → Can inspect, modify, translate

---

## 2. Expression Trees vs Delegates

### 2.1 Comparison

```
┌─────────────────────────────────────────────────────────────────────────┐
│               DELEGATE vs EXPRESSION TREE                               │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   Func<int, bool> isEven = x => x % 2 == 0;                             │
│   ┌──────────────────┐                                                  │
│   │   IL Code        │  ← Compiled, ready to execute                    │
│   │   (Black Box)    │  ← Cannot see inside                             │
│   └──────────────────┘                                                  │
│                                                                         │
│   Expression<Func<int, bool>> isEvenExpr = x => x % 2 == 0;             │
│   ┌──────────────────┐                                                  │
│   │   [Equal]        │  ← Tree structure                                │
│   │   /      \       │  ← Can traverse                                  │
│   │ [Modulo] [0]     │  ← Can translate (e.g., to SQL)                  │
│   │  /    \          │                                                  │
│   │ [x]   [2]        │                                                  │
│   └──────────────────┘                                                  │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 2.2 When to Use What?

| Scenario                          | Use Delegate        | Use Expression Tree  |
| --------------------------------- | ------------------- | -------------------- |
| In-memory filtering               | ✅ `Func<T, bool>`  | ❌ Overkill          |
| Database queries (EF/LINQ to SQL) | ❌ Fetches all data | ✅ Translates to SQL |
| Dynamic query building            | ❌ Limited          | ✅ Full flexibility  |
| Performance-critical loops        | ✅ Faster           | ❌ Slower (overhead) |
| Code analysis/transformation      | ❌ Not possible     | ✅ Full access       |

### 2.3 IEnumerable vs IQueryable

```csharp
// IEnumerable - uses Func<> (delegate)
IEnumerable<Trade> trades = GetTrades();
var filtered = trades.Where(t => t.Amount > 100000);
// ↑ Filtering happens IN MEMORY after fetching ALL data

// IQueryable - uses Expression<Func<>> (expression tree)
IQueryable<Trade> trades = dbContext.Trades;
var filtered = trades.Where(t => t.Amount > 100000);
// ↑ Expression tree is translated to: SELECT * FROM Trades WHERE Amount > 100000
// Only matching rows are fetched from database!
```

---

## 3. How Expression Trees Work

### 3.1 Internal Structure

Every expression tree is built from `Expression` class nodes:

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    EXPRESSION CLASS HIERARCHY                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   Expression (abstract base)                                            │
│   ├── LambdaExpression                                                  │
│   ├── BinaryExpression      (Add, Subtract, Equal, GreaterThan...)      │
│   ├── UnaryExpression       (Negate, Not, Convert...)                   │
│   ├── ConstantExpression    (42, "hello", null...)                      │
│   ├── ParameterExpression   (x, y, item...)                             │
│   ├── MemberExpression      (obj.Property, obj.Field)                   │
│   ├── MethodCallExpression  (obj.Method(), String.IsNullOrEmpty())      │
│   ├── ConditionalExpression (condition ? trueVal : falseVal)            │
│   ├── NewExpression         (new ClassName())                           │
│   └── ... many more                                                     │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 3.2 Expression Tree for: `x => x > 5 && x < 10`

```
                        [AndAlso]
                       /         \
                [GreaterThan]   [LessThan]
                /        \       /       \
         [Parameter:x] [Const:5] [Param:x] [Const:10]
```

### 3.3 Key Properties of Expression

```csharp
Expression expr = /* some expression */;

expr.NodeType    // ExpressionType enum (Add, Subtract, Call, Lambda, etc.)
expr.Type        // System.Type - return type of the expression
expr.CanReduce   // Whether expression can be simplified
```

---

## 4. Creating Expression Trees

### 4.1 Method 1: Compiler Creates (Lambda Assignment)

```csharp
// Compiler automatically builds the tree when you assign to Expression<>
Expression<Func<int, int>> doubleIt = x => x * 2;

// What compiler creates internally:
// ParameterExpression paramX = Expression.Parameter(typeof(int), "x");
// BinaryExpression multiply = Expression.Multiply(paramX, Expression.Constant(2));
// Expression<Func<int, int>> lambda = Expression.Lambda<Func<int, int>>(multiply, paramX);
```

### 4.2 Method 2: Build Manually (Expression Factory Methods)

```csharp
using System.Linq.Expressions;

// Build: x => x * 2

// Step 1: Create parameter
ParameterExpression x = Expression.Parameter(typeof(int), "x");

// Step 2: Create constant
ConstantExpression two = Expression.Constant(2, typeof(int));

// Step 3: Create binary operation (multiply)
BinaryExpression multiply = Expression.Multiply(x, two);

// Step 4: Create lambda
Expression<Func<int, int>> lambda = Expression.Lambda<Func<int, int>>(multiply, x);

// Result: x => x * 2
Console.WriteLine(lambda);  // Outputs: x => (x * 2)
```

### 4.3 Building More Complex Expressions

```csharp
// Build: (x, y) => x > 0 && y > 0

ParameterExpression x = Expression.Parameter(typeof(int), "x");
ParameterExpression y = Expression.Parameter(typeof(int), "y");
ConstantExpression zero = Expression.Constant(0);

BinaryExpression xGreaterThanZero = Expression.GreaterThan(x, zero);
BinaryExpression yGreaterThanZero = Expression.GreaterThan(y, zero);
BinaryExpression andAlso = Expression.AndAlso(xGreaterThanZero, yGreaterThanZero);

var lambda = Expression.Lambda<Func<int, int, bool>>(andAlso, x, y);

Console.WriteLine(lambda);  // (x, y) => ((x > 0) AndAlso (y > 0))
```

### 4.4 Building Property Access

```csharp
// Build: trade => trade.Amount > 100000

ParameterExpression trade = Expression.Parameter(typeof(Trade), "trade");

// Access trade.Amount property
MemberExpression amountProperty = Expression.Property(trade, "Amount");

// Create comparison
ConstantExpression threshold = Expression.Constant(100000m);
BinaryExpression comparison = Expression.GreaterThan(amountProperty, threshold);

// Create lambda
var lambda = Expression.Lambda<Func<Trade, bool>>(comparison, trade);

// Result: trade => (trade.Amount > 100000)
```

### 4.5 Building Method Calls

```csharp
// Build: s => s.ToUpper().Contains("USD")

ParameterExpression s = Expression.Parameter(typeof(string), "s");

// Call s.ToUpper()
MethodCallExpression toUpper = Expression.Call(s, typeof(string).GetMethod("ToUpper", Type.EmptyTypes)!);

// Call .Contains("USD")
MethodInfo containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
ConstantExpression searchTerm = Expression.Constant("USD");
MethodCallExpression contains = Expression.Call(toUpper, containsMethod, searchTerm);

var lambda = Expression.Lambda<Func<string, bool>>(contains, s);

// Result: s => s.ToUpper().Contains("USD")
```

---

## 5. Analyzing Expression Trees

### 5.1 Using ExpressionVisitor

The `ExpressionVisitor` class lets you traverse and optionally modify expression trees:

```csharp
public class ExpressionPrinter : ExpressionVisitor
{
    private int _indent = 0;

    public void Print(Expression expression)
    {
        Visit(expression);
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        PrintIndented($"Binary: {node.NodeType}");
        _indent++;
        Visit(node.Left);
        Visit(node.Right);
        _indent--;
        return node;
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
        PrintIndented($"Constant: {node.Value}");
        return node;
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        PrintIndented($"Parameter: {node.Name} ({node.Type.Name})");
        return node;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        PrintIndented($"Member: {node.Member.Name}");
        _indent++;
        Visit(node.Expression);
        _indent--;
        return node;
    }

    protected override Expression VisitLambda<T>(Expression<T> node)
    {
        PrintIndented($"Lambda: {node.Parameters.Count} params");
        _indent++;
        Visit(node.Body);
        _indent--;
        return node;
    }

    private void PrintIndented(string text)
    {
        Console.WriteLine(new string(' ', _indent * 2) + text);
    }
}

// Usage:
Expression<Func<Trade, bool>> expr = t => t.Amount > 100000 && t.CurrencyPair == "EUR/USD";
var printer = new ExpressionPrinter();
printer.Print(expr);

// Output:
// Lambda: 1 params
//   Binary: AndAlso
//     Binary: GreaterThan
//       Member: Amount
//         Parameter: t (Trade)
//       Constant: 100000
//     Binary: Equal
//       Member: CurrencyPair
//         Parameter: t (Trade)
//       Constant: EUR/USD
```

### 5.2 Extracting Information from Expressions

```csharp
// Get property name from expression (common pattern in EF/frameworks)
public static string GetPropertyName<T, TProperty>(Expression<Func<T, TProperty>> propertyExpression)
{
    if (propertyExpression.Body is MemberExpression member)
    {
        return member.Member.Name;
    }

    if (propertyExpression.Body is UnaryExpression unary &&
        unary.Operand is MemberExpression unaryMember)
    {
        return unaryMember.Member.Name;
    }

    throw new ArgumentException("Expression must be a property access");
}

// Usage:
string propName = GetPropertyName<Trade, decimal>(t => t.Amount);
Console.WriteLine(propName);  // Output: "Amount"

// This pattern is used by:
// - EF Core for .Include(x => x.Orders)
// - FluentValidation for RuleFor(x => x.Name)
// - AutoMapper for ForMember(x => x.Name, ...)
```

---

## 6. Expression Tree in LINQ

### 6.1 IQueryable and Expression Trees

```csharp
public interface IQueryable<T> : IEnumerable<T>, IQueryable
{
    // The expression tree representing this query
    Expression Expression { get; }

    // The provider that will execute the expression
    IQueryProvider Provider { get; }
}
```

### 6.2 How LINQ to SQL Works

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    LINQ TO SQL FLOW                                     │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   C# Code:                                                              │
│   dbContext.Trades.Where(t => t.Amount > 100000).Select(t => t.Pair)    │
│                           │                                             │
│                           ▼                                             │
│   Expression Tree Built:                                                │
│   ┌─────────────────────────────────────────────────────────────┐       │
│   │ Call: Queryable.Select                                      │       │
│   │   └── Call: Queryable.Where                                 │       │
│   │         ├── Constant: DbSet<Trade>                          │       │
│   │         └── Lambda: t => t.Amount > 100000                  │       │
│   │   └── Lambda: t => t.Pair                                   │       │
│   └─────────────────────────────────────────────────────────────┘       │
│                           │                                             │
│                           ▼                                             │
│   IQueryProvider.Execute() is called                                    │
│                           │                                             │
│                           ▼                                             │
│   Provider traverses tree and generates:                                │
│   SELECT Pair FROM Trades WHERE Amount > 100000                         │
│                           │                                             │
│                           ▼                                             │
│   SQL sent to database, results returned                                │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 6.3 Why This Matters

```csharp
// BAD - Using IEnumerable (in-memory filtering)
List<Trade> allTrades = dbContext.Trades.ToList();  // Fetches ALL trades
var filtered = allTrades.Where(t => t.Amount > 100000);  // Filters in C#

// GOOD - Using IQueryable (database filtering)
var filtered = dbContext.Trades.Where(t => t.Amount > 100000);  // Expression tree
// Only when you enumerate (ToList, foreach) does it execute:
// SELECT * FROM Trades WHERE Amount > 100000
```

---

## 7. Building Dynamic Queries

### 7.1 The Problem

```csharp
// What if filter criteria are determined at runtime?
// User selects: "Filter by Amount > X" OR "Filter by Pair = Y"

// You CAN'T do this with static lambdas easily
```

### 7.2 Solution: Build Expression Trees Dynamically

```csharp
public class DynamicQueryBuilder<T>
{
    public static Expression<Func<T, bool>> BuildFilter(
        string propertyName,
        string operation,
        object value)
    {
        // Parameter: t
        ParameterExpression param = Expression.Parameter(typeof(T), "t");

        // Property access: t.PropertyName
        MemberExpression property = Expression.Property(param, propertyName);

        // Constant: value
        ConstantExpression constant = Expression.Constant(value);

        // Build comparison based on operation
        Expression comparison = operation switch
        {
            "==" => Expression.Equal(property, constant),
            "!=" => Expression.NotEqual(property, constant),
            ">"  => Expression.GreaterThan(property, constant),
            ">=" => Expression.GreaterThanOrEqual(property, constant),
            "<"  => Expression.LessThan(property, constant),
            "<=" => Expression.LessThanOrEqual(property, constant),
            _    => throw new ArgumentException($"Unknown operation: {operation}")
        };

        // Create and return lambda
        return Expression.Lambda<Func<T, bool>>(comparison, param);
    }
}

// Usage:
var filter = DynamicQueryBuilder<Trade>.BuildFilter("Amount", ">", 100000m);
var results = dbContext.Trades.Where(filter).ToList();
// Generates: SELECT * FROM Trades WHERE Amount > 100000
```

### 7.3 Combining Multiple Filters (AND/OR)

```csharp
public static class ExpressionExtensions
{
    // Combine two expressions with AND
    public static Expression<Func<T, bool>> And<T>(
        this Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        var parameter = Expression.Parameter(typeof(T), "x");

        var combined = Expression.AndAlso(
            Expression.Invoke(left, parameter),
            Expression.Invoke(right, parameter)
        );

        return Expression.Lambda<Func<T, bool>>(combined, parameter);
    }

    // Combine two expressions with OR
    public static Expression<Func<T, bool>> Or<T>(
        this Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        var parameter = Expression.Parameter(typeof(T), "x");

        var combined = Expression.OrElse(
            Expression.Invoke(left, parameter),
            Expression.Invoke(right, parameter)
        );

        return Expression.Lambda<Func<T, bool>>(combined, parameter);
    }
}

// Usage:
Expression<Func<Trade, bool>> filter1 = t => t.Amount > 100000;
Expression<Func<Trade, bool>> filter2 = t => t.CurrencyPair == "EUR/USD";

var combinedAnd = filter1.And(filter2);  // Amount > 100000 AND Pair == "EUR/USD"
var combinedOr = filter1.Or(filter2);    // Amount > 100000 OR Pair == "EUR/USD"
```

### 7.4 PredicateBuilder Pattern (Popular Library Pattern)

```csharp
public static class PredicateBuilder
{
    // Start with always-true
    public static Expression<Func<T, bool>> True<T>() => x => true;

    // Start with always-false
    public static Expression<Func<T, bool>> False<T>() => x => false;

    // More efficient And that rewrites the expression
    public static Expression<Func<T, bool>> AndAlso<T>(
        this Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        var param = left.Parameters[0];

        // Replace right's parameter with left's parameter
        var rightBody = new ParameterReplacer(right.Parameters[0], param)
            .Visit(right.Body);

        var body = Expression.AndAlso(left.Body, rightBody);
        return Expression.Lambda<Func<T, bool>>(body, param);
    }
}

// Parameter replacer visitor
class ParameterReplacer : ExpressionVisitor
{
    private readonly ParameterExpression _oldParam;
    private readonly ParameterExpression _newParam;

    public ParameterReplacer(ParameterExpression oldParam, ParameterExpression newParam)
    {
        _oldParam = oldParam;
        _newParam = newParam;
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        return node == _oldParam ? _newParam : base.VisitParameter(node);
    }
}

// Usage - Building dynamic search
var predicate = PredicateBuilder.True<Trade>();

if (searchAmount.HasValue)
    predicate = predicate.AndAlso(t => t.Amount > searchAmount.Value);

if (!string.IsNullOrEmpty(searchPair))
    predicate = predicate.AndAlso(t => t.CurrencyPair == searchPair);

var results = dbContext.Trades.Where(predicate).ToList();
```

---

## 8. Compiling Expression Trees

### 8.1 Expression.Compile()

```csharp
// Expression tree (data)
Expression<Func<int, int>> expr = x => x * 2;

// Compile to executable delegate
Func<int, int> compiled = expr.Compile();

// Now you can execute it
int result = compiled(5);  // Returns 10
```

### 8.2 Performance Consideration

```csharp
// Compile() is EXPENSIVE - do it once, reuse many times

// BAD - Compiling in a loop
for (int i = 0; i < 1000000; i++)
{
    Expression<Func<int, int>> expr = x => x * 2;
    var func = expr.Compile();  // Expensive! Don't do this in loops
    var result = func(i);
}

// GOOD - Compile once, reuse
Expression<Func<int, int>> expr = x => x * 2;
Func<int, int> func = expr.Compile();  // Compile once

for (int i = 0; i < 1000000; i++)
{
    var result = func(i);  // Just invoke, no compilation
}
```

### 8.3 When to Compile

| Scenario                    | Compile?                    |
| --------------------------- | --------------------------- |
| Passing to EF/LINQ provider | ❌ No - provider handles it |
| Need to execute in memory   | ✅ Yes - compile first      |
| Caching dynamic expressions | ✅ Yes - compile and cache  |
| Building for analysis only  | ❌ No - just inspect        |

---

## 9. Real-World Use Cases

### 9.1 Entity Framework Core

```csharp
// EF Core uses expression trees for everything!

// Where clause - expression tree translated to SQL WHERE
dbContext.Trades.Where(t => t.Amount > 100000);

// Select projection - expression tree translated to SQL SELECT
dbContext.Trades.Select(t => new { t.Pair, t.Amount });

// Include - expression tree to determine relationships
dbContext.Trades.Include(t => t.Counterparty);

// OrderBy - expression tree for ORDER BY clause
dbContext.Trades.OrderBy(t => t.TradeDate);
```

### 9.2 AutoMapper

```csharp
// AutoMapper uses expressions for member mapping
CreateMap<TradeDto, Trade>()
    .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.TradeAmount));
    //         ↑ Expression tree used to get property name
```

### 9.3 FluentValidation

```csharp
// FluentValidation uses expressions for rule definitions
RuleFor(x => x.Amount)
    .GreaterThan(0)
    .WithMessage("Amount must be positive");
// ↑ x => x.Amount is an expression tree
```

### 9.4 Specification Pattern

```csharp
public abstract class Specification<T>
{
    public abstract Expression<Func<T, bool>> ToExpression();

    public bool IsSatisfiedBy(T entity)
    {
        return ToExpression().Compile()(entity);
    }
}

public class LargeTradeSpecification : Specification<Trade>
{
    private readonly decimal _threshold;

    public LargeTradeSpecification(decimal threshold)
    {
        _threshold = threshold;
    }

    public override Expression<Func<Trade, bool>> ToExpression()
    {
        return t => t.Amount > _threshold;
    }
}

// Usage with EF
var spec = new LargeTradeSpecification(100000m);
var largeTrades = dbContext.Trades.Where(spec.ToExpression()).ToList();
```

### 9.5 Building a Simple Query Provider

```csharp
// This is how ORMs work internally (simplified)
public class SqlGenerator : ExpressionVisitor
{
    private StringBuilder _sql = new();

    public string GenerateSql(Expression expression)
    {
        Visit(expression);
        return _sql.ToString();
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        _sql.Append("(");
        Visit(node.Left);

        _sql.Append(node.NodeType switch
        {
            ExpressionType.Equal => " = ",
            ExpressionType.GreaterThan => " > ",
            ExpressionType.LessThan => " < ",
            ExpressionType.AndAlso => " AND ",
            ExpressionType.OrElse => " OR ",
            _ => throw new NotSupportedException()
        });

        Visit(node.Right);
        _sql.Append(")");
        return node;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        _sql.Append(node.Member.Name);
        return node;
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
        if (node.Value is string)
            _sql.Append($"'{node.Value}'");
        else
            _sql.Append(node.Value);
        return node;
    }
}

// Usage:
Expression<Func<Trade, bool>> expr = t => t.Amount > 100000 && t.CurrencyPair == "EUR/USD";
var generator = new SqlGenerator();
string sql = generator.GenerateSql(expr.Body);
// Output: ((Amount > 100000) AND (CurrencyPair = 'EUR/USD'))
```

---

## 10. Interview Questions

### Q1: What is an Expression Tree?

**Answer:** An expression tree is a data structure that represents code as a tree, where each node is an expression (like a method call, operator, property access, etc.). Unlike compiled delegates, expression trees can be inspected, analyzed, and translated at runtime.

### Q2: Difference between `Func<T, bool>` and `Expression<Func<T, bool>>`?

**Answer:**
| Aspect | `Func<T, bool>` | `Expression<Func<T, bool>>` |
|--------|-----------------|----------------------------|
| What it is | Compiled delegate (IL code) | Data structure (tree) |
| Can inspect? | No (black box) | Yes (can traverse) |
| Can translate? | No | Yes (to SQL, etc.) |
| Performance | Faster execution | Overhead for analysis |
| Used by | LINQ to Objects, in-memory | LINQ to SQL, EF Core |

### Q3: Why does EF Core use Expression Trees?

**Answer:** EF Core needs to translate C# queries to SQL. With expression trees, EF can:

1. Traverse the tree to understand what the code does
2. Translate each node to equivalent SQL
3. Build optimized SQL queries
4. Execute only the necessary SQL on the database

Without expression trees, EF would have to execute everything in memory after fetching all data.

### Q4: What happens when you call `.ToList()` on an `IQueryable`?

**Answer:**

1. The LINQ provider examines the expression tree
2. Translates it to the target language (SQL)
3. Executes the query on the database
4. Materializes results into .NET objects
5. Returns the list

### Q5: How do you get the property name from an expression?

**Answer:**

```csharp
public static string GetPropertyName<T, TProp>(Expression<Func<T, TProp>> expr)
{
    if (expr.Body is MemberExpression member)
        return member.Member.Name;
    throw new ArgumentException("Expression must be property access");
}

// Usage: GetPropertyName<Trade, decimal>(t => t.Amount) returns "Amount"
```

### Q6: Can you modify an expression tree?

**Answer:** Expression trees are **immutable**. You cannot modify an existing tree, but you can:

1. Create a new tree with modifications using `ExpressionVisitor`
2. Build a completely new tree from scratch

### Q7: What is `ExpressionVisitor` used for?

**Answer:** `ExpressionVisitor` is a base class for traversing expression trees. Override its `Visit*` methods to:

- **Analyze** - Extract information from expressions
- **Transform** - Create modified copies of expressions
- **Translate** - Convert expressions to another format (like SQL)

### Q8: Performance implications of Expression Trees?

**Answer:**

- **Building**: Creating expression trees has overhead
- **Compiling**: `Compile()` is expensive (JIT compilation)
- **Execution**: Compiled expressions are fast like regular delegates
- **Best practice**: Build and compile once, cache and reuse

### Q9: What limitations do Expression Trees have?

**Answer:**

- Cannot contain statements (only expressions)
- Cannot contain `await`
- Cannot contain assignment operators
- Cannot contain `try-catch`
- Cannot contain loops directly

### Q10: How would you build a dynamic filter at runtime?

**Answer:**

```csharp
public Expression<Func<T, bool>> BuildFilter<T>(string property, object value)
{
    var param = Expression.Parameter(typeof(T), "x");
    var prop = Expression.Property(param, property);
    var constant = Expression.Constant(value);
    var equal = Expression.Equal(prop, constant);
    return Expression.Lambda<Func<T, bool>>(equal, param);
}
```

---

## Quick Reference

### Common Expression Types

| ExpressionType         | Example           | Description           |
| ---------------------- | ----------------- | --------------------- | --- | ------- |
| `Parameter`            | `x`               | Lambda parameter      |
| `Constant`             | `42`, `"hello"`   | Literal values        |
| `MemberAccess`         | `x.Property`      | Property/field access |
| `Call`                 | `x.Method()`      | Method invocation     |
| `Add`, `Subtract`      | `x + y`           | Arithmetic            |
| `Equal`, `GreaterThan` | `x == y`, `x > y` | Comparison            |
| `AndAlso`, `OrElse`    | `x && y`, `x      |                       | y`  | Logical |
| `Lambda`               | `x => x * 2`      | Lambda expression     |

### Expression Factory Methods

```csharp
Expression.Parameter(typeof(int), "x")           // Parameter
Expression.Constant(42)                          // Constant
Expression.Property(param, "Name")               // Property access
Expression.Call(instance, methodInfo, args)      // Method call
Expression.Add(left, right)                      // Addition
Expression.GreaterThan(left, right)              // Comparison
Expression.AndAlso(left, right)                  // Logical AND
Expression.Lambda<Func<T, R>>(body, parameters)  // Lambda
```

---

_Master expression trees to understand how ORMs work internally!_ 🎯
