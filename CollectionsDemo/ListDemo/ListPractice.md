# List<T> Practice Problems – Coding Challenges

> **Difficulty Levels:** ⭐ Easy | ⭐⭐ Medium | ⭐⭐⭐ Hard  
> **Focus:** Problem-solving with List<T> operations

---

## Problem 1: Two Sum ⭐

**Problem:** Given a list of integers and a target sum, return indices of two numbers that add up to the target.

```csharp
// Input: nums = [2, 7, 11, 15], target = 9
// Output: [0, 1] (because nums[0] + nums[1] = 2 + 7 = 9)

public int[] TwoSum(List<int> nums, int target)
{
    // Your code here
}
```

<details>
<summary>💡 Hint</summary>
Use a Dictionary to store number → index mapping for O(n) solution.
</details>

<details>
<summary>✅ Solution</summary>

```csharp
public int[] TwoSum(List<int> nums, int target)
{
    Dictionary<int, int> seen = new Dictionary<int, int>();

    for (int i = 0; i < nums.Count; i++)
    {
        int complement = target - nums[i];

        if (seen.ContainsKey(complement))
        {
            return new int[] { seen[complement], i };
        }

        seen[nums[i]] = i;
    }

    return Array.Empty<int>();
}
```

**Time:** O(n) | **Space:** O(n)

</details>

---

## Problem 2: Remove Duplicates ⭐

**Problem:** Remove duplicates from a sorted list in-place and return the new length.

```csharp
// Input: [1, 1, 2, 2, 2, 3, 4, 4, 5]
// Output: 5 (list becomes [1, 2, 3, 4, 5, ...])

public int RemoveDuplicates(List<int> nums)
{
    // Your code here
}
```

<details>
<summary>✅ Solution</summary>

```csharp
public int RemoveDuplicates(List<int> nums)
{
    if (nums.Count == 0) return 0;

    int writeIndex = 1;

    for (int i = 1; i < nums.Count; i++)
    {
        if (nums[i] != nums[i - 1])
        {
            nums[writeIndex] = nums[i];
            writeIndex++;
        }
    }

    // Remove extra elements
    nums.RemoveRange(writeIndex, nums.Count - writeIndex);
    return writeIndex;
}

// Alternative using LINQ (creates new list)
public List<int> RemoveDuplicatesLinq(List<int> nums)
{
    return nums.Distinct().ToList();
}
```

</details>

---

## Problem 3: Rotate List ⭐⭐

**Problem:** Rotate a list to the right by k positions.

```csharp
// Input: [1, 2, 3, 4, 5], k = 2
// Output: [4, 5, 1, 2, 3]

public void Rotate(List<int> nums, int k)
{
    // Your code here
}
```

<details>
<summary>💡 Hint</summary>
Think about reversing parts of the list.
</details>

<details>
<summary>✅ Solution</summary>

```csharp
public void Rotate(List<int> nums, int k)
{
    if (nums.Count == 0) return;

    k = k % nums.Count;  // Handle k > Count
    if (k == 0) return;

    // Reverse entire list
    nums.Reverse();

    // Reverse first k elements
    nums.Reverse(0, k);

    // Reverse remaining elements
    nums.Reverse(k, nums.Count - k);
}

// Alternative: Using GetRange
public void RotateAlt(List<int> nums, int k)
{
    k = k % nums.Count;
    var temp = nums.GetRange(nums.Count - k, k);
    nums.RemoveRange(nums.Count - k, k);
    nums.InsertRange(0, temp);
}
```

</details>

---

## Problem 4: Merge Sorted Lists ⭐⭐

**Problem:** Merge two sorted lists into one sorted list.

```csharp
// Input: list1 = [1, 3, 5], list2 = [2, 4, 6]
// Output: [1, 2, 3, 4, 5, 6]

public List<int> MergeSorted(List<int> list1, List<int> list2)
{
    // Your code here
}
```

<details>
<summary>✅ Solution</summary>

```csharp
public List<int> MergeSorted(List<int> list1, List<int> list2)
{
    List<int> result = new List<int>(list1.Count + list2.Count);
    int i = 0, j = 0;

    while (i < list1.Count && j < list2.Count)
    {
        if (list1[i] <= list2[j])
        {
            result.Add(list1[i]);
            i++;
        }
        else
        {
            result.Add(list2[j]);
            j++;
        }
    }

    // Add remaining elements
    while (i < list1.Count) result.Add(list1[i++]);
    while (j < list2.Count) result.Add(list2[j++]);

    return result;
}

// LINQ alternative (less efficient for large lists)
public List<int> MergeSortedLinq(List<int> list1, List<int> list2)
{
    return list1.Concat(list2).OrderBy(x => x).ToList();
}
```

</details>

---

## Problem 5: Find Missing Number ⭐

**Problem:** Given a list containing n distinct numbers from 0 to n, find the missing one.

```csharp
// Input: [3, 0, 1]
// Output: 2 (numbers should be 0, 1, 2, 3)

public int FindMissing(List<int> nums)
{
    // Your code here
}
```

<details>
<summary>✅ Solution</summary>

```csharp
public int FindMissing(List<int> nums)
{
    int n = nums.Count;
    int expectedSum = n * (n + 1) / 2;
    int actualSum = nums.Sum();
    return expectedSum - actualSum;
}

// Alternative using XOR
public int FindMissingXor(List<int> nums)
{
    int xor = nums.Count;
    for (int i = 0; i < nums.Count; i++)
    {
        xor ^= i ^ nums[i];
    }
    return xor;
}
```

</details>

---

## Problem 6: Move Zeroes ⭐

**Problem:** Move all zeroes to the end while maintaining relative order of non-zero elements.

```csharp
// Input: [0, 1, 0, 3, 12]
// Output: [1, 3, 12, 0, 0]

public void MoveZeroes(List<int> nums)
{
    // Your code here
}
```

<details>
<summary>✅ Solution</summary>

```csharp
public void MoveZeroes(List<int> nums)
{
    int writeIndex = 0;

    // Move all non-zero to front
    for (int i = 0; i < nums.Count; i++)
    {
        if (nums[i] != 0)
        {
            nums[writeIndex] = nums[i];
            writeIndex++;
        }
    }

    // Fill remaining with zeros
    while (writeIndex < nums.Count)
    {
        nums[writeIndex] = 0;
        writeIndex++;
    }
}
```

</details>

---

## Problem 7: FX Rate History Analysis ⭐⭐

**Problem:** Given a list of FX rate snapshots, find the maximum profit (buy low, sell high).

```csharp
// Input: rates = [1.0800, 1.0750, 1.0850, 1.0720, 1.0900, 1.0820]
// Output: 0.0180 (buy at 1.0720, sell at 1.0900)

public decimal MaxProfit(List<decimal> rates)
{
    // Your code here
}
```

<details>
<summary>✅ Solution</summary>

```csharp
public decimal MaxProfit(List<decimal> rates)
{
    if (rates.Count < 2) return 0;

    decimal minRate = rates[0];
    decimal maxProfit = 0;

    for (int i = 1; i < rates.Count; i++)
    {
        decimal profit = rates[i] - minRate;
        maxProfit = Math.Max(maxProfit, profit);
        minRate = Math.Min(minRate, rates[i]);
    }

    return maxProfit;
}
```

**Time:** O(n) | **Space:** O(1)

</details>

---

## Problem 8: Group Trades by Currency Pair ⭐⭐

**Problem:** Group a list of trades by currency pair and calculate total volume for each.

```csharp
public record Trade(string TradeId, string Pair, decimal Amount);

// Input: trades = [TRD1/EUR-USD/100000, TRD2/GBP-USD/50000, TRD3/EUR-USD/75000]
// Output: Dictionary { "EUR/USD": 175000, "GBP/USD": 50000 }

public Dictionary<string, decimal> GroupByPair(List<Trade> trades)
{
    // Your code here
}
```

<details>
<summary>✅ Solution</summary>

```csharp
public Dictionary<string, decimal> GroupByPair(List<Trade> trades)
{
    return trades
        .GroupBy(t => t.Pair)
        .ToDictionary(
            g => g.Key,
            g => g.Sum(t => t.Amount)
        );
}

// Without LINQ
public Dictionary<string, decimal> GroupByPairManual(List<Trade> trades)
{
    var result = new Dictionary<string, decimal>();

    foreach (var trade in trades)
    {
        if (result.ContainsKey(trade.Pair))
        {
            result[trade.Pair] += trade.Amount;
        }
        else
        {
            result[trade.Pair] = trade.Amount;
        }
    }

    return result;
}
```

</details>

---

## Problem 9: Find Pairs with Target Spread ⭐⭐

**Problem:** Find all unique pairs in a list where the difference equals the target spread.

```csharp
// Input: rates = [1.08, 1.10, 1.12, 1.06, 1.14], targetSpread = 0.04
// Output: [(1.06, 1.10), (1.08, 1.12), (1.10, 1.14)]

public List<(decimal, decimal)> FindPairsWithSpread(List<decimal> rates, decimal targetSpread)
{
    // Your code here
}
```

<details>
<summary>✅ Solution</summary>

```csharp
public List<(decimal, decimal)> FindPairsWithSpread(List<decimal> rates, decimal targetSpread)
{
    var result = new List<(decimal, decimal)>();
    var rateSet = new HashSet<decimal>(rates);
    var seen = new HashSet<decimal>();

    foreach (var rate in rates)
    {
        decimal higher = rate + targetSpread;
        decimal lower = rate - targetSpread;

        if (rateSet.Contains(higher) && !seen.Contains(rate))
        {
            result.Add((rate, higher));
            seen.Add(rate);
            seen.Add(higher);
        }
    }

    return result.OrderBy(p => p.Item1).ToList();
}
```

</details>

---

## Problem 10: Sliding Window Average ⭐⭐⭐

**Problem:** Calculate the moving average of FX rates with a given window size.

```csharp
// Input: rates = [1.08, 1.09, 1.10, 1.08, 1.07, 1.11], windowSize = 3
// Output: [1.09, 1.09, 1.0833, 1.0867]

public List<decimal> MovingAverage(List<decimal> rates, int windowSize)
{
    // Your code here
}
```

<details>
<summary>✅ Solution</summary>

```csharp
public List<decimal> MovingAverage(List<decimal> rates, int windowSize)
{
    if (rates.Count < windowSize) return new List<decimal>();

    var result = new List<decimal>();
    decimal windowSum = 0;

    // Calculate first window sum
    for (int i = 0; i < windowSize; i++)
    {
        windowSum += rates[i];
    }
    result.Add(windowSum / windowSize);

    // Slide the window
    for (int i = windowSize; i < rates.Count; i++)
    {
        windowSum += rates[i];          // Add new element
        windowSum -= rates[i - windowSize];  // Remove old element
        result.Add(windowSum / windowSize);
    }

    return result;
}
```

**Time:** O(n) | **Space:** O(n-k+1) for result

</details>

---

## Problem 11: Implement a Rate Limiter ⭐⭐⭐

**Problem:** Design a rate limiter that allows max N requests in a sliding window of T seconds.

```csharp
public class RateLimiter
{
    // Allow max 5 requests per 10 seconds
    public RateLimiter(int maxRequests, int windowSeconds)
    {
        // Your code here
    }

    public bool AllowRequest()
    {
        // Return true if request allowed, false if rate limited
    }
}
```

<details>
<summary>✅ Solution</summary>

```csharp
public class RateLimiter
{
    private readonly int _maxRequests;
    private readonly int _windowSeconds;
    private readonly List<DateTime> _requestTimes;

    public RateLimiter(int maxRequests, int windowSeconds)
    {
        _maxRequests = maxRequests;
        _windowSeconds = windowSeconds;
        _requestTimes = new List<DateTime>(maxRequests);
    }

    public bool AllowRequest()
    {
        var now = DateTime.UtcNow;
        var windowStart = now.AddSeconds(-_windowSeconds);

        // Remove expired timestamps
        _requestTimes.RemoveAll(t => t < windowStart);

        if (_requestTimes.Count < _maxRequests)
        {
            _requestTimes.Add(now);
            return true;
        }

        return false;
    }
}

// Usage:
// var limiter = new RateLimiter(5, 10);  // 5 requests per 10 seconds
// if (limiter.AllowRequest()) { /* process */ }
// else { /* rate limited */ }
```

</details>

---

## Quick Reference: Common Patterns

| Pattern        | Use Case               | Example                             |
| -------------- | ---------------------- | ----------------------------------- |
| Two Pointers   | Sorted list operations | Two Sum (sorted), Remove duplicates |
| Sliding Window | Subarray problems      | Moving average, Max in window       |
| Fast & Slow    | Cycle detection        | Find middle, Detect loop            |
| Hash Map       | O(1) lookup            | Two Sum, Find duplicates            |
| Binary Search  | Sorted list            | Find element, Find insertion point  |

---

## Self-Assessment Checklist

- [ ] Can implement Two Sum in O(n)
- [ ] Can remove duplicates in-place
- [ ] Can rotate list efficiently
- [ ] Can merge sorted lists
- [ ] Understand sliding window technique
- [ ] Can group and aggregate with LINQ
- [ ] Can implement binary search

---

_Practice these problems until you can solve them without looking at solutions!_
