# HashSet<T> Practice Problems

> Interview-focused coding challenges for HashSet operations  
> Difficulty: ⭐ Easy | ⭐⭐ Medium | ⭐⭐⭐ Hard

---

## Problem 1: Contains Duplicate (⭐)

**Classic interview problem!**

Given an integer array, return true if any value appears at least twice.

```csharp
// Input: nums = [1, 2, 3, 1]
// Output: true

// Input: nums = [1, 2, 3, 4]
// Output: false

public bool ContainsDuplicate(int[] nums)
{
    // Your solution here
}
```

**Hint:** What does HashSet.Add() return?

<details>
<summary>Solution</summary>

```csharp
public bool ContainsDuplicate(int[] nums)
{
    var seen = new HashSet<int>();

    foreach (int num in nums)
    {
        if (!seen.Add(num))  // Add returns false if already exists
        {
            return true;
        }
    }

    return false;
}

// One-liner alternative
public bool ContainsDuplicate_Alt(int[] nums)
{
    return nums.Length != nums.ToHashSet().Count;
}
```

**Time Complexity:** O(n)  
**Space Complexity:** O(n)

</details>

---

## Problem 2: Intersection of Two Arrays (⭐)

Find common elements between two arrays. Each element in result must be unique.

```csharp
// Input: nums1 = [1, 2, 2, 1], nums2 = [2, 2]
// Output: [2]

// Input: nums1 = [4, 9, 5], nums2 = [9, 4, 9, 8, 4]
// Output: [4, 9] (order doesn't matter)

public int[] Intersection(int[] nums1, int[] nums2)
{
    // Your solution here
}
```

<details>
<summary>Solution</summary>

```csharp
public int[] Intersection(int[] nums1, int[] nums2)
{
    var set1 = new HashSet<int>(nums1);
    var set2 = new HashSet<int>(nums2);

    set1.IntersectWith(set2);

    return set1.ToArray();
}

// LINQ alternative
public int[] Intersection_Alt(int[] nums1, int[] nums2)
{
    return nums1.ToHashSet().Intersect(nums2).ToArray();
}
```

**Time Complexity:** O(n + m)  
**Space Complexity:** O(min(n, m))

</details>

---

## Problem 3: Happy Number (⭐⭐)

A happy number is where repeatedly summing squares of digits eventually reaches 1. Return false if it loops endlessly.

```csharp
// 19 → 1² + 9² = 82 → 8² + 2² = 68 → 6² + 8² = 100 → 1² + 0² + 0² = 1 ✓
// Input: 19
// Output: true

// 2 → 4 → 16 → 37 → 58 → 89 → 145 → 42 → 20 → 4 (loop!)
// Input: 2
// Output: false

public bool IsHappy(int n)
{
    // Your solution here
}
```

**Hint:** How to detect a cycle?

<details>
<summary>Solution</summary>

```csharp
public bool IsHappy(int n)
{
    var seen = new HashSet<int>();

    while (n != 1)
    {
        if (!seen.Add(n))  // If we've seen this number, it's a cycle
        {
            return false;
        }

        n = GetSumOfSquares(n);
    }

    return true;
}

private int GetSumOfSquares(int n)
{
    int sum = 0;
    while (n > 0)
    {
        int digit = n % 10;
        sum += digit * digit;
        n /= 10;
    }
    return sum;
}
```

**Time Complexity:** O(log n) iterations  
**Space Complexity:** O(log n)

</details>

---

## Problem 4: Longest Consecutive Sequence (⭐⭐⭐)

**Popular interview problem!**

Find the length of the longest consecutive elements sequence. Must be O(n) time.

```csharp
// Input: nums = [100, 4, 200, 1, 3, 2]
// Output: 4 (sequence: [1, 2, 3, 4])

// Input: nums = [0, 3, 7, 2, 5, 8, 4, 6, 0, 1]
// Output: 9 (sequence: [0, 1, 2, 3, 4, 5, 6, 7, 8])

public int LongestConsecutive(int[] nums)
{
    // Your solution here
}
```

**Hint:** Only start counting from a number that is the START of a sequence.

<details>
<summary>Solution</summary>

```csharp
public int LongestConsecutive(int[] nums)
{
    var numSet = new HashSet<int>(nums);
    int longestStreak = 0;

    foreach (int num in numSet)
    {
        // Only start counting if this is the START of a sequence
        // (i.e., num-1 doesn't exist)
        if (!numSet.Contains(num - 1))
        {
            int currentNum = num;
            int currentStreak = 1;

            // Count consecutive numbers
            while (numSet.Contains(currentNum + 1))
            {
                currentNum++;
                currentStreak++;
            }

            longestStreak = Math.Max(longestStreak, currentStreak);
        }
    }

    return longestStreak;
}
```

**Time Complexity:** O(n) - each element visited at most twice  
**Space Complexity:** O(n)

</details>

---

## Problem 5: Single Number (⭐)

Find the element that appears only once (all others appear twice).

```csharp
// Input: nums = [2, 2, 1]
// Output: 1

// Input: nums = [4, 1, 2, 1, 2]
// Output: 4

public int SingleNumber(int[] nums)
{
    // Your solution here
}
```

**Hint:** HashSet approach: add if not exists, remove if exists.

<details>
<summary>Solution</summary>

```csharp
// HashSet approach
public int SingleNumber(int[] nums)
{
    var set = new HashSet<int>();

    foreach (int num in nums)
    {
        if (!set.Add(num))  // If Add returns false, it already exists
        {
            set.Remove(num);  // Remove it
        }
    }

    return set.First();  // Only one left
}

// Optimal XOR approach (O(1) space)
public int SingleNumber_XOR(int[] nums)
{
    int result = 0;
    foreach (int num in nums)
    {
        result ^= num;  // XOR: a ^ a = 0, 0 ^ a = a
    }
    return result;
}
```

**Time Complexity:** O(n)  
**Space Complexity:** O(n) for HashSet, O(1) for XOR

</details>

---

## Problem 6: FX Trade Reconciliation (⭐⭐)

**Domain: FX Trading**

Given trades from two systems, find discrepancies.

```csharp
// System A: ["T1", "T2", "T3", "T4"]
// System B: ["T2", "T3", "T4", "T5"]
// Output:
//   - Missing in B: ["T1"]
//   - Missing in A: ["T5"]
//   - Matched: ["T2", "T3", "T4"]

public record ReconciliationResult(
    List<string> MissingInB,
    List<string> MissingInA,
    List<string> Matched);

public ReconciliationResult ReconcileTrades(string[] systemA, string[] systemB)
{
    // Your solution here
}
```

<details>
<summary>Solution</summary>

```csharp
public ReconciliationResult ReconcileTrades(string[] systemA, string[] systemB)
{
    var setA = new HashSet<string>(systemA);
    var setB = new HashSet<string>(systemB);

    // Matched: intersection
    var matched = new HashSet<string>(setA);
    matched.IntersectWith(setB);

    // Missing in B: in A but not in B
    var missingInB = new HashSet<string>(setA);
    missingInB.ExceptWith(setB);

    // Missing in A: in B but not in A
    var missingInA = new HashSet<string>(setB);
    missingInA.ExceptWith(setA);

    return new ReconciliationResult(
        missingInB.ToList(),
        missingInA.ToList(),
        matched.ToList());
}
```

**Time Complexity:** O(n + m)  
**Space Complexity:** O(n + m)

</details>

---

## Problem 7: Unique Email Addresses (⭐⭐)

Email rules:

- Ignore dots before @ in local name
- Ignore everything after + in local name

```csharp
// Input: ["test.email+alex@leetcode.com", "test.e.mail+bob@leetcode.com", "testemail+david@lee.tcode.com"]
// Output: 2
// Explanation: "testemail@leetcode.com" and "testemail@lee.tcode.com"

public int NumUniqueEmails(string[] emails)
{
    // Your solution here
}
```

<details>
<summary>Solution</summary>

```csharp
public int NumUniqueEmails(string[] emails)
{
    var uniqueEmails = new HashSet<string>();

    foreach (string email in emails)
    {
        string[] parts = email.Split('@');
        string local = parts[0];
        string domain = parts[1];

        // Remove everything after +
        int plusIndex = local.IndexOf('+');
        if (plusIndex >= 0)
        {
            local = local[..plusIndex];
        }

        // Remove dots
        local = local.Replace(".", "");

        // Combine and add
        uniqueEmails.Add($"{local}@{domain}");
    }

    return uniqueEmails.Count;
}
```

**Time Complexity:** O(n _ k) where k = email length  
**Space Complexity:** O(n _ k)

</details>

---

## Problem 8: Find All Duplicates (⭐⭐)

Find all elements that appear twice in array where 1 ≤ a[i] ≤ n.

```csharp
// Input: nums = [4, 3, 2, 7, 8, 2, 3, 1]
// Output: [2, 3]

public IList<int> FindDuplicates(int[] nums)
{
    // Your solution here
}
```

<details>
<summary>Solution</summary>

```csharp
// HashSet approach
public IList<int> FindDuplicates(int[] nums)
{
    var seen = new HashSet<int>();
    var duplicates = new List<int>();

    foreach (int num in nums)
    {
        if (!seen.Add(num))  // If already exists
        {
            duplicates.Add(num);
        }
    }

    return duplicates;
}

// O(1) space approach using sign flipping
public IList<int> FindDuplicates_NoExtraSpace(int[] nums)
{
    var duplicates = new List<int>();

    foreach (int num in nums)
    {
        int index = Math.Abs(num) - 1;

        if (nums[index] < 0)  // Already marked
        {
            duplicates.Add(Math.Abs(num));
        }
        else
        {
            nums[index] = -nums[index];  // Mark as seen
        }
    }

    return duplicates;
}
```

**Time Complexity:** O(n)  
**Space Complexity:** O(n) for HashSet, O(1) for sign-flipping

</details>

---

## Problem 9: Jewels and Stones (⭐)

Count how many stones are jewels.

```csharp
// Input: jewels = "aA", stones = "aAAbbbb"
// Output: 3 (a, A, A)

public int NumJewelsInStones(string jewels, string stones)
{
    // Your solution here
}
```

<details>
<summary>Solution</summary>

```csharp
public int NumJewelsInStones(string jewels, string stones)
{
    var jewelSet = new HashSet<char>(jewels);

    int count = 0;
    foreach (char stone in stones)
    {
        if (jewelSet.Contains(stone))
        {
            count++;
        }
    }

    return count;
}

// LINQ one-liner
public int NumJewelsInStones_Linq(string jewels, string stones)
{
    var jewelSet = jewels.ToHashSet();
    return stones.Count(s => jewelSet.Contains(s));
}
```

**Time Complexity:** O(j + s)  
**Space Complexity:** O(j)

</details>

---

## Problem 10: Active Trading Sessions (⭐⭐)

**Domain: FX Trading**

Track active currency pairs across trading sessions. Support adding/removing pairs and finding pairs active in multiple sessions.

```csharp
public class TradingSessionManager
{
    // Add pair to session
    public void AddPair(string session, string pair) { }

    // Remove pair from session
    public void RemovePair(string session, string pair) { }

    // Get pairs active in specific session
    public ISet<string> GetActivePairs(string session) { }

    // Get pairs active in ALL given sessions
    public ISet<string> GetCommonPairs(params string[] sessions) { }

    // Get pairs active in ANY of the given sessions
    public ISet<string> GetAllPairs(params string[] sessions) { }

    // Get pairs exclusive to a session (not in any other)
    public ISet<string> GetExclusivePairs(string session) { }
}
```

<details>
<summary>Solution</summary>

```csharp
public class TradingSessionManager
{
    private readonly Dictionary<string, HashSet<string>> _sessions = new();

    public void AddPair(string session, string pair)
    {
        if (!_sessions.ContainsKey(session))
        {
            _sessions[session] = new HashSet<string>();
        }
        _sessions[session].Add(pair);
    }

    public void RemovePair(string session, string pair)
    {
        if (_sessions.TryGetValue(session, out var pairs))
        {
            pairs.Remove(pair);
        }
    }

    public ISet<string> GetActivePairs(string session)
    {
        if (_sessions.TryGetValue(session, out var pairs))
        {
            return new HashSet<string>(pairs);  // Return copy
        }
        return new HashSet<string>();
    }

    public ISet<string> GetCommonPairs(params string[] sessions)
    {
        if (sessions.Length == 0) return new HashSet<string>();

        var result = GetActivePairs(sessions[0]);

        for (int i = 1; i < sessions.Length; i++)
        {
            result.IntersectWith(GetActivePairs(sessions[i]));
        }

        return result;
    }

    public ISet<string> GetAllPairs(params string[] sessions)
    {
        var result = new HashSet<string>();

        foreach (var session in sessions)
        {
            result.UnionWith(GetActivePairs(session));
        }

        return result;
    }

    public ISet<string> GetExclusivePairs(string session)
    {
        var result = GetActivePairs(session);

        foreach (var (otherSession, pairs) in _sessions)
        {
            if (otherSession != session)
            {
                result.ExceptWith(pairs);
            }
        }

        return result;
    }
}
```

</details>

---

## Problem 11: Word Pattern (⭐⭐)

Check if string follows given pattern (bijection mapping).

```csharp
// Input: pattern = "abba", s = "dog cat cat dog"
// Output: true (a→dog, b→cat)

// Input: pattern = "abba", s = "dog cat cat fish"
// Output: false

public bool WordPattern(string pattern, string s)
{
    // Your solution here
}
```

**Hint:** Need to ensure 1:1 mapping in BOTH directions.

<details>
<summary>Solution</summary>

```csharp
public bool WordPattern(string pattern, string s)
{
    string[] words = s.Split(' ');

    if (pattern.Length != words.Length) return false;

    var charToWord = new Dictionary<char, string>();
    var wordToChar = new Dictionary<string, char>();

    for (int i = 0; i < pattern.Length; i++)
    {
        char c = pattern[i];
        string word = words[i];

        // Check char → word mapping
        if (charToWord.TryGetValue(c, out string? mappedWord))
        {
            if (mappedWord != word) return false;
        }
        else
        {
            charToWord[c] = word;
        }

        // Check word → char mapping
        if (wordToChar.TryGetValue(word, out char mappedChar))
        {
            if (mappedChar != c) return false;
        }
        else
        {
            wordToChar[word] = c;
        }
    }

    return true;
}

// Alternative using HashSet for used words
public bool WordPattern_Alt(string pattern, string s)
{
    string[] words = s.Split(' ');
    if (pattern.Length != words.Length) return false;

    var map = new Dictionary<char, string>();
    var usedWords = new HashSet<string>();

    for (int i = 0; i < pattern.Length; i++)
    {
        char c = pattern[i];
        string word = words[i];

        if (map.TryGetValue(c, out string? mapped))
        {
            if (mapped != word) return false;
        }
        else
        {
            if (usedWords.Contains(word)) return false;  // Word already mapped
            map[c] = word;
            usedWords.Add(word);
        }
    }

    return true;
}
```

**Time Complexity:** O(n)  
**Space Complexity:** O(k) where k = unique patterns/words

</details>

---

## Big-O Quick Reference

| Problem              | Time     | Space       |
| -------------------- | -------- | ----------- |
| Contains Duplicate   | O(n)     | O(n)        |
| Array Intersection   | O(n+m)   | O(min(n,m)) |
| Happy Number         | O(log n) | O(log n)    |
| Longest Consecutive  | O(n)     | O(n)        |
| Single Number        | O(n)     | O(n)/O(1)   |
| Trade Reconciliation | O(n+m)   | O(n+m)      |
| Unique Emails        | O(n·k)   | O(n·k)      |
| Find All Duplicates  | O(n)     | O(n)/O(1)   |
| Jewels and Stones    | O(j+s)   | O(j)        |
| Word Pattern         | O(n)     | O(k)        |

---

_Practice makes perfect! Try solving these problems without looking at solutions first._
