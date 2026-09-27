# LINQ Complete Guide – C# Collections & Data Processing

> **Interview Focus:** Query vs Method syntax, Deferred Execution, Common Operators  
> **Java Equivalent:** Stream API (Java 8+)

---

## What is LINQ?

LINQ (Language Integrated Query) is a unified query syntax for collections, databases, XML, and more. Think of it as SQL for C# objects.

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                          LINQ - TWO SYNTAXES                                  ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║   QUERY SYNTAX (SQL-like):                                                    ║
║   ─────────────────────────                                                   ║
║   var results = from trade in trades                                          ║
║                 where trade.Amount > 100000                                   ║
║                 orderby trade.Timestamp                                       ║
║                 select trade;                                                 ║
║                                                                               ║
║   METHOD SYNTAX (fluent):                                                     ║
║   ────────────────────────                                                    ║
║   var results = trades                                                        ║
║                     .Where(t => t.Amount > 100000)                            ║
║                     .OrderBy(t => t.Timestamp);                               ║
║                                                                               ║
║   BOTH COMPILE TO THE SAME CODE!                                              ║
║   Method syntax is more common in practice.                                   ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

---

## Deferred vs Immediate Execution

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                    DEFERRED EXECUTION - CRITICAL CONCEPT!                     ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║   DEFERRED - Query is NOT executed when defined!                              ║
║   ──────────────────────────────────────────────                              ║
║   var query = trades.Where(t => t.Amount > 100000);  // NOT executed yet!     ║
║   // ... trades list could change here ...                                    ║
║   var results = query.ToList();  // NOW it executes with current data         ║
║                                                                               ║
║   Returns IEnumerable<T> - creates "recipe" for execution                     ║
║                                                                               ║
║   DEFERRED operators:                                                         ║
║   Where, Select, SelectMany, OrderBy, ThenBy, Skip, Take,                     ║
║   GroupBy, Join, Distinct, Union, Intersect, Except, Zip                      ║
║                                                                               ║
║   ═══════════════════════════════════════════════════════════════════════════ ║
║                                                                               ║
║   IMMEDIATE - Query executes RIGHT NOW                                        ║
║   ────────────────────────────────────                                        ║
║   var results = trades.Where(t => t.Amount > 100000).ToList();  // Executes!  ║
║   int count = trades.Count();  // Executes immediately!                       ║
║                                                                               ║
║   Returns concrete type (List<T>, int, bool, T)                               ║
║                                                                               ║
║   IMMEDIATE operators:                                                        ║
║   ToList, ToArray, ToDictionary, ToHashSet, ToLookup                          ║
║   Count, Sum, Average, Min, Max, First, Last, Single                          ║
║   Any, All, Contains, Aggregate                                               ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

---

## LINQ vs Java Streams

| C# LINQ               | Java Stream                         | Description                |
| --------------------- | ----------------------------------- | -------------------------- |
| `Where()`             | `filter()`                          | Filter elements            |
| `Select()`            | `map()`                             | Transform elements         |
| `SelectMany()`        | `flatMap()`                         | Flatten nested collections |
| `OrderBy()`           | `sorted()`                          | Sort ascending             |
| `OrderByDescending()` | `sorted(Comparator.reverseOrder())` | Sort descending            |
| `First()`             | `findFirst().get()`                 | First element              |
| `FirstOrDefault()`    | `findFirst().orElse(null)`          | First or default           |
| `Any()`               | `anyMatch()`                        | Check if any match         |
| `All()`               | `allMatch()`                        | Check if all match         |
| `Count()`             | `count()`                           | Count elements             |
| `ToList()`            | `collect(Collectors.toList())`      | To list                    |
| `GroupBy()`           | `collect(Collectors.groupingBy())`  | Group by key               |
| `Aggregate()`         | `reduce()`                          | Reduce to single value     |
| `Distinct()`          | `distinct()`                        | Remove duplicates          |
| `Skip()`              | `skip()`                            | Skip first n               |
| `Take()`              | `limit()`                           | Take first n               |

---

## Common LINQ Operators

### Filtering

```csharp
var trades = GetTrades();

// Where - filter by condition
var bigTrades = trades.Where(t => t.Amount > 100000);

// OfType - filter by type
var objects = new object[] { 1, "two", 3, "four" };
var strings = objects.OfType<string>();  // ["two", "four"]

// Distinct - remove duplicates
var uniquePairs = trades.Select(t => t.Pair).Distinct();

// Take - first n elements
var firstFive = trades.Take(5);

// Skip - skip first n elements
var afterFive = trades.Skip(5);

// TakeLast / SkipLast (NET Core 2.0+)
var lastThree = trades.TakeLast(3);

// TakeWhile / SkipWhile
var earlyTrades = trades.TakeWhile(t => t.Timestamp.Hour < 12);
```

### Projection (Transformation)

```csharp
// Select - transform each element
var amounts = trades.Select(t => t.Amount);

// Select with anonymous type
var summaries = trades.Select(t => new
{
    t.TradeId,
    t.Pair,
    Side = t.IsBuy ? "BUY" : "SELL"
});

// Select with index
var indexed = trades.Select((t, index) => $"{index}: {t.TradeId}");

// SelectMany - flatten nested collections
var allOrders = clients.SelectMany(c => c.Orders);

// SelectMany with result selector
var clientOrders = clients.SelectMany(
    c => c.Orders,
    (client, order) => new { client.Name, order.Amount });
```

### Ordering

```csharp
// OrderBy - sort ascending
var byAmount = trades.OrderBy(t => t.Amount);

// OrderByDescending - sort descending
var byAmountDesc = trades.OrderByDescending(t => t.Amount);

// ThenBy - secondary sort
var sorted = trades
    .OrderBy(t => t.Pair)
    .ThenByDescending(t => t.Amount);

// Reverse
var reversed = trades.Reverse();
```

### Grouping

```csharp
// GroupBy - group by key
var byPair = trades.GroupBy(t => t.Pair);

foreach (var group in byPair)
{
    Console.WriteLine($"{group.Key}: {group.Count()} trades");
}

// GroupBy with element selector
var amountsByPair = trades.GroupBy(
    t => t.Pair,
    t => t.Amount);

// GroupBy with result selector
var summaryByPair = trades.GroupBy(
    t => t.Pair,
    (pair, trades) => new
    {
        Pair = pair,
        TotalAmount = trades.Sum(t => t.Amount),
        TradeCount = trades.Count()
    });

// ToLookup - like GroupBy but immediate execution
var lookup = trades.ToLookup(t => t.Pair);
var eurTrades = lookup["EUR/USD"];
```

### Aggregation

```csharp
// Count
int tradeCount = trades.Count();
int bigTradeCount = trades.Count(t => t.Amount > 100000);

// Sum
decimal totalAmount = trades.Sum(t => t.Amount);

// Average
decimal avgAmount = trades.Average(t => t.Amount);

// Min / Max
decimal minAmount = trades.Min(t => t.Amount);
decimal maxAmount = trades.Max(t => t.Amount);

// MinBy / MaxBy (NET 6+)
var smallestTrade = trades.MinBy(t => t.Amount);
var largestTrade = trades.MaxBy(t => t.Amount);

// Aggregate (reduce)
decimal total = trades.Aggregate(0m, (sum, t) => sum + t.Amount);

// Aggregate with result selector
var report = trades.Aggregate(
    (Total: 0m, Count: 0),
    (acc, t) => (acc.Total + t.Amount, acc.Count + 1),
    acc => $"Total: {acc.Total}, Count: {acc.Count}");
```

### Element Operations

```csharp
// First / FirstOrDefault
var first = trades.First();
var firstBig = trades.FirstOrDefault(t => t.Amount > 1000000);

// Last / LastOrDefault
var last = trades.Last();
var lastEur = trades.LastOrDefault(t => t.Pair == "EUR/USD");

// Single / SingleOrDefault (expects exactly one match)
var unique = trades.Single(t => t.TradeId == "T001");

// ElementAt / ElementAtOrDefault
var third = trades.ElementAt(2);

// DefaultIfEmpty - return default if empty
var result = emptyTrades.DefaultIfEmpty(new Trade("DEFAULT", "EUR/USD", 0));
```

### Quantifiers

```csharp
// Any - any element satisfies condition?
bool hasEur = trades.Any(t => t.Pair == "EUR/USD");
bool hasAny = trades.Any();  // Not empty?

// All - all elements satisfy condition?
bool allBig = trades.All(t => t.Amount > 10000);

// Contains
bool hasTrade = trades.Contains(specificTrade);

// SequenceEqual - are sequences equal?
bool equal = list1.SequenceEqual(list2);
```

### Set Operations

```csharp
// Distinct
var uniquePairs = trades.Select(t => t.Pair).Distinct();

// DistinctBy (NET 6+)
var uniqueByPair = trades.DistinctBy(t => t.Pair);

// Union - combine and dedupe
var allPairs = eurPairs.Union(gbpPairs);

// Intersect - common elements
var commonPairs = desk1Pairs.Intersect(desk2Pairs);

// Except - in first but not in second
var desk1Only = desk1Pairs.Except(desk2Pairs);

// UnionBy, IntersectBy, ExceptBy (NET 6+)
var unionByPair = trades1.UnionBy(trades2, t => t.TradeId);
```

### Joining

```csharp
// Join - inner join
var joined = trades.Join(
    rates,
    trade => trade.Pair,
    rate => rate.Pair,
    (trade, rate) => new { trade.TradeId, rate.Value });

// GroupJoin - left join with grouped results
var grouped = clients.GroupJoin(
    orders,
    client => client.Id,
    order => order.ClientId,
    (client, clientOrders) => new
    {
        client.Name,
        OrderCount = clientOrders.Count()
    });

// Zip - pair elements from two sequences
var zipped = names.Zip(ages, (name, age) => $"{name}: {age}");

// Zip (NET 6+) - with tuples
var tuples = names.Zip(ages);  // Returns IEnumerable<(string, int)>
```

### Conversion

```csharp
// ToList / ToArray
List<Trade> list = trades.ToList();
Trade[] array = trades.ToArray();

// ToHashSet
HashSet<string> pairs = trades.Select(t => t.Pair).ToHashSet();

// ToDictionary
Dictionary<string, Trade> dict = trades.ToDictionary(t => t.TradeId);

// ToDictionary with value selector
var amountsByPair = trades.ToDictionary(t => t.TradeId, t => t.Amount);

// ToLookup (one key → many values)
var tradesByPair = trades.ToLookup(t => t.Pair);

// Cast<T> - cast all elements
var ints = objects.Cast<int>();

// AsEnumerable - convert to IEnumerable
var enumerable = trades.AsEnumerable();
```

---

## Common Patterns

### 1. Filter + Transform + Collect

```csharp
var results = trades
    .Where(t => t.Pair == "EUR/USD" && t.Amount > 100000)
    .Select(t => new { t.TradeId, t.Amount, t.Timestamp })
    .OrderByDescending(t => t.Amount)
    .ToList();
```

### 2. Group + Aggregate

```csharp
var summary = trades
    .GroupBy(t => t.Pair)
    .Select(g => new
    {
        Pair = g.Key,
        TotalAmount = g.Sum(t => t.Amount),
        AverageAmount = g.Average(t => t.Amount),
        TradeCount = g.Count()
    })
    .OrderByDescending(s => s.TotalAmount)
    .ToList();
```

### 3. Conditional Default

```csharp
// Safe first
var trade = trades
    .Where(t => t.Amount > 1000000)
    .FirstOrDefault();

if (trade != null)
{
    ProcessTrade(trade);
}

// Or with null-conditional
trades.FirstOrDefault(t => t.Amount > 1000000)?.Process();
```

### 4. Chunking

```csharp
// Chunk into batches (NET 6+)
var batches = trades.Chunk(100);

foreach (var batch in batches)
{
    ProcessBatch(batch);  // batch is Trade[]
}

// Pre-NET 6 alternative
var batches = trades
    .Select((trade, index) => new { trade, index })
    .GroupBy(x => x.index / 100)
    .Select(g => g.Select(x => x.trade).ToList());
```

### 5. Flatten Nested Collections

```csharp
// Get all orders from all clients
var allOrders = clients.SelectMany(c => c.Orders);

// With parent reference
var orderDetails = clients.SelectMany(
    c => c.Orders,
    (client, order) => new
    {
        ClientName = client.Name,
        OrderId = order.Id,
        order.Amount
    });
```

---

## Performance Tips

```csharp
// ❌ Multiple enumerations - bad!
var trades = GetTrades();
var count = trades.Count();          // First enumeration
var total = trades.Sum(t => t.Amount); // Second enumeration!

// ✓ Single enumeration
var list = GetTrades().ToList();  // Materialize once
var count = list.Count;
var total = list.Sum(t => t.Amount);

// ✓ Or use Aggregate for multiple calculations
var result = trades.Aggregate(
    (Count: 0, Total: 0m),
    (acc, t) => (acc.Count + 1, acc.Total + t.Amount));

// ❌ Using Count() > 0 when you only need to check existence
if (trades.Count() > 0) { }  // Iterates entire collection

// ✓ Use Any() instead
if (trades.Any()) { }  // Stops at first element

// ❌ First() when you should use FirstOrDefault()
var trade = trades.First(t => t.Pair == "XYZ");  // Throws if not found!

// ✓ Use FirstOrDefault for safety
var trade = trades.FirstOrDefault(t => t.Pair == "XYZ");
```

---

## Interview Questions

**Q: What is deferred execution in LINQ?**

- Query is not executed when defined
- Executes only when results are enumerated (foreach, ToList, etc.)
- Allows composition without performance penalty

**Q: Difference between First() and Single()?**

- `First()`: Returns first element (throws if empty)
- `Single()`: Returns only element (throws if 0 or 2+ elements)
- Use Single when expecting exactly one match

**Q: Difference between Select() and SelectMany()?**

- `Select()`: 1-to-1 transformation
- `SelectMany()`: 1-to-many transformation (flattens nested collections)

**Q: What operators force immediate execution?**

- `ToList()`, `ToArray()`, `ToDictionary()`, `ToHashSet()`
- `Count()`, `Sum()`, `Min()`, `Max()`, `Average()`
- `First()`, `Last()`, `Single()`, `Any()`, `All()`

**Q: How to avoid multiple enumeration?**

- Materialize with `ToList()` or `ToArray()`
- Use `Aggregate()` for multiple calculations
- Store in variable before multiple operations

---

_See LINQDemo.cs for runnable examples and LINQPractice.md for coding challenges._
