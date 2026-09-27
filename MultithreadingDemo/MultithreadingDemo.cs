/*
╔══════════════════════════════════════════════════════════════════════════════╗
║               MULTITHREADING IN C# - COMPREHENSIVE DEMO                       ║
║                                                                               ║
║  This file demonstrates all multithreading concepts in C# with FX Trading     ║
║  domain examples. Run with: dotnet run                                        ║
╚══════════════════════════════════════════════════════════════════════════════╝
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MultithreadingDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║            MULTITHREADING IN C# - COMPREHENSIVE DEMO                 ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════╝\n");
            
            var demos = new (string Name, Action Demo)[]
            {
                ("1. Thread Basics", Demo1_ThreadBasics),
                ("2. Thread with Parameters", Demo2_ThreadParameters),
                ("3. Thread Synchronization (lock)", Demo3_LockSynchronization),
                ("4. Monitor (Wait/Pulse)", Demo4_MonitorWaitPulse),
                ("5. Mutex (Cross-Process)", Demo5_Mutex),
                ("6. Semaphore", Demo6_Semaphore),
                ("7. ReaderWriterLockSlim", Demo7_ReaderWriterLock),
                ("8. Interlocked Operations", Demo8_Interlocked),
                ("9. ThreadPool", Demo9_ThreadPool),
                ("10. Task Parallel Library", Demo10_TaskParallelLibrary),
                ("11. async/await Pattern", Demo11_AsyncAwait),
                ("12. CancellationToken", Demo12_CancellationToken),
                ("13. Concurrent Collections", Demo13_ConcurrentCollections),
                ("14. Parallel.For and Parallel.ForEach", Demo14_ParallelLoops),
                ("15. Producer-Consumer Pattern", Demo15_ProducerConsumer),
                ("16. Thread-Safe Singleton", Demo16_ThreadSafeSingleton),
                ("17. Deadlock Example & Prevention", Demo17_DeadlockPrevention),
                ("18. Race Condition Demo", Demo18_RaceCondition),
            };
            
            foreach (var (name, demo) in demos)
            {
                Console.WriteLine($"\n{'═',70}");
                Console.WriteLine($"  {name}");
                Console.WriteLine($"{'═',70}\n");
                
                try
                {
                    demo();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
                
                Console.WriteLine("\nPress Enter to continue...");
                Console.ReadLine();
            }
            
            Console.WriteLine("\n✓ All demos completed!");
        }
        
        #region Demo 1: Thread Basics
        
        static void Demo1_ThreadBasics()
        {
            Console.WriteLine("Creating and starting threads...\n");
            
            // Method 1: Using ThreadStart delegate
            Thread thread1 = new Thread(new ThreadStart(SimpleThreadMethod));
            thread1.Name = "SimpleThread";
            
            // Method 2: Using lambda expression
            Thread thread2 = new Thread(() =>
            {
                for (int i = 0; i < 5; i++)
                {
                    Console.WriteLine($"[Lambda Thread] Iteration {i} on Thread {Thread.CurrentThread.ManagedThreadId}");
                    Thread.Sleep(100);
                }
            });
            thread2.Name = "LambdaThread";
            
            // Start threads
            thread1.Start();
            thread2.Start();
            
            // Wait for threads to complete
            thread1.Join();
            thread2.Join();
            
            Console.WriteLine("\n✓ Both threads completed");
            
            // Thread properties
            Console.WriteLine($"\nCurrent Thread Info:");
            Console.WriteLine($"  ID: {Thread.CurrentThread.ManagedThreadId}");
            Console.WriteLine($"  Name: {Thread.CurrentThread.Name ?? "Main"}");
            Console.WriteLine($"  IsBackground: {Thread.CurrentThread.IsBackground}");
            Console.WriteLine($"  IsThreadPoolThread: {Thread.CurrentThread.IsThreadPoolThread}");
            Console.WriteLine($"  Priority: {Thread.CurrentThread.Priority}");
        }
        
        static void SimpleThreadMethod()
        {
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine($"[{Thread.CurrentThread.Name}] Iteration {i}");
                Thread.Sleep(100);
            }
        }
        
        #endregion
        
        #region Demo 2: Thread with Parameters
        
        static void Demo2_ThreadParameters()
        {
            Console.WriteLine("Passing data to threads...\n");
            
            // Method 1: Using ParameterizedThreadStart
            Thread thread1 = new Thread(new ParameterizedThreadStart(ProcessCurrencyPair!));
            thread1.Start("EUR/USD");
            
            // Method 2: Using closure (recommended)
            string pair = "GBP/USD";
            decimal rate = 1.2650m;
            Thread thread2 = new Thread(() => ProcessRateUpdate(pair, rate));
            thread2.Start();
            
            // Method 3: Using a class to pass multiple parameters
            var tradeInfo = new TradeInfo
            {
                TradeId = "TRD001",
                CurrencyPair = "USD/JPY",
                Amount = 100000,
                Rate = 149.50m
            };
            Thread thread3 = new Thread(() => ProcessTrade(tradeInfo));
            thread3.Start();
            
            thread1.Join();
            thread2.Join();
            thread3.Join();
            
            Console.WriteLine("\n✓ All parameterized threads completed");
        }
        
        static void ProcessCurrencyPair(object pair)
        {
            Console.WriteLine($"Processing currency pair: {pair}");
            Thread.Sleep(200);
        }
        
        static void ProcessRateUpdate(string pair, decimal rate)
        {
            Console.WriteLine($"Rate update for {pair}: {rate:F5}");
            Thread.Sleep(200);
        }
        
        static void ProcessTrade(TradeInfo trade)
        {
            Console.WriteLine($"Processing trade {trade.TradeId}: {trade.CurrencyPair} " +
                            $"Amount={trade.Amount:N0} @ {trade.Rate:F4}");
            Thread.Sleep(200);
        }
        
        class TradeInfo
        {
            public string TradeId { get; set; } = "";
            public string CurrencyPair { get; set; } = "";
            public decimal Amount { get; set; }
            public decimal Rate { get; set; }
        }
        
        #endregion
        
        #region Demo 3: Lock Synchronization
        
        static void Demo3_LockSynchronization()
        {
            Console.WriteLine("Demonstrating lock synchronization...\n");
            
            var account = new TradingAccount("ACC001", 100000m);
            
            // Without lock - race condition
            Console.WriteLine("WITHOUT LOCK (potential race condition):");
            account.Reset();
            var tasksUnsafe = new Thread[10];
            for (int i = 0; i < 10; i++)
            {
                tasksUnsafe[i] = new Thread(() => account.WithdrawUnsafe(10000));
            }
            Array.ForEach(tasksUnsafe, t => t.Start());
            Array.ForEach(tasksUnsafe, t => t.Join());
            Console.WriteLine($"  Final Balance (Unsafe): {account.Balance:C}\n");
            
            // With lock - thread-safe
            Console.WriteLine("WITH LOCK (thread-safe):");
            account.Reset();
            var tasksSafe = new Thread[10];
            for (int i = 0; i < 10; i++)
            {
                tasksSafe[i] = new Thread(() => account.WithdrawSafe(10000));
            }
            Array.ForEach(tasksSafe, t => t.Start());
            Array.ForEach(tasksSafe, t => t.Join());
            Console.WriteLine($"  Final Balance (Safe): {account.Balance:C}");
        }
        
        class TradingAccount
        {
            private readonly string _accountId;
            private decimal _balance;
            private readonly object _lock = new object();
            
            public decimal Balance => _balance;
            
            public TradingAccount(string accountId, decimal initialBalance)
            {
                _accountId = accountId;
                _balance = initialBalance;
            }
            
            public void Reset() => _balance = 100000m;
            
            // UNSAFE - race condition possible
            public bool WithdrawUnsafe(decimal amount)
            {
                if (_balance >= amount)
                {
                    Thread.Sleep(1);  // Simulate processing
                    _balance -= amount;
                    return true;
                }
                return false;
            }
            
            // SAFE - using lock
            public bool WithdrawSafe(decimal amount)
            {
                lock (_lock)
                {
                    if (_balance >= amount)
                    {
                        Thread.Sleep(1);
                        _balance -= amount;
                        return true;
                    }
                    return false;
                }
            }
        }
        
        #endregion
        
        #region Demo 4: Monitor Wait/Pulse
        
        static void Demo4_MonitorWaitPulse()
        {
            Console.WriteLine("Demonstrating Monitor.Wait/Pulse (producer-consumer)...\n");
            
            var queue = new RateUpdateQueue();
            
            // Producer thread
            var producer = new Thread(() =>
            {
                string[] pairs = { "EUR/USD", "GBP/USD", "USD/JPY" };
                for (int i = 0; i < 6; i++)
                {
                    var update = new RateUpdate(pairs[i % 3], 1.0m + (decimal)i * 0.001m);
                    queue.Enqueue(update);
                    Console.WriteLine($"[Producer] Enqueued: {update}");
                    Thread.Sleep(100);
                }
                queue.Complete();
            });
            
            // Consumer thread
            var consumer = new Thread(() =>
            {
                while (true)
                {
                    var update = queue.Dequeue();
                    if (update == null) break;
                    Console.WriteLine($"[Consumer] Processing: {update}");
                    Thread.Sleep(200);
                }
                Console.WriteLine("[Consumer] Completed");
            });
            
            producer.Start();
            consumer.Start();
            
            producer.Join();
            consumer.Join();
        }
        
        class RateUpdate
        {
            public string Pair { get; }
            public decimal Rate { get; }
            public DateTime Timestamp { get; } = DateTime.UtcNow;
            
            public RateUpdate(string pair, decimal rate)
            {
                Pair = pair;
                Rate = rate;
            }
            
            public override string ToString() => $"{Pair}: {Rate:F5}";
        }
        
        class RateUpdateQueue
        {
            private readonly Queue<RateUpdate> _queue = new();
            private readonly object _lock = new();
            private bool _completed = false;
            
            public void Enqueue(RateUpdate update)
            {
                lock (_lock)
                {
                    _queue.Enqueue(update);
                    Monitor.Pulse(_lock);  // Signal waiting consumer
                }
            }
            
            public RateUpdate? Dequeue()
            {
                lock (_lock)
                {
                    while (_queue.Count == 0 && !_completed)
                    {
                        Monitor.Wait(_lock);  // Wait for producer signal
                    }
                    
                    if (_queue.Count > 0)
                        return _queue.Dequeue();
                    
                    return null;  // Completed and empty
                }
            }
            
            public void Complete()
            {
                lock (_lock)
                {
                    _completed = true;
                    Monitor.PulseAll(_lock);  // Wake all waiting consumers
                }
            }
        }
        
        #endregion
        
        #region Demo 5: Mutex
        
        static void Demo5_Mutex()
        {
            Console.WriteLine("Demonstrating Mutex (cross-process synchronization)...\n");
            
            const string mutexName = "Global\\FXTradingMutex";
            
            // Try to acquire a named mutex (cross-process)
            using var mutex = new Mutex(false, mutexName);
            
            Console.WriteLine($"Trying to acquire mutex '{mutexName}'...");
            
            bool acquired = false;
            try
            {
                acquired = mutex.WaitOne(TimeSpan.FromSeconds(2));
                
                if (acquired)
                {
                    Console.WriteLine("✓ Mutex acquired successfully");
                    Console.WriteLine("  Simulating exclusive operation...");
                    Thread.Sleep(1000);
                    Console.WriteLine("  Operation completed");
                }
                else
                {
                    Console.WriteLine("✗ Could not acquire mutex (another process holds it)");
                }
            }
            finally
            {
                if (acquired)
                {
                    mutex.ReleaseMutex();
                    Console.WriteLine("✓ Mutex released");
                }
            }
            
            Console.WriteLine("\nNote: Mutex is useful for:");
            Console.WriteLine("  - Single-instance application enforcement");
            Console.WriteLine("  - Cross-process resource coordination");
            Console.WriteLine("  - System-wide exclusive access");
        }
        
        #endregion
        
        #region Demo 6: Semaphore
        
        static void Demo6_Semaphore()
        {
            Console.WriteLine("Demonstrating Semaphore (limiting concurrent access)...\n");
            
            // Allow max 3 concurrent connections
            using var semaphore = new SemaphoreSlim(3, 3);
            
            Console.WriteLine("Simulating 10 traders trying to connect (max 3 concurrent):\n");
            
            var traders = new Task[10];
            for (int i = 0; i < 10; i++)
            {
                int traderId = i + 1;
                traders[i] = Task.Run(async () =>
                {
                    Console.WriteLine($"Trader {traderId}: Waiting for connection slot...");
                    
                    await semaphore.WaitAsync();
                    try
                    {
                        Console.WriteLine($"Trader {traderId}: CONNECTED (Slots used: {3 - semaphore.CurrentCount})");
                        await Task.Delay(500);  // Simulate trading activity
                    }
                    finally
                    {
                        semaphore.Release();
                        Console.WriteLine($"Trader {traderId}: Disconnected");
                    }
                });
            }
            
            Task.WaitAll(traders);
            Console.WriteLine("\n✓ All traders completed");
        }
        
        #endregion
        
        #region Demo 7: ReaderWriterLockSlim
        
        static void Demo7_ReaderWriterLock()
        {
            Console.WriteLine("Demonstrating ReaderWriterLockSlim...\n");
            
            var rateCache = new RateCacheWithRWLock();
            
            // Seed some initial data
            rateCache.UpdateRate("EUR/USD", 1.0850m);
            rateCache.UpdateRate("GBP/USD", 1.2650m);
            
            var tasks = new List<Task>();
            
            // Multiple readers (should run concurrently)
            for (int i = 0; i < 5; i++)
            {
                int readerId = i + 1;
                tasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < 3; j++)
                    {
                        var rate = rateCache.GetRate("EUR/USD");
                        Console.WriteLine($"[Reader {readerId}] EUR/USD = {rate?.ToString("F5") ?? "N/A"}");
                        Thread.Sleep(50);
                    }
                }));
            }
            
            // Single writer (should have exclusive access)
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 3; j++)
                {
                    decimal newRate = 1.0850m + (decimal)j * 0.001m;
                    rateCache.UpdateRate("EUR/USD", newRate);
                    Console.WriteLine($"[Writer] Updated EUR/USD = {newRate:F5}");
                    Thread.Sleep(100);
                }
            }));
            
            Task.WaitAll(tasks.ToArray());
            
            Console.WriteLine($"\nStatistics: {rateCache.ReadCount} reads, {rateCache.WriteCount} writes");
        }
        
        class RateCacheWithRWLock
        {
            private readonly Dictionary<string, decimal> _rates = new();
            private readonly ReaderWriterLockSlim _lock = new();
            
            public int ReadCount { get; private set; }
            public int WriteCount { get; private set; }
            
            public decimal? GetRate(string pair)
            {
                _lock.EnterReadLock();
                try
                {
                    ReadCount++;
                    return _rates.TryGetValue(pair, out var rate) ? rate : null;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            
            public void UpdateRate(string pair, decimal rate)
            {
                _lock.EnterWriteLock();
                try
                {
                    WriteCount++;
                    _rates[pair] = rate;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }
        
        #endregion
        
        #region Demo 8: Interlocked Operations
        
        static void Demo8_Interlocked()
        {
            Console.WriteLine("Demonstrating Interlocked (lock-free atomic operations)...\n");
            
            int unsafeCounter = 0;
            int safeCounter = 0;
            
            const int iterations = 100000;
            const int threads = 4;
            
            // UNSAFE increment
            var unsafeTasks = new Task[threads];
            for (int i = 0; i < threads; i++)
            {
                unsafeTasks[i] = Task.Run(() =>
                {
                    for (int j = 0; j < iterations; j++)
                    {
                        unsafeCounter++;  // NOT thread-safe
                    }
                });
            }
            Task.WaitAll(unsafeTasks);
            
            // SAFE increment with Interlocked
            var safeTasks = new Task[threads];
            for (int i = 0; i < threads; i++)
            {
                safeTasks[i] = Task.Run(() =>
                {
                    for (int j = 0; j < iterations; j++)
                    {
                        Interlocked.Increment(ref safeCounter);  // Thread-safe
                    }
                });
            }
            Task.WaitAll(safeTasks);
            
            int expected = iterations * threads;
            Console.WriteLine($"Expected count: {expected:N0}");
            Console.WriteLine($"Unsafe counter: {unsafeCounter:N0} (lost {expected - unsafeCounter:N0})");
            Console.WriteLine($"Safe counter:   {safeCounter:N0} (lost {expected - safeCounter:N0})");
            
            // Other Interlocked operations
            Console.WriteLine("\nOther Interlocked operations:");
            
            long value = 100;
            Console.WriteLine($"  Original: {value}");
            Console.WriteLine($"  Increment: {Interlocked.Increment(ref value)}");
            Console.WriteLine($"  Decrement: {Interlocked.Decrement(ref value)}");
            Console.WriteLine($"  Add 50: {Interlocked.Add(ref value, 50)}");
            Console.WriteLine($"  Exchange (200): {Interlocked.Exchange(ref value, 200)} → {value}");
            Console.WriteLine($"  CompareExchange (if 200, set 300): {Interlocked.CompareExchange(ref value, 300, 200)} → {value}");
        }
        
        #endregion
        
        #region Demo 9: ThreadPool
        
        static void Demo9_ThreadPool()
        {
            Console.WriteLine("Demonstrating ThreadPool...\n");
            
            // Get pool info
            ThreadPool.GetMinThreads(out int minWorker, out int minIO);
            ThreadPool.GetMaxThreads(out int maxWorker, out int maxIO);
            
            Console.WriteLine($"ThreadPool Configuration:");
            Console.WriteLine($"  Min Worker Threads: {minWorker}");
            Console.WriteLine($"  Max Worker Threads: {maxWorker}");
            Console.WriteLine($"  Min IO Threads: {minIO}");
            Console.WriteLine($"  Max IO Threads: {maxIO}\n");
            
            // Queue work items
            using var countdown = new CountdownEvent(5);
            
            Console.WriteLine("Queuing 5 work items to ThreadPool:\n");
            
            for (int i = 0; i < 5; i++)
            {
                int workId = i + 1;
                ThreadPool.QueueUserWorkItem(state =>
                {
                    Console.WriteLine($"Work item {workId} running on Thread {Thread.CurrentThread.ManagedThreadId} (IsThreadPoolThread: {Thread.CurrentThread.IsThreadPoolThread})");
                    Thread.Sleep(300);
                    countdown.Signal();
                });
            }
            
            countdown.Wait();
            Console.WriteLine("\n✓ All work items completed");
        }
        
        #endregion
        
        #region Demo 10: Task Parallel Library
        
        static void Demo10_TaskParallelLibrary()
        {
            Console.WriteLine("Demonstrating Task Parallel Library (TPL)...\n");
            
            // Basic Task creation
            Console.WriteLine("1. Basic Task creation:");
            var task1 = Task.Run(() => "Result from Task.Run");
            var task2 = Task.Factory.StartNew(() => "Result from Task.Factory.StartNew");
            Console.WriteLine($"   {task1.Result}");
            Console.WriteLine($"   {task2.Result}");
            
            // Task with return value
            Console.WriteLine("\n2. Task with return value:");
            var rateTask = Task.Run(() =>
            {
                Thread.Sleep(100);  // Simulate API call
                return new { Pair = "EUR/USD", Bid = 1.0850m, Ask = 1.0852m };
            });
            var rate = rateTask.Result;
            Console.WriteLine($"   {rate.Pair}: {rate.Bid}/{rate.Ask}");
            
            // Task continuation
            Console.WriteLine("\n3. Task continuation (ContinueWith):");
            Task.Run(() => "Step 1: Fetch rate")
                .ContinueWith(t => $"{t.Result} → Step 2: Validate")
                .ContinueWith(t => $"{t.Result} → Step 3: Execute")
                .ContinueWith(t => Console.WriteLine($"   {t.Result}"))
                .Wait();
            
            // WaitAll and WhenAll
            Console.WriteLine("\n4. WaitAll vs WhenAll:");
            var tasks = new Task<string>[3];
            for (int i = 0; i < 3; i++)
            {
                int idx = i;
                tasks[i] = Task.Run(() =>
                {
                    Thread.Sleep(100 * (idx + 1));
                    return $"Task {idx + 1} completed";
                });
            }
            
            Task.WaitAll(tasks);  // Blocks until all complete
            foreach (var t in tasks)
            {
                Console.WriteLine($"   {t.Result}");
            }
            
            // WhenAny
            Console.WriteLine("\n5. WhenAny (first completed):");
            var raceTasks = new[]
            {
                Task.Run(async () => { await Task.Delay(300); return "Reuters"; }),
                Task.Run(async () => { await Task.Delay(100); return "Bloomberg"; }),
                Task.Run(async () => { await Task.Delay(200); return "EBS"; })
            };
            var winner = Task.WhenAny(raceTasks).Result.Result;
            Console.WriteLine($"   First rate provider to respond: {winner}");
        }
        
        #endregion
        
        #region Demo 11: async/await
        
        static void Demo11_AsyncAwait()
        {
            Console.WriteLine("Demonstrating async/await pattern...\n");
            
            // Run async method synchronously for demo
            RunAsyncDemo().GetAwaiter().GetResult();
        }
        
        static async Task RunAsyncDemo()
        {
            Console.WriteLine("1. Simple async/await:");
            string result = await FetchRateAsync("EUR/USD");
            Console.WriteLine($"   {result}\n");
            
            Console.WriteLine("2. Multiple async calls (sequential):");
            var sw = Stopwatch.StartNew();
            var rate1 = await FetchRateAsync("EUR/USD");
            var rate2 = await FetchRateAsync("GBP/USD");
            var rate3 = await FetchRateAsync("USD/JPY");
            sw.Stop();
            Console.WriteLine($"   Sequential time: {sw.ElapsedMilliseconds}ms\n");
            
            Console.WriteLine("3. Multiple async calls (parallel):");
            sw.Restart();
            var tasks = new[]
            {
                FetchRateAsync("EUR/USD"),
                FetchRateAsync("GBP/USD"),
                FetchRateAsync("USD/JPY")
            };
            var results = await Task.WhenAll(tasks);
            sw.Stop();
            Console.WriteLine($"   Parallel time: {sw.ElapsedMilliseconds}ms");
            foreach (var r in results)
            {
                Console.WriteLine($"   {r}");
            }
            
            Console.WriteLine("\n4. ConfigureAwait(false):");
            Console.WriteLine($"   Before await: Thread {Thread.CurrentThread.ManagedThreadId}");
            await Task.Delay(100).ConfigureAwait(false);
            Console.WriteLine($"   After await (ConfigureAwait false): Thread {Thread.CurrentThread.ManagedThreadId}");
        }
        
        static async Task<string> FetchRateAsync(string pair)
        {
            await Task.Delay(200);  // Simulate async API call
            decimal rate = pair switch
            {
                "EUR/USD" => 1.0850m,
                "GBP/USD" => 1.2650m,
                "USD/JPY" => 149.50m,
                _ => 1.0m
            };
            return $"{pair}: {rate:F4}";
        }
        
        #endregion
        
        #region Demo 12: CancellationToken
        
        static void Demo12_CancellationToken()
        {
            Console.WriteLine("Demonstrating CancellationToken...\n");
            
            using var cts = new CancellationTokenSource();
            
            // Start a long-running task
            var task = Task.Run(async () =>
            {
                Console.WriteLine("[Task] Starting rate streaming...");
                int count = 0;
                
                try
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        count++;
                        Console.WriteLine($"[Task] Rate update #{count}");
                        
                        // This will throw if cancelled during delay
                        await Task.Delay(300, cts.Token);
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("[Task] Cancellation requested during delay");
                }
                
                Console.WriteLine($"[Task] Stopped after {count} updates");
            }, cts.Token);
            
            // Let it run for a bit
            Thread.Sleep(1500);
            
            // Cancel the task
            Console.WriteLine("\n[Main] Requesting cancellation...");
            cts.Cancel();
            
            try
            {
                task.Wait();
            }
            catch (AggregateException ex) when (ex.InnerException is OperationCanceledException)
            {
                Console.WriteLine("[Main] Task was cancelled");
            }
            
            Console.WriteLine($"\n[Main] Task status: {task.Status}");
        }
        
        #endregion
        
        #region Demo 13: Concurrent Collections
        
        static void Demo13_ConcurrentCollections()
        {
            Console.WriteLine("Demonstrating Concurrent Collections...\n");
            
            // ConcurrentDictionary
            Console.WriteLine("1. ConcurrentDictionary:");
            var rates = new ConcurrentDictionary<string, decimal>();
            
            var tasks = new Task[4];
            for (int i = 0; i < 4; i++)
            {
                int threadId = i;
                tasks[i] = Task.Run(() =>
                {
                    for (int j = 0; j < 100; j++)
                    {
                        string pair = $"PAIR_{j % 10}";
                        rates.AddOrUpdate(pair, 1.0m, (k, v) => v + 0.0001m);
                    }
                });
            }
            Task.WaitAll(tasks);
            Console.WriteLine($"   Dictionary has {rates.Count} pairs");
            
            // ConcurrentQueue
            Console.WriteLine("\n2. ConcurrentQueue:");
            var tradeQueue = new ConcurrentQueue<string>();
            
            var producer = Task.Run(() =>
            {
                for (int i = 0; i < 5; i++)
                {
                    tradeQueue.Enqueue($"Trade_{i}");
                    Thread.Sleep(50);
                }
            });
            
            var consumer = Task.Run(() =>
            {
                int processed = 0;
                while (processed < 5)
                {
                    if (tradeQueue.TryDequeue(out string? trade))
                    {
                        Console.WriteLine($"   Processed: {trade}");
                        processed++;
                    }
                    Thread.Sleep(30);
                }
            });
            
            Task.WaitAll(producer, consumer);
            
            // ConcurrentBag
            Console.WriteLine("\n3. ConcurrentBag:");
            var orderBag = new ConcurrentBag<string>();
            
            Parallel.For(0, 10, i =>
            {
                orderBag.Add($"Order_{i}");
            });
            
            Console.WriteLine($"   Bag has {orderBag.Count} orders");
            
            // BlockingCollection
            Console.WriteLine("\n4. BlockingCollection:");
            using var blockingCollection = new BlockingCollection<string>(boundedCapacity: 3);
            
            var bcProducer = Task.Run(() =>
            {
                for (int i = 0; i < 6; i++)
                {
                    blockingCollection.Add($"Item_{i}");
                    Console.WriteLine($"   [Producer] Added Item_{i}");
                }
                blockingCollection.CompleteAdding();
            });
            
            var bcConsumer = Task.Run(() =>
            {
                foreach (var item in blockingCollection.GetConsumingEnumerable())
                {
                    Console.WriteLine($"   [Consumer] Processing {item}");
                    Thread.Sleep(100);
                }
            });
            
            Task.WaitAll(bcProducer, bcConsumer);
        }
        
        #endregion
        
        #region Demo 14: Parallel Loops
        
        static void Demo14_ParallelLoops()
        {
            Console.WriteLine("Demonstrating Parallel.For and Parallel.ForEach...\n");
            
            // Parallel.For
            Console.WriteLine("1. Parallel.For:");
            var results = new ConcurrentBag<(int Index, int ThreadId)>();
            
            Parallel.For(0, 10, i =>
            {
                results.Add((i, Thread.CurrentThread.ManagedThreadId));
                Thread.Sleep(50);
            });
            
            Console.WriteLine($"   Processed {results.Count} items across threads: " +
                            $"{string.Join(", ", results.Select(r => r.ThreadId).Distinct())}");
            
            // Parallel.ForEach
            Console.WriteLine("\n2. Parallel.ForEach:");
            var pairs = new[] { "EUR/USD", "GBP/USD", "USD/JPY", "AUD/USD", "USD/CHF" };
            
            Parallel.ForEach(pairs, pair =>
            {
                Console.WriteLine($"   Processing {pair} on Thread {Thread.CurrentThread.ManagedThreadId}");
                Thread.Sleep(100);
            });
            
            // With ParallelOptions
            Console.WriteLine("\n3. Parallel with MaxDegreeOfParallelism:");
            var options = new ParallelOptions { MaxDegreeOfParallelism = 2 };
            
            Parallel.ForEach(pairs, options, pair =>
            {
                Console.WriteLine($"   Processing {pair} on Thread {Thread.CurrentThread.ManagedThreadId}");
                Thread.Sleep(100);
            });
        }
        
        #endregion
        
        #region Demo 15: Producer-Consumer Pattern
        
        static void Demo15_ProducerConsumer()
        {
            Console.WriteLine("Demonstrating Producer-Consumer pattern...\n");
            
            using var buffer = new BlockingCollection<FxRateUpdate>(boundedCapacity: 5);
            using var cts = new CancellationTokenSource();
            
            // Producer (rate feed)
            var producer = Task.Run(() =>
            {
                var random = new Random();
                var pairs = new[] { "EUR/USD", "GBP/USD", "USD/JPY" };
                
                for (int i = 0; i < 10 && !cts.Token.IsCancellationRequested; i++)
                {
                    var update = new FxRateUpdate
                    {
                        Pair = pairs[i % pairs.Length],
                        Rate = 1.0m + (decimal)random.NextDouble() * 0.1m,
                        Timestamp = DateTime.UtcNow
                    };
                    
                    buffer.Add(update, cts.Token);
                    Console.WriteLine($"[Producer] Published: {update}");
                    Thread.Sleep(100);
                }
                
                buffer.CompleteAdding();
                Console.WriteLine("[Producer] Completed");
            }, cts.Token);
            
            // Consumer (rate processor)
            var consumer = Task.Run(() =>
            {
                foreach (var update in buffer.GetConsumingEnumerable(cts.Token))
                {
                    Console.WriteLine($"[Consumer] Processing: {update}");
                    Thread.Sleep(200);  // Simulate processing
                }
                Console.WriteLine("[Consumer] Completed");
            }, cts.Token);
            
            Task.WaitAll(producer, consumer);
        }
        
        class FxRateUpdate
        {
            public string Pair { get; set; } = "";
            public decimal Rate { get; set; }
            public DateTime Timestamp { get; set; }
            
            public override string ToString() => $"{Pair}: {Rate:F5} @ {Timestamp:HH:mm:ss.fff}";
        }
        
        #endregion
        
        #region Demo 16: Thread-Safe Singleton
        
        static void Demo16_ThreadSafeSingleton()
        {
            Console.WriteLine("Demonstrating Thread-Safe Singleton patterns...\n");
            
            // Pattern 1: Lazy<T> (recommended)
            Console.WriteLine("1. Lazy<T> pattern (recommended):");
            var instances = new ConcurrentBag<int>();
            
            Parallel.For(0, 10, _ =>
            {
                var instance = RateServiceSingleton.Instance;
                instances.Add(instance.GetHashCode());
            });
            
            Console.WriteLine($"   All instances same: {instances.Distinct().Count() == 1}");
            Console.WriteLine($"   Instance hash: {instances.First()}");
            
            // Pattern 2: Double-check locking
            Console.WriteLine("\n2. Double-check locking pattern:");
            instances = new ConcurrentBag<int>();
            
            Parallel.For(0, 10, _ =>
            {
                var instance = TradingEngineSingleton.Instance;
                instances.Add(instance.GetHashCode());
            });
            
            Console.WriteLine($"   All instances same: {instances.Distinct().Count() == 1}");
            Console.WriteLine($"   Instance hash: {instances.First()}");
        }
        
        // Pattern 1: Using Lazy<T>
        class RateServiceSingleton
        {
            private static readonly Lazy<RateServiceSingleton> _lazy = 
                new Lazy<RateServiceSingleton>(() => new RateServiceSingleton());
            
            public static RateServiceSingleton Instance => _lazy.Value;
            
            private RateServiceSingleton()
            {
                Console.WriteLine("   [RateServiceSingleton] Instance created");
            }
        }
        
        // Pattern 2: Double-check locking
        class TradingEngineSingleton
        {
            private static TradingEngineSingleton? _instance;
            private static readonly object _lock = new object();
            
            public static TradingEngineSingleton Instance
            {
                get
                {
                    if (_instance == null)
                    {
                        lock (_lock)
                        {
                            if (_instance == null)
                            {
                                _instance = new TradingEngineSingleton();
                            }
                        }
                    }
                    return _instance;
                }
            }
            
            private TradingEngineSingleton()
            {
                Console.WriteLine("   [TradingEngineSingleton] Instance created");
            }
        }
        
        #endregion
        
        #region Demo 17: Deadlock Prevention
        
        static void Demo17_DeadlockPrevention()
        {
            Console.WriteLine("Demonstrating Deadlock scenario and prevention...\n");
            
            var accountA = new BankAccount("A", 1000);
            var accountB = new BankAccount("B", 1000);
            
            // DEADLOCK SCENARIO (commented out - would hang!)
            Console.WriteLine("1. Deadlock Scenario (conceptual - not running):");
            Console.WriteLine("   Thread 1: Lock(A) → Lock(B)");
            Console.WriteLine("   Thread 2: Lock(B) → Lock(A)");
            Console.WriteLine("   Both threads wait forever!\n");
            
            // PREVENTION: Lock ordering
            Console.WriteLine("2. Prevention: Consistent lock ordering:");
            Console.WriteLine("   Always lock accounts in ID order\n");
            
            var tasks = new[]
            {
                Task.Run(() => TransferSafe(accountA, accountB, 100)),
                Task.Run(() => TransferSafe(accountB, accountA, 50)),
                Task.Run(() => TransferSafe(accountA, accountB, 75)),
                Task.Run(() => TransferSafe(accountB, accountA, 25))
            };
            
            Task.WaitAll(tasks);
            
            Console.WriteLine($"\nFinal balances: A={accountA.Balance:C}, B={accountB.Balance:C}");
            Console.WriteLine($"Total: {accountA.Balance + accountB.Balance:C} (should be $2,000)");
        }
        
        class BankAccount
        {
            public string Id { get; }
            public decimal Balance { get; private set; }
            public object Lock { get; } = new object();
            
            public BankAccount(string id, decimal balance)
            {
                Id = id;
                Balance = balance;
            }
            
            public void Debit(decimal amount) => Balance -= amount;
            public void Credit(decimal amount) => Balance += amount;
        }
        
        static void TransferSafe(BankAccount from, BankAccount to, decimal amount)
        {
            // Always lock in consistent order (by ID)
            var first = string.CompareOrdinal(from.Id, to.Id) < 0 ? from : to;
            var second = first == from ? to : from;
            
            lock (first.Lock)
            {
                lock (second.Lock)
                {
                    if (from.Balance >= amount)
                    {
                        from.Debit(amount);
                        to.Credit(amount);
                        Console.WriteLine($"   Transferred {amount:C} from {from.Id} to {to.Id}");
                    }
                }
            }
        }
        
        #endregion
        
        #region Demo 18: Race Condition
        
        static void Demo18_RaceCondition()
        {
            Console.WriteLine("Demonstrating Race Condition...\n");
            
            // UNSAFE: Race condition in counter
            int unsafeCounter = 0;
            var unsafeTasks = new Task[4];
            
            for (int i = 0; i < 4; i++)
            {
                unsafeTasks[i] = Task.Run(() =>
                {
                    for (int j = 0; j < 10000; j++)
                    {
                        unsafeCounter++;  // Read-modify-write: NOT atomic!
                    }
                });
            }
            Task.WaitAll(unsafeTasks);
            
            Console.WriteLine("1. Race Condition in Counter:");
            Console.WriteLine($"   Expected: 40,000");
            Console.WriteLine($"   Actual (unsafe): {unsafeCounter:N0}");
            Console.WriteLine($"   Lost updates: {40000 - unsafeCounter:N0}\n");
            
            // SAFE: Using Interlocked
            int safeCounter = 0;
            var safeTasks = new Task[4];
            
            for (int i = 0; i < 4; i++)
            {
                safeTasks[i] = Task.Run(() =>
                {
                    for (int j = 0; j < 10000; j++)
                    {
                        Interlocked.Increment(ref safeCounter);  // Atomic!
                    }
                });
            }
            Task.WaitAll(safeTasks);
            
            Console.WriteLine("2. Fixed with Interlocked:");
            Console.WriteLine($"   Actual (safe): {safeCounter:N0}");
            Console.WriteLine($"   Lost updates: {40000 - safeCounter:N0}");
        }
        
        #endregion
    }
}
