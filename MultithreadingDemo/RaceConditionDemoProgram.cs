/*
╔══════════════════════════════════════════════════════════════════════════════╗
║              RACE CONDITION DEMO - FX TRADING SCENARIOS                       ║
║                                                                               ║
║  This file demonstrates race conditions and their fixes in FX trading         ║
║  scenarios. Run with: dotnet run                                              ║
╚══════════════════════════════════════════════════════════════════════════════╝
*/

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RaceConditionDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                RACE CONDITION DEMONSTRATION                          ║");
            Console.WriteLine("║                                                                      ║");
            Console.WriteLine("║  This demo shows race conditions in FX trading scenarios:            ║");
            Console.WriteLine("║  1. Trade Balance Race Condition (Check-Then-Act)                    ║");
            Console.WriteLine("║  2. Counter Increment Race Condition (Read-Modify-Write)             ║");
            Console.WriteLine("║  3. Dictionary Access Race Condition (Compound Operations)           ║");
            Console.WriteLine("║  4. Order Matching Race Condition (List Iteration)                   ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════╝\n");
            
            Demo1_TradeBalanceRaceCondition();
            Console.WriteLine("\n" + new string('═', 70) + "\n");
            
            Demo2_CounterRaceCondition();
            Console.WriteLine("\n" + new string('═', 70) + "\n");
            
            Demo3_DictionaryRaceCondition();
            Console.WriteLine("\n" + new string('═', 70) + "\n");
            
            Demo4_OrderMatchingRaceCondition();
            
            Console.WriteLine("\n\n✓ All demos completed!");
            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }
        
        #region Demo 1: Trade Balance Race Condition
        
        static void Demo1_TradeBalanceRaceCondition()
        {
            Console.WriteLine("═══ DEMO 1: TRADE BALANCE RACE CONDITION ═══");
            Console.WriteLine("Scenario: Multiple traders executing BUY orders simultaneously\n");
            
            // UNSAFE VERSION
            Console.WriteLine("【UNSAFE VERSION】");
            var unsafeAccount = new UnsafeTradingAccount(100_000m);
            
            Console.WriteLine($"Starting Balance: {unsafeAccount.Balance:C}");
            Console.WriteLine("10 traders each trying to BUY $10,000 simultaneously...\n");
            
            var unsafeTasks = new Task[10];
            int successCount = 0;
            int failCount = 0;
            
            for (int i = 0; i < 10; i++)
            {
                int traderId = i + 1;
                unsafeTasks[i] = Task.Run(() =>
                {
                    bool success = unsafeAccount.ExecuteTrade(traderId, 10_000m, "BUY");
                    if (success)
                        Interlocked.Increment(ref successCount);
                    else
                        Interlocked.Increment(ref failCount);
                });
            }
            Task.WaitAll(unsafeTasks);
            
            Console.WriteLine($"Final Balance (UNSAFE): {unsafeAccount.Balance:C}");
            Console.WriteLine($"Successful trades: {successCount}, Failed: {failCount}");
            
            if (unsafeAccount.Balance < 0)
            {
                Console.WriteLine("⚠️  RACE CONDITION DETECTED: Balance went negative!");
                Console.WriteLine("    Multiple threads passed the check before any deducted.");
            }
            else if (successCount > 10)
            {
                Console.WriteLine("⚠️  RACE CONDITION DETECTED: More trades executed than expected!");
            }
            
            // SAFE VERSION
            Console.WriteLine("\n【SAFE VERSION (with lock)】");
            var safeAccount = new SafeTradingAccount(100_000m);
            
            Console.WriteLine($"Starting Balance: {safeAccount.Balance:C}");
            Console.WriteLine("10 traders each trying to BUY $10,000 simultaneously...\n");
            
            successCount = 0;
            failCount = 0;
            
            var safeTasks = new Task[10];
            for (int i = 0; i < 10; i++)
            {
                int traderId = i + 1;
                safeTasks[i] = Task.Run(() =>
                {
                    bool success = safeAccount.ExecuteTrade(traderId, 10_000m, "BUY");
                    if (success)
                        Interlocked.Increment(ref successCount);
                    else
                        Interlocked.Increment(ref failCount);
                });
            }
            Task.WaitAll(safeTasks);
            
            Console.WriteLine($"Final Balance (SAFE): {safeAccount.Balance:C}");
            Console.WriteLine($"Successful trades: {successCount}, Failed: {failCount}");
            Console.WriteLine("✓ With lock, balance is exactly $0 - all 10 trades of $10,000 executed correctly");
        }
        
        class UnsafeTradingAccount
        {
            public decimal Balance { get; private set; }
            
            public UnsafeTradingAccount(decimal initialBalance) => Balance = initialBalance;
            
            // RACE CONDITION: Check-then-act pattern without synchronization
            public bool ExecuteTrade(int traderId, decimal amount, string direction)
            {
                if (direction == "BUY")
                {
                    // CHECK: Multiple threads can pass this check simultaneously
                    if (Balance >= amount)
                    {
                        Thread.Sleep(1);  // Simulate processing - widens race window
                        // ACT: All passed threads will deduct
                        Balance -= amount;  // Can result in negative balance!
                        return true;
                    }
                    return false;
                }
                else
                {
                    Thread.Sleep(1);
                    Balance += amount;
                    return true;
                }
            }
        }
        
        class SafeTradingAccount
        {
            private decimal _balance;
            private readonly object _lock = new();
            
            public decimal Balance => _balance;
            
            public SafeTradingAccount(decimal initialBalance) => _balance = initialBalance;
            
            // THREAD-SAFE: lock ensures only one thread executes at a time
            public bool ExecuteTrade(int traderId, decimal amount, string direction)
            {
                lock (_lock)  // Only one thread can hold this lock at a time
                {
                    if (direction == "BUY")
                    {
                        if (_balance >= amount)
                        {
                            Thread.Sleep(1);  // Even with delay, we're protected
                            _balance -= amount;
                            return true;
                        }
                        return false;
                    }
                    else
                    {
                        Thread.Sleep(1);
                        _balance += amount;
                        return true;
                    }
                }
            }
        }
        
        #endregion
        
        #region Demo 2: Counter Race Condition
        
        static void Demo2_CounterRaceCondition()
        {
            Console.WriteLine("═══ DEMO 2: COUNTER INCREMENT RACE CONDITION ═══");
            Console.WriteLine("Scenario: Multiple threads counting processed trades\n");
            
            const int iterations = 100_000;
            const int threadCount = 4;
            int expected = iterations * threadCount;
            
            // UNSAFE VERSION
            Console.WriteLine("【UNSAFE VERSION】");
            int unsafeCounter = 0;
            
            var unsafeTasks = new Task[threadCount];
            for (int t = 0; t < threadCount; t++)
            {
                unsafeTasks[t] = Task.Run(() =>
                {
                    for (int i = 0; i < iterations; i++)
                    {
                        unsafeCounter++;  // NOT ATOMIC!
                        // Decomposes to:
                        // 1. int temp = unsafeCounter;  (READ)
                        // 2. temp = temp + 1;           (MODIFY)
                        // 3. unsafeCounter = temp;      (WRITE)
                    }
                });
            }
            Task.WaitAll(unsafeTasks);
            
            Console.WriteLine($"Expected: {expected:N0}");
            Console.WriteLine($"Actual (UNSAFE): {unsafeCounter:N0}");
            Console.WriteLine($"Lost increments: {expected - unsafeCounter:N0}");
            Console.WriteLine($"⚠️  {((double)(expected - unsafeCounter) / expected * 100):F2}% of increments were lost!\n");
            
            // SAFE VERSION with lock
            Console.WriteLine("【SAFE VERSION (with lock)】");
            int safeCounterLock = 0;
            object lockObj = new();
            
            var safeTasksLock = new Task[threadCount];
            for (int t = 0; t < threadCount; t++)
            {
                safeTasksLock[t] = Task.Run(() =>
                {
                    for (int i = 0; i < iterations; i++)
                    {
                        lock (lockObj)
                        {
                            safeCounterLock++;
                        }
                    }
                });
            }
            Task.WaitAll(safeTasksLock);
            
            Console.WriteLine($"Actual (with lock): {safeCounterLock:N0}");
            Console.WriteLine($"Lost increments: {expected - safeCounterLock:N0}\n");
            
            // SAFE VERSION with Interlocked (BEST for counters)
            Console.WriteLine("【SAFE VERSION (with Interlocked - RECOMMENDED)】");
            int safeCounterInterlocked = 0;
            
            var safeTasksInterlocked = new Task[threadCount];
            for (int t = 0; t < threadCount; t++)
            {
                safeTasksInterlocked[t] = Task.Run(() =>
                {
                    for (int i = 0; i < iterations; i++)
                    {
                        Interlocked.Increment(ref safeCounterInterlocked);  // ATOMIC!
                    }
                });
            }
            Task.WaitAll(safeTasksInterlocked);
            
            Console.WriteLine($"Actual (with Interlocked): {safeCounterInterlocked:N0}");
            Console.WriteLine($"Lost increments: {expected - safeCounterInterlocked:N0}");
            Console.WriteLine("✓ Interlocked is lock-free and faster than lock for simple counters");
        }
        
        #endregion
        
        #region Demo 3: Dictionary Race Condition
        
        static void Demo3_DictionaryRaceCondition()
        {
            Console.WriteLine("═══ DEMO 3: DICTIONARY ACCESS RACE CONDITION ═══");
            Console.WriteLine("Scenario: Multiple threads updating FX rates\n");
            
            const int updatesPerThread = 1000;
            const int threadCount = 4;
            string[] pairs = { "EUR/USD", "GBP/USD", "USD/JPY", "AUD/USD" };
            
            // UNSAFE VERSION
            Console.WriteLine("【UNSAFE VERSION (regular Dictionary)】");
            var unsafeRates = new Dictionary<string, decimal>();
            bool hadException = false;
            
            var unsafeTasks = new Task[threadCount];
            for (int t = 0; t < threadCount; t++)
            {
                int threadId = t;
                unsafeTasks[t] = Task.Run(() =>
                {
                    var random = new Random(threadId);
                    try
                    {
                        for (int i = 0; i < updatesPerThread; i++)
                        {
                            var pair = pairs[random.Next(pairs.Length)];
                            var rate = 1.0m + (decimal)random.NextDouble() * 0.1m;
                            
                            // Multiple race conditions here:
                            // 1. Dictionary internal state corruption
                            // 2. Lost updates
                            // 3. Iteration exceptions
                            unsafeRates[pair] = rate;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"   Thread {threadId} threw: {ex.GetType().Name}");
                        hadException = true;
                    }
                });
            }
            Task.WaitAll(unsafeTasks);
            
            if (hadException)
            {
                Console.WriteLine("⚠️  Race condition caused exception (Dictionary corrupted)!\n");
            }
            else
            {
                Console.WriteLine($"   No exception, but data might be inconsistent");
                Console.WriteLine($"   Final rates count: {unsafeRates.Count}\n");
            }
            
            // SAFE VERSION with ConcurrentDictionary
            Console.WriteLine("【SAFE VERSION (ConcurrentDictionary)】");
            var safeRates = new ConcurrentDictionary<string, decimal>();
            
            var safeTasks = new Task[threadCount];
            for (int t = 0; t < threadCount; t++)
            {
                int threadId = t;
                safeTasks[t] = Task.Run(() =>
                {
                    var random = new Random(threadId);
                    for (int i = 0; i < updatesPerThread; i++)
                    {
                        var pair = pairs[random.Next(pairs.Length)];
                        var rate = 1.0m + (decimal)random.NextDouble() * 0.1m;
                        
                        // AddOrUpdate is atomic
                        safeRates.AddOrUpdate(pair, rate, (k, v) => rate);
                    }
                });
            }
            Task.WaitAll(safeTasks);
            
            Console.WriteLine("   ConcurrentDictionary completed without errors");
            Console.WriteLine($"   Final rates count: {safeRates.Count}");
            Console.WriteLine("   Current rates:");
            foreach (var kvp in safeRates)
            {
                Console.WriteLine($"     {kvp.Key}: {kvp.Value:F5}");
            }
            Console.WriteLine("✓ ConcurrentDictionary provides thread-safe access");
        }
        
        #endregion
        
        #region Demo 4: Order Matching Race Condition
        
        static void Demo4_OrderMatchingRaceCondition()
        {
            Console.WriteLine("═══ DEMO 4: ORDER MATCHING RACE CONDITION ═══");
            Console.WriteLine("Scenario: Multiple threads trying to match buy/sell orders\n");
            
            // UNSAFE VERSION
            Console.WriteLine("【UNSAFE VERSION】");
            var unsafeBook = new UnsafeOrderBook();
            
            // Add some orders
            for (int i = 0; i < 5; i++)
            {
                unsafeBook.AddOrder(new Order($"BUY-{i}", "BUY", 100m + i));
                unsafeBook.AddOrder(new Order($"SELL-{i}", "SELL", 99m + i));
            }
            
            Console.WriteLine($"Added 5 BUY orders and 5 SELL orders");
            Console.WriteLine("2 threads trying to match orders simultaneously...\n");
            
            bool hadError = false;
            var unsafeTasks = new[]
            {
                Task.Run(() =>
                {
                    try { unsafeBook.TryMatchOrders(); }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"   Thread 1 error: {ex.Message}");
                        hadError = true;
                    }
                }),
                Task.Run(() =>
                {
                    try { unsafeBook.TryMatchOrders(); }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"   Thread 2 error: {ex.Message}");
                        hadError = true;
                    }
                })
            };
            Task.WaitAll(unsafeTasks);
            
            if (hadError)
            {
                Console.WriteLine("⚠️  Race condition caused errors!\n");
            }
            else
            {
                Console.WriteLine($"   Total matches: {unsafeBook.MatchCount}");
                Console.WriteLine("   (Might have double-matched orders or lost matches)\n");
            }
            
            // SAFE VERSION
            Console.WriteLine("【SAFE VERSION (with ReaderWriterLockSlim)】");
            var safeBook = new SafeOrderBook();
            
            // Add same orders
            for (int i = 0; i < 5; i++)
            {
                safeBook.AddOrder(new Order($"BUY-{i}", "BUY", 100m + i));
                safeBook.AddOrder(new Order($"SELL-{i}", "SELL", 99m + i));
            }
            
            Console.WriteLine($"Added 5 BUY orders and 5 SELL orders");
            Console.WriteLine("2 threads trying to match orders simultaneously...\n");
            
            var safeTasks = new[]
            {
                Task.Run(() => safeBook.TryMatchOrders()),
                Task.Run(() => safeBook.TryMatchOrders())
            };
            Task.WaitAll(safeTasks);
            
            Console.WriteLine($"   Total matches: {safeBook.MatchCount}");
            Console.WriteLine("✓ With proper locking, no double-matches and no errors");
        }
        
        class Order
        {
            public string OrderId { get; }
            public string Side { get; }
            public decimal Price { get; }
            public string Status { get; set; } = "NEW";
            
            public Order(string orderId, string side, decimal price)
            {
                OrderId = orderId;
                Side = side;
                Price = price;
            }
        }
        
        class UnsafeOrderBook
        {
            private readonly List<Order> _orders = new();
            public int MatchCount { get; private set; }
            
            public void AddOrder(Order order)
            {
                _orders.Add(order);
            }
            
            public void TryMatchOrders()
            {
                // RACE: Iterating and modifying list simultaneously
                var buyOrders = _orders.Where(o => o.Side == "BUY" && o.Status == "NEW")
                                       .OrderByDescending(o => o.Price).ToList();
                var sellOrders = _orders.Where(o => o.Side == "SELL" && o.Status == "NEW")
                                        .OrderBy(o => o.Price).ToList();
                
                foreach (var buy in buyOrders)
                {
                    foreach (var sell in sellOrders)
                    {
                        // RACE: Status might change between check and update
                        if (buy.Price >= sell.Price && buy.Status == "NEW" && sell.Status == "NEW")
                        {
                            Thread.Sleep(1);  // Widens race window
                            buy.Status = "MATCHED";
                            sell.Status = "MATCHED";
                            MatchCount++;
                            Console.WriteLine($"   [Unsafe] Matched {buy.OrderId} with {sell.OrderId}");
                            break;
                        }
                    }
                }
            }
        }
        
        class SafeOrderBook
        {
            private readonly List<Order> _orders = new();
            private readonly ReaderWriterLockSlim _lock = new();
            private int _matchCount;
            
            public int MatchCount => _matchCount;
            
            public void AddOrder(Order order)
            {
                _lock.EnterWriteLock();
                try
                {
                    _orders.Add(order);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            
            public void TryMatchOrders()
            {
                _lock.EnterWriteLock();  // Exclusive access for matching
                try
                {
                    var buyOrders = _orders.Where(o => o.Side == "BUY" && o.Status == "NEW")
                                           .OrderByDescending(o => o.Price).ToList();
                    var sellOrders = _orders.Where(o => o.Side == "SELL" && o.Status == "NEW")
                                            .OrderBy(o => o.Price).ToList();
                    
                    foreach (var buy in buyOrders)
                    {
                        foreach (var sell in sellOrders)
                        {
                            if (buy.Price >= sell.Price && buy.Status == "NEW" && sell.Status == "NEW")
                            {
                                Thread.Sleep(1);
                                buy.Status = "MATCHED";
                                sell.Status = "MATCHED";
                                Interlocked.Increment(ref _matchCount);
                                Console.WriteLine($"   [Safe] Matched {buy.OrderId} with {sell.OrderId}");
                                break;
                            }
                        }
                    }
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }
        
        #endregion
    }
}
