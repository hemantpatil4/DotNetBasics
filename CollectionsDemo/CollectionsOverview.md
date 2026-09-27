# C# Collections Framework – Complete Guide

> **Interview Focus:** Collections hierarchy, when to use what, Big-O complexity  
> **Comparison:** Java Collections Framework vs C# Collections

---

## Java vs C# Collections Mapping

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                    JAVA vs C# COLLECTIONS COMPARISON                          ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║   JAVA                              C# (.NET)                                 ║
║   ────                              ─────────                                 ║
║                                                                               ║
║   List<E>        (interface)   →    IList<T>        (interface)              ║
║   ArrayList<E>                 →    List<T>                                   ║
║   LinkedList<E>                →    LinkedList<T>                             ║
║                                                                               ║
║   Set<E>         (interface)   →    ISet<T>         (interface)              ║
║   HashSet<E>                   →    HashSet<T>                                ║
║   TreeSet<E>                   →    SortedSet<T>                              ║
║   LinkedHashSet<E>             →    (no direct equivalent)                    ║
║                                                                               ║
║   Map<K,V>       (interface)   →    IDictionary<K,V> (interface)             ║
║   HashMap<K,V>                 →    Dictionary<K,V>                           ║
║   TreeMap<K,V>                 →    SortedDictionary<K,V>                     ║
║   LinkedHashMap<K,V>           →    (no direct equivalent)                    ║
║   Hashtable                    →    Hashtable (legacy)                        ║
║                                                                               ║
║   Queue<E>       (interface)   →    Queue<T>                                  ║
║   Deque<E>       (interface)   →    (no direct, use LinkedList<T>)           ║
║   PriorityQueue<E>             →    PriorityQueue<T> (.NET 6+)               ║
║   ArrayDeque<E>                →    (no direct equivalent)                    ║
║                                                                               ║
║   Stack<E>                     →    Stack<T>                                  ║
║                                                                               ║
║   Collections (utility)        →    LINQ (extension methods)                  ║
║   Arrays (utility)             →    Array class + LINQ                        ║
║                                                                               ║
║   CONCURRENT COLLECTIONS                                                      ║
║   ──────────────────────                                                      ║
║   ConcurrentHashMap<K,V>       →    ConcurrentDictionary<K,V>                ║
║   CopyOnWriteArrayList<E>      →    ImmutableList<T>                         ║
║   BlockingQueue<E>             →    BlockingCollection<T>                     ║
║   ConcurrentLinkedQueue<E>     →    ConcurrentQueue<T>                        ║
║   ConcurrentSkipListSet<E>     →    (no direct equivalent)                    ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

---

## C# Collections Hierarchy

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                     C# COLLECTIONS HIERARCHY                                  ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║                           IEnumerable<T>                                      ║
║                                 │                                             ║
║                                 ▼                                             ║
║                           ICollection<T>                                      ║
║                    ┌────────────┼────────────┐                               ║
║                    ▼            ▼            ▼                               ║
║               IList<T>      ISet<T>    IDictionary<K,V>                      ║
║                    │            │            │                               ║
║           ┌───────┴───────┐    │     ┌──────┴──────┐                        ║
║           ▼               ▼    ▼     ▼             ▼                        ║
║       List<T>    LinkedList<T> │  Dictionary   SortedDictionary             ║
║                                │     <K,V>        <K,V>                      ║
║                         ┌──────┴──────┐                                      ║
║                         ▼             ▼                                      ║
║                    HashSet<T>   SortedSet<T>                                 ║
║                                                                               ║
║   OTHER COLLECTIONS (not in hierarchy):                                       ║
║   ─────────────────────────────────────                                       ║
║   Queue<T>  ──►  FIFO (First In, First Out)                                  ║
║   Stack<T>  ──►  LIFO (Last In, First Out)                                   ║
║   PriorityQueue<T,TPriority>  ──►  Priority-based (.NET 6+)                  ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

---

## Namespaces

```csharp
using System.Collections;              // Non-generic (legacy): ArrayList, Hashtable
using System.Collections.Generic;      // Generic: List<T>, Dictionary<K,V>, etc.
using System.Collections.Concurrent;   // Thread-safe: ConcurrentDictionary, etc.
using System.Collections.Immutable;    // Immutable: ImmutableList, etc. (NuGet)
using System.Collections.ObjectModel;  // ReadOnlyCollection, ObservableCollection
using System.Linq;                     // LINQ extension methods
```

---

## 🔥 IEnumerable vs IEnumerator vs IQueryable (Interview Favorite!)

> **This is one of the most asked interview questions in C#/.NET interviews!**

### The Iterator Pattern - Foundation of All Collections

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                    IEnumerable / IEnumerator RELATIONSHIP                     ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║   IEnumerable<T>                    IEnumerator<T>                            ║
║   ──────────────                    ────────────────                          ║
║   "I can be iterated"               "I know how to iterate"                   ║
║                                                                               ║
║   ┌─────────────────────┐           ┌─────────────────────────┐              ║
║   │ IEnumerable<T>      │           │ IEnumerator<T>          │              ║
║   ├─────────────────────┤           ├─────────────────────────┤              ║
║   │ GetEnumerator()     │──────────►│ Current { get; }        │              ║
║   │   returns           │           │ MoveNext() : bool       │              ║
║   │   IEnumerator<T>    │           │ Reset()                 │              ║
║   └─────────────────────┘           │ Dispose()               │              ║
║                                     └─────────────────────────┘              ║
║                                                                               ║
║   ANALOGY:                                                                    ║
║   IEnumerable = A Book (can be read)                                          ║
║   IEnumerator = A Bookmark (tracks where you are, moves page by page)         ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

### How `foreach` Works Internally

```csharp
// This foreach loop:
foreach (var item in collection)
{
    Console.WriteLine(item);
}

// Is compiled to this by the compiler:
IEnumerator<T> enumerator = collection.GetEnumerator();
try
{
    while (enumerator.MoveNext())
    {
        T item = enumerator.Current;
        Console.WriteLine(item);
    }
}
finally
{
    enumerator.Dispose();
}
```

### IEnumerable<T> Interface

```csharp
// Definition
public interface IEnumerable<T> : IEnumerable
{
    IEnumerator<T> GetEnumerator();
}

// Usage - ALL collections implement this!
IEnumerable<int> numbers = new List<int> { 1, 2, 3 };
IEnumerable<int> array = new int[] { 1, 2, 3 };
IEnumerable<int> hashSet = new HashSet<int> { 1, 2, 3 };

// Common usage: method return type for flexibility
public IEnumerable<Trade> GetTrades()
{
    return _tradeList;  // Can return List<T>, array, or any collection
}
```

### IEnumerator<T> Interface

```csharp
// Definition
public interface IEnumerator<T> : IDisposable, IEnumerator
{
    T Current { get; }      // Gets current element
    bool MoveNext();        // Moves to next, returns false when done
    void Reset();           // Rarely used - resets to beginning
}

// Custom implementation example
public class TradeEnumerator : IEnumerator<Trade>
{
    private Trade[] _trades;
    private int _position = -1;  // Start BEFORE first element!

    public TradeEnumerator(Trade[] trades) => _trades = trades;

    public Trade Current => _trades[_position];

    object IEnumerator.Current => Current;

    public bool MoveNext()
    {
        _position++;
        return _position < _trades.Length;
    }

    public void Reset() => _position = -1;

    public void Dispose() { }
}
```

### 🔥 IEnumerable<T> vs IQueryable<T> (Critical Interview Question!)

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                  IEnumerable<T> vs IQueryable<T>                              ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║   Feature            IEnumerable<T>           IQueryable<T>                   ║
║   ───────────────    ─────────────────────    ─────────────────────────────   ║
║   Namespace          System.Collections       System.Linq                     ║
║                      .Generic                                                 ║
║                                                                               ║
║   Execution          In-memory (client)       Translated to SQL (server)      ║
║   Location                                                                    ║
║                                                                               ║
║   When Executed      When you iterate         When you iterate (deferred)     ║
║                      (deferred execution)                                     ║
║                                                                               ║
║   Expression Type    Func<T, bool>            Expression<Func<T, bool>>       ║
║                      (compiled delegate)      (expression tree - parseable)   ║
║                                                                               ║
║   Best For           In-memory collections    Database queries (EF, LINQ2SQL) ║
║                      List<T>, arrays          IQueryable from DbContext       ║
║                                                                               ║
║   Performance        Loads ALL data first,    Filters on DATABASE server      ║
║   with Filters       then filters in memory   Returns only matching rows      ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

### Critical Example: IEnumerable vs IQueryable Performance

```csharp
// ❌ BAD: Using IEnumerable - loads ALL trades, then filters in memory
public IEnumerable<Trade> GetLargeTrades_Bad(DbContext db)
{
    IEnumerable<Trade> trades = db.Trades;  // Converts to IEnumerable
    return trades.Where(t => t.Amount > 1_000_000);
    // SQL: SELECT * FROM Trades  (ALL rows loaded!)
    // Then filters in C# memory - SLOW for large tables!
}

// ✅ GOOD: Using IQueryable - filters on database server
public IQueryable<Trade> GetLargeTrades_Good(DbContext db)
{
    IQueryable<Trade> trades = db.Trades;  // Stays as IQueryable
    return trades.Where(t => t.Amount > 1_000_000);
    // SQL: SELECT * FROM Trades WHERE Amount > 1000000
    // Database does the filtering - FAST!
}
```

### Expression Trees (Why IQueryable Works)

```csharp
// IEnumerable.Where takes a compiled delegate:
Func<Trade, bool> filter = t => t.Amount > 1_000_000;
// This is already compiled IL code - cannot be parsed

// IQueryable.Where takes an expression tree:
Expression<Func<Trade, bool>> filter = t => t.Amount > 1_000_000;
// This is a DATA STRUCTURE representing the code
// Can be parsed and translated to SQL!

// Expression tree structure:
//         Lambda
//           |
//      GreaterThan
//        /     \
//   Property   Constant
//   (Amount)   (1000000)
```

### When to Use What?

```csharp
// ✅ Use IEnumerable<T> when:
// - Working with in-memory collections (List, Array, HashSet)
// - Data is already loaded
// - Doing complex operations not supported in SQL
IEnumerable<Trade> inMemoryTrades = tradeList.Where(t => t.IsValid());

// ✅ Use IQueryable<T> when:
// - Working with database (Entity Framework)
// - Want query to execute on server
// - Composing queries (adding filters before execution)
IQueryable<Trade> dbTrades = dbContext.Trades
    .Where(t => t.Date == today)
    .Where(t => t.Amount > 100000);  // Builds ONE SQL query

// ✅ Use List<T> when:
// - Need to modify collection
// - Need Count without enumeration
// - Need indexed access
List<Trade> trades = dbContext.Trades.ToList();  // Executes query NOW
```

### Implementing Custom IEnumerable (Interview Question!)

```csharp
// FX Trading example: Custom collection of rates
public class FXRateBook : IEnumerable<FXRate>
{
    private readonly List<FXRate> _rates = new();

    public void AddRate(string pair, decimal bid, decimal ask)
    {
        _rates.Add(new FXRate(pair, bid, ask));
    }

    // Required by IEnumerable<T>
    public IEnumerator<FXRate> GetEnumerator()
    {
        return _rates.GetEnumerator();
    }

    // Required by IEnumerable (non-generic)
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

// Now you can use foreach:
var rateBook = new FXRateBook();
rateBook.AddRate("EUR/USD", 1.0845m, 1.0848m);
rateBook.AddRate("GBP/USD", 1.2645m, 1.2648m);

foreach (var rate in rateBook)
{
    Console.WriteLine($"{rate.Pair}: {rate.Bid}/{rate.Ask}");
}

// And LINQ works too!
var euroRates = rateBook.Where(r => r.Pair.StartsWith("EUR"));
```

### Using `yield return` (Simplifies IEnumerable Implementation)

```csharp
// Instead of implementing IEnumerator manually, use yield return:
public class FXRateBook : IEnumerable<FXRate>
{
    private readonly List<FXRate> _rates = new();

    public IEnumerator<FXRate> GetEnumerator()
    {
        foreach (var rate in _rates)
        {
            yield return rate;  // Compiler generates IEnumerator for you!
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

// Or create generator methods:
public static IEnumerable<int> GetEvenNumbers(int max)
{
    for (int i = 0; i <= max; i += 2)
    {
        yield return i;  // Lazy evaluation - generates on demand!
    }
}

// Usage - doesn't generate all numbers until iterated:
foreach (int n in GetEvenNumbers(1000000))
{
    if (n > 10) break;  // Only generated numbers 0, 2, 4, 6, 8, 10!
}
```

### Key Interview Points Summary

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                    KEY INTERVIEW ANSWERS                                      ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║ Q: What's the difference between IEnumerable and IEnumerator?                 ║
║ A: IEnumerable is the collection that CAN be iterated (has GetEnumerator)     ║
║    IEnumerator is the CURSOR that knows HOW to iterate (Current, MoveNext)   ║
║                                                                               ║
║ Q: What's the difference between IEnumerable and IQueryable?                  ║
║ A: IEnumerable: Executes in MEMORY, uses Func<T,bool> delegates               ║
║    IQueryable: Translates to SQL, uses Expression<Func<T,bool>> trees         ║
║    Use IQueryable for database queries to filter on server!                   ║
║                                                                               ║
║ Q: What is deferred execution?                                                ║
║ A: LINQ queries don't execute until you iterate (foreach) or call            ║
║    ToList(), ToArray(), Count(), First(), etc.                               ║
║                                                                               ║
║ Q: How does foreach work internally?                                          ║
║ A: Calls GetEnumerator(), then loops: while(MoveNext()) { use Current }      ║
║                                                                               ║
║ Q: What is yield return?                                                      ║
║ A: Compiler magic that generates IEnumerator implementation for you.          ║
║    Enables lazy evaluation - generates values on demand.                      ║
║                                                                               ║
║ Q: Can you modify a collection while iterating with foreach?                  ║
║ A: NO! Throws InvalidOperationException. Use for loop with index or          ║
║    create a copy first with ToList().                                        ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

---

## Quick Reference: When to Use What?

| Need                                    | Use                     | Why                    |
| --------------------------------------- | ----------------------- | ---------------------- |
| Ordered list with index access          | `List<T>`               | O(1) access by index   |
| Frequent insertions/deletions in middle | `LinkedList<T>`         | O(1) insert/delete     |
| Unique elements, fast lookup            | `HashSet<T>`            | O(1) contains check    |
| Unique elements, sorted order           | `SortedSet<T>`          | O(log n) operations    |
| Key-value pairs, fast lookup            | `Dictionary<K,V>`       | O(1) by key            |
| Key-value pairs, sorted by key          | `SortedDictionary<K,V>` | O(log n) operations    |
| FIFO processing                         | `Queue<T>`              | Enqueue/Dequeue        |
| LIFO processing (undo, etc.)            | `Stack<T>`              | Push/Pop               |
| Priority-based processing               | `PriorityQueue<T,P>`    | Min/Max heap           |
| Thread-safe dictionary                  | `ConcurrentDictionary`  | Multi-threaded access  |
| Read-only collection                    | `ReadOnlyCollection<T>` | Prevent modifications  |
| Immutable data                          | `ImmutableList<T>`      | Functional programming |

---

## Big-O Complexity Comparison

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                    TIME COMPLEXITY COMPARISON                                 ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║   Operation          List<T>   LinkedList  HashSet  Dictionary  SortedDict   ║
║   ─────────────────  ────────  ──────────  ───────  ──────────  ──────────   ║
║   Add (end)          O(1)*     O(1)        O(1)*    O(1)*       O(log n)     ║
║   Add (beginning)    O(n)      O(1)        N/A      N/A         O(log n)     ║
║   Add (middle)       O(n)      O(1)†       N/A      N/A         O(log n)     ║
║   Remove (by value)  O(n)      O(n)        O(1)     O(1)        O(log n)     ║
║   Remove (by index)  O(n)      O(n)        N/A      N/A         N/A          ║
║   Access (by index)  O(1)      O(n)        N/A      N/A         N/A          ║
║   Access (by key)    N/A       N/A         N/A      O(1)        O(log n)     ║
║   Contains           O(n)      O(n)        O(1)     O(1)‡       O(log n)     ║
║   Find (by value)    O(n)      O(n)        O(1)     O(n)        O(n)         ║
║                                                                               ║
║   * Amortized - may need to resize internal array                            ║
║   † O(1) if you have reference to node, O(n) to find node first              ║
║   ‡ ContainsKey is O(1), ContainsValue is O(n)                               ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

---

## Collection Types Overview

### 1. **List<T>** - Dynamic Array

```csharp
List<int> numbers = new List<int> { 1, 2, 3 };
numbers.Add(4);           // Add to end
numbers.Insert(0, 0);     // Insert at index
numbers.Remove(2);        // Remove by value
numbers.RemoveAt(0);      // Remove by index
int val = numbers[0];     // Access by index
```

### 2. **Dictionary<K,V>** - Hash Map

```csharp
Dictionary<string, decimal> rates = new()
{
    ["EUR/USD"] = 1.0850m,
    ["GBP/USD"] = 1.2650m
};
rates.Add("USD/JPY", 149.50m);
rates["EUR/USD"] = 1.0855m;              // Update
bool exists = rates.ContainsKey("EUR/USD");
rates.TryGetValue("EUR/USD", out decimal rate);
```

### 3. **HashSet<T>** - Unique Elements

```csharp
HashSet<string> currencies = new() { "USD", "EUR", "GBP" };
currencies.Add("USD");    // No effect - already exists
bool added = currencies.Add("JPY");  // Returns true
bool contains = currencies.Contains("EUR");  // O(1)
```

### 4. **Queue<T>** - FIFO

```csharp
Queue<string> orders = new();
orders.Enqueue("Order1");
orders.Enqueue("Order2");
string next = orders.Dequeue();  // "Order1"
string peek = orders.Peek();     // "Order2" (doesn't remove)
```

### 5. **Stack<T>** - LIFO

```csharp
Stack<string> undoHistory = new();
undoHistory.Push("Action1");
undoHistory.Push("Action2");
string lastAction = undoHistory.Pop();   // "Action2"
string peekAction = undoHistory.Peek();  // "Action1"
```

---

## Folder Structure for This Guide

```
CollectionsDemo/
├── CollectionsOverview.md          ← You are here
├── ListDemo/
│   ├── ListGuide.md                ← Deep dive into List<T>
│   ├── ListDemo.cs                 ← Runnable examples
│   ├── ListDemo.csproj
│   └── ListPractice.md             ← Practice problems
├── DictionaryDemo/
│   ├── DictionaryGuide.md
│   ├── DictionaryDemo.cs
│   ├── DictionaryDemo.csproj
│   └── DictionaryPractice.md
├── HashSetDemo/
│   ├── HashSetGuide.md
│   ├── HashSetDemo.cs
│   └── HashSetPractice.md
├── QueueStackDemo/
│   ├── QueueStackGuide.md
│   ├── QueueStackDemo.cs
│   └── QueueStackPractice.md
└── LINQDemo/
    ├── LINQGuide.md
    ├── LINQDemo.cs
    └── LINQPractice.md
```

---

## Interview Questions

**Q: What's the difference between Array and List<T>?**

- Array: Fixed size, faster, less memory overhead
- List<T>: Dynamic size, wraps array internally, more features

**Q: When would you use LinkedList<T> over List<T>?**

- LinkedList: Frequent insertions/deletions in middle
- List: Random access by index, cache-friendly for iteration

**Q: How does Dictionary<K,V> work internally?**

- Uses hash table with buckets
- Key's GetHashCode() determines bucket
- Equals() resolves collisions within bucket

**Q: What happens if you modify a collection while iterating?**

- Throws `InvalidOperationException`
- Use `ToList()` to create copy, or iterate backwards for removal

---

_Let's dive into each collection type in detail..._
