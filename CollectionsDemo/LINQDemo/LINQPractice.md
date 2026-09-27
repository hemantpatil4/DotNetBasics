# LINQ Practice Problems

> Interview-focused coding challenges using LINQ  
> Difficulty: ⭐ Easy | ⭐⭐ Medium | ⭐⭐⭐ Hard

---

## Problem 1: Top K Frequent Elements (⭐⭐)

Find the k most frequent elements in an array.

```csharp
// Input: nums = [1,1,1,2,2,3], k = 2
// Output: [1, 2]

public int[] TopKFrequent(int[] nums, int k)
{
    // Solve using LINQ
}
```

<details>
<summary>Solution</summary>

```csharp
public int[] TopKFrequent(int[] nums, int k)
{
    return nums
        .GroupBy(n => n)
        .OrderByDescending(g => g.Count())
        .Take(k)
        .Select(g => g.Key)
        .ToArray();
}
```

**Time Complexity:** O(n log n)  
**Space Complexity:** O(n)

</details>

---

## Problem 2: Find Duplicates (⭐)

Find all elements that appear more than once.

```csharp
// Input: [1, 2, 3, 1, 2, 4, 5, 2]
// Output: [1, 2]

public List<int> FindDuplicates(int[] nums)
{
    // Solve using LINQ
}
```

<details>
<summary>Solution</summary>

```csharp
public List<int> FindDuplicates(int[] nums)
{
    return nums
        .GroupBy(n => n)
        .Where(g => g.Count() > 1)
        .Select(g => g.Key)
        .ToList();
}
```

**Time Complexity:** O(n)  
**Space Complexity:** O(n)

</details>

---

## Problem 3: Intersection of Two Arrays II (⭐⭐)

Find intersection including duplicates (each element appears as many times as it shows in both arrays).

```csharp
// Input: nums1 = [1,2,2,1], nums2 = [2,2]
// Output: [2, 2]

public int[] Intersect(int[] nums1, int[] nums2)
{
    // Solve using LINQ
}
```

<details>
<summary>Solution</summary>

```csharp
public int[] Intersect(int[] nums1, int[] nums2)
{
    var counts = nums1.GroupBy(n => n)
                      .ToDictionary(g => g.Key, g => g.Count());

    return nums2
        .Where(n => counts.TryGetValue(n, out int count) && count > 0)
        .Select(n => { counts[n]--; return n; })
        .ToArray();
}

// Alternative cleaner approach
public int[] Intersect_Alt(int[] nums1, int[] nums2)
{
    var list1 = nums1.ToList();

    return nums2.Where(n => list1.Remove(n)).ToArray();
}
```

**Time Complexity:** O(n + m)  
**Space Complexity:** O(n)

</details>

---

## Problem 4: Group Anagrams (⭐⭐)

Group strings that are anagrams of each other.

```csharp
// Input: ["eat", "tea", "tan", "ate", "nat", "bat"]
// Output: [["eat","tea","ate"], ["tan","nat"], ["bat"]]

public IList<IList<string>> GroupAnagrams(string[] strs)
{
    // Solve using LINQ
}
```

<details>
<summary>Solution</summary>

```csharp
public IList<IList<string>> GroupAnagrams(string[] strs)
{
    return strs
        .GroupBy(s => new string(s.OrderBy(c => c).ToArray()))
        .Select(g => (IList<string>)g.ToList())
        .ToList();
}
```

**Time Complexity:** O(n _ k log k) where k = max string length  
**Space Complexity:** O(n _ k)

</details>

---

## Problem 5: FX Trade Analysis (⭐⭐)

**Domain: FX Trading**

Analyze trading data to find insights.

```csharp
public record Trade(string Id, string Pair, decimal Amount, string Side, DateTime Timestamp);

// Given a list of trades, implement:

// 1. Get total volume by currency pair
public Dictionary<string, decimal> GetVolumeByPair(List<Trade> trades) { }

// 2. Get net position per pair (buys - sells)
public Dictionary<string, decimal> GetNetPositions(List<Trade> trades) { }

// 3. Get largest trade per pair
public Dictionary<string, Trade> GetLargestByPair(List<Trade> trades) { }

// 4. Get hourly trade count
public Dictionary<int, int> GetHourlyTradeCount(List<Trade> trades) { }
```

<details>
<summary>Solution</summary>

```csharp
// 1. Total volume by pair
public Dictionary<string, decimal> GetVolumeByPair(List<Trade> trades)
{
    return trades
        .GroupBy(t => t.Pair)
        .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));
}

// 2. Net position per pair
public Dictionary<string, decimal> GetNetPositions(List<Trade> trades)
{
    return trades
        .GroupBy(t => t.Pair)
        .ToDictionary(
            g => g.Key,
            g => g.Sum(t => t.Side == "BUY" ? t.Amount : -t.Amount));
}

// 3. Largest trade per pair
public Dictionary<string, Trade> GetLargestByPair(List<Trade> trades)
{
    return trades
        .GroupBy(t => t.Pair)
        .ToDictionary(
            g => g.Key,
            g => g.MaxBy(t => t.Amount)!);
}

// 4. Hourly trade count
public Dictionary<int, int> GetHourlyTradeCount(List<Trade> trades)
{
    return trades
        .GroupBy(t => t.Timestamp.Hour)
        .ToDictionary(g => g.Key, g => g.Count());
}
```

</details>

---

## Problem 6: Flatten Nested List (⭐⭐)

Flatten a nested list structure.

```csharp
// Input: [[1, 2], [3, 4, 5], [6]]
// Output: [1, 2, 3, 4, 5, 6]

public List<int> Flatten(List<List<int>> nested)
{
    // Solve using LINQ
}

// Bonus: Flatten with depth tracking
// Input: [[1, 2], [3, 4], [5]]
// Output: [(0,1), (0,2), (1,3), (1,4), (2,5)] - (listIndex, value)

public List<(int Index, int Value)> FlattenWithIndex(List<List<int>> nested)
{
    // Solve using LINQ
}
```

<details>
<summary>Solution</summary>

```csharp
public List<int> Flatten(List<List<int>> nested)
{
    return nested.SelectMany(list => list).ToList();
}

public List<(int Index, int Value)> FlattenWithIndex(List<List<int>> nested)
{
    return nested
        .SelectMany((list, listIndex) =>
            list.Select(value => (listIndex, value)))
        .ToList();
}
```

**Time Complexity:** O(n) total elements  
**Space Complexity:** O(n)

</details>

---

## Problem 7: Most Common Word (⭐⭐)

Find the most frequent word that isn't in a banned list.

```csharp
// Input: paragraph = "Bob hit a ball, the hit BALL flew far after it was hit."
//        banned = ["hit"]
// Output: "ball" (appears 2 times, "hit" is banned)

public string MostCommonWord(string paragraph, string[] banned)
{
    // Solve using LINQ
}
```

<details>
<summary>Solution</summary>

```csharp
public string MostCommonWord(string paragraph, string[] banned)
{
    var bannedSet = banned.ToHashSet(StringComparer.OrdinalIgnoreCase);

    return paragraph
        .ToLower()
        .Split(new[] { ' ', ',', '.', '!', '?', ';', '\'' },
               StringSplitOptions.RemoveEmptyEntries)
        .Where(word => !bannedSet.Contains(word))
        .GroupBy(word => word)
        .OrderByDescending(g => g.Count())
        .First()
        .Key;
}
```

**Time Complexity:** O(n + m) where m = banned words  
**Space Complexity:** O(n)

</details>

---

## Problem 8: Custom Sort (⭐⭐)

Sort strings by length, then alphabetically.

```csharp
// Input: ["apple", "pie", "a", "zoo", "dog"]
// Output: ["a", "dog", "pie", "zoo", "apple"]

public List<string> CustomSort(List<string> words)
{
    // Solve using LINQ
}
```

<details>
<summary>Solution</summary>

```csharp
public List<string> CustomSort(List<string> words)
{
    return words
        .OrderBy(w => w.Length)
        .ThenBy(w => w)
        .ToList();
}
```

**Time Complexity:** O(n log n)  
**Space Complexity:** O(n)

</details>

---

## Problem 9: Running Sum (⭐)

Calculate running sum of array.

```csharp
// Input: [1, 2, 3, 4]
// Output: [1, 3, 6, 10]

public int[] RunningSum(int[] nums)
{
    // Solve using LINQ (hint: Aggregate or Scan)
}
```

<details>
<summary>Solution</summary>

```csharp
// Using Aggregate with collection
public int[] RunningSum(int[] nums)
{
    int sum = 0;
    return nums.Select(n => sum += n).ToArray();
}

// Using custom Scan extension (if implemented)
public int[] RunningSum_Scan(int[] nums)
{
    return nums.Scan(0, (acc, n) => acc + n).Skip(1).ToArray();
}

// Scan extension method
public static IEnumerable<T> Scan<T>(
    this IEnumerable<T> source,
    T seed,
    Func<T, T, T> func)
{
    yield return seed;
    foreach (var item in source)
    {
        seed = func(seed, item);
        yield return seed;
    }
}
```

**Time Complexity:** O(n)  
**Space Complexity:** O(n)

</details>

---

## Problem 10: Matrix to List of Tuples (⭐⭐)

Convert 2D matrix to list of (row, col, value) tuples for non-zero elements.

```csharp
// Input: [[1,0,2], [0,3,0], [4,0,5]]
// Output: [(0,0,1), (0,2,2), (1,1,3), (2,0,4), (2,2,5)]

public List<(int Row, int Col, int Value)> MatrixToTuples(int[][] matrix)
{
    // Solve using LINQ
}
```

<details>
<summary>Solution</summary>

```csharp
public List<(int Row, int Col, int Value)> MatrixToTuples(int[][] matrix)
{
    return matrix
        .SelectMany((row, rowIndex) =>
            row.Select((value, colIndex) => (rowIndex, colIndex, value)))
        .Where(tuple => tuple.value != 0)
        .ToList();
}
```

**Time Complexity:** O(rows × cols)  
**Space Complexity:** O(non-zero elements)

</details>

---

## Problem 11: Partition by Condition (⭐⭐)

Split a list into two lists based on a condition.

```csharp
// Input: [1, 2, 3, 4, 5, 6], condition: n => n % 2 == 0
// Output: (Evens: [2, 4, 6], Odds: [1, 3, 5])

public (List<int> Matching, List<int> NonMatching) Partition(
    List<int> nums,
    Func<int, bool> condition)
{
    // Solve using LINQ
}
```

<details>
<summary>Solution</summary>

```csharp
public (List<int> Matching, List<int> NonMatching) Partition(
    List<int> nums,
    Func<int, bool> condition)
{
    var grouped = nums.ToLookup(condition);

    return (grouped[true].ToList(), grouped[false].ToList());
}

// Alternative using GroupBy
public (List<int> Matching, List<int> NonMatching) Partition_Alt(
    List<int> nums,
    Func<int, bool> condition)
{
    var groups = nums.GroupBy(condition)
                     .ToDictionary(g => g.Key, g => g.ToList());

    return (
        groups.GetValueOrDefault(true) ?? new List<int>(),
        groups.GetValueOrDefault(false) ?? new List<int>()
    );
}
```

**Time Complexity:** O(n)  
**Space Complexity:** O(n)

</details>

---

## Problem 12: FX Order Book Analysis (⭐⭐⭐)

**Domain: FX Trading**

Comprehensive order book analysis.

```csharp
public record Order(string Id, string Pair, decimal Price, decimal Amount, string Side);

public class OrderBookAnalyzer
{
    private readonly List<Order> _orders;

    // Best bid (highest buy price)
    public decimal? GetBestBid(string pair) { }

    // Best ask (lowest sell price)
    public decimal? GetBestAsk(string pair) { }

    // Spread (ask - bid)
    public decimal? GetSpread(string pair) { }

    // Total depth at price level
    public decimal GetDepthAtPrice(string pair, decimal price, string side) { }

    // VWAP (Volume Weighted Average Price)
    public decimal? GetVWAP(string pair, string side) { }

    // Top N price levels with depth
    public List<(decimal Price, decimal TotalAmount)> GetTopLevels(
        string pair, string side, int n) { }
}
```

<details>
<summary>Solution</summary>

```csharp
public class OrderBookAnalyzer
{
    private readonly List<Order> _orders;

    public OrderBookAnalyzer(List<Order> orders) => _orders = orders;

    public decimal? GetBestBid(string pair)
    {
        return _orders
            .Where(o => o.Pair == pair && o.Side == "BUY")
            .MaxBy(o => o.Price)?.Price;
    }

    public decimal? GetBestAsk(string pair)
    {
        return _orders
            .Where(o => o.Pair == pair && o.Side == "SELL")
            .MinBy(o => o.Price)?.Price;
    }

    public decimal? GetSpread(string pair)
    {
        var bid = GetBestBid(pair);
        var ask = GetBestAsk(pair);

        return (bid.HasValue && ask.HasValue) ? ask - bid : null;
    }

    public decimal GetDepthAtPrice(string pair, decimal price, string side)
    {
        return _orders
            .Where(o => o.Pair == pair && o.Side == side && o.Price == price)
            .Sum(o => o.Amount);
    }

    public decimal? GetVWAP(string pair, string side)
    {
        var orders = _orders
            .Where(o => o.Pair == pair && o.Side == side)
            .ToList();

        if (!orders.Any()) return null;

        var totalVolume = orders.Sum(o => o.Amount);
        var weightedSum = orders.Sum(o => o.Price * o.Amount);

        return weightedSum / totalVolume;
    }

    public List<(decimal Price, decimal TotalAmount)> GetTopLevels(
        string pair, string side, int n)
    {
        var query = _orders
            .Where(o => o.Pair == pair && o.Side == side)
            .GroupBy(o => o.Price)
            .Select(g => (Price: g.Key, TotalAmount: g.Sum(o => o.Amount)));

        // For bids, highest prices first; for asks, lowest first
        return (side == "BUY"
            ? query.OrderByDescending(x => x.Price)
            : query.OrderBy(x => x.Price))
            .Take(n)
            .ToList();
    }
}
```

</details>

---

## Big-O Quick Reference

| Problem          | Time         | Space  |
| ---------------- | ------------ | ------ |
| Top K Frequent   | O(n log n)   | O(n)   |
| Find Duplicates  | O(n)         | O(n)   |
| Intersection II  | O(n + m)     | O(n)   |
| Group Anagrams   | O(n·k·log k) | O(n·k) |
| FX Analysis      | O(n)         | O(n)   |
| Flatten List     | O(n)         | O(n)   |
| Most Common Word | O(n + m)     | O(n)   |
| Custom Sort      | O(n log n)   | O(n)   |
| Running Sum      | O(n)         | O(n)   |
| Matrix to Tuples | O(r × c)     | O(nz)  |
| Partition        | O(n)         | O(n)   |
| Order Book       | varies       | O(n)   |

---

_Practice makes perfect! Try solving these problems without looking at solutions first._
