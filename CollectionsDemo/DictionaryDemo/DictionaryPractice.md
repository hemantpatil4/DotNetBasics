# Dictionary<K,V> Practice Problems

> Interview-focused coding challenges for Dictionary operations  
> Difficulty: ⭐ Easy | ⭐⭐ Medium | ⭐⭐⭐ Hard

---

## Problem 1: Two Sum (⭐⭐)

**Classic interview problem!**

Given an array of integers and a target sum, return indices of two numbers that add up to the target.

```csharp
// Input: nums = [2, 7, 11, 15], target = 9
// Output: [0, 1] (because nums[0] + nums[1] = 2 + 7 = 9)

public int[] TwoSum(int[] nums, int target)
{
    // Your solution here
}
```

**Hint:** For each number, what value do you need to find? Can you store seen numbers?

<details>
<summary>Solution</summary>

```csharp
public int[] TwoSum(int[] nums, int target)
{
    // Key: number, Value: index
    var seen = new Dictionary<int, int>();

    for (int i = 0; i < nums.Length; i++)
    {
        int complement = target - nums[i];

        if (seen.TryGetValue(complement, out int index))
        {
            return new int[] { index, i };
        }

        seen[nums[i]] = i;
    }

    return Array.Empty<int>();
}
```

**Time Complexity:** O(n)  
**Space Complexity:** O(n)

</details>

---

## Problem 2: First Non-Repeating Character (⭐⭐)

Find the first character in a string that appears only once.

```csharp
// Input: "fxtrading"
// Output: 'f' (first non-repeating)

// Input: "aabbcc"
// Output: null (all repeat)

public char? FirstUniqueChar(string s)
{
    // Your solution here
}
```

**Hint:** Two passes - first count, then find first with count 1.

<details>
<summary>Solution</summary>

```csharp
public char? FirstUniqueChar(string s)
{
    var charCount = new Dictionary<char, int>();

    // First pass: count occurrences
    foreach (char c in s)
    {
        charCount.TryGetValue(c, out int count);
        charCount[c] = count + 1;
    }

    // Second pass: find first with count 1
    foreach (char c in s)
    {
        if (charCount[c] == 1)
        {
            return c;
        }
    }

    return null;
}
```

**Time Complexity:** O(n)  
**Space Complexity:** O(k) where k = unique characters

</details>

---

## Problem 3: Group Anagrams (⭐⭐⭐)

Group strings that are anagrams of each other.

```csharp
// Input: ["eat", "tea", "tan", "ate", "nat", "bat"]
// Output: [["eat","tea","ate"], ["tan","nat"], ["bat"]]

public IList<IList<string>> GroupAnagrams(string[] strs)
{
    // Your solution here
}
```

**Hint:** Anagrams have the same characters when sorted. What can you use as a key?

<details>
<summary>Solution</summary>

```csharp
public IList<IList<string>> GroupAnagrams(string[] strs)
{
    // Key: sorted string, Value: list of anagrams
    var groups = new Dictionary<string, List<string>>();

    foreach (var str in strs)
    {
        // Sort characters to create key
        char[] chars = str.ToCharArray();
        Array.Sort(chars);
        string key = new string(chars);

        if (!groups.ContainsKey(key))
        {
            groups[key] = new List<string>();
        }
        groups[key].Add(str);
    }

    return groups.Values.ToList<IList<string>>();
}

// Alternative: Use character count as key
public IList<IList<string>> GroupAnagrams_Alt(string[] strs)
{
    var groups = new Dictionary<string, List<string>>();

    foreach (var str in strs)
    {
        // Count characters
        int[] count = new int[26];
        foreach (char c in str)
        {
            count[c - 'a']++;
        }

        // Create key from count
        string key = string.Join(",", count);

        if (!groups.ContainsKey(key))
        {
            groups[key] = new List<string>();
        }
        groups[key].Add(str);
    }

    return groups.Values.ToList<IList<string>>();
}
```

**Time Complexity:** O(n _ k log k) where k = max string length  
**Space Complexity:** O(n _ k)

</details>

---

## Problem 4: Subarray Sum Equals K (⭐⭐⭐)

Find the total number of continuous subarrays whose sum equals k.

```csharp
// Input: nums = [1, 1, 1], k = 2
// Output: 2 (subarrays [1,1] at indices 0-1 and 1-2)

// Input: nums = [1, 2, 3], k = 3
// Output: 2 (subarrays [1,2] and [3])

public int SubarraySum(int[] nums, int k)
{
    // Your solution here
}
```

**Hint:** Use prefix sum. If prefix[j] - prefix[i] = k, then subarray i+1 to j has sum k.

<details>
<summary>Solution</summary>

```csharp
public int SubarraySum(int[] nums, int k)
{
    // Key: prefix sum, Value: count of occurrences
    var prefixCount = new Dictionary<int, int>();
    prefixCount[0] = 1;  // Empty prefix has sum 0

    int count = 0;
    int prefixSum = 0;

    foreach (int num in nums)
    {
        prefixSum += num;

        // If (prefixSum - k) exists, we found subarrays with sum k
        if (prefixCount.TryGetValue(prefixSum - k, out int occurrences))
        {
            count += occurrences;
        }

        // Record current prefix sum
        prefixCount.TryGetValue(prefixSum, out int currentCount);
        prefixCount[prefixSum] = currentCount + 1;
    }

    return count;
}
```

**Time Complexity:** O(n)  
**Space Complexity:** O(n)

</details>

---

## Problem 5: LRU Cache (⭐⭐⭐)

**Popular interview problem!**

Implement a Least Recently Used (LRU) cache with O(1) get and put operations.

```csharp
// LRUCache cache = new LRUCache(2); // capacity 2
// cache.Put(1, 1);
// cache.Put(2, 2);
// cache.Get(1);      // returns 1, marks 1 as recently used
// cache.Put(3, 3);   // evicts key 2 (least recently used)
// cache.Get(2);      // returns -1 (not found)

public class LRUCache
{
    public LRUCache(int capacity) { }
    public int Get(int key) { }
    public void Put(int key, int value) { }
}
```

**Hint:** Dictionary for O(1) lookup + LinkedList for O(1) order management.

<details>
<summary>Solution</summary>

```csharp
public class LRUCache
{
    private readonly int _capacity;
    private readonly Dictionary<int, LinkedListNode<(int Key, int Value)>> _cache;
    private readonly LinkedList<(int Key, int Value)> _order;  // Head = MRU, Tail = LRU

    public LRUCache(int capacity)
    {
        _capacity = capacity;
        _cache = new Dictionary<int, LinkedListNode<(int, int)>>(capacity);
        _order = new LinkedList<(int, int)>();
    }

    public int Get(int key)
    {
        if (!_cache.TryGetValue(key, out var node))
        {
            return -1;
        }

        // Move to front (most recently used)
        _order.Remove(node);
        _order.AddFirst(node);

        return node.Value.Value;
    }

    public void Put(int key, int value)
    {
        if (_cache.TryGetValue(key, out var existingNode))
        {
            // Update existing
            _order.Remove(existingNode);
            existingNode.Value = (key, value);
            _order.AddFirst(existingNode);
        }
        else
        {
            // Check capacity
            if (_cache.Count >= _capacity)
            {
                // Remove LRU (tail)
                var lru = _order.Last!;
                _cache.Remove(lru.Value.Key);
                _order.RemoveLast();
            }

            // Add new
            var newNode = _order.AddFirst((key, value));
            _cache[key] = newNode;
        }
    }
}
```

**Time Complexity:** O(1) for both operations  
**Space Complexity:** O(capacity)

</details>

---

## Problem 6: FX Rate History Tracker (⭐⭐)

**Domain: FX Trading**

Track rate updates for multiple currency pairs with timestamps.

```csharp
// Requirements:
// - UpdateRate(pair, rate) - record rate with current timestamp
// - GetLatestRate(pair) - return most recent rate
// - GetRateHistory(pair) - return all rates in chronological order
// - GetAverageRate(pair, lastNMinutes) - average rate in time window

public class RateTracker
{
    public void UpdateRate(string pair, decimal rate) { }
    public decimal? GetLatestRate(string pair) { }
    public IList<(DateTime Time, decimal Rate)> GetRateHistory(string pair) { }
    public decimal? GetAverageRate(string pair, int lastNMinutes) { }
}
```

<details>
<summary>Solution</summary>

```csharp
public class RateTracker
{
    // Pair → List of (Timestamp, Rate)
    private readonly Dictionary<string, List<(DateTime Time, decimal Rate)>> _history = new();

    public void UpdateRate(string pair, decimal rate)
    {
        if (!_history.ContainsKey(pair))
        {
            _history[pair] = new List<(DateTime, decimal)>();
        }
        _history[pair].Add((DateTime.UtcNow, rate));
    }

    public decimal? GetLatestRate(string pair)
    {
        if (!_history.TryGetValue(pair, out var rates) || rates.Count == 0)
        {
            return null;
        }
        return rates[^1].Rate;  // Last element
    }

    public IList<(DateTime Time, decimal Rate)> GetRateHistory(string pair)
    {
        if (!_history.TryGetValue(pair, out var rates))
        {
            return Array.Empty<(DateTime, decimal)>();
        }
        return rates.ToList();
    }

    public decimal? GetAverageRate(string pair, int lastNMinutes)
    {
        if (!_history.TryGetValue(pair, out var rates))
        {
            return null;
        }

        var cutoff = DateTime.UtcNow.AddMinutes(-lastNMinutes);
        var recentRates = rates.Where(r => r.Time >= cutoff).ToList();

        if (recentRates.Count == 0)
        {
            return null;
        }

        return recentRates.Average(r => r.Rate);
    }
}
```

**Time Complexity:**

- UpdateRate: O(1) amortized
- GetLatestRate: O(1)
- GetRateHistory: O(n)
- GetAverageRate: O(n)

</details>

---

## Problem 7: Word Frequency Counter (⭐)

Count word frequency in a text, case-insensitive, sorted by frequency.

```csharp
// Input: "The quick brown fox jumps over the lazy dog. The dog barks."
// Output: [("the", 3), ("dog", 2), ("quick", 1), ...]

public List<(string Word, int Count)> CountWords(string text)
{
    // Your solution here
}
```

<details>
<summary>Solution</summary>

```csharp
public List<(string Word, int Count)> CountWords(string text)
{
    // Use case-insensitive dictionary
    var wordCount = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

    // Split into words (remove punctuation)
    var words = text.Split(new[] { ' ', '.', ',', '!', '?' },
                          StringSplitOptions.RemoveEmptyEntries);

    foreach (var word in words)
    {
        wordCount.TryGetValue(word, out int count);
        wordCount[word] = count + 1;
    }

    // Sort by frequency (descending), then alphabetically
    return wordCount
        .OrderByDescending(kvp => kvp.Value)
        .ThenBy(kvp => kvp.Key)
        .Select(kvp => (kvp.Key.ToLower(), kvp.Value))
        .ToList();
}
```

**Time Complexity:** O(n log n) due to sorting  
**Space Complexity:** O(n)

</details>

---

## Problem 8: Valid Parentheses Extended (⭐⭐)

Check if string has valid bracket pairing using multiple bracket types.

```csharp
// Input: "{[()]}"
// Output: true

// Input: "{[(])}"
// Output: false

public bool IsValid(string s)
{
    // Your solution here
}
```

**Hint:** Use Dictionary to map closing brackets to opening brackets, Stack to track.

<details>
<summary>Solution</summary>

```csharp
public bool IsValid(string s)
{
    // Map closing bracket to its opening counterpart
    var bracketMap = new Dictionary<char, char>
    {
        { ')', '(' },
        { ']', '[' },
        { '}', '{' }
    };

    var stack = new Stack<char>();

    foreach (char c in s)
    {
        if (bracketMap.ContainsKey(c))
        {
            // Closing bracket - check if matches top of stack
            if (stack.Count == 0 || stack.Pop() != bracketMap[c])
            {
                return false;
            }
        }
        else if (bracketMap.ContainsValue(c))
        {
            // Opening bracket - push to stack
            stack.Push(c);
        }
    }

    return stack.Count == 0;
}
```

**Time Complexity:** O(n)  
**Space Complexity:** O(n)

</details>

---

## Problem 9: Trade Position Calculator (⭐⭐)

**Domain: FX Trading**

Calculate net positions from a list of trades.

```csharp
// Trades: [
//   { Pair: "EUR/USD", Amount: 100000, Side: "BUY" },
//   { Pair: "EUR/USD", Amount: 50000, Side: "SELL" },
//   { Pair: "GBP/USD", Amount: 200000, Side: "BUY" }
// ]
// Output: { "EUR/USD": 50000, "GBP/USD": 200000 }

public record Trade(string Pair, decimal Amount, string Side);

public Dictionary<string, decimal> CalculatePositions(List<Trade> trades)
{
    // Your solution here
}
```

<details>
<summary>Solution</summary>

```csharp
public Dictionary<string, decimal> CalculatePositions(List<Trade> trades)
{
    var positions = new Dictionary<string, decimal>();

    foreach (var trade in trades)
    {
        positions.TryGetValue(trade.Pair, out decimal current);

        decimal signedAmount = trade.Side.ToUpper() == "BUY"
            ? trade.Amount
            : -trade.Amount;

        positions[trade.Pair] = current + signedAmount;
    }

    // Remove flat positions (optional)
    var flatPairs = positions.Where(kvp => kvp.Value == 0).Select(kvp => kvp.Key).ToList();
    foreach (var pair in flatPairs)
    {
        positions.Remove(pair);
    }

    return positions;
}
```

**Time Complexity:** O(n)  
**Space Complexity:** O(k) where k = unique pairs

</details>

---

## Problem 10: Isomorphic Strings (⭐⭐)

Check if two strings are isomorphic (characters can be mapped 1:1).

```csharp
// Input: s = "egg", t = "add"
// Output: true (e→a, g→d)

// Input: s = "foo", t = "bar"
// Output: false (o maps to both a and r)

public bool IsIsomorphic(string s, string t)
{
    // Your solution here
}
```

**Hint:** Need TWO dictionaries - one for s→t mapping, one for t→s mapping.

<details>
<summary>Solution</summary>

```csharp
public bool IsIsomorphic(string s, string t)
{
    if (s.Length != t.Length) return false;

    var sToT = new Dictionary<char, char>();  // Maps s chars to t chars
    var tToS = new Dictionary<char, char>();  // Maps t chars to s chars

    for (int i = 0; i < s.Length; i++)
    {
        char sChar = s[i];
        char tChar = t[i];

        // Check s→t mapping
        if (sToT.TryGetValue(sChar, out char mappedT))
        {
            if (mappedT != tChar) return false;
        }
        else
        {
            sToT[sChar] = tChar;
        }

        // Check t→s mapping
        if (tToS.TryGetValue(tChar, out char mappedS))
        {
            if (mappedS != sChar) return false;
        }
        else
        {
            tToS[tChar] = sChar;
        }
    }

    return true;
}
```

**Time Complexity:** O(n)  
**Space Complexity:** O(k) where k = unique characters

</details>

---

## Problem 11: Longest Substring Without Repeating Characters (⭐⭐⭐)

Find length of longest substring without repeating characters.

```csharp
// Input: "abcabcbb"
// Output: 3 ("abc")

// Input: "pwwkew"
// Output: 3 ("wke")

public int LengthOfLongestSubstring(string s)
{
    // Your solution here
}
```

**Hint:** Sliding window with Dictionary to track last seen index of each character.

<details>
<summary>Solution</summary>

```csharp
public int LengthOfLongestSubstring(string s)
{
    // Key: character, Value: last seen index
    var lastSeen = new Dictionary<char, int>();

    int maxLength = 0;
    int windowStart = 0;

    for (int windowEnd = 0; windowEnd < s.Length; windowEnd++)
    {
        char c = s[windowEnd];

        // If character was seen and is within current window
        if (lastSeen.TryGetValue(c, out int lastIndex) && lastIndex >= windowStart)
        {
            // Move window start past the duplicate
            windowStart = lastIndex + 1;
        }

        lastSeen[c] = windowEnd;
        maxLength = Math.Max(maxLength, windowEnd - windowStart + 1);
    }

    return maxLength;
}
```

**Time Complexity:** O(n)  
**Space Complexity:** O(min(n, k)) where k = alphabet size

</details>

---

## Big-O Quick Reference

| Problem            | Time           | Space       |
| ------------------ | -------------- | ----------- |
| Two Sum            | O(n)           | O(n)        |
| First Unique Char  | O(n)           | O(k)        |
| Group Anagrams     | O(n·k·log k)   | O(n·k)      |
| Subarray Sum = K   | O(n)           | O(n)        |
| LRU Cache          | O(1)           | O(capacity) |
| Rate History       | O(1) amortized | O(n)        |
| Word Frequency     | O(n log n)     | O(n)        |
| Valid Parentheses  | O(n)           | O(n)        |
| Trade Positions    | O(n)           | O(k)        |
| Isomorphic Strings | O(n)           | O(k)        |
| Longest Substring  | O(n)           | O(k)        |

---

_Practice makes perfect! Try solving these problems without looking at solutions first._
