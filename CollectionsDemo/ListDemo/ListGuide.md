# List<T> Complete Guide – C# Collections

> **Interview Focus:** Internal working, Big-O complexity, common operations  
> **Java Equivalent:** ArrayList<E>

---

## What is List<T>?

`List<T>` is a **dynamic array** that automatically resizes when capacity is exceeded. It's the most commonly used collection in C#.

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                         LIST<T> INTERNAL STRUCTURE                            ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║   List<int> numbers = new List<int>();                                        ║
║                                                                               ║
║   MEMORY LAYOUT:                                                              ║
║   ──────────────                                                              ║
║                                                                               ║
║   List<T> object (HEAP)                Internal Array (HEAP)                  ║
║   ┌─────────────────────┐              ┌───┬───┬───┬───┬───┬───┬───┬───┐     ║
║   │ _items ─────────────┼─────────────►│ 1 │ 2 │ 3 │ 4 │   │   │   │   │     ║
║   │ _size = 4           │              └───┴───┴───┴───┴───┴───┴───┴───┘     ║
║   │ _version = 4        │              Index: 0   1   2   3   4   5   6   7  ║
║   │ Capacity = 8        │                    ◄─── Used ──►◄── Available ──►  ║
║   └─────────────────────┘                                                     ║
║                                                                               ║
║   - _items: Reference to internal array                                       ║
║   - _size: Actual number of elements (Count property)                         ║
║   - _version: Incremented on modification (for enumeration safety)            ║
║   - Capacity: Size of internal array                                          ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

---

## How Capacity Growth Works

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                         CAPACITY GROWTH (DOUBLING)                            ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║   Initial: Capacity = 0 (no array allocated until first Add)                  ║
║                                                                               ║
║   After Add(1):  Capacity = 4    [1][_][_][_]                                ║
║   After Add(2):  Capacity = 4    [1][2][_][_]                                ║
║   After Add(3):  Capacity = 4    [1][2][3][_]                                ║
║   After Add(4):  Capacity = 4    [1][2][3][4]                                ║
║                                                                               ║
║   After Add(5):  ⚠️ RESIZE! New array created, elements copied                ║
║                  Capacity = 8    [1][2][3][4][5][_][_][_]                    ║
║                                                                               ║
║   After Add(6-8): Capacity = 8   [1][2][3][4][5][6][7][8]                    ║
║                                                                               ║
║   After Add(9):  ⚠️ RESIZE!                                                   ║
║                  Capacity = 16   [1][2][3][4][5][6][7][8][9][_]...[_]        ║
║                                                                               ║
║   GROWTH PATTERN: 0 → 4 → 8 → 16 → 32 → 64 → 128 → 256 → ...                 ║
║                                                                               ║
║   WHY DOUBLING?                                                               ║
║   - Amortized O(1) for Add operations                                         ║
║   - Too small growth = frequent resizes = slow                                ║
║   - Too large growth = wasted memory                                          ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

---

## All List<T> Operations with Complexity

### Creation

```csharp
// Empty list (default capacity)
List<int> list1 = new List<int>();

// With initial capacity (avoids resizing)
List<int> list2 = new List<int>(100);  // ✓ Good practice when you know size

// Collection initializer
List<int> list3 = new List<int> { 1, 2, 3, 4, 5 };

// From another collection
List<int> list4 = new List<int>(existingArray);
List<int> list5 = existingList.ToList();

// Using range (.NET 8+)
List<int> list6 = [1, 2, 3, 4, 5];  // Collection expression
```

### Adding Elements

```csharp
List<string> trades = new List<string>();

// Add single element - O(1) amortized
trades.Add("TRD001");

// Add at specific index - O(n)
// Shifts all elements after index
trades.Insert(0, "TRD000");  // Insert at beginning

// Add multiple elements - O(n) where n = items being added
trades.AddRange(new[] { "TRD002", "TRD003", "TRD004" });

// Insert multiple at index - O(n+m)
trades.InsertRange(1, new[] { "TRD001A", "TRD001B" });
```

```
INSERT AT INDEX 0 (Beginning) - O(n):
────────────────────────────────────
Before: [A][B][C][D][_][_]

Step 1: Shift all elements right
        [_][A][B][C][D][_]

Step 2: Insert new element
        [X][A][B][C][D][_]

All n elements must be moved!
```

### Removing Elements

```csharp
List<int> numbers = new List<int> { 1, 2, 3, 2, 4, 5 };

// Remove first occurrence of value - O(n)
numbers.Remove(2);  // Removes first '2', returns true/false

// Remove at index - O(n)
numbers.RemoveAt(0);  // Removes element at index 0

// Remove range - O(n)
numbers.RemoveRange(1, 2);  // Remove 2 elements starting at index 1

// Remove all matching condition - O(n)
numbers.RemoveAll(x => x > 3);  // Returns count of removed items

// Clear all - O(n) for reference types, O(1) for value types
numbers.Clear();
```

### Accessing Elements

```csharp
List<string> pairs = new List<string> { "EUR/USD", "GBP/USD", "USD/JPY" };

// By index - O(1) ✓ FAST
string first = pairs[0];
string last = pairs[pairs.Count - 1];

// Using methods
string firstItem = pairs.First();      // LINQ
string lastItem = pairs.Last();        // LINQ
string secondItem = pairs.ElementAt(1); // LINQ

// Safe access
string item = pairs.ElementAtOrDefault(10);  // Returns null if out of range

// With pattern matching (C# 8+)
if (pairs is [var head, .. var rest])
{
    Console.WriteLine($"First: {head}, Rest count: {rest.Length}");
}
```

### Searching

```csharp
List<int> numbers = new List<int> { 10, 20, 30, 40, 50, 30, 60 };

// Check existence - O(n)
bool exists = numbers.Contains(30);  // true

// Find index - O(n)
int index = numbers.IndexOf(30);      // 2 (first occurrence)
int lastIndex = numbers.LastIndexOf(30);  // 5 (last occurrence)

// Find with condition - O(n)
int found = numbers.Find(x => x > 25);      // 30 (first match)
int foundLast = numbers.FindLast(x => x > 25);  // 60 (last match)
int foundIndex = numbers.FindIndex(x => x > 25);  // 2
List<int> all = numbers.FindAll(x => x > 25);  // [30, 40, 50, 30, 60]

// Check any/all - O(n)
bool anyOver40 = numbers.Any(x => x > 40);   // true
bool allPositive = numbers.All(x => x > 0);  // true

// Binary Search (LIST MUST BE SORTED!) - O(log n)
numbers.Sort();  // Must sort first!
int bsIndex = numbers.BinarySearch(30);  // Returns index or negative
```

### Sorting

```csharp
List<int> numbers = new List<int> { 5, 2, 8, 1, 9 };

// Default sort (ascending) - O(n log n)
numbers.Sort();  // Modifies original list

// Descending
numbers.Sort((a, b) => b.CompareTo(a));

// Custom comparison
List<Trade> trades = GetTrades();
trades.Sort((t1, t2) => t1.Amount.CompareTo(t2.Amount));

// Using Comparison delegate
trades.Sort(Comparison<Trade> comparison);

// LINQ (creates new collection, doesn't modify original)
var sorted = numbers.OrderBy(x => x).ToList();
var sortedDesc = numbers.OrderByDescending(x => x).ToList();

// Sort by multiple criteria
var sortedTrades = trades
    .OrderBy(t => t.CurrencyPair)
    .ThenByDescending(t => t.Amount)
    .ToList();
```

### Transforming

```csharp
List<int> numbers = new List<int> { 1, 2, 3, 4, 5 };

// Reverse - O(n)
numbers.Reverse();  // Modifies original: [5, 4, 3, 2, 1]

// Convert all elements - O(n)
List<string> strings = numbers.ConvertAll(x => x.ToString());

// LINQ Select (preferred)
List<int> doubled = numbers.Select(x => x * 2).ToList();

// Filter
List<int> evens = numbers.Where(x => x % 2 == 0).ToList();

// Get subset
List<int> subset = numbers.GetRange(1, 3);  // 3 elements starting at index 1

// ToArray
int[] array = numbers.ToArray();
```

### Capacity Management

```csharp
List<int> list = new List<int>();

// Check current capacity
int capacity = list.Capacity;

// Pre-allocate capacity (avoid resizing)
list.Capacity = 1000;

// Or use constructor
List<int> optimized = new List<int>(1000);

// Trim excess capacity
list.TrimExcess();  // Reduces Capacity to Count

// EnsureCapacity (.NET 6+)
list.EnsureCapacity(500);  // Ensures at least 500 capacity
```

---

## Common Patterns & Best Practices

### 1. Removing While Iterating

```csharp
// ❌ WRONG - Throws InvalidOperationException
foreach (var item in list)
{
    if (condition) list.Remove(item);  // Modifying during iteration!
}

// ✓ CORRECT - Option 1: Iterate backwards
for (int i = list.Count - 1; i >= 0; i--)
{
    if (condition) list.RemoveAt(i);
}

// ✓ CORRECT - Option 2: Use RemoveAll
list.RemoveAll(item => condition);

// ✓ CORRECT - Option 3: Create new list
list = list.Where(item => !condition).ToList();
```

### 2. Performance Tips

```csharp
// ✓ Pre-size when you know approximate count
List<Trade> trades = new List<Trade>(expectedCount);

// ✓ Use AddRange instead of multiple Add
list.AddRange(items);  // Better than loop with Add

// ✓ Use Contains carefully on large lists
HashSet<int> set = new HashSet<int>(list);  // Convert for O(1) lookups

// ✓ Avoid Insert(0, item) for large lists - use Queue<T> or LinkedList<T>
```

### 3. FX Trading Examples

```csharp
// Rate history tracking
List<(DateTime Time, decimal Rate)> rateHistory = new();

void RecordRate(decimal rate)
{
    rateHistory.Add((DateTime.UtcNow, rate));

    // Keep only last 1000 rates
    if (rateHistory.Count > 1000)
    {
        rateHistory.RemoveAt(0);  // O(n) - consider Queue<T>
    }
}

// Finding best rate
decimal bestBid = rateHistory
    .Where(r => r.Time > DateTime.UtcNow.AddMinutes(-5))
    .Max(r => r.Rate);
```

---

## List<T> vs Other Collections

| Feature          | List<T>      | LinkedList<T> | Array        |
| ---------------- | ------------ | ------------- | ------------ |
| Access by index  | O(1) ✓       | O(n)          | O(1) ✓       |
| Add to end       | O(1)\*       | O(1)          | N/A (fixed)  |
| Add to beginning | O(n)         | O(1) ✓        | N/A          |
| Insert in middle | O(n)         | O(1)†         | N/A          |
| Remove by index  | O(n)         | O(n)          | N/A          |
| Memory           | Contiguous ✓ | Scattered     | Contiguous ✓ |
| Cache friendly   | Yes ✓        | No            | Yes ✓        |

---

## Interview Questions

**Q: What's the default capacity of List<T>?**

- 0 initially, 4 after first Add, then doubles

**Q: How is List<T> different from ArrayList?**

- List<T> is generic (type-safe, no boxing)
- ArrayList stores objects (boxing for value types)

**Q: When would you NOT use List<T>?**

- Frequent insertions at beginning → LinkedList<T> or Queue<T>
- Need unique elements → HashSet<T>
- Need key-based lookup → Dictionary<K,V>
- Fixed size known → Array (less overhead)

**Q: What exception does List<T> throw on invalid index?**

- `ArgumentOutOfRangeException`

---

_See ListDemo.cs for runnable examples and ListPractice.md for coding challenges._
