# HashSet<T> Complete Guide – C# Collections

> **Interview Focus:** Hash Table internals, Set operations, Big-O  
> **Java Equivalent:** HashSet<E>

---

## What is HashSet<T>?

`HashSet<T>` is an **unordered collection of unique elements** implemented using a hash table. It provides O(1) average time for add, remove, and contains operations.

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                      HASHSET<T> INTERNAL STRUCTURE                            ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║   HashSet<string> activePairs = new HashSet<string>                           ║
║   {                                                                           ║
║       "EUR/USD", "GBP/USD", "USD/JPY"                                         ║
║   };                                                                          ║
║                                                                               ║
║   HOW IT WORKS (same as Dictionary but no values):                            ║
║   ───────────────────────────────────────────────                             ║
║                                                                               ║
║   Step 1: hashCode = "EUR/USD".GetHashCode() → 123456789                      ║
║   Step 2: bucketIndex = hashCode % bucketCount → 5                            ║
║   Step 3: Store in bucket                                                     ║
║                                                                               ║
║   BUCKET ARRAY:                                                               ║
║   ┌───────────────────────────────────────────────────────────────┐          ║
║   │ Bucket │ Entry                                                │          ║
║   ├────────┼──────────────────────────────────────────────────────┤          ║
║   │   0    │ → null                                               │          ║
║   │   1    │ → [GBP/USD] → null                                   │          ║
║   │   2    │ → null                                               │          ║
║   │   3    │ → [USD/JPY] → null                                   │          ║
║   │   4    │ → null                                               │          ║
║   │   5    │ → [EUR/USD] → null                                   │          ║
║   └────────┴──────────────────────────────────────────────────────┘          ║
║                                                                               ║
║   KEY PROPERTY: NO DUPLICATES!                                                ║
║   activePairs.Add("EUR/USD");  // Returns false, already exists               ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

---

## HashSet vs List vs Dictionary

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                    WHEN TO USE HASHSET?                                       ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║   USE HASHSET when:                                                           ║
║   ✓ Need to track unique items only                                          ║
║   ✓ Need fast Contains() check                                               ║
║   ✓ Need set operations (union, intersection)                                ║
║   ✓ Order doesn't matter                                                     ║
║                                                                               ║
║   USE LIST when:                                                              ║
║   ✓ Need to maintain insertion order                                         ║
║   ✓ Need index-based access                                                  ║
║   ✓ Duplicates are allowed                                                   ║
║                                                                               ║
║   USE DICTIONARY when:                                                        ║
║   ✓ Need to map keys to values                                               ║
║   ✓ Need fast lookup by key                                                  ║
║                                                                               ║
║   PERFORMANCE COMPARISON:                                                     ║
║   ┌──────────────┬────────────┬────────────┬─────────────────┐               ║
║   │ Operation    │ HashSet    │ List       │ Dictionary      │               ║
║   ├──────────────┼────────────┼────────────┼─────────────────┤               ║
║   │ Contains     │ O(1)       │ O(n)       │ O(1) by key     │               ║
║   │ Add          │ O(1)       │ O(1)*      │ O(1)            │               ║
║   │ Remove       │ O(1)       │ O(n)       │ O(1)            │               ║
║   │ Get by index │ N/A        │ O(1)       │ N/A             │               ║
║   └──────────────┴────────────┴────────────┴─────────────────┘               ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

---

## All HashSet<T> Operations

### Creation

```csharp
// Empty HashSet
HashSet<string> set1 = new HashSet<string>();

// With initial values
HashSet<string> set2 = new HashSet<string> { "EUR/USD", "GBP/USD", "USD/JPY" };

// From existing collection
List<string> pairs = new List<string> { "EUR/USD", "EUR/USD", "GBP/USD" };
HashSet<string> set3 = new HashSet<string>(pairs);  // Only 2 items (no duplicates)

// With custom comparer (case-insensitive)
HashSet<string> set4 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

// From LINQ (removes duplicates)
var set5 = pairs.ToHashSet();
```

### Adding

```csharp
var set = new HashSet<string>();

// Add - returns bool (true if added, false if existed)
bool added1 = set.Add("EUR/USD");  // true - added
bool added2 = set.Add("EUR/USD");  // false - already exists!

Console.WriteLine($"First add: {added1}, Second add: {added2}");

// Add range - no built-in method, use UnionWith
set.UnionWith(new[] { "GBP/USD", "USD/JPY", "EUR/USD" });  // Only adds 2
```

### Checking Membership

```csharp
var set = new HashSet<string> { "EUR/USD", "GBP/USD", "USD/JPY" };

// Contains - O(1)
bool hasEur = set.Contains("EUR/USD");  // true
bool hasAud = set.Contains("AUD/USD");  // false

// Check count
Console.WriteLine($"Count: {set.Count}");
Console.WriteLine($"Any: {set.Any()}");
```

### Removing

```csharp
var set = new HashSet<string> { "EUR/USD", "GBP/USD", "USD/JPY", "AUD/USD" };

// Remove single item - returns bool
bool removed = set.Remove("EUR/USD");  // true

// Remove items matching condition
set.RemoveWhere(pair => pair.StartsWith("USD"));  // Removes USD/JPY

// Clear all
set.Clear();
```

### Set Operations

```csharp
var majorPairs = new HashSet<string> { "EUR/USD", "GBP/USD", "USD/JPY" };
var crossPairs = new HashSet<string> { "EUR/GBP", "EUR/JPY" };
var allPairs = new HashSet<string> { "EUR/USD", "GBP/USD", "EUR/GBP" };

// UNION - combines all elements from both sets
majorPairs.UnionWith(crossPairs);
// majorPairs = { "EUR/USD", "GBP/USD", "USD/JPY", "EUR/GBP", "EUR/JPY" }

// INTERSECTION - keeps only common elements
majorPairs.IntersectWith(allPairs);
// majorPairs = { "EUR/USD", "GBP/USD", "EUR/GBP" }

// EXCEPT - removes elements that exist in other set
majorPairs.ExceptWith(new[] { "EUR/GBP" });
// majorPairs = { "EUR/USD", "GBP/USD" }

// SYMMETRIC EXCEPT - keeps elements in either set, but not both
var set1 = new HashSet<string> { "A", "B", "C" };
var set2 = new HashSet<string> { "B", "C", "D" };
set1.SymmetricExceptWith(set2);
// set1 = { "A", "D" }
```

### Set Comparisons

```csharp
var setA = new HashSet<string> { "EUR/USD", "GBP/USD" };
var setB = new HashSet<string> { "EUR/USD", "GBP/USD", "USD/JPY" };
var setC = new HashSet<string> { "EUR/USD", "GBP/USD" };

// Subset check
bool isSubset = setA.IsSubsetOf(setB);         // true - all of A in B
bool isProperSubset = setA.IsProperSubsetOf(setB);  // true - A ⊂ B

// Superset check
bool isSuperset = setB.IsSupersetOf(setA);     // true - B contains all of A
bool isProperSuperset = setB.IsProperSupersetOf(setA);  // true

// Set equality
bool equals = setA.SetEquals(setC);            // true - same elements

// Overlap check
bool overlaps = setA.Overlaps(setB);           // true - at least one common element
```

---

## Venn Diagram of Set Operations

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                          SET OPERATIONS VISUALIZED                            ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║   Set A = { 1, 2, 3 }        Set B = { 2, 3, 4 }                              ║
║                                                                               ║
║           ┌───────────────┐     ┌───────────────┐                             ║
║           │       A       │     │       B       │                             ║
║           │    ┌──────────┼─────┼──────────┐    │                             ║
║           │    │   1      │  2  │      4   │    │                             ║
║           │    │          │  3  │          │    │                             ║
║           │    └──────────┼─────┼──────────┘    │                             ║
║           └───────────────┘     └───────────────┘                             ║
║                                                                               ║
║   UNION (A ∪ B):           { 1, 2, 3, 4 }                                     ║
║   UnionWith()              All elements from both                             ║
║                                                                               ║
║   INTERSECTION (A ∩ B):    { 2, 3 }                                           ║
║   IntersectWith()          Only elements in both                              ║
║                                                                               ║
║   EXCEPT (A - B):          { 1 }                                              ║
║   ExceptWith()             Elements in A but not in B                         ║
║                                                                               ║
║   SYMMETRIC EXCEPT:        { 1, 4 }                                           ║
║   SymmetricExceptWith()    Elements in A or B, but not both                   ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

---

## Common Patterns

### 1. Remove Duplicates from List

```csharp
// Method 1: HashSet constructor
List<string> trades = new List<string> { "T1", "T2", "T1", "T3", "T2" };
var uniqueTrades = new HashSet<string>(trades);  // { "T1", "T2", "T3" }

// Method 2: ToHashSet()
var unique = trades.ToHashSet();

// Method 3: LINQ Distinct() (preserves order)
var distinctList = trades.Distinct().ToList();  // ["T1", "T2", "T3"]
```

### 2. Fast Lookup (Contains Check)

```csharp
// ❌ Slow with List - O(n) for each Contains
var validPairs = new List<string> { "EUR/USD", "GBP/USD", "USD/JPY", /* ... 1000 more */ };
foreach (var trade in trades)
{
    if (validPairs.Contains(trade.Pair))  // O(n) each time!
    {
        // Process
    }
}

// ✓ Fast with HashSet - O(1) for each Contains
var validPairsSet = new HashSet<string> { "EUR/USD", "GBP/USD", "USD/JPY" };
foreach (var trade in trades)
{
    if (validPairsSet.Contains(trade.Pair))  // O(1) each time!
    {
        // Process
    }
}
```

### 3. Find Common Elements

```csharp
// Find currency pairs traded by both desks
var desk1Pairs = new HashSet<string> { "EUR/USD", "GBP/USD", "USD/JPY" };
var desk2Pairs = new HashSet<string> { "EUR/USD", "AUD/USD", "USD/JPY" };

var commonPairs = new HashSet<string>(desk1Pairs);
commonPairs.IntersectWith(desk2Pairs);
// commonPairs = { "EUR/USD", "USD/JPY" }
```

### 4. Find Unique Elements

```csharp
// Find pairs traded by desk1 but not desk2
var desk1Only = new HashSet<string>(desk1Pairs);
desk1Only.ExceptWith(desk2Pairs);
// desk1Only = { "GBP/USD" }

// Find pairs traded by either desk but not both
var exclusivePairs = new HashSet<string>(desk1Pairs);
exclusivePairs.SymmetricExceptWith(desk2Pairs);
// exclusivePairs = { "GBP/USD", "AUD/USD" }
```

---

## SortedSet<T> - Ordered Alternative

When you need unique elements **in sorted order**:

```csharp
var sortedPairs = new SortedSet<string> { "USD/JPY", "EUR/USD", "GBP/USD" };

// Automatically sorted!
foreach (var pair in sortedPairs)
{
    Console.WriteLine(pair);  // EUR/USD, GBP/USD, USD/JPY (alphabetical)
}

// Additional operations
var min = sortedPairs.Min;    // "EUR/USD"
var max = sortedPairs.Max;    // "USD/JPY"

// Range view
var range = sortedPairs.GetViewBetween("E", "H");  // EUR/USD, GBP/USD
```

| Operation | HashSet | SortedSet |
| --------- | ------- | --------- |
| Add       | O(1)    | O(log n)  |
| Remove    | O(1)    | O(log n)  |
| Contains  | O(1)    | O(log n)  |
| Min/Max   | O(n)    | O(1)      |
| Ordered   | No      | Yes       |

---

## Custom Objects in HashSet

```csharp
// ❌ Without GetHashCode/Equals - won't work correctly
public class Trade
{
    public string TradeId { get; set; }
    public string Pair { get; set; }
}

var set = new HashSet<Trade>();
set.Add(new Trade { TradeId = "T1", Pair = "EUR/USD" });
set.Add(new Trade { TradeId = "T1", Pair = "EUR/USD" });  // ADDED (duplicate!)

// ✓ With record - works automatically
public record TradeRecord(string TradeId, string Pair);

var recordSet = new HashSet<TradeRecord>();
recordSet.Add(new TradeRecord("T1", "EUR/USD"));
recordSet.Add(new TradeRecord("T1", "EUR/USD"));  // NOT added (duplicate)

// ✓ With IEqualityComparer<T>
var customSet = new HashSet<Trade>(new TradeIdComparer());

public class TradeIdComparer : IEqualityComparer<Trade>
{
    public bool Equals(Trade? x, Trade? y) => x?.TradeId == y?.TradeId;
    public int GetHashCode(Trade obj) => obj.TradeId?.GetHashCode() ?? 0;
}
```

---

## Interview Questions

**Q: How does HashSet ensure uniqueness?**

- Uses hash table internally
- When adding, computes hash code
- If bucket already has items, uses `Equals()` to check for duplicates
- Only adds if no equal item exists

**Q: What's the time complexity of Contains in HashSet?**

- O(1) average case
- O(n) worst case (all items hash to same bucket)

**Q: Difference between HashSet and SortedSet?**

- HashSet: O(1) operations, unordered
- SortedSet: O(log n) operations, ordered (red-black tree)

**Q: How to make custom class work with HashSet?**

- Override `GetHashCode()` and `Equals()`, OR
- Use `record` type (auto-generates equality), OR
- Provide `IEqualityComparer<T>` to constructor

**Q: When would you use HashSet over Dictionary?**

- When you only need to track existence (no values)
- When you need set operations (union, intersection)

---

_See HashSetDemo.cs for runnable examples and HashSetPractice.md for coding challenges._
