# Dictionary<K,V> Complete Guide – C# Collections

> **Interview Focus:** Internal working (Hash Table), Collision handling, Big-O  
> **Java Equivalent:** HashMap<K,V>

---

## What is Dictionary<K,V>?

`Dictionary<K,V>` is a **hash table** implementation that stores key-value pairs. It provides O(1) average time complexity for lookup, insert, and delete operations.

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                    DICTIONARY<K,V> INTERNAL STRUCTURE                         ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║   Dictionary<string, decimal> rates = new Dictionary<string, decimal>()       ║
║   {                                                                           ║
║       ["EUR/USD"] = 1.0850m,                                                  ║
║       ["GBP/USD"] = 1.2650m,                                                  ║
║       ["USD/JPY"] = 149.50m                                                   ║
║   };                                                                          ║
║                                                                               ║
║   HOW IT WORKS:                                                               ║
║   ─────────────                                                               ║
║                                                                               ║
║   Step 1: Compute hash code                                                   ║
║           hashCode = "EUR/USD".GetHashCode() → 123456789                      ║
║                                                                               ║
║   Step 2: Compute bucket index                                                ║
║           bucketIndex = hashCode % bucketCount → 5                            ║
║                                                                               ║
║   Step 3: Store in bucket (with collision handling)                           ║
║                                                                               ║
║   BUCKET ARRAY:                                                               ║
║   ┌───────────────────────────────────────────────────────────────┐          ║
║   │ Bucket │ Entry                                                │          ║
║   ├────────┼──────────────────────────────────────────────────────┤          ║
║   │   0    │ → null                                               │          ║
║   │   1    │ → [GBP/USD: 1.2650] → null                           │          ║
║   │   2    │ → null                                               │          ║
║   │   3    │ → [USD/JPY: 149.50] → null                           │          ║
║   │   4    │ → null                                               │          ║
║   │   5    │ → [EUR/USD: 1.0850] → null                           │          ║
║   │   6    │ → null                                               │          ║
║   │   7    │ → null                                               │          ║
║   └────────┴──────────────────────────────────────────────────────┘          ║
║                                                                               ║
║   COLLISION (two keys hash to same bucket):                                   ║
║   │   5    │ → [EUR/USD: 1.0850] → [AUD/NZD: 1.0750] → null       │          ║
║                     ↑                    ↑                                    ║
║                  First entry        Chained entry                             ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

---

## Hash Code and Equals Contract

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                    HASH CODE & EQUALS CONTRACT                                ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║   RULE 1: If a.Equals(b) is true, then a.GetHashCode() == b.GetHashCode()    ║
║           (Equal objects MUST have equal hash codes)                          ║
║                                                                               ║
║   RULE 2: If a.GetHashCode() != b.GetHashCode(), then a.Equals(b) is false   ║
║           (Different hash codes = definitely different objects)               ║
║                                                                               ║
║   RULE 3: If a.GetHashCode() == b.GetHashCode(), a.Equals(b) may be          ║
║           true OR false (hash collision is possible)                          ║
║                                                                               ║
║   WHY THIS MATTERS:                                                           ║
║   ─────────────────                                                           ║
║                                                                               ║
║   dict["EUR/USD"] = 1.08;   // Step 1: Compute hashCode of "EUR/USD"         ║
║                              // Step 2: Find bucket                           ║
║                              // Step 3: Store entry                           ║
║                                                                               ║
║   var rate = dict["EUR/USD"]; // Step 1: Compute hashCode (SAME!)            ║
║                                // Step 2: Find SAME bucket                    ║
║                                // Step 3: Use Equals() to find exact key     ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

---

## All Dictionary<K,V> Operations

### Creation

```csharp
// Empty dictionary
Dictionary<string, decimal> dict1 = new Dictionary<string, decimal>();

// With initial capacity (recommended when size known)
Dictionary<string, decimal> dict2 = new Dictionary<string, decimal>(100);

// Collection initializer
Dictionary<string, decimal> dict3 = new Dictionary<string, decimal>
{
    { "EUR/USD", 1.0850m },
    { "GBP/USD", 1.2650m }
};

// Index initializer (C# 6+)
Dictionary<string, decimal> dict4 = new Dictionary<string, decimal>
{
    ["EUR/USD"] = 1.0850m,
    ["GBP/USD"] = 1.2650m
};

// With custom comparer (case-insensitive keys)
Dictionary<string, decimal> dict5 = new Dictionary<string, decimal>(
    StringComparer.OrdinalIgnoreCase);

// From LINQ
var dict6 = trades.ToDictionary(t => t.TradeId, t => t.Amount);
```

### Adding & Updating

```csharp
var rates = new Dictionary<string, decimal>();

// Add - O(1) average, throws if key exists
rates.Add("EUR/USD", 1.0850m);

// Indexer - O(1), adds OR updates
rates["EUR/USD"] = 1.0855m;  // Updates if exists
rates["GBP/USD"] = 1.2650m;  // Adds if not exists

// TryAdd (.NET Core 2.0+) - O(1), returns bool
bool added = rates.TryAdd("USD/JPY", 149.50m);  // Returns false if exists

// AddRange - not built in, use loop or LINQ
foreach (var kvp in newRates)
{
    rates[kvp.Key] = kvp.Value;
}
```

### Retrieving

```csharp
var rates = new Dictionary<string, decimal>
{
    ["EUR/USD"] = 1.0850m,
    ["GBP/USD"] = 1.2650m
};

// Indexer - O(1), throws KeyNotFoundException if not exists
decimal eurRate = rates["EUR/USD"];

// TryGetValue - O(1), SAFE (recommended!)
if (rates.TryGetValue("USD/JPY", out decimal jpyRate))
{
    Console.WriteLine($"USD/JPY: {jpyRate}");
}
else
{
    Console.WriteLine("USD/JPY not found");
}

// ContainsKey - O(1)
bool hasEur = rates.ContainsKey("EUR/USD");  // true

// ContainsValue - O(n) ⚠️ SLOW!
bool hasValue = rates.ContainsValue(1.0850m);

// GetValueOrDefault (.NET Core 2.0+)
decimal rate = rates.GetValueOrDefault("XYZ/ABC", 0m);  // Returns 0 if not found
```

### Removing

```csharp
// Remove by key - O(1)
bool removed = rates.Remove("EUR/USD");

// Remove with out parameter (.NET Core 2.0+)
if (rates.Remove("GBP/USD", out decimal removedRate))
{
    Console.WriteLine($"Removed rate: {removedRate}");
}

// Clear all
rates.Clear();
```

### Iterating

```csharp
var rates = new Dictionary<string, decimal>
{
    ["EUR/USD"] = 1.0850m,
    ["GBP/USD"] = 1.2650m,
    ["USD/JPY"] = 149.50m
};

// Iterate KeyValuePair - MOST COMMON
foreach (var kvp in rates)
{
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
}

// Iterate with deconstruction (C# 7+)
foreach (var (pair, rate) in rates)
{
    Console.WriteLine($"{pair}: {rate}");
}

// Iterate keys only
foreach (var key in rates.Keys)
{
    Console.WriteLine(key);
}

// Iterate values only
foreach (var value in rates.Values)
{
    Console.WriteLine(value);
}

// ⚠️ Do NOT modify during iteration!
// This throws InvalidOperationException:
// foreach (var kvp in rates)
// {
//     if (condition) rates.Remove(kvp.Key);  // ERROR!
// }

// ✓ Safe way to remove during iteration
var keysToRemove = rates.Where(kvp => kvp.Value < 1m).Select(kvp => kvp.Key).ToList();
foreach (var key in keysToRemove)
{
    rates.Remove(key);
}
```

---

## Custom Key Types

When using custom classes as keys, you MUST override `GetHashCode()` and `Equals()`:

```csharp
public class CurrencyPair
{
    public string Base { get; }
    public string Quote { get; }

    public CurrencyPair(string baseCurrency, string quoteCurrency)
    {
        Base = baseCurrency;
        Quote = quoteCurrency;
    }

    // MUST override Equals
    public override bool Equals(object? obj)
    {
        if (obj is CurrencyPair other)
        {
            return Base == other.Base && Quote == other.Quote;
        }
        return false;
    }

    // MUST override GetHashCode
    public override int GetHashCode()
    {
        return HashCode.Combine(Base, Quote);
    }
}

// Usage:
var ratesByPair = new Dictionary<CurrencyPair, decimal>();
ratesByPair[new CurrencyPair("EUR", "USD")] = 1.0850m;

// This works because GetHashCode and Equals are overridden:
decimal rate = ratesByPair[new CurrencyPair("EUR", "USD")];  // Works!
```

### Using Records (Recommended - C# 9+)

Records automatically implement `GetHashCode()` and `Equals()`:

```csharp
public record CurrencyPair(string Base, string Quote);

var rates = new Dictionary<CurrencyPair, decimal>();
rates[new CurrencyPair("EUR", "USD")] = 1.0850m;

// Works automatically!
decimal rate = rates[new CurrencyPair("EUR", "USD")];
```

---

## Common Patterns

### 1. Counting Occurrences

```csharp
// Count word frequency
List<string> words = new List<string> { "buy", "sell", "buy", "hold", "buy", "sell" };

var frequency = new Dictionary<string, int>();
foreach (var word in words)
{
    if (frequency.ContainsKey(word))
        frequency[word]++;
    else
        frequency[word] = 1;
}

// Cleaner with TryGetValue
foreach (var word in words)
{
    frequency.TryGetValue(word, out int count);
    frequency[word] = count + 1;
}

// LINQ GroupBy
var freq = words.GroupBy(w => w).ToDictionary(g => g.Key, g => g.Count());
```

### 2. Grouping

```csharp
// Group trades by currency pair
List<Trade> trades = GetTrades();

var tradesByPair = new Dictionary<string, List<Trade>>();
foreach (var trade in trades)
{
    if (!tradesByPair.ContainsKey(trade.CurrencyPair))
    {
        tradesByPair[trade.CurrencyPair] = new List<Trade>();
    }
    tradesByPair[trade.CurrencyPair].Add(trade);
}

// LINQ ToLookup (immutable grouping)
var lookup = trades.ToLookup(t => t.CurrencyPair);

// LINQ GroupBy + ToDictionary
var grouped = trades
    .GroupBy(t => t.CurrencyPair)
    .ToDictionary(g => g.Key, g => g.ToList());
```

### 3. Caching / Memoization

```csharp
public class RateCache
{
    private readonly Dictionary<string, (decimal Rate, DateTime Timestamp)> _cache = new();
    private readonly TimeSpan _expiry = TimeSpan.FromMinutes(1);

    public decimal GetRate(string pair, Func<string, decimal> fetchRate)
    {
        if (_cache.TryGetValue(pair, out var cached))
        {
            if (DateTime.UtcNow - cached.Timestamp < _expiry)
            {
                return cached.Rate;  // Cache hit
            }
        }

        // Cache miss - fetch and store
        decimal rate = fetchRate(pair);
        _cache[pair] = (rate, DateTime.UtcNow);
        return rate;
    }
}
```

### 4. Two-Way Lookup

```csharp
public class BiDictionary<T1, T2> where T1 : notnull where T2 : notnull
{
    private readonly Dictionary<T1, T2> _forward = new();
    private readonly Dictionary<T2, T1> _reverse = new();

    public void Add(T1 key, T2 value)
    {
        _forward[key] = value;
        _reverse[value] = key;
    }

    public T2 GetByKey(T1 key) => _forward[key];
    public T1 GetByValue(T2 value) => _reverse[value];
}

// Usage: Currency code ↔ Currency name
var currencies = new BiDictionary<string, string>();
currencies.Add("USD", "US Dollar");
currencies.Add("EUR", "Euro");

string name = currencies.GetByKey("USD");    // "US Dollar"
string code = currencies.GetByValue("Euro"); // "EUR"
```

---

## Dictionary vs Other Collections

| Feature     | Dictionary | SortedDictionary | ConcurrentDictionary |
| ----------- | ---------- | ---------------- | -------------------- |
| Lookup      | O(1)       | O(log n)         | O(1)                 |
| Insert      | O(1)       | O(log n)         | O(1)                 |
| Ordered     | No         | Yes (by key)     | No                   |
| Thread-safe | No         | No               | Yes                  |
| Memory      | Less       | More (tree)      | More                 |

---

## Performance Tips

```csharp
// ✓ Pre-size when count is known
var dict = new Dictionary<string, decimal>(expectedCount);

// ✓ Use TryGetValue instead of ContainsKey + indexer
// ❌ Bad
if (dict.ContainsKey(key))
{
    var value = dict[key];  // Second lookup!
}

// ✓ Good
if (dict.TryGetValue(key, out var value))
{
    // Use value
}

// ✓ Use appropriate key types
// string keys with case-insensitive comparison
var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

// ✓ Avoid ContainsValue - it's O(n)
```

---

## Interview Questions

**Q: How does Dictionary<K,V> work internally?**

- Uses hash table with buckets
- Key's `GetHashCode()` determines bucket
- `Equals()` resolves collisions within bucket

**Q: What happens on hash collision?**

- Multiple entries stored in same bucket as linked list (chaining)
- `Equals()` is used to find the correct entry

**Q: Why is lookup O(1)?**

- Hash code computation is O(1)
- With good distribution, each bucket has few entries
- Average case: O(1), Worst case (all in one bucket): O(n)

**Q: What if key is modified after adding to dictionary?**

- Hash code changes, can't find the entry anymore
- Keys should be immutable (or at least hash-affecting properties)

**Q: Difference between Dictionary and Hashtable?**

- Dictionary is generic (type-safe)
- Hashtable stores objects (boxing for value types)
- Dictionary throws on missing key, Hashtable returns null

---

_See DictionaryDemo.cs for runnable examples and DictionaryPractice.md for coding challenges._
