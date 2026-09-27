# Dictionary<K,V> Internals – Deep Dive for Interviews

> **This is THE most asked collection interview topic in C#/.NET**  
> Understanding hash tables separates senior developers from juniors

---

## Table of Contents

1. [What is a Hash Table?](#1-what-is-a-hash-table)
2. [Dictionary Internal Structure](#2-dictionary-internal-structure)
3. [How GetHashCode() Works](#3-how-gethashcode-works)
4. [Step-by-Step: Adding an Element](#4-step-by-step-adding-an-element)
5. [Step-by-Step: Retrieving an Element](#5-step-by-step-retrieving-an-element)
6. [Collision Handling](#6-collision-handling)
7. [Resizing (Rehashing)](#7-resizing-rehashing)
8. [The GetHashCode/Equals Contract](#8-the-gethashcodeequals-contract)
9. [Why Keys Should Be Immutable](#9-why-keys-should-be-immutable)
10. [Big-O Analysis](#10-big-o-analysis)
11. [Interview Questions with Answers](#11-interview-questions-with-answers)

---

## 1. What is a Hash Table?

A **hash table** is a data structure that maps keys to values using a **hash function**.

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                         HASH TABLE CONCEPT                                    ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║   WITHOUT HASH TABLE (Linear Search):                                         ║
║   ───────────────────────────────────                                         ║
║   To find "EUR/USD" rate, scan ALL entries → O(n)                            ║
║                                                                               ║
║   List: [("USD/JPY", 149), ("GBP/USD", 1.26), ("EUR/USD", 1.08), ...]        ║
║          ↓              ↓                ↓                                    ║
║          Check          Check            Found! (after 3 comparisons)         ║
║                                                                               ║
║   ═══════════════════════════════════════════════════════════════════════    ║
║                                                                               ║
║   WITH HASH TABLE (Direct Access):                                            ║
║   ────────────────────────────────                                            ║
║   Compute hash, go DIRECTLY to location → O(1)                               ║
║                                                                               ║
║   "EUR/USD".GetHashCode() → 12345                                            ║
║   12345 % 8 (bucket count) → 5                                               ║
║   Go directly to bucket 5 → Found in 1 step!                                 ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

### Real-World Analogy

Think of a **library**:

- **Without hash table**: Search every book to find "C# Programming"
- **With hash table**: Go to Section "C" → Programming shelf → Find immediately

---

## 2. Dictionary Internal Structure

Dictionary<K,V> uses **TWO internal arrays**:

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                    DICTIONARY INTERNAL ARRAYS                                 ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║   ARRAY 1: buckets[] (int array)                                             ║
║   ───────────────────────────────                                             ║
║   Maps hash code to first entry index in that bucket                          ║
║                                                                               ║
║   Index:    0      1      2      3      4      5      6      7               ║
║           ┌──────┬──────┬──────┬──────┬──────┬──────┬──────┬──────┐          ║
║   Value:  │  -1  │   2  │  -1  │   0  │  -1  │   1  │  -1  │  -1  │          ║
║           └──────┴──────┴──────┴──────┴──────┴──────┴──────┴──────┘          ║
║             ↑              ↑      ↑      ↑                                    ║
║           empty         entry   entry  entry                                  ║
║                          #2      #0     #1                                    ║
║                                                                               ║
║   ═══════════════════════════════════════════════════════════════════════    ║
║                                                                               ║
║   ARRAY 2: entries[] (Entry struct array)                                    ║
║   ───────────────────────────────────────                                     ║
║   Stores actual key-value pairs + metadata                                    ║
║                                                                               ║
║   struct Entry                                                                ║
║   {                                                                           ║
║       int hashCode;      // Cached hash code of key                          ║
║       int next;          // Index of next entry in same bucket (-1 if none)  ║
║       TKey key;          // The key                                          ║
║       TValue value;      // The value                                        ║
║   }                                                                           ║
║                                                                               ║
║   Index:      0                    1                    2                     ║
║           ┌─────────────────┬─────────────────┬─────────────────┐            ║
║   Entry:  │ hash: 12345     │ hash: 67890     │ hash: 11111     │            ║
║           │ next: -1        │ next: -1        │ next: -1        │            ║
║           │ key: "USD/JPY"  │ key: "EUR/USD"  │ key: "GBP/USD"  │            ║
║           │ value: 149.50   │ value: 1.0850   │ value: 1.2650   │            ║
║           └─────────────────┴─────────────────┴─────────────────┘            ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

╔═══════════════════════════════════════════════════════════════════════════════╗
║ YOUR MENTAL MODEL vs ACTUAL .NET IMPLEMENTATION ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║ ║
║ YOUR THINKING (Multiple Arrays): ║
║ ───────────────────────────────── ║
║ buckets[0] → [ Entry, Entry, Entry ] ← Array for bucket 0 ║
║ buckets[1] → [ Entry ] ← Array for bucket 1 ║
║ buckets[2] → [ ] ← Empty array ║
║ buckets[3] → [ Entry, Entry ] ← Array for bucket 3 ║
║ ║
║ Problem: Many small arrays = memory fragmentation + allocation overhead ║
║ ║
║ ═══════════════════════════════════════════════════════════════════════ ║
║ ║
║ ACTUAL .NET (One Array + Linking): ║
║ ────────────────────────────────── ║
║ ║
║ buckets[] (just indices): ║
║ ┌─────┬─────┬─────┬─────┐ ║
║ │ 2 │ 0 │ -1 │ 3 │ ← Index of FIRST entry in each bucket ║
║ └─────┴─────┴─────┴─────┘ ║
║ ↓ ↓ ↓ ║
║ │ │ │ ║
║ entries[] (ONE flat array for ALL entries): ║
║ ┌─────────────┬─────────────┬─────────────┬─────────────┬─────────────┐ ║
║ │ Index: 0 │ Index: 1 │ Index: 2 │ Index: 3 │ Index: 4 │ ║
║ │ key: "B" │ key: "D" │ key: "A" │ key: "E" │ key: "C" │ ║
║ │ value: 20 │ value: 40 │ value: 10 │ value: 50 │ value: 30 │ ║
║ │ next: -1 │ next: 4 │ next: 1 │ next: -1 │ next: -1 │ ║
║ └─────────────┴─────────────┴─────────────┴─────────────┴─────────────┘ ║
║ ↑ ↑ ║
║ │ │ ║
║ └─────────────┘ ║
║ "next" links entries in same bucket! ║
║ ║
╚═══════════════════════════════════════════════════════════════════════════════╝

### Actual .NET Source Code (Simplified)

```csharp
public class Dictionary<TKey, TValue>
{
    private int[] _buckets;        // Index into entries for each bucket
    private Entry[] _entries;      // Actual storage
    private int _count;            // Number of entries
    private int _freeList;         // Head of free entry list (for reuse after removal)
    private int _freeCount;        // Number of free entries

    private struct Entry
    {
        public uint hashCode;      // Lower 31 bits of hash code, 0 if unused
        public int next;           // Index of next entry, -1 if last
        public TKey key;           // Key of entry
        public TValue value;       // Value of entry
    }
}
```

---

## 3. How GetHashCode() Works

`GetHashCode()` converts any object into a **32-bit integer**. This number determines which bucket the entry goes into.

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                         HASH CODE GENERATION                                  ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║   INPUT: "EUR/USD"                                                            ║
║                                                                               ║
║   String.GetHashCode() algorithm (simplified):                                ║
║   ────────────────────────────────────────────                                ║
║                                                                               ║
║   'E' = 69   'U' = 85   'R' = 82   '/' = 47   ...                            ║
║                                                                               ║
║   hash = seed                                                                 ║
║   for each char c:                                                            ║
║       hash = ((hash << 5) + hash) ^ c                                        ║
║                                                                               ║
║   OUTPUT: 1234567890 (32-bit integer)                                        ║
║                                                                               ║
║   ═══════════════════════════════════════════════════════════════════════    ║
║                                                                               ║
║   BUCKET CALCULATION:                                                         ║
║   ───────────────────                                                         ║
║                                                                               ║
║   bucketIndex = hashCode % buckets.Length                                    ║
║                                                                               ║
║   Example:                                                                    ║
║   hashCode = 1234567890                                                       ║
║   buckets.Length = 8                                                          ║
║   bucketIndex = 1234567890 % 8 = 2                                           ║
║                                                                               ║
║   So "EUR/USD" goes into bucket 2                                            ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

### Hash Codes for Different Types

```csharp
// String - based on character sequence
"EUR/USD".GetHashCode();  // -1234567890 (varies per run in .NET Core!)

// int - returns itself
42.GetHashCode();  // 42

// Custom object - based on reference (default) or override
var trade = new Trade();
trade.GetHashCode();  // Memory address based (default)

// Value tuple - combines all field hashes
(1, "EUR/USD").GetHashCode();  // Combined hash

// Record - combines all property hashes automatically
record Trade(int Id, string Pair);
new Trade(1, "EUR/USD").GetHashCode();  // Combines Id and Pair hashes
```

### ⚠️ Important: Hash Code Randomization

```csharp
// In .NET Core/.NET 5+, string hash codes are RANDOMIZED per process!
// This is for security (hash DoS prevention)

// Run 1: "hello".GetHashCode() → 123456789
// Run 2: "hello".GetHashCode() → 987654321  (DIFFERENT!)

// This means: NEVER persist hash codes to files/databases!
```

---

## 4. Step-by-Step: Adding an Element

Let's trace through what happens when you execute:

```csharp
var dict = new Dictionary<string, decimal>();
dict["EUR/USD"] = 1.0850m;
```

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║              STEP-BY-STEP: dict["EUR/USD"] = 1.0850m                         ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║   STEP 1: Compute Hash Code                                                   ║
║   ─────────────────────────                                                   ║
║   hashCode = "EUR/USD".GetHashCode()                                         ║
║   hashCode = 1234567890  (example value)                                     ║
║                                                                               ║
║   STEP 2: Compute Bucket Index                                                ║
║   ────────────────────────────                                                ║
║   bucketIndex = hashCode % buckets.Length                                    ║
║   bucketIndex = 1234567890 % 8 = 2                                           ║
║                                                                               ║
║   STEP 3: Check for Existing Key                                              ║
║   ──────────────────────────────                                              ║
║   Start at buckets[2]                                                         ║
║   If bucket is empty (-1): proceed to add                                    ║
║   If bucket has entries: walk chain, use Equals() to check each key          ║
║                                                                               ║
║   STEP 4: Add Entry                                                           ║
║   ─────────────────                                                           ║
║   Create new Entry:                                                           ║
║   {                                                                           ║
║       hashCode: 1234567890,                                                  ║
║       next: -1,              // End of chain                                 ║
║       key: "EUR/USD",                                                        ║
║       value: 1.0850                                                          ║
║   }                                                                           ║
║                                                                               ║
║   Store at entries[count] (next available slot)                              ║
║   Update buckets[2] to point to this entry index                             ║
║   Increment count                                                             ║
║                                                                               ║
║   BEFORE:                              AFTER:                                 ║
║   buckets: [-1,-1,-1,-1,-1,-1,-1,-1]  buckets: [-1,-1, 0,-1,-1,-1,-1,-1]    ║
║   entries: []                          entries: [Entry for EUR/USD]          ║
║   count: 0                             count: 1                               ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

### Actual .NET Code (Simplified)

```csharp
private bool TryInsert(TKey key, TValue value, InsertionBehavior behavior)
{
    // Step 1: Get hash code
    uint hashCode = (uint)key.GetHashCode();

    // Step 2: Find bucket
    ref int bucket = ref GetBucket(hashCode);

    // Step 3: Walk existing entries in bucket
    int i = bucket - 1;  // buckets store 1-based index
    while (i >= 0)
    {
        ref Entry entry = ref _entries[i];

        // Check if key already exists
        if (entry.hashCode == hashCode && EqualityComparer<TKey>.Default.Equals(entry.key, key))
        {
            // Key exists - update or throw based on behavior
            if (behavior == InsertionBehavior.OverwriteExisting)
            {
                entry.value = value;
                return true;
            }
            // ThrowOnExisting would throw here
            return false;
        }

        i = entry.next;  // Move to next in chain
    }

    // Step 4: Key not found - add new entry
    int index = _count;
    _count++;

    ref Entry newEntry = ref _entries[index];
    newEntry.hashCode = hashCode;
    newEntry.next = bucket - 1;  // Point to previous head of chain
    newEntry.key = key;
    newEntry.value = value;

    bucket = index + 1;  // Update bucket to point to new entry

    return true;
}
```

---

## 5. Step-by-Step: Retrieving an Element

Let's trace through:

```csharp
decimal rate = dict["EUR/USD"];
```

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║              STEP-BY-STEP: var rate = dict["EUR/USD"]                        ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║   STEP 1: Compute Hash Code (SAME as when added!)                            ║
║   ───────────────────────────────────────────────                             ║
║   hashCode = "EUR/USD".GetHashCode()                                         ║
║   hashCode = 1234567890                                                       ║
║                                                                               ║
║   STEP 2: Compute Bucket Index                                                ║
║   ────────────────────────────                                                ║
║   bucketIndex = 1234567890 % 8 = 2                                           ║
║                                                                               ║
║   STEP 3: Get First Entry in Bucket                                           ║
║   ─────────────────────────────────                                           ║
║   firstEntryIndex = buckets[2] = 0                                           ║
║                                                                               ║
║   STEP 4: Walk Chain Until Key Found                                          ║
║   ──────────────────────────────────                                          ║
║   entry = entries[0]                                                          ║
║                                                                               ║
║   Compare hash codes first (FAST):                                            ║
║   entry.hashCode == 1234567890 ✓                                             ║
║                                                                               ║
║   If hash matches, compare keys with Equals():                                ║
║   entry.key.Equals("EUR/USD") → "EUR/USD".Equals("EUR/USD") → true ✓         ║
║                                                                               ║
║   STEP 5: Return Value                                                        ║
║   ────────────────────                                                        ║
║   return entry.value → 1.0850m                                               ║
║                                                                               ║
║   Total operations: O(1) on average!                                          ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝

```

MORE DETAILING
╔═══════════════════════════════════════════════════════════════════════════════╗
║ WHY COMPARE HASH CODE IN THE CHAIN? ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║ ║
║ SCENARIO: Bucket 2 has 3 entries (collision) ║
║ ║
║ buckets[2] = 5 (first entry is at index 5) ║
║ ║
║ entries[5] → entries[8] → entries[12] → -1 (end) ║
║ ↓ ↓ ↓ ║
║ "EUR/USD" "AUD/NZD" "CHF/JPY" ║
║ hash: 1234 hash: 5678 hash: 9012 ║
║ ║
║ ALL THREE have different hash codes, but same BUCKET INDEX! ║
║ Why? Because: ║
║ 1234 % 8 = 2 ║
║ 5678 % 8 = 2 ║
║ 9012 % 8 = 2 ║
║ ║
╚═══════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════╗
║ SEARCHING: dict["EUR/USD"] ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║ ║
║ Step 1: hashCode = "EUR/USD".GetHashCode() = 1234 ║
║ Step 2: bucketIndex = 1234 % 8 = 2 ║
║ Step 3: firstEntry = buckets[2] = 5 → Go to entries[5] ║
║ ║
║ Step 4: TRAVERSE THE CHAIN ║
║ ────────────────────────────── ║
║ ║
║ ┌─────────────────────────────────────────────────────────────────────┐ ║
║ │ entries[5]: │ ║
║ │ stored hashCode: 1234 │ ║
║ │ key: "EUR/USD" │ ║
║ │ │ ║
║ │ Compare: searchHash (1234) == storedHash (1234)? ✓ YES │ ║
║ │ │ ║
║ │ Hash matches! Now call Equals(): │ ║
║ │ "EUR/USD".Equals("EUR/USD") → true ✓ │ ║
║ │ │ ║
║ │ FOUND! Return value. │ ║
║ └─────────────────────────────────────────────────────────────────────┘ ║
║ ║
╚═══════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════╗
║ SEARCHING: dict["CHF/JPY"] ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║ ║
║ Step 1: hashCode = "CHF/JPY".GetHashCode() = 9012 ║
║ Step 2: bucketIndex = 9012 % 8 = 2 ║
║ Step 3: firstEntry = buckets[2] = 5 → Go to entries[5] ║
║ ║
║ Step 4: TRAVERSE THE CHAIN ║
║ ────────────────────────────── ║
║ ║
║ ┌─ entries[5]: ───────────────────────────────────────────────────────┐ ║
║ │ stored hashCode: 1234 │ ║
║ │ Compare: searchHash (9012) == storedHash (1234)? ✗ NO │ ║
║ │ │ ║
║ │ Hash DOESN'T match → SKIP Equals() call! (saves time) │ ║
║ │ Go to next: entries[5].next = 8 │ ║
║ └─────────────────────────────────────────────────────────────────────┘ ║
║ ↓ ║
║ ┌─ entries[8]: ───────────────────────────────────────────────────────┐ ║
║ │ stored hashCode: 5678 │ ║
║ │ Compare: searchHash (9012) == storedHash (5678)? ✗ NO │ ║
║ │ │ ║
║ │ Hash DOESN'T match → SKIP Equals() call! │ ║
║ │ Go to next: entries[8].next = 12 │ ║
║ └─────────────────────────────────────────────────────────────────────┘ ║
║ ↓ ║
║ ┌─ entries[12]: ──────────────────────────────────────────────────────┐ ║
║ │ stored hashCode: 9012 │ ║
║ │ Compare: searchHash (9012) == storedHash (9012)? ✓ YES │ ║
║ │ │ ║
║ │ Hash matches! Now call Equals(): │ ║
║ │ "CHF/JPY".Equals("CHF/JPY") → true ✓ │ ║
║ │ │ ║
║ │ FOUND! Return value. │ ║
║ └─────────────────────────────────────────────────────────────────────┘ ║
║ ║
╚═══════════════════════════════════════════════════════════════════════════════╝

### Why Compare Hash Code BEFORE Equals()?

```csharp
// Hash code comparison is FAST (just comparing two integers)
if (entry.hashCode == hashCode)  // O(1) - just integer comparison
{
    // Equals() can be SLOW (comparing strings character by character)
    if (entry.key.Equals(key))  // O(n) for strings
    {
        return entry.value;
    }
}

// If hash codes are different, keys are DEFINITELY different
// No need to call expensive Equals()!
```

---

## 6. Collision Handling

A **collision** occurs when two different keys hash to the same bucket.

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                         COLLISION HANDLING                                    ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║   COLLISION EXAMPLE:                                                          ║
║   ──────────────────                                                          ║
║   "EUR/USD".GetHashCode() % 8 = 2                                            ║
║   "AUD/NZD".GetHashCode() % 8 = 2  ← Same bucket!                            ║
║                                                                               ║
║   This is NORMAL and EXPECTED!                                                ║
║   With 8 buckets and 100 entries, collisions are guaranteed.                 ║
║                                                                               ║
║   ═══════════════════════════════════════════════════════════════════════    ║
║                                                                               ║
║   .NET Dictionary uses SEPARATE CHAINING:                                     ║
║   ──────────────────────────────────────                                      ║
║                                                                               ║
║   buckets[2] → Entry[0] → Entry[3] → Entry[7] → -1                          ║
║                  ↓          ↓          ↓                                      ║
║              "EUR/USD"  "AUD/NZD"  "CHF/JPY"                                  ║
║                                                                               ║
║   Each Entry has a 'next' field pointing to next entry in same bucket        ║
║                                                                               ║
║   MEMORY LAYOUT:                                                              ║
║   ┌─────────────────────────────────────────────────────────────────┐        ║
║   │ entries[0]     │ entries[1]     │ entries[2]     │ entries[3]   │        ║
║   │ key: EUR/USD   │ key: GBP/USD   │ key: USD/JPY   │ key: AUD/NZD │        ║
║   │ next: 3 ───────┼────────────────┼────────────────┼→             │        ║
║   └─────────────────────────────────────────────────────────────────┘        ║
║                     │                                 ↓                       ║
║                     └─────────────────────────────→ entries[3]               ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

### Other Collision Resolution Strategies (Interview Knowledge)

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                    COLLISION RESOLUTION STRATEGIES                            ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║   1. SEPARATE CHAINING (Used by .NET Dictionary)                              ║
║   ──────────────────────────────────────────────                              ║
║   - Each bucket contains a linked list of entries                            ║
║   - Pros: Simple, no limit on entries per bucket                             ║
║   - Cons: Extra memory for 'next' pointers, cache unfriendly                 ║
║                                                                               ║
║   Bucket[2]: → [EUR/USD] → [AUD/NZD] → [CHF/JPY] → null                      ║
║                                                                               ║
║   ─────────────────────────────────────────────────────────────────────────   ║
║                                                                               ║
║   2. OPEN ADDRESSING - LINEAR PROBING                                         ║
║   ──────────────────────────────────                                          ║
║   - If bucket full, try next bucket (bucket + 1)                             ║
║   - Pros: Cache friendly, no extra pointers                                  ║
║   - Cons: Clustering problem, deletion is complex                            ║
║                                                                               ║
║   Bucket: [    ] [EUR/USD] [AUD/NZD] [CHF/JPY] [    ] ...                    ║
║             0       1          2         3       4                            ║
║           hash→1  placed    collision  collision                              ║
║                             try 2      try 3                                  ║
║                                                                               ║
║   ─────────────────────────────────────────────────────────────────────────   ║
║                                                                               ║
║   3. OPEN ADDRESSING - QUADRATIC PROBING                                      ║
║   ──────────────────────────────────────                                      ║
║   - If bucket full, try bucket + 1², bucket + 2², bucket + 3²...             ║
║   - Reduces clustering compared to linear probing                            ║
║                                                                               ║
║   ─────────────────────────────────────────────────────────────────────────   ║
║                                                                               ║
║   4. DOUBLE HASHING                                                           ║
║   ─────────────────                                                           ║
║   - Use second hash function to determine probe step                         ║
║   - newIndex = (hash1 + i * hash2) % size                                    ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

╔═══════════════════════════════════════════════════════════════════════════════╗
║ OPEN ADDRESSING: RUNNING OUT OF BUCKETS ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║ ║
║ PROBLEM: No chaining = each bucket holds EXACTLY ONE entry ║
║ ───────────────────────────────────────────────────────── ║
║ ║
║ Table size: 8 buckets ║
║ After 6 inserts (75% full): ║
║ ║
║ [EUR/USD] [GBP/USD] [USD/JPY] [AUD/NZD] [ ] [CHF/JPY] [NZD/USD] [ ] ║
║ 0 1 2 3 4 5 6 7 ║
║ ║
║ Insert "USD/CHF" → hash % 8 = 2 (OCCUPIED!) ║
║ Linear probe: try 3 (OCCUPIED!), try 4 (empty!) → Place at 4 ║
║ ║
║ [EUR/USD] [GBP/USD] [USD/JPY] [AUD/NZD] [USD/CHF] [CHF/JPY] [NZD/USD] [ ] ║
║ 0 1 2 3 4 5 6 7 ║
║ ║
║ Now 7/8 = 87.5% full... Insert one more and... ║
║ ║
║ [EUR/USD] [GBP/USD] [USD/JPY] [AUD/NZD] [USD/CHF] [CHF/JPY] [NZD/USD] [CAD]║
║ 0 1 2 3 4 5 6 7 ║
║ ║
║ TABLE IS 100% FULL! Next insert → MUST RESIZE! ║
║ ║
╚═══════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════╗
║ LOAD FACTOR THRESHOLD ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║ ║
║ Open Addressing tables resize at a LOWER threshold than chaining! ║
║ ║
║ WHY? ║
║ ───── ║
║ As table fills up, probe sequences get LONGER and LONGER ║
║ ║
║ Load Factor Avg Probes (Linear) Performance ║
║ ─────────── ────────────────── ─────────────── ║
║ 50% 1.5 Good ║
║ 70% 2.5 Acceptable ║
║ 80% 5.0 Degrading ║
║ 90% 50.0 TERRIBLE! ║
║ 95% 200.0 Basically O(n) ║
║ ║
║ TYPICAL THRESHOLD: Resize at 50-75% full ║
║ ║
║ Example: Python dict resizes at 66% (2/3) ║
║ ║
╚═══════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════╗
║ RESIZING IN OPEN ADDRESSING ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║ ║
║ BEFORE (8 buckets, 6 entries = 75% full → RESIZE!) ║
║ ║
║ [EUR/USD] [GBP/USD] [USD/JPY] [AUD/NZD] [ ] [CHF/JPY] [NZD/USD] [ ] ║
║ 0 1 2 3 4 5 6 7 ║
║ ║
║ STEP 1: Create new larger table (typically 2x size) ║
║ ───────────────────────────────────────────────────── ║
║ New table: 16 buckets (all empty) ║
║ ║
║ [ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ] ... ║
║ 0 1 2 3 4 5 6 7 8 9 10 11 12 13 ║
║ ║
║ STEP 2: REHASH every entry (recompute position) ║
║ ───────────────────────────────────────────────── ║
║ ║
║ "EUR/USD": hash % 16 = 10 (was hash % 8 = 2) ║
║ "GBP/USD": hash % 16 = 1 (was hash % 8 = 1) ║
║ "USD/JPY": hash % 16 = 6 (was hash % 8 = 2) ║
║ ... etc ║
║ ║
║ AFTER (16 buckets, 6 entries = 37.5% full - lots of room!) ║
║ ║
║ [ ] [GBP] [ ] [ ] [AUD] [ ] [JPY] [ ] [ ] [ ] [EUR] [CHF] [ ] [NZD]║
║ 0 1 2 3 4 5 6 7 8 9 10 11 12 13 ║
║ ║
║ Entries are now SPREAD OUT → fewer collisions → faster lookups! ║
║ ║
╚═══════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════╗
║ CHAINING vs OPEN ADDRESSING: WHEN FULL ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║ ║
║ SEPARATE CHAINING OPEN ADDRESSING ║
║ (.NET Dictionary) (Python dict) ║
║ ───────────────────────────────────────────────────────────────────────── ║
║ ║
║ Can exceed 100%? YES! Load factor can NO! Max 100% ║
║ be > 1.0 (more entries (one entry per bucket) ║
║ than buckets) ║
║ ║
║ When to resize? When entries array full When ~50-75% full ║
║ (more flexible) (must resize earlier) ║
║ ║
║ Performance at Degrades gracefully Degrades RAPIDLY ║
║ high load (longer chains) (very long probes) ║
║ ║
║ Memory Extra 'next' pointer No extra pointers ║
║ per entry More compact ║
║ ║
║ Cache Worse (pointer chasing) Better (contiguous) ║
║ friendliness ║
║ ║
║ Deletion Simple (remove from Complex (tombstones ║
║ chain) or rehash) ║
║ ║
╚═══════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════╗
║ DELETION IS TRICKY IN OPEN ADDRESSING! ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║ ║
║ PROBLEM: Can't just empty a slot! ║
║ ─────────────────────────────────── ║
║ ║
║ Initial state: ║
║ [ ] [A] [B] [C] [ ] [ ] ← B and C were placed by probing ║
║ 0 1 2 3 4 5 (A at 1, B→probe→2, C→probe→3) ║
║ ║
║ Delete B (just clear slot 2): ║
║ [ ] [A] [ ] [C] [ ] [ ] ║
║ 0 1 2 3 4 5 ║
║ ║
║ Now search for C: ║
║ hash(C) % 6 = 1 → Check slot 1 → Found A, not C ║
║ Probe to slot 2 → EMPTY! → "C not found" ❌ WRONG! ║
║ ║
║ C is at slot 3, but we stopped at empty slot 2! ║
║ ║
║ ═══════════════════════════════════════════════════════════════════════ ║
║ ║
║ SOLUTION 1: TOMBSTONES ║
║ ────────────────────── ║
║ Mark deleted slots as "DELETED" (not empty) ║
║ ║
║ [ ] [A] [DEL] [C] [ ] [ ] ║
║ 0 1 2 3 4 5 ║
║ ║
║ Search continues past DELETED markers ║
║ Problem: Tombstones accumulate → table fills with garbage ║
║ ║
║ SOLUTION 2: REHASH ON DELETE ║
║ ──────────────────────────── ║
║ After deleting, rehash all entries that might be affected ║
║ More complex, but no tombstone accumulation ║
║ ║
╚═══════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════╗
║ WHY .NET DICTIONARY USES CHAINING ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║ ║
║ 1. SIMPLER DELETION ║
║ Just remove entry from chain - no tombstones needed ║
║ ║
║ 2. MORE FLEXIBLE LOAD FACTOR ║
║ Can handle load factor > 1.0 gracefully ║
║ Open addressing MUST resize before 100% ║
║ ║
║ 3. PREDICTABLE PERFORMANCE ║
║ Performance degrades linearly with chain length ║
║ Open addressing can have SUDDEN performance cliffs ║
║ ║
║ 4. REFERENCE TYPE FRIENDLY ║
║ .NET has many reference types - extra pointer overhead is acceptable ║
║ ║
║ ═══════════════════════════════════════════════════════════════════════ ║
║ ║
║ PYTHON uses Open Addressing because: ║
║ - Designed for small dicts (most are < 10 entries) ║
║ - Cache locality matters for tight loops ║
║ - Compact memory for embedded/small systems ║
║ ║
╚═══════════════════════════════════════════════════════════════════════════════╝

### Pathological Case: All Keys in One Bucket

```csharp
// WORST CASE: All keys hash to same bucket
// Lookup becomes O(n) - linear search through chain!

// This is why GetHashCode() quality matters
// A bad hash function that returns same value for all keys:

public override int GetHashCode() => 42;  // TERRIBLE! All entries in bucket 42 % size

// Dictionary lookup degrades from O(1) to O(n)
```

---

## 7. Resizing (Rehashing)

When Dictionary gets too full, it **resizes** to maintain O(1) performance.

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                         RESIZING / REHASHING                                  ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║   LOAD FACTOR:                                                                ║
║   ────────────                                                                ║
║   loadFactor = count / buckets.Length                                        ║
║                                                                               ║
║   .NET resizes when entries array is full (not based on load factor)         ║
║   New size = next prime number greater than 2 * current size                 ║
║                                                                               ║
║   SIZE PROGRESSION:                                                           ║
║   3 → 7 → 17 → 37 → 89 → 197 → 431 → 919 → 1931 → ...                       ║
║   (Prime numbers reduce collision clustering)                                 ║
║                                                                               ║
║   ═══════════════════════════════════════════════════════════════════════    ║
║                                                                               ║
║   REHASHING PROCESS:                                                          ║
║   ──────────────────                                                          ║
║                                                                               ║
║   BEFORE RESIZE (4 buckets, 4 entries - FULL):                               ║
║                                                                               ║
║   buckets:  [0] [1] [2] [3]                                                  ║
║              ↓   ↓   ↓   ↓                                                   ║
║   entries:   A   B   C   D                                                   ║
║                                                                               ║
║   AFTER RESIZE (8 buckets):                                                  ║
║                                                                               ║
║   Step 1: Allocate new larger arrays                                         ║
║   Step 2: For EACH entry, recompute bucket index with new size               ║
║           bucketIndex = hashCode % 8  (was % 4)                              ║
║   Step 3: Reinsert into new buckets                                          ║
║                                                                               ║
║   buckets:  [0] [1] [2] [3] [4] [5] [6] [7]                                 ║
║              ↓       ↓       ↓   ↓                                           ║
║   entries:   A       C       B   D                                           ║
║                                                                               ║
║   Entries redistributed based on new bucket count!                           ║
║                                                                               ║
║   ═══════════════════════════════════════════════════════════════════════    ║
║                                                                               ║
║   PERFORMANCE IMPACT:                                                         ║
║   ───────────────────                                                         ║
║   - Single resize is O(n) - must rehash all entries                         ║
║   - But amortized over all Add operations = O(1)                             ║
║   - Same logic as List<T> resize                                             ║
║                                                                               ║
║   TIP: Pre-size dictionary if you know count!                                ║
║   var dict = new Dictionary<string, int>(1000);  // Avoids resizing         ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

### Why Prime Number Sizes?

```csharp
// Prime numbers distribute entries more evenly

// With size 8 (not prime):
// hashCode % 8 only uses last 3 bits of hash code
// If hash codes share patterns in low bits → clustering

// With size 7 (prime):
// hashCode % 7 uses more bits of hash code
// Better distribution even with poor hash functions
```

---

## 8. The GetHashCode/Equals Contract

**THE MOST IMPORTANT INTERVIEW TOPIC!**

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                    THE CONTRACT (MUST FOLLOW!)                                ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║   RULE 1: CONSISTENCY                                                         ║
║   ───────────────────                                                         ║
║   If a.Equals(b) returns TRUE → a.GetHashCode() MUST == b.GetHashCode()      ║
║                                                                               ║
║   WHY: If equal objects have different hash codes, they go to different      ║
║   buckets. You add with key A, search with equal key B, but B goes to        ║
║   different bucket → NOT FOUND! Dictionary is BROKEN.                        ║
║                                                                               ║
║   ─────────────────────────────────────────────────────────────────────────   ║
║                                                                               ║
║   RULE 2: REVERSE NOT REQUIRED                                                ║
║   ────────────────────────────                                                ║
║   If a.GetHashCode() == b.GetHashCode() → a.Equals(b) can be TRUE or FALSE   ║
║                                                                               ║
║   WHY: Hash collisions are normal. Multiple different keys can have same     ║
║   hash code. That's why we check Equals() after finding the bucket.          ║
║                                                                               ║
║   ─────────────────────────────────────────────────────────────────────────   ║
║                                                                               ║
║   RULE 3: IMMUTABILITY                                                        ║
║   ────────────────────                                                        ║
║   GetHashCode() MUST return same value for object's lifetime                 ║
║   (at least while object is in a hash-based collection)                      ║
║                                                                               ║
║   WHY: If hash code changes, the object is "lost" in the wrong bucket.       ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

### ❌ BROKEN Implementation

```csharp
// This BREAKS the contract!
public class Trade
{
    public string Id { get; set; }
    public decimal Amount { get; set; }

    // WRONG: Only comparing Id
    public override bool Equals(object? obj)
    {
        return obj is Trade t && Id == t.Id;
    }

    // WRONG: Using Amount in hash code but not in Equals!
    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Amount);  // VIOLATION!
    }
}

// What goes wrong:
var dict = new Dictionary<Trade, string>();
var trade1 = new Trade { Id = "T1", Amount = 100 };
dict[trade1] = "First trade";

var trade2 = new Trade { Id = "T1", Amount = 200 };  // Different amount
// trade1.Equals(trade2) = true (same Id)
// trade1.GetHashCode() != trade2.GetHashCode() (different Amount)
// CONTRACT VIOLATED!

dict[trade2] = "Updated";  // Goes to DIFFERENT bucket!
// Now dict has TWO entries for "equal" keys!
```

### ✅ CORRECT Implementation

```csharp
public class Trade
{
    public string Id { get; }  // Immutable!
    public decimal Amount { get; set; }

    public Trade(string id) => Id = id;

    // Equals uses Id
    public override bool Equals(object? obj)
    {
        return obj is Trade t && Id == t.Id;
    }

    // GetHashCode uses SAME fields as Equals
    public override int GetHashCode()
    {
        return Id.GetHashCode();  // Same field as Equals!
    }
}

// OR just use a record (auto-implements correctly):
public record Trade(string Id, decimal Amount);
```

### Quick Formula for GetHashCode()

```csharp
// Use HashCode.Combine for multiple fields:
public override int GetHashCode()
{
    return HashCode.Combine(Field1, Field2, Field3);
}

// For collections:
public override int GetHashCode()
{
    var hash = new HashCode();
    foreach (var item in Items)
    {
        hash.Add(item);
    }
    return hash.ToHashCode();
}
```

---

## 9. Why Keys Should Be Immutable

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                    THE LOST KEY PROBLEM                                       ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║   SCENARIO: Mutable key modified after adding to Dictionary                   ║
║                                                                               ║
║   var key = new Person { Name = "Alice" };                                   ║
║   dict[key] = "Developer";                                                    ║
║                                                                               ║
║   // Hash code at insertion: "Alice".GetHashCode() % 8 = 3                   ║
║   // Entry stored in bucket 3                                                 ║
║                                                                               ║
║   key.Name = "Bob";  // MUTATING THE KEY!                                    ║
║                                                                               ║
║   // Now key.GetHashCode() = "Bob".GetHashCode() % 8 = 7                     ║
║   // Searching in bucket 7, but entry is in bucket 3!                        ║
║                                                                               ║
║   var result = dict[key];  // KeyNotFoundException!                          ║
║   // The entry is STILL THERE, just unreachable!                             ║
║                                                                               ║
║   ═══════════════════════════════════════════════════════════════════════    ║
║                                                                               ║
║   VISUAL:                                                                     ║
║                                                                               ║
║   BEFORE MUTATION:           AFTER MUTATION:                                  ║
║   key="Alice" hash=3        key="Bob" hash=7                                 ║
║                                                                               ║
║   Bucket 3: [Alice→Dev]     Bucket 3: [Alice→Dev] ← Entry still here!       ║
║   Bucket 7: [empty]         Bucket 7: [empty]     ← But we're looking here! ║
║                                                                               ║
║   SOLUTIONS:                                                                  ║
║   1. Use immutable types as keys (string, int, records)                      ║
║   2. Use defensive copies                                                     ║
║   3. Only use immutable properties in GetHashCode()                          ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

### Best Practice: Use Records or Immutable Classes

```csharp
// ✅ Record - immutable by default, auto-implements GetHashCode/Equals
public record CurrencyPair(string Base, string Quote);

// ✅ Immutable class
public class CurrencyPair
{
    public string Base { get; }    // No setter = immutable
    public string Quote { get; }

    public CurrencyPair(string baseCcy, string quote)
    {
        Base = baseCcy;
        Quote = quote;
    }

    // Override Equals and GetHashCode...
}

// ✅ Just use strings/value types
Dictionary<string, decimal> rates;  // string is immutable
Dictionary<int, Trade> tradesById;  // int is immutable
```

---

## 10. Big-O Analysis

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                    DICTIONARY<K,V> TIME COMPLEXITY                            ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║   Operation              Average Case    Worst Case     Notes                 ║
║   ─────────────────────  ─────────────   ───────────    ─────────────────    ║
║   Add                    O(1)            O(n)           Resize or collision   ║
║   TryAdd                 O(1)            O(n)           Same as Add           ║
║   Remove                 O(1)            O(n)           If collision chain    ║
║   Get by key (indexer)   O(1)            O(n)           If collision chain    ║
║   TryGetValue            O(1)            O(n)           Same as Get           ║
║   ContainsKey            O(1)            O(n)           Same as Get           ║
║   ContainsValue          O(n)            O(n)           Must scan all values! ║
║   Clear                  O(n)            O(n)           Must clear arrays     ║
║   Iteration              O(n)            O(n)           Visit all entries     ║
║                                                                               ║
║   ═══════════════════════════════════════════════════════════════════════    ║
║                                                                               ║
║   WHY O(1) AVERAGE?                                                           ║
║   ─────────────────                                                           ║
║   1. Hash computation: O(1) for fixed-size keys                              ║
║   2. Bucket lookup: O(1) - direct array access                               ║
║   3. Chain traversal: O(1) average if load factor is low                     ║
║      (good hash function distributes evenly)                                 ║
║                                                                               ║
║   WHY O(n) WORST CASE?                                                        ║
║   ────────────────────                                                        ║
║   All keys hash to same bucket → linked list of all n entries               ║
║   Must traverse entire chain → O(n)                                          ║
║                                                                               ║
║   ═══════════════════════════════════════════════════════════════════════    ║
║                                                                               ║
║   SPACE COMPLEXITY: O(n)                                                      ║
║   ──────────────────────                                                      ║
║   - buckets array: O(n)                                                       ║
║   - entries array: O(n)                                                       ║
║   - Total: O(n) where n = number of entries                                  ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

### Comparison with Other Data Structures

| Operation         | Dictionary | List (unsorted) | SortedDictionary | Array  |
| ----------------- | ---------- | --------------- | ---------------- | ------ |
| Search by key     | O(1)       | O(n)            | O(log n)         | O(n)   |
| Insert            | O(1)       | O(1) or O(n)    | O(log n)         | O(n)   |
| Delete            | O(1)       | O(n)            | O(log n)         | O(n)   |
| Ordered iteration | No         | No              | Yes              | No     |
| Memory overhead   | Higher     | Lower           | Higher           | Lowest |

---

## 11. Interview Questions with Answers

### Q1: How does Dictionary<K,V> work internally?

**Answer:**

> Dictionary uses a **hash table** with two arrays:
>
> 1. `buckets[]` - maps hash codes to entry indices
> 2. `entries[]` - stores actual key-value pairs with chaining
>
> **Lookup process:**
>
> 1. Compute `hashCode = key.GetHashCode()`
> 2. Find bucket: `bucketIndex = hashCode % buckets.Length`
> 3. Walk the chain in that bucket, comparing hash codes first, then `Equals()`
> 4. Return value when matching key found

---

### Q2: What is a hash collision and how does Dictionary handle it?

**Answer:**

> A **collision** occurs when two different keys produce the same bucket index.
>
> Dictionary uses **separate chaining**: each bucket can hold multiple entries linked together. When collision occurs:
>
> 1. New entry is added to the chain
> 2. On lookup, we traverse the chain comparing each key with `Equals()`
>
> This maintains O(1) average case while handling collisions gracefully.

---

### Q3: What is the GetHashCode/Equals contract?

**Answer:**

> **If `a.Equals(b)` is true, then `a.GetHashCode()` MUST equal `b.GetHashCode()`**
>
> Why: Equal objects must hash to the same bucket, or one won't be found when searching with the other.
>
> The reverse is NOT required: equal hash codes don't mean equal objects (collisions are allowed).

---

### Q4: Why should Dictionary keys be immutable?

**Answer:**

> If a key is modified after insertion, its hash code changes. The entry remains in the **old bucket** based on the original hash code, but searches use the **new hash code** and look in a different bucket.
>
> Result: The entry becomes "lost" - it exists but can't be found. This is why strings, value types, or records are preferred as keys.

---

### Q5: What's the time complexity of ContainsValue()?

**Answer:**

> **O(n)** - it must scan ALL values in the dictionary.
>
> Unlike `ContainsKey()` which is O(1) using the hash table, there's no index for values. You must iterate through every entry and compare.
>
> If you need fast value lookup, consider maintaining a reverse dictionary.

---

### Q6: When does Dictionary resize and what happens?

**Answer:**

> Dictionary resizes when the `entries` array is full.
>
> **Process:**
>
> 1. Allocate new arrays (next prime number > 2× current size)
> 2. **Rehash** all entries: recalculate bucket index for each entry using new size
> 3. Update bucket pointers
>
> This is O(n) but happens infrequently, giving O(1) amortized insertion.
>
> **Tip:** Pre-size with `new Dictionary<K,V>(expectedCount)` to avoid resizing.

---

### Q7: Difference between Dictionary and Hashtable?

**Answer:**

> | Feature     | Dictionary<K,V>             | Hashtable                       |
> | ----------- | --------------------------- | ------------------------------- |
> | Type safety | Generic (compile-time)      | Non-generic (runtime)           |
> | Null keys   | Not allowed                 | One null key allowed            |
> | Missing key | Throws KeyNotFoundException | Returns null                    |
> | Performance | Faster (no boxing)          | Slower (boxing for value types) |
> | Thread-safe | No                          | Partially (single writer)       |
>
> **Always use Dictionary<K,V>** in modern code.

---

### Q8: How would you implement a thread-safe dictionary operation?

**Answer:**

```csharp
// Option 1: ConcurrentDictionary (preferred)
var dict = new ConcurrentDictionary<string, int>();
dict.AddOrUpdate("key", 1, (k, v) => v + 1);

// Option 2: Lock for regular Dictionary
private readonly object _lock = new();
private readonly Dictionary<string, int> _dict = new();

public void SafeAdd(string key, int value)
{
    lock (_lock)
    {
        _dict[key] = value;
    }
}
```

---

### Q9: Write a custom class that can be used as a Dictionary key

**Answer:**

```csharp
public class CurrencyPair : IEquatable<CurrencyPair>
{
    public string Base { get; }
    public string Quote { get; }

    public CurrencyPair(string baseCcy, string quote)
    {
        Base = baseCcy ?? throw new ArgumentNullException(nameof(baseCcy));
        Quote = quote ?? throw new ArgumentNullException(nameof(quote));
    }

    public bool Equals(CurrencyPair? other)
    {
        if (other is null) return false;
        return Base == other.Base && Quote == other.Quote;
    }

    public override bool Equals(object? obj) => Equals(obj as CurrencyPair);

    public override int GetHashCode() => HashCode.Combine(Base, Quote);
}

// Or simply use a record:
public record CurrencyPair(string Base, string Quote);
```

---

### Q10: What's the difference between Dictionary and SortedDictionary?

**Answer:**

> | Feature            | Dictionary<K,V>     | SortedDictionary<K,V> |
> | ------------------ | ------------------- | --------------------- |
> | Internal structure | Hash table          | Red-Black tree        |
> | Lookup             | O(1)                | O(log n)              |
> | Insert             | O(1)                | O(log n)              |
> | Ordering           | No guaranteed order | Sorted by key         |
> | Memory             | Less                | More (tree nodes)     |
>
> Use Dictionary when you don't need ordering (most cases).
> Use SortedDictionary when you need to iterate in key order.

---

## Summary Cheat Sheet

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                    DICTIONARY INTERNALS CHEAT SHEET                           ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║   STRUCTURE:      buckets[] + entries[] (hash table with chaining)           ║
║   LOOKUP:         O(1) average - hash → bucket → chain → Equals()            ║
║   COLLISION:      Separate chaining (linked list in bucket)                  ║
║   RESIZE:         When full, 2× size (next prime), rehash all                ║
║                                                                               ║
║   CONTRACT:       Equals() = true → GetHashCode() MUST be equal              ║
║   KEYS:           Should be immutable (or hash-affecting properties)         ║
║   CUSTOM KEYS:    Override GetHashCode() + Equals() OR use record            ║
║                                                                               ║
║   AVOID:          ContainsValue() is O(n)                                    ║
║                   Modifying keys after insertion                             ║
║                   Modifying collection during iteration                      ║
║                                                                               ║
║   PREFER:         TryGetValue over ContainsKey + indexer                     ║
║                   Pre-sizing when count is known                             ║
║                   ConcurrentDictionary for thread-safety                     ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

---

_Master this and you'll ace any Dictionary-related interview question!_ 🎯
