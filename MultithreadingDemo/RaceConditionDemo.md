# Race Conditions in C# – Deep Dive with FX Trading Examples

> **Interview Focus:** Understanding, Identifying, and Fixing Race Conditions  
> **Domain:** FX Trading Systems

---

## What is a Race Condition?

A **race condition** occurs when the behavior of software depends on the **timing or order** of events (such as thread execution order) in a way that produces **unpredictable or incorrect results**.

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    RACE CONDITION ANATOMY                               │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   Thread A                          Thread B                            │
│   ────────                          ────────                            │
│   1. Read counter (0)               1. Read counter (0)                 │
│   2. Increment (1)                  2. Increment (1)                    │
│   3. Write counter (1)              3. Write counter (1)                │
│                                                                         │
│   Expected: counter = 2                                                 │
│   Actual:   counter = 1  ← RACE CONDITION!                              │
│                                                                         │
│   WHY? Both threads read 0 before either wrote                          │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## Classic Race Condition Categories

### 1. Check-Then-Act (TOCTOU - Time of Check to Time of Use)

```csharp
// DANGEROUS - Race condition in check-then-act pattern
class UnsafeTradeExecutor
{
    private decimal _availableBalance = 10000m;

    public bool ExecuteTrade(decimal amount)
    {
        // CHECK
        if (_availableBalance >= amount)  // Thread A checks: 10000 >= 5000 ✓
        {                                  // Thread B checks: 10000 >= 6000 ✓
            // ACT
            Thread.Sleep(10);  // Simulating some processing
            _availableBalance -= amount;   // Thread A: 10000 - 5000 = 5000
                                           // Thread B: 5000 - 6000 = -1000 ❌ NEGATIVE!
            return true;
        }
        return false;
    }
}
```

**The Problem:** Between checking the condition and acting on it, another thread can change the state.

### 2. Read-Modify-Write

```csharp
// DANGEROUS - Non-atomic read-modify-write
class UnsafeTradeCounter
{
    private int _tradeCount = 0;

    public void RecordTrade()
    {
        _tradeCount++;  // Looks atomic but ISN'T!

        // Actually decomposes to:
        // 1. int temp = _tradeCount;  // READ
        // 2. temp = temp + 1;          // MODIFY
        // 3. _tradeCount = temp;       // WRITE

        // Two threads can interleave these operations!
    }
}
```

### 3. Compound Operations on Collections

```csharp
// DANGEROUS - Multiple dictionary operations
class UnsafeRateCache
{
    private Dictionary<string, decimal> _rates = new();

    public void UpdateRateIfBetter(string pair, decimal newRate)
    {
        if (!_rates.ContainsKey(pair))               // Thread A: EUR/USD not found
        {
            _rates[pair] = newRate;                  // Thread A: adds EUR/USD = 1.05
        }                                            // Thread B: EUR/USD not found (but A just added!)
        else if (newRate > _rates[pair])             // Thread B: adds EUR/USD = 1.06 (overwrites!)
        {
            _rates[pair] = newRate;
        }
        // Result: Lost Thread A's logic flow
    }
}
```

---

## FX Trading Race Condition Scenarios

### Scenario 1: Trade Balance Race Condition

```csharp
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RaceConditionDemo
{
    /// <summary>
    /// Demonstrates race condition in FX Trading balance updates
    /// </summary>
    public class TradeBalanceRaceCondition
    {
        private decimal _usdBalance = 100_000m;
        private readonly List<string> _tradeLog = new();

        public decimal Balance => _usdBalance;
        public IReadOnlyList<string> TradeLog => _tradeLog;

        // ╔════════════════════════════════════════════════════════════════╗
        // ║  UNSAFE VERSION - Contains Race Condition                      ║
        // ╚════════════════════════════════════════════════════════════════╝

        /// <summary>
        /// UNSAFE: Execute trade without proper synchronization
        /// </summary>
        public bool ExecuteTradeUnsafe(string tradeId, decimal amount, string direction)
        {
            decimal currentBalance = _usdBalance;  // READ

            if (direction == "BUY" && currentBalance >= amount)
            {
                // Simulate processing delay
                Thread.Sleep(1);  // This allows other threads to interleave

                _usdBalance = currentBalance - amount;  // WRITE (using stale value!)
                _tradeLog.Add($"[UNSAFE] {tradeId}: {direction} {amount:C} - Balance: {_usdBalance:C}");
                return true;
            }
            else if (direction == "SELL")
            {
                Thread.Sleep(1);
                _usdBalance = currentBalance + amount;
                _tradeLog.Add($"[UNSAFE] {tradeId}: {direction} {amount:C} - Balance: {_usdBalance:C}");
                return true;
            }

            return false;
        }

        // ╔════════════════════════════════════════════════════════════════╗
        // ║  SAFE VERSION - Using lock for synchronization                 ║
        // ╚════════════════════════════════════════════════════════════════╝

        private readonly object _balanceLock = new object();

        /// <summary>
        /// SAFE: Execute trade with proper synchronization
        /// </summary>
        public bool ExecuteTradeSafe(string tradeId, decimal amount, string direction)
        {
            lock (_balanceLock)
            {
                if (direction == "BUY" && _usdBalance >= amount)
                {
                    Thread.Sleep(1);  // Same delay, but now we're protected

                    _usdBalance -= amount;
                    _tradeLog.Add($"[SAFE] {tradeId}: {direction} {amount:C} - Balance: {_usdBalance:C}");
                    return true;
                }
                else if (direction == "SELL")
                {
                    Thread.Sleep(1);
                    _usdBalance += amount;
                    _tradeLog.Add($"[SAFE] {tradeId}: {direction} {amount:C} - Balance: {_usdBalance:C}");
                    return true;
                }

                return false;
            }
        }

        public void Reset()
        {
            _usdBalance = 100_000m;
            _tradeLog.Clear();
        }
    }
}
```

### Scenario 2: Rate Update Race Condition

```csharp
using System;
using System.Collections.Generic;
using System.Threading;

namespace RaceConditionDemo
{
    /// <summary>
    /// Demonstrates race condition in rate updates
    /// </summary>
    public class RateUpdateRaceCondition
    {
        // ╔════════════════════════════════════════════════════════════════╗
        // ║  UNSAFE VERSION - Dictionary without synchronization           ║
        // ╚════════════════════════════════════════════════════════════════╝

        private Dictionary<string, (decimal Bid, decimal Ask, long SeqNum)> _unsafeRates = new();

        public void UpdateRateUnsafe(string pair, decimal bid, decimal ask, long seqNum)
        {
            // Race 1: Two threads might both see ContainsKey = false
            // Race 2: Dictionary resize can cause iteration issues
            // Race 3: Sequence number check is not atomic with update

            if (!_unsafeRates.ContainsKey(pair))
            {
                _unsafeRates[pair] = (bid, ask, seqNum);
            }
            else if (_unsafeRates[pair].SeqNum < seqNum)
            {
                _unsafeRates[pair] = (bid, ask, seqNum);
            }
        }

        public (decimal Bid, decimal Ask)? GetRateUnsafe(string pair)
        {
            // Race: Another thread might be modifying during this read
            if (_unsafeRates.ContainsKey(pair))
            {
                var rate = _unsafeRates[pair];
                return (rate.Bid, rate.Ask);
            }
            return null;
        }

        // ╔════════════════════════════════════════════════════════════════╗
        // ║  SAFE VERSION - Using ConcurrentDictionary                     ║
        // ╚════════════════════════════════════════════════════════════════╝

        private System.Collections.Concurrent.ConcurrentDictionary<string, (decimal Bid, decimal Ask, long SeqNum)>
            _safeRates = new();

        public void UpdateRateSafe(string pair, decimal bid, decimal ask, long seqNum)
        {
            _safeRates.AddOrUpdate(
                pair,
                // Add factory - called if key doesn't exist
                (bid, ask, seqNum),
                // Update factory - called if key exists
                (key, existing) => seqNum > existing.SeqNum
                    ? (bid, ask, seqNum)
                    : existing
            );
        }

        public (decimal Bid, decimal Ask)? GetRateSafe(string pair)
        {
            if (_safeRates.TryGetValue(pair, out var rate))
            {
                return (rate.Bid, rate.Ask);
            }
            return null;
        }
    }
}
```

### Scenario 3: Order Book Race Condition

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RaceConditionDemo
{
    /// <summary>
    /// Demonstrates race condition in order book management
    /// </summary>
    public class OrderBookRaceCondition
    {
        public class Order
        {
            public string OrderId { get; set; }
            public string Side { get; set; }  // "BUY" or "SELL"
            public decimal Price { get; set; }
            public decimal Quantity { get; set; }
            public DateTime Timestamp { get; set; }
            public string Status { get; set; } = "NEW";
        }

        // ╔════════════════════════════════════════════════════════════════╗
        // ║  UNSAFE VERSION - Race conditions everywhere                   ║
        // ╚════════════════════════════════════════════════════════════════╝

        private List<Order> _unsafeOrders = new();

        public void AddOrderUnsafe(Order order)
        {
            // Race 1: Two threads adding simultaneously can corrupt list
            // Race 2: Matching might see partial state
            _unsafeOrders.Add(order);
            TryMatchOrdersUnsafe();
        }

        public void TryMatchOrdersUnsafe()
        {
            // Race: Another thread might modify list during iteration
            var buyOrders = _unsafeOrders
                .Where(o => o.Side == "BUY" && o.Status == "NEW")
                .OrderByDescending(o => o.Price)
                .ToList();

            var sellOrders = _unsafeOrders
                .Where(o => o.Side == "SELL" && o.Status == "NEW")
                .OrderBy(o => o.Price)
                .ToList();

            foreach (var buy in buyOrders)
            {
                foreach (var sell in sellOrders)
                {
                    if (buy.Price >= sell.Price &&
                        buy.Status == "NEW" &&
                        sell.Status == "NEW")
                    {
                        // Race: Status might have changed by now!
                        buy.Status = "MATCHED";   // Another thread might also match this
                        sell.Status = "MATCHED";
                        Console.WriteLine($"[UNSAFE] Matched: Buy {buy.OrderId} with Sell {sell.OrderId}");
                        break;
                    }
                }
            }
        }

        // ╔════════════════════════════════════════════════════════════════╗
        // ║  SAFE VERSION - Proper synchronization                         ║
        // ╚════════════════════════════════════════════════════════════════╝

        private List<Order> _safeOrders = new();
        private readonly ReaderWriterLockSlim _orderLock = new(LockRecursionPolicy.SupportsRecursion);

        public void AddOrderSafe(Order order)
        {
            _orderLock.EnterWriteLock();
            try
            {
                _safeOrders.Add(order);
                TryMatchOrdersSafe();  // Already under write lock
            }
            finally
            {
                _orderLock.ExitWriteLock();
            }
        }

        public void TryMatchOrdersSafe()
        {
            // Assumes already under write lock or call with lock
            var buyOrders = _safeOrders
                .Where(o => o.Side == "BUY" && o.Status == "NEW")
                .OrderByDescending(o => o.Price)
                .ToList();

            var sellOrders = _safeOrders
                .Where(o => o.Side == "SELL" && o.Status == "NEW")
                .OrderBy(o => o.Price)
                .ToList();

            foreach (var buy in buyOrders)
            {
                foreach (var sell in sellOrders)
                {
                    if (buy.Price >= sell.Price &&
                        buy.Status == "NEW" &&
                        sell.Status == "NEW")
                    {
                        buy.Status = "MATCHED";
                        sell.Status = "MATCHED";
                        Console.WriteLine($"[SAFE] Matched: Buy {buy.OrderId} with Sell {sell.OrderId}");
                        break;
                    }
                }
            }
        }

        public List<Order> GetOrderBookSnapshot()
        {
            _orderLock.EnterReadLock();
            try
            {
                // Return a copy to prevent external modification
                return _safeOrders.Select(o => new Order
                {
                    OrderId = o.OrderId,
                    Side = o.Side,
                    Price = o.Price,
                    Quantity = o.Quantity,
                    Timestamp = o.Timestamp,
                    Status = o.Status
                }).ToList();
            }
            finally
            {
                _orderLock.ExitReadLock();
            }
        }
    }
}
```

---

## Complete Demo Program

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RaceConditionDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║           RACE CONDITION DEMONSTRATION                        ║");
            Console.WriteLine("║                                                               ║");
            Console.WriteLine("║  This demo shows race conditions in FX trading scenarios:     ║");
            Console.WriteLine("║  1. Trade Balance Race Condition                              ║");
            Console.WriteLine("║  2. Counter Increment Race Condition                          ║");
            Console.WriteLine("║  3. Rate Update Race Condition                                ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");

            DemoTradeBalanceRaceCondition();
            Console.WriteLine("\n" + new string('═', 70) + "\n");

            DemoCounterRaceCondition();
            Console.WriteLine("\n" + new string('═', 70) + "\n");

            DemoRateUpdateRaceCondition();
        }

        static void DemoTradeBalanceRaceCondition()
        {
            Console.WriteLine("═══ DEMO 1: TRADE BALANCE RACE CONDITION ═══\n");

            var executor = new TradeBalanceRaceCondition();

            // Test UNSAFE version
            Console.WriteLine("UNSAFE VERSION:");
            Console.WriteLine($"Starting Balance: {executor.Balance:C}");
            Console.WriteLine("Executing 10 concurrent BUY trades of $10,000 each...\n");

            var unsafeTasks = new Task[10];
            for (int i = 0; i < 10; i++)
            {
                int tradeNum = i + 1;
                unsafeTasks[i] = Task.Run(() =>
                    executor.ExecuteTradeUnsafe($"TRD-{tradeNum:D3}", 10_000m, "BUY"));
            }
            Task.WaitAll(unsafeTasks);

            Console.WriteLine($"Final Balance (UNSAFE): {executor.Balance:C}");
            Console.WriteLine($"Expected Balance: $0 (if all trades executed) or higher (if some rejected)");
            Console.WriteLine($"⚠️  If balance is negative or unexpected, race condition occurred!\n");

            // Reset and test SAFE version
            executor.Reset();

            Console.WriteLine("SAFE VERSION:");
            Console.WriteLine($"Starting Balance: {executor.Balance:C}");
            Console.WriteLine("Executing 10 concurrent BUY trades of $10,000 each...\n");

            var safeTasks = new Task[10];
            for (int i = 0; i < 10; i++)
            {
                int tradeNum = i + 1;
                safeTasks[i] = Task.Run(() =>
                    executor.ExecuteTradeSafe($"TRD-{tradeNum:D3}", 10_000m, "BUY"));
            }
            Task.WaitAll(safeTasks);

            Console.WriteLine($"Final Balance (SAFE): {executor.Balance:C}");
            Console.WriteLine($"✓ Balance is exactly $0 - all 10 trades of $10,000 executed correctly");
        }

        static void DemoCounterRaceCondition()
        {
            Console.WriteLine("═══ DEMO 2: COUNTER INCREMENT RACE CONDITION ═══\n");

            int unsafeCounter = 0;
            int safeCounter = 0;
            object lockObj = new object();

            const int iterations = 100_000;
            const int threadCount = 4;

            // UNSAFE version
            Console.WriteLine($"UNSAFE: {threadCount} threads each incrementing counter {iterations:N0} times");
            Console.WriteLine($"Expected result: {iterations * threadCount:N0}\n");

            var unsafeTasks = new Task[threadCount];
            for (int t = 0; t < threadCount; t++)
            {
                unsafeTasks[t] = Task.Run(() =>
                {
                    for (int i = 0; i < iterations; i++)
                    {
                        unsafeCounter++;  // Not thread-safe!
                    }
                });
            }
            Task.WaitAll(unsafeTasks);

            Console.WriteLine($"UNSAFE Result: {unsafeCounter:N0}");
            Console.WriteLine($"Lost increments: {(iterations * threadCount) - unsafeCounter:N0}");
            Console.WriteLine($"⚠️  Race condition caused {(iterations * threadCount) - unsafeCounter:N0} lost updates!\n");

            // SAFE version with lock
            Console.WriteLine("SAFE (using lock):");
            var safeTasksLock = new Task[threadCount];
            for (int t = 0; t < threadCount; t++)
            {
                safeTasksLock[t] = Task.Run(() =>
                {
                    for (int i = 0; i < iterations; i++)
                    {
                        lock (lockObj)
                        {
                            safeCounter++;
                        }
                    }
                });
            }
            Task.WaitAll(safeTasksLock);

            Console.WriteLine($"SAFE (lock) Result: {safeCounter:N0}");
            Console.WriteLine($"✓ Correct - no lost increments\n");

            // SAFE version with Interlocked (better performance)
            int interlockedCounter = 0;
            Console.WriteLine("SAFE (using Interlocked - lock-free, faster):");
            var safeTasksInterlocked = new Task[threadCount];
            for (int t = 0; t < threadCount; t++)
            {
                safeTasksInterlocked[t] = Task.Run(() =>
                {
                    for (int i = 0; i < iterations; i++)
                    {
                        Interlocked.Increment(ref interlockedCounter);
                    }
                });
            }
            Task.WaitAll(safeTasksInterlocked);

            Console.WriteLine($"SAFE (Interlocked) Result: {interlockedCounter:N0}");
            Console.WriteLine($"✓ Correct - no lost increments, better performance");
        }

        static void DemoRateUpdateRaceCondition()
        {
            Console.WriteLine("═══ DEMO 3: RATE UPDATE RACE CONDITION ═══\n");

            var rateService = new RateUpdateRaceCondition();

            const int updatesPerThread = 1000;
            const int threadCount = 4;
            string[] pairs = { "EUR/USD", "GBP/USD", "USD/JPY" };

            Console.WriteLine($"Simulating {threadCount} threads each updating {updatesPerThread} rates...\n");

            // UNSAFE version - might throw or produce wrong results
            Console.WriteLine("UNSAFE VERSION:");
            var unsafeTasks = new Task[threadCount];
            var random = new Random();

            try
            {
                for (int t = 0; t < threadCount; t++)
                {
                    int threadId = t;
                    unsafeTasks[t] = Task.Run(() =>
                    {
                        var localRandom = new Random(threadId);
                        for (int i = 0; i < updatesPerThread; i++)
                        {
                            var pair = pairs[localRandom.Next(pairs.Length)];
                            var bid = 1.0m + (decimal)localRandom.NextDouble() * 0.1m;
                            var ask = bid + 0.0001m;
                            rateService.UpdateRateUnsafe(pair, bid, ask, i * threadCount + threadId);
                        }
                    });
                }
                Task.WaitAll(unsafeTasks);
                Console.WriteLine("UNSAFE completed (may have corrupted data)\n");
            }
            catch (AggregateException ex)
            {
                Console.WriteLine($"⚠️  UNSAFE version threw exception: {ex.InnerException?.Message}\n");
            }

            // SAFE version - always correct
            Console.WriteLine("SAFE VERSION:");
            var safeTasks = new Task[threadCount];

            for (int t = 0; t < threadCount; t++)
            {
                int threadId = t;
                safeTasks[t] = Task.Run(() =>
                {
                    var localRandom = new Random(threadId);
                    for (int i = 0; i < updatesPerThread; i++)
                    {
                        var pair = pairs[localRandom.Next(pairs.Length)];
                        var bid = 1.0m + (decimal)localRandom.NextDouble() * 0.1m;
                        var ask = bid + 0.0001m;
                        rateService.UpdateRateSafe(pair, bid, ask, i * threadCount + threadId);
                    }
                });
            }
            Task.WaitAll(safeTasks);

            Console.WriteLine("SAFE completed successfully");
            Console.WriteLine("\nFinal rates:");
            foreach (var pair in pairs)
            {
                var rate = rateService.GetRateSafe(pair);
                if (rate.HasValue)
                {
                    Console.WriteLine($"  {pair}: {rate.Value.Bid:F5}/{rate.Value.Ask:F5}");
                }
            }
            Console.WriteLine("✓ All rates consistent and correct");
        }
    }
}
```

---

## Race Condition Detection Techniques

### 1. Code Review Red Flags

```csharp
// 🚨 RED FLAG: Shared mutable state without synchronization
private int _counter;  // Accessed from multiple threads

// 🚨 RED FLAG: Check-then-act pattern
if (collection.Count > 0)
{
    var item = collection[0];  // Another thread might have removed it!
}

// 🚨 RED FLAG: Non-atomic compound operations
dictionary[key] = dictionary[key] + 1;  // Read-modify-write

// 🚨 RED FLAG: Lazy initialization without synchronization
if (_instance == null)
{
    _instance = new ExpensiveObject();  // Multiple threads might create multiple instances
}
```

### 2. Testing for Race Conditions

```csharp
/// <summary>
/// Stress test to detect race conditions
/// </summary>
public static void StressTest(Action action, int threadCount, int iterationsPerThread)
{
    var barrier = new Barrier(threadCount);  // Synchronize start
    var tasks = new Task[threadCount];

    for (int t = 0; t < threadCount; t++)
    {
        tasks[t] = Task.Run(() =>
        {
            barrier.SignalAndWait();  // All threads start together
            for (int i = 0; i < iterationsPerThread; i++)
            {
                action();
            }
        });
    }

    Task.WaitAll(tasks);
}
```

---

## Fixing Race Conditions: Summary

| Problem            | Solution                  | Example                             |
| ------------------ | ------------------------- | ----------------------------------- |
| Simple counter     | `Interlocked.Increment`   | `Interlocked.Increment(ref _count)` |
| Check-then-act     | `lock` block              | `lock(_obj) { check(); act(); }`    |
| Dictionary access  | `ConcurrentDictionary`    | `_dict.AddOrUpdate(...)`            |
| List access        | `ConcurrentBag` or `lock` | `lock(_obj) { _list.Add(item); }`   |
| Complex operations | `ReaderWriterLockSlim`    | Read lock / Write lock              |
| Singleton          | `Lazy<T>`                 | `Lazy<T>(factory)`                  |

---

## Interview Questions

**Q: What is a race condition?**
A: A race condition occurs when the correctness of a program depends on the relative timing of events, such as the order in which threads execute. The behavior becomes unpredictable and can lead to incorrect results.

**Q: How do you detect race conditions?**
A: Through code review (look for shared mutable state), stress testing with many concurrent threads, tools like Thread Sanitizer, and careful analysis of check-then-act and read-modify-write patterns.

**Q: Difference between lock and Interlocked?**
A: `lock` provides mutual exclusion for a code block (heavyweight), while `Interlocked` provides atomic operations on single variables (lightweight, lock-free). Use `Interlocked` for simple counters, `lock` for complex operations.

**Q: Can volatile prevent race conditions?**
A: No! `volatile` only ensures visibility (all threads see the latest value) but doesn't provide atomicity. Use `Interlocked` or `lock` for atomic operations.

---

_Document created for interview preparation. February 2026_
