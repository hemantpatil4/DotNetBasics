# Multithreading in C# – Complete Interview Guide

> **Target Audience:** 3+ years .NET developer preparing for product-based company interviews  
> **Domain Focus:** FX Trading Systems, Rate Streaming, Real-time Data Processing

---

## Table of Contents

1. [Fundamentals - Process vs Thread](#1-fundamentals---process-vs-thread)
2. [Thread Basics](#2-thread-basics)
3. [Thread Lifecycle & States](#3-thread-lifecycle--states)
4. [Thread Pool](#4-thread-pool)
5. [Synchronization Primitives](#5-synchronization-primitives)
6. [Thread Safety Concepts](#6-thread-safety-concepts)
7. [Race Conditions](#7-race-conditions)
8. [Deadlocks](#8-deadlocks)
9. [Async/Await vs Threads](#9-asyncawait-vs-threads)
10. [Concurrent Collections](#10-concurrent-collections)
11. [Task Parallel Library (TPL)](#11-task-parallel-library-tpl)
12. [Cancellation Tokens](#12-cancellation-tokens)
13. [Best Practices](#13-best-practices)
14. [Interview Questions](#14-interview-questions)

---

## 1. Fundamentals - Process vs Thread

### 1.1 What is a Process?

A **process** is an instance of a running program with its own memory space, resources, and at least one thread.

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           PROCESS                                       │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                      PROCESS MEMORY                              │   │
│  │  ┌───────────┐  ┌───────────┐  ┌───────────┐  ┌───────────┐    │   │
│  │  │   CODE    │  │   DATA    │  │   HEAP    │  │  RESOURCES│    │   │
│  │  │ (Program) │  │ (Global)  │  │ (Dynamic) │  │  (Files,  │    │   │
│  │  │           │  │           │  │           │  │  Sockets) │    │   │
│  │  └───────────┘  └───────────┘  └───────────┘  └───────────┘    │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  ┌───────────────────────────────────────────────────────────────┐     │
│  │                        THREADS                                 │     │
│  │  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐          │     │
│  │  │Thread 1 │  │Thread 2 │  │Thread 3 │  │Thread N │          │     │
│  │  │ (Main)  │  │         │  │         │  │         │          │     │
│  │  │┌───────┐│  │┌───────┐│  │┌───────┐│  │┌───────┐│          │     │
│  │  ││ Stack ││  ││ Stack ││  ││ Stack ││  ││ Stack ││          │     │
│  │  │└───────┘│  │└───────┘│  │└───────┘│  │└───────┘│          │     │
│  │  └─────────┘  └─────────┘  └─────────┘  └─────────┘          │     │
│  └───────────────────────────────────────────────────────────────┘     │
│                                                                         │
│  Threads SHARE: Code, Data, Heap, Resources                             │
│  Threads OWN: Their own Stack, Registers, Program Counter               │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 1.2 What is a Thread?

A **thread** is the smallest unit of execution within a process. Multiple threads share the process's memory but have their own:

- Stack (local variables)
- Program counter (current instruction)
- Registers

### 1.3 Process vs Thread Comparison

| Aspect             | Process                                    | Thread                              |
| ------------------ | ------------------------------------------ | ----------------------------------- |
| **Memory**         | Separate memory space                      | Shared memory space                 |
| **Creation cost**  | Heavy (new memory allocation)              | Light (shares parent's memory)      |
| **Communication**  | IPC (Inter-Process Communication)          | Direct memory access                |
| **Crash impact**   | Isolated (one crash doesn't affect others) | Shared (one crash can kill process) |
| **Context switch** | Expensive                                  | Cheaper                             |
| **Use case**       | Isolation, security                        | Parallelism, responsiveness         |

### 1.4 Why Use Multithreading?

```csharp
// FX Trading Example: Without multithreading
public class SingleThreadedRateProcessor
{
    public void ProcessRates()
    {
        // ❌ Sequential - SLOW!
        FetchRatesFromReuters();      // 100ms
        FetchRatesFromBloomberg();    // 100ms
        FetchRatesFromEBS();          // 100ms
        CalculateMidRates();          // 50ms
        PublishToClients();           // 50ms
        // Total: 400ms - Rates are STALE!
    }
}

// ✅ With multithreading - Parallel fetching
public class MultiThreadedRateProcessor
{
    public void ProcessRates()
    {
        // Fetch from all sources in parallel
        var tasks = new[]
        {
            Task.Run(() => FetchRatesFromReuters()),
            Task.Run(() => FetchRatesFromBloomberg()),
            Task.Run(() => FetchRatesFromEBS())
        };
        Task.WaitAll(tasks);  // ~100ms (parallel)

        CalculateMidRates();   // 50ms
        PublishToClients();    // 50ms
        // Total: ~200ms - Much fresher rates!
    }
}
```

**Benefits:**

1. **Responsiveness**: UI remains responsive during long operations
2. **Performance**: Utilize multiple CPU cores
3. **Scalability**: Handle more concurrent requests
4. **Resource efficiency**: Overlap I/O wait times

---

## 2. Thread Basics

### 2.1 Creating Threads

```csharp
using System;
using System.Threading;

public class ThreadBasics
{
    // Method 1: Thread with ThreadStart delegate (no parameters)
    public void CreateThread_NoParams()
    {
        Thread thread = new Thread(DoWork);
        thread.Start();
    }

    private void DoWork()
    {
        Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} doing work");
    }

    // Method 2: Thread with ParameterizedThreadStart (with parameter)
    public void CreateThread_WithParams()
    {
        Thread thread = new Thread(DoWorkWithParam);
        thread.Start("EUR/USD");  // Pass parameter
    }

    private void DoWorkWithParam(object? currencyPair)
    {
        Console.WriteLine($"Processing {currencyPair}");
    }

    // Method 3: Lambda expression (most common)
    public void CreateThread_Lambda()
    {
        string pair = "GBP/USD";
        decimal rate = 1.2500m;

        Thread thread = new Thread(() =>
        {
            // Can capture variables from outer scope
            Console.WriteLine($"Rate for {pair}: {rate}");
        });
        thread.Start();
    }

    // Method 4: Thread with return value (using shared state)
    public void CreateThread_WithResult()
    {
        decimal result = 0;

        Thread thread = new Thread(() =>
        {
            result = CalculateSpread("EUR/USD");
        });

        thread.Start();
        thread.Join();  // Wait for completion

        Console.WriteLine($"Spread: {result}");
    }

    private decimal CalculateSpread(string pair) => 0.0002m;
}
```

### 2.2 Thread Properties

```csharp
public void DemonstrateThreadProperties()
{
    Thread thread = new Thread(LongRunningTask);

    // Name - for debugging
    thread.Name = "RateStreamingThread";

    // Priority - hint to scheduler (use sparingly!)
    thread.Priority = ThreadPriority.AboveNormal;

    // IsBackground - daemon thread behavior
    // Background threads don't prevent app from exiting
    thread.IsBackground = true;

    // Check if thread is alive
    Console.WriteLine($"IsAlive before start: {thread.IsAlive}");  // false

    thread.Start();

    Console.WriteLine($"IsAlive after start: {thread.IsAlive}");   // true
    Console.WriteLine($"ThreadState: {thread.ThreadState}");       // Running
    Console.WriteLine($"ManagedThreadId: {thread.ManagedThreadId}");

    // Current thread info
    Thread current = Thread.CurrentThread;
    Console.WriteLine($"Current thread: {current.Name}");
}

private void LongRunningTask()
{
    Thread.Sleep(5000);  // Simulate work
}
```

### 2.3 Foreground vs Background Threads

```
┌─────────────────────────────────────────────────────────────────────────┐
│                 FOREGROUND vs BACKGROUND THREADS                        │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  FOREGROUND (IsBackground = false - default)                            │
│  ──────────────────────────────────────────                             │
│  • Application waits for ALL foreground threads to complete             │
│  • Even if Main() returns, app keeps running until threads finish       │
│  • Use for: Critical tasks that MUST complete                           │
│                                                                         │
│  BACKGROUND (IsBackground = true)                                       │
│  ────────────────────────────────                                       │
│  • Application can exit even if background threads are running          │
│  • Threads are terminated abruptly when app exits                       │
│  • Use for: Non-critical tasks, monitoring, logging                     │
│                                                                         │
│  Example:                                                               │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │ Main Thread ──────────────────────────────────► Exit            │   │
│  │                                                                  │   │
│  │ Foreground  ─────────────────────────────────────────────► Done │   │
│  │ Thread      (App waits for this)                                │   │
│  │                                                                  │   │
│  │ Background  ─────────────X (Terminated when app exits)          │   │
│  │ Thread                                                           │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

```csharp
// FX Example: Background thread for rate monitoring
public class RateMonitor
{
    private Thread _monitorThread;
    private volatile bool _running = true;

    public void StartMonitoring()
    {
        _monitorThread = new Thread(MonitorLoop)
        {
            IsBackground = true,  // Won't prevent app shutdown
            Name = "RateMonitor"
        };
        _monitorThread.Start();
    }

    private void MonitorLoop()
    {
        while (_running)
        {
            CheckRateHealth();
            Thread.Sleep(1000);
        }
    }

    public void StopMonitoring()
    {
        _running = false;
    }

    private void CheckRateHealth() { /* ... */ }
}
```

### 2.4 Thread Join - Waiting for Completion

```csharp
public void DemonstrateJoin()
{
    var threads = new List<Thread>();

    // Create threads for fetching rates from different sources
    foreach (var source in new[] { "Reuters", "Bloomberg", "EBS" })
    {
        var thread = new Thread(() => FetchRates(source));
        threads.Add(thread);
        thread.Start();
    }

    // Wait for ALL threads to complete
    foreach (var thread in threads)
    {
        thread.Join();  // Blocks until thread completes
    }

    Console.WriteLine("All rate sources fetched!");

    // Join with timeout
    Thread longThread = new Thread(LongOperation);
    longThread.Start();

    bool completed = longThread.Join(TimeSpan.FromSeconds(5));
    if (!completed)
    {
        Console.WriteLine("Thread didn't complete in time!");
    }
}

private void FetchRates(string source)
{
    Thread.Sleep(100);  // Simulate fetch
    Console.WriteLine($"Fetched from {source}");
}

private void LongOperation()
{
    Thread.Sleep(10000);
}
```

---

## 3. Thread Lifecycle & States

### 3.1 Thread States

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    THREAD LIFECYCLE STATES                              │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│                        ┌─────────────┐                                  │
│                        │  Unstarted  │                                  │
│                        │ (new Thread)│                                  │
│                        └──────┬──────┘                                  │
│                               │ Start()                                 │
│                               ▼                                         │
│                        ┌─────────────┐                                  │
│           ┌───────────►│   Running   │◄───────────┐                    │
│           │            └──────┬──────┘            │                    │
│           │                   │                   │                    │
│           │     ┌─────────────┼─────────────┐     │                    │
│           │     │             │             │     │                    │
│           │     ▼             ▼             ▼     │                    │
│      ┌────┴─────────┐  ┌───────────┐  ┌──────────┴───┐                 │
│      │ WaitSleepJoin│  │  Blocked  │  │  Suspended   │                 │
│      │              │  │ (waiting  │  │ (deprecated) │                 │
│      │ Sleep()      │  │  for lock)│  │              │                 │
│      │ Join()       │  │           │  │              │                 │
│      │ Wait()       │  │           │  │              │                 │
│      └──────────────┘  └───────────┘  └──────────────┘                 │
│           │                   │             │                          │
│           │                   │             │                          │
│           │     timeout/      │   lock      │   Resume()               │
│           │     signal        │   acquired  │                          │
│           │                   │             │                          │
│           └───────────────────┴─────────────┘                          │
│                               │                                         │
│                               │ Method completes / Exception            │
│                               ▼                                         │
│                        ┌─────────────┐                                  │
│                        │   Stopped   │                                  │
│                        │  (Dead)     │                                  │
│                        └─────────────┘                                  │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 3.2 ThreadState Enum

```csharp
public void CheckThreadStates()
{
    Thread thread = new Thread(SomeWork);

    Console.WriteLine($"After creation: {thread.ThreadState}");  // Unstarted

    thread.Start();
    Console.WriteLine($"After start: {thread.ThreadState}");     // Running

    Thread.Sleep(50);  // Let it get to Sleep
    Console.WriteLine($"During sleep: {thread.ThreadState}");    // WaitSleepJoin

    thread.Join();
    Console.WriteLine($"After completion: {thread.ThreadState}"); // Stopped
}

private void SomeWork()
{
    Thread.Sleep(100);
}

// Thread state is a bit flag - can be combination
public void AnalyzeThreadState(Thread thread)
{
    ThreadState state = thread.ThreadState;

    if ((state & ThreadState.Unstarted) != 0)
        Console.WriteLine("Thread has not been started");

    if ((state & ThreadState.Running) != 0)
        Console.WriteLine("Thread is running");

    if ((state & ThreadState.WaitSleepJoin) != 0)
        Console.WriteLine("Thread is waiting/sleeping");

    if ((state & ThreadState.Stopped) != 0)
        Console.WriteLine("Thread has completed");

    if ((state & ThreadState.Background) != 0)
        Console.WriteLine("Thread is background thread");
}
```

---

## 4. Thread Pool

### 4.1 Why Thread Pool?

Creating threads is expensive:

- Memory allocation for stack (~1MB per thread)
- OS kernel object creation
- Context switching overhead

**Thread Pool** maintains a pool of reusable worker threads.

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         THREAD POOL                                     │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  WITHOUT POOL:                     WITH POOL:                           │
│  ─────────────                     ──────────                           │
│                                                                         │
│  Task1 → Create Thread             Task1 ─┐                             │
│          │    Execute              Task2 ─┼─► Thread Pool ─► Execute    │
│          │    Destroy              Task3 ─┤   (Reuses threads)          │
│          ▼                         Task4 ─┘                             │
│  Task2 → Create Thread                                                  │
│          │    Execute              ┌─────────────────────────────┐     │
│          │    Destroy              │ Worker  Worker  Worker  ... │     │
│          ▼                         │ Thread  Thread  Thread      │     │
│  Task3 → Create Thread             └─────────────────────────────┘     │
│          ...                                                            │
│                                                                         │
│  Overhead: High                    Overhead: Low                        │
│  (Create/destroy each time)        (Threads reused)                     │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 4.2 Using ThreadPool

```csharp
public class ThreadPoolExamples
{
    // Method 1: QueueUserWorkItem
    public void UseThreadPool_Basic()
    {
        // Queue work item - simplest way
        ThreadPool.QueueUserWorkItem(_ =>
        {
            Console.WriteLine($"Running on pool thread {Thread.CurrentThread.ManagedThreadId}");
            ProcessRate("EUR/USD", 1.0850m);
        });

        // With state parameter
        ThreadPool.QueueUserWorkItem(state =>
        {
            var (pair, rate) = ((string, decimal))state!;
            ProcessRate(pair, rate);
        }, ("GBP/USD", 1.2650m));
    }

    // Method 2: Using Task (preferred in modern C#)
    public void UseThreadPool_Task()
    {
        // Task.Run uses thread pool internally
        Task.Run(() =>
        {
            ProcessRate("USD/JPY", 149.50m);
        });

        // With return value
        Task<decimal> task = Task.Run(() =>
        {
            return CalculateSpread("EUR/USD");
        });

        decimal spread = task.Result;  // Blocks until complete
    }

    // Thread pool configuration
    public void ConfigureThreadPool()
    {
        // Get current limits
        ThreadPool.GetMinThreads(out int minWorker, out int minIO);
        ThreadPool.GetMaxThreads(out int maxWorker, out int maxIO);

        Console.WriteLine($"Worker threads: {minWorker} - {maxWorker}");
        Console.WriteLine($"I/O threads: {minIO} - {maxIO}");

        // Set minimum (increases responsiveness for burst loads)
        // Use carefully - too high wastes memory
        ThreadPool.SetMinThreads(10, 10);
    }

    private void ProcessRate(string pair, decimal rate) { }
    private decimal CalculateSpread(string pair) => 0.0002m;
}
```

### 4.3 Thread vs ThreadPool vs Task

| Aspect                 | Thread                  | ThreadPool             | Task               |
| ---------------------- | ----------------------- | ---------------------- | ------------------ |
| **Creation cost**      | High                    | Low (reused)           | Low (uses pool)    |
| **Control**            | Full control            | Limited                | Good balance       |
| **Priority**           | Yes                     | No                     | No                 |
| **IsBackground**       | Configurable            | Always true            | Always true        |
| **Return value**       | Manual                  | Manual                 | Built-in           |
| **Exception handling** | Crashes thread          | Swallowed              | Propagated         |
| **Cancellation**       | Manual                  | Manual                 | CancellationToken  |
| **Continuation**       | Manual                  | Manual                 | ContinueWith/await |
| **Use case**           | Long-running, dedicated | Short, fire-and-forget | General purpose    |

---

## 5. Synchronization Primitives

### 5.1 The Problem: Shared State

```csharp
// ❌ UNSAFE: Race condition!
public class UnsafeCounter
{
    private int _count = 0;

    public void Increment()
    {
        _count++;  // NOT atomic! Read-Modify-Write
    }

    public int Count => _count;
}

// What happens with _count++:
// 1. Read _count into register    (value = 0)
// 2. Add 1 to register            (value = 1)
// 3. Write register to _count     (_count = 1)
//
// If two threads do this simultaneously:
// Thread A: Read 0, Add 1, Write 1
// Thread B: Read 0, Add 1, Write 1
// Result: _count = 1 (should be 2!)
```

### 5.2 lock Statement (Monitor)

The most common synchronization mechanism.

```csharp
public class ThreadSafeCounter
{
    private int _count = 0;
    private readonly object _lock = new object();  // Lock object

    public void Increment()
    {
        lock (_lock)  // Only one thread can enter at a time
        {
            _count++;
        }
    }

    public void Decrement()
    {
        lock (_lock)
        {
            _count--;
        }
    }

    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _count;
            }
        }
    }
}

// lock is syntactic sugar for:
public void IncrementExplicit()
{
    bool lockTaken = false;
    try
    {
        Monitor.Enter(_lock, ref lockTaken);
        _count++;
    }
    finally
    {
        if (lockTaken)
            Monitor.Exit(_lock);
    }
}
```

**Lock Best Practices:**

```csharp
public class LockBestPractices
{
    // ✅ GOOD: Private readonly object
    private readonly object _syncRoot = new object();

    // ❌ BAD: Locking on 'this'
    // External code can also lock on your object causing deadlock
    public void BadLock()
    {
        lock (this) { }  // Don't do this!
    }

    // ❌ BAD: Locking on Type
    public void BadLockOnType()
    {
        lock (typeof(LockBestPractices)) { }  // Don't do this!
    }

    // ❌ BAD: Locking on string
    public void BadLockOnString()
    {
        lock ("mylock") { }  // Strings are interned - dangerous!
    }

    // ✅ GOOD: Dedicated lock object
    public void GoodLock()
    {
        lock (_syncRoot)
        {
            // Protected code
        }
    }
}
```

### 5.3 Monitor Class

More control than `lock` statement.

```csharp
public class MonitorExample
{
    private readonly object _lock = new object();
    private Queue<decimal> _rateQueue = new Queue<decimal>();

    // TryEnter - don't block forever
    public bool TryProcessRate(decimal rate, int timeoutMs)
    {
        bool lockTaken = false;
        try
        {
            Monitor.TryEnter(_lock, timeoutMs, ref lockTaken);
            if (lockTaken)
            {
                ProcessRate(rate);
                return true;
            }
            return false;  // Couldn't acquire lock
        }
        finally
        {
            if (lockTaken)
                Monitor.Exit(_lock);
        }
    }

    // Wait/Pulse - Producer-Consumer pattern
    public void Producer()
    {
        lock (_lock)
        {
            _rateQueue.Enqueue(1.0850m);
            Monitor.Pulse(_lock);  // Wake up ONE waiting consumer
            // Monitor.PulseAll(_lock);  // Wake up ALL waiting consumers
        }
    }

    public decimal Consumer()
    {
        lock (_lock)
        {
            while (_rateQueue.Count == 0)
            {
                Monitor.Wait(_lock);  // Release lock and wait for Pulse
            }
            return _rateQueue.Dequeue();
        }
    }

    private void ProcessRate(decimal rate) { }
}
```

### 5.4 Mutex

Cross-process synchronization.

```csharp
public class MutexExample
{
    // Named mutex - works across processes
    public void EnsureSingleInstance()
    {
        const string mutexName = "Global\\FXRateEngine";

        using (var mutex = new Mutex(false, mutexName, out bool createdNew))
        {
            if (!createdNew)
            {
                Console.WriteLine("Another instance is already running!");
                return;
            }

            // We are the only instance
            RunRateEngine();
        }
    }

    // Local mutex - within process
    private readonly Mutex _localMutex = new Mutex();

    public void UseLocalMutex()
    {
        _localMutex.WaitOne();  // Acquire
        try
        {
            // Critical section
        }
        finally
        {
            _localMutex.ReleaseMutex();  // Release
        }
    }

    private void RunRateEngine() { }
}
```

### 5.5 Semaphore

Limit concurrent access (e.g., connection pool, rate limiting).

```csharp
public class SemaphoreExample
{
    // Allow max 5 concurrent rate fetches
    private readonly SemaphoreSlim _rateLimiter = new SemaphoreSlim(5, 5);

    public async Task FetchRateAsync(string pair)
    {
        await _rateLimiter.WaitAsync();  // Decrement count
        try
        {
            await FetchFromProviderAsync(pair);
        }
        finally
        {
            _rateLimiter.Release();  // Increment count
        }
    }

    // Cross-process semaphore
    public void UseCrossProcessSemaphore()
    {
        using (var semaphore = new Semaphore(3, 3, "Global\\RateFetchLimit"))
        {
            semaphore.WaitOne();
            try
            {
                // Only 3 processes can be here simultaneously
            }
            finally
            {
                semaphore.Release();
            }
        }
    }

    private Task FetchFromProviderAsync(string pair) => Task.Delay(100);
}
```

### 5.6 ReaderWriterLockSlim

Optimized for read-heavy workloads (like rate caches).

```csharp
public class RateCache
{
    private readonly Dictionary<string, decimal> _rates = new();
    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

    // Multiple readers can read simultaneously
    public decimal GetRate(string pair)
    {
        _lock.EnterReadLock();
        try
        {
            return _rates.TryGetValue(pair, out var rate) ? rate : 0;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    // Only one writer at a time, blocks readers
    public void UpdateRate(string pair, decimal rate)
    {
        _lock.EnterWriteLock();
        try
        {
            _rates[pair] = rate;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    // Upgrade from read to write (check then modify pattern)
    public void UpdateIfStale(string pair, decimal newRate, TimeSpan maxAge)
    {
        _lock.EnterUpgradeableReadLock();
        try
        {
            // Check condition under read lock
            bool needsUpdate = ShouldUpdate(pair, maxAge);

            if (needsUpdate)
            {
                _lock.EnterWriteLock();
                try
                {
                    _rates[pair] = newRate;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }
        finally
        {
            _lock.ExitUpgradeableReadLock();
        }
    }

    private bool ShouldUpdate(string pair, TimeSpan maxAge) => true;

    // Clean up
    public void Dispose()
    {
        _lock.Dispose();
    }
}
```

### 5.7 Interlocked

Atomic operations without locks (best performance).

```csharp
public class InterlockedExample
{
    private long _counter = 0;
    private long _rateUpdateCount = 0;

    // Atomic increment
    public void IncrementCounter()
    {
        Interlocked.Increment(ref _counter);
    }

    // Atomic decrement
    public void DecrementCounter()
    {
        Interlocked.Decrement(ref _counter);
    }

    // Atomic add
    public void AddToCounter(long value)
    {
        Interlocked.Add(ref _counter, value);
    }

    // Atomic exchange (set and return old value)
    public long ResetCounter()
    {
        return Interlocked.Exchange(ref _counter, 0);
    }

    // Atomic compare-and-swap (CAS)
    public bool TryUpdateIfEqual(long expectedValue, long newValue)
    {
        // Only updates if current value equals expected
        long original = Interlocked.CompareExchange(
            ref _counter, newValue, expectedValue);
        return original == expectedValue;
    }

    // Read with memory barrier
    public long GetCounter()
    {
        return Interlocked.Read(ref _counter);
    }
}

// Real FX example: Thread-safe rate statistics
public class RateStatistics
{
    private long _updateCount = 0;
    private long _totalLatencyTicks = 0;

    public void RecordUpdate(TimeSpan latency)
    {
        Interlocked.Increment(ref _updateCount);
        Interlocked.Add(ref _totalLatencyTicks, latency.Ticks);
    }

    public (long Count, TimeSpan AverageLatency) GetStats()
    {
        long count = Interlocked.Read(ref _updateCount);
        long totalTicks = Interlocked.Read(ref _totalLatencyTicks);

        var avgLatency = count > 0
            ? TimeSpan.FromTicks(totalTicks / count)
            : TimeSpan.Zero;

        return (count, avgLatency);
    }
}
```

### 5.8 Synchronization Primitives Comparison

```
┌─────────────────────────────────────────────────────────────────────────┐
│                 SYNCHRONIZATION PRIMITIVES COMPARISON                   │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Primitive          │ Scope        │ Use Case                           │
│  ───────────────────┼──────────────┼────────────────────────────────    │
│  lock (Monitor)     │ In-process   │ General purpose mutual exclusion   │
│  Mutex              │ Cross-process│ Single instance apps, IPC          │
│  Semaphore          │ Cross-process│ Resource pool limiting             │
│  SemaphoreSlim      │ In-process   │ Async-friendly resource limiting   │
│  ReaderWriterLock   │ In-process   │ Read-heavy caches                  │
│  Interlocked        │ In-process   │ Simple counters, flags             │
│  SpinLock           │ In-process   │ Very short critical sections       │
│  ManualResetEvent   │ In-process   │ Signal once, many waiters          │
│  AutoResetEvent     │ In-process   │ Signal one waiter at a time        │
│                                                                         │
│  Performance (fastest to slowest for simple operations):                │
│  1. Interlocked (no locking overhead)                                   │
│  2. SpinLock (busy wait, good for short waits)                          │
│  3. lock/Monitor (kernel transition if contended)                       │
│  4. ReaderWriterLockSlim (more overhead, but optimized for reads)       │
│  5. Mutex (kernel object, cross-process capable)                        │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 6. Thread Safety Concepts

### 6.1 What is Thread Safety?

A piece of code is **thread-safe** if it behaves correctly when accessed from multiple threads simultaneously, with no additional synchronization from the caller.

### 6.2 Levels of Thread Safety

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    THREAD SAFETY LEVELS                                 │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  1. IMMUTABLE (Highest Safety)                                          │
│     • Object cannot be modified after creation                          │
│     • Always thread-safe                                                │
│     • Example: string, DateTime, records                                │
│                                                                         │
│  2. THREAD-SAFE (Full Safety)                                           │
│     • All methods can be called from multiple threads safely            │
│     • Internal synchronization handles everything                       │
│     • Example: ConcurrentDictionary, BlockingCollection                 │
│                                                                         │
│  3. CONDITIONALLY THREAD-SAFE                                           │
│     • Safe for specific operations                                      │
│     • May need external synchronization for others                      │
│     • Example: Dictionary (safe reads, unsafe writes)                   │
│                                                                         │
│  4. NOT THREAD-SAFE                                                     │
│     • Caller must synchronize all access                                │
│     • Example: List<T>, StringBuilder                                   │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 6.3 Volatile Keyword

Ensures reads/writes go directly to main memory, not CPU cache.

```csharp
public class VolatileExample
{
    // Without volatile, compiler/CPU might optimize away the read
    private volatile bool _running = true;

    public void WorkerLoop()
    {
        while (_running)  // Without volatile, might be optimized to while(true)
        {
            DoWork();
        }
    }

    public void Stop()
    {
        _running = false;  // Immediately visible to WorkerLoop
    }

    private void DoWork() { }
}

// Note: volatile is NOT enough for read-modify-write operations
public class VolatileNotEnough
{
    private volatile int _counter = 0;

    public void Increment()
    {
        _counter++;  // STILL NOT THREAD-SAFE!
        // volatile only affects individual reads/writes
        // _counter++ is read-modify-write (3 operations)
    }
}
```

### 6.4 Immutability for Thread Safety

```csharp
// Immutable FX Rate - always thread-safe
public sealed class ImmutableRate
{
    public string CurrencyPair { get; }
    public decimal Bid { get; }
    public decimal Ask { get; }
    public DateTime Timestamp { get; }

    public ImmutableRate(string pair, decimal bid, decimal ask)
    {
        CurrencyPair = pair;
        Bid = bid;
        Ask = ask;
        Timestamp = DateTime.UtcNow;
    }

    // Create new instance with modifications
    public ImmutableRate WithBid(decimal newBid)
    {
        return new ImmutableRate(CurrencyPair, newBid, Ask);
    }

    public ImmutableRate WithAsk(decimal newAsk)
    {
        return new ImmutableRate(CurrencyPair, Bid, newAsk);
    }
}

// Using C# record (immutable by default)
public record FxRate(string Pair, decimal Bid, decimal Ask, DateTime Timestamp);

// Thread-safe rate holder using immutable objects
public class ThreadSafeRateHolder
{
    private volatile ImmutableRate _currentRate;

    public ImmutableRate CurrentRate => _currentRate;

    public void UpdateRate(ImmutableRate newRate)
    {
        // Atomic reference assignment
        _currentRate = newRate;
    }
}
```

---

## 7. Race Conditions

### 7.1 What is a Race Condition?

A **race condition** occurs when the behavior of code depends on the relative timing of events, such as the order in which threads execute.

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        RACE CONDITION                                   │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Example: balance = 100, both threads try to withdraw 60                │
│                                                                         │
│  EXPECTED (Sequential):                                                 │
│  Thread A: Check 100 >= 60 ✓, Withdraw, balance = 40                    │
│  Thread B: Check 40 >= 60 ✗, Rejected                                   │
│                                                                         │
│  RACE CONDITION (Parallel):                                             │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │ Time │ Thread A              │ Thread B              │ Balance  │   │
│  │──────┼───────────────────────┼───────────────────────┼──────────│   │
│  │  T1  │ Read balance (100)    │                       │   100    │   │
│  │  T2  │                       │ Read balance (100)    │   100    │   │
│  │  T3  │ Check 100 >= 60 ✓     │                       │   100    │   │
│  │  T4  │                       │ Check 100 >= 60 ✓     │   100    │   │
│  │  T5  │ balance = 100-60      │                       │    40    │   │
│  │  T6  │                       │ balance = 100-60      │    40    │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  RESULT: balance = 40 (should be -20 is impossible, or one rejected)   │
│  Two withdrawals succeeded when only one should have!                   │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 7.2 Common Race Condition Patterns

#### Check-Then-Act Race

```csharp
// ❌ RACE CONDITION: Check-then-act
public class UnsafeAccountV1
{
    private decimal _balance = 1000;

    public bool Withdraw(decimal amount)
    {
        if (_balance >= amount)  // Check
        {
            // Another thread might withdraw between check and act!
            Thread.Sleep(10);  // Simulate delay
            _balance -= amount;  // Act
            return true;
        }
        return false;
    }
}

// ✅ FIXED: Atomic check-and-act
public class SafeAccount
{
    private decimal _balance = 1000;
    private readonly object _lock = new object();

    public bool Withdraw(decimal amount)
    {
        lock (_lock)  // Check AND act atomically
        {
            if (_balance >= amount)
            {
                _balance -= amount;
                return true;
            }
            return false;
        }
    }
}
```

#### Read-Modify-Write Race

```csharp
// ❌ RACE CONDITION: Read-modify-write
public class UnsafeCounter
{
    private int _count = 0;

    public void Increment()
    {
        _count++;  // Actually: read, add, write (3 operations!)
    }
}

// ✅ FIXED: Atomic operation
public class SafeCounter
{
    private int _count = 0;

    public void Increment()
    {
        Interlocked.Increment(ref _count);  // Atomic
    }
}
```

#### Lazy Initialization Race

```csharp
// ❌ RACE CONDITION: Double initialization
public class UnsafeSingleton
{
    private static UnsafeSingleton _instance;

    public static UnsafeSingleton Instance
    {
        get
        {
            if (_instance == null)  // Both threads might see null!
            {
                _instance = new UnsafeSingleton();  // Created twice!
            }
            return _instance;
        }
    }
}

// ✅ FIXED: Thread-safe lazy initialization
public class SafeSingleton
{
    private static SafeSingleton _instance;
    private static readonly object _lock = new object();

    // Double-checked locking
    public static SafeSingleton Instance
    {
        get
        {
            if (_instance == null)  // First check (no lock)
            {
                lock (_lock)
                {
                    if (_instance == null)  // Second check (with lock)
                    {
                        _instance = new SafeSingleton();
                    }
                }
            }
            return _instance;
        }
    }
}

// ✅ BETTER: Use Lazy<T>
public class BestSingleton
{
    private static readonly Lazy<BestSingleton> _instance =
        new Lazy<BestSingleton>(() => new BestSingleton());

    public static BestSingleton Instance => _instance.Value;
}
```

### 7.3 FX Trading Race Condition Example

```csharp
// ❌ RACE CONDITION in FX Quote Management
public class UnsafeQuoteBook
{
    private Dictionary<string, Quote> _quotes = new();

    public void UpdateQuote(string pair, decimal bid, decimal ask)
    {
        // Race: Thread A and B both updating EUR/USD
        if (_quotes.ContainsKey(pair))
        {
            // Another thread might remove the key here!
            _quotes[pair] = new Quote(pair, bid, ask);
        }
        else
        {
            _quotes.Add(pair, new Quote(pair, bid, ask));
        }
    }

    public Quote GetQuote(string pair)
    {
        // Race: Dictionary might be modified during read
        return _quotes.TryGetValue(pair, out var quote) ? quote : null;
    }
}

// ✅ FIXED: Thread-safe quote book
public class SafeQuoteBook
{
    private readonly ConcurrentDictionary<string, Quote> _quotes = new();

    public void UpdateQuote(string pair, decimal bid, decimal ask)
    {
        var quote = new Quote(pair, bid, ask);
        _quotes.AddOrUpdate(pair, quote, (_, _) => quote);
    }

    public Quote GetQuote(string pair)
    {
        return _quotes.TryGetValue(pair, out var quote) ? quote : null;
    }
}

public record Quote(string Pair, decimal Bid, decimal Ask);
```

---

## 8. Deadlocks

### 8.1 What is a Deadlock?

A **deadlock** occurs when two or more threads are blocked forever, each waiting for the other to release a resource.

```
┌─────────────────────────────────────────────────────────────────────────┐
│                          DEADLOCK                                       │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Four conditions for deadlock (ALL must be present):                    │
│  ─────────────────────────────────────────────────                      │
│  1. Mutual Exclusion - Resources can't be shared                        │
│  2. Hold and Wait - Thread holds one resource, waits for another        │
│  3. No Preemption - Resources can't be forcibly taken                   │
│  4. Circular Wait - Circular chain of waiting                           │
│                                                                         │
│  Visual:                                                                │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                                                                  │   │
│  │   Thread A                              Thread B                 │   │
│  │   ┌──────────┐                         ┌──────────┐             │   │
│  │   │ Holds    │                         │ Holds    │             │   │
│  │   │ Lock 1   │                         │ Lock 2   │             │   │
│  │   └────┬─────┘                         └────┬─────┘             │   │
│  │        │                                    │                   │   │
│  │        │ Wants Lock 2                       │ Wants Lock 1      │   │
│  │        │                                    │                   │   │
│  │        └──────────────►   ◄────────────────┘                   │   │
│  │                     BLOCKED!                                    │   │
│  │                                                                  │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  Both threads waiting forever - DEADLOCK!                               │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 8.2 Deadlock Example

```csharp
// ❌ DEADLOCK: Inconsistent lock ordering
public class DeadlockExample
{
    private readonly object _lockA = new object();
    private readonly object _lockB = new object();

    public void Method1()
    {
        lock (_lockA)
        {
            Thread.Sleep(100);  // Simulate work
            lock (_lockB)
            {
                Console.WriteLine("Method1 completed");
            }
        }
    }

    public void Method2()
    {
        lock (_lockB)  // Opposite order!
        {
            Thread.Sleep(100);
            lock (_lockA)  // Deadlock if Method1 runs concurrently!
            {
                Console.WriteLine("Method2 completed");
            }
        }
    }

    // This will deadlock:
    public void CauseDeadlock()
    {
        var t1 = new Thread(Method1);
        var t2 = new Thread(Method2);

        t1.Start();
        t2.Start();

        t1.Join();  // Never completes!
        t2.Join();
    }
}
```

### 8.3 Deadlock Prevention Strategies

```csharp
// ✅ FIX 1: Consistent lock ordering
public class ConsistentLockOrder
{
    private readonly object _lockA = new object();
    private readonly object _lockB = new object();

    public void Method1()
    {
        lock (_lockA)  // Always A first
        {
            lock (_lockB)  // Then B
            {
                // Work
            }
        }
    }

    public void Method2()
    {
        lock (_lockA)  // Always A first (same order!)
        {
            lock (_lockB)  // Then B
            {
                // Work
            }
        }
    }
}

// ✅ FIX 2: Lock timeout
public class LockWithTimeout
{
    private readonly object _lockA = new object();
    private readonly object _lockB = new object();

    public bool Method1()
    {
        bool lockATaken = false;
        bool lockBTaken = false;

        try
        {
            Monitor.TryEnter(_lockA, TimeSpan.FromSeconds(1), ref lockATaken);
            if (!lockATaken)
            {
                Console.WriteLine("Could not acquire lock A");
                return false;
            }

            Monitor.TryEnter(_lockB, TimeSpan.FromSeconds(1), ref lockBTaken);
            if (!lockBTaken)
            {
                Console.WriteLine("Could not acquire lock B");
                return false;
            }

            // Do work
            return true;
        }
        finally
        {
            if (lockBTaken) Monitor.Exit(_lockB);
            if (lockATaken) Monitor.Exit(_lockA);
        }
    }
}

// ✅ FIX 3: Use single lock
public class SingleLock
{
    private readonly object _singleLock = new object();

    public void Method1()
    {
        lock (_singleLock)
        {
            // All shared resource access
        }
    }

    public void Method2()
    {
        lock (_singleLock)
        {
            // All shared resource access
        }
    }
}
```

### 8.4 FX Trading Deadlock Scenario

```csharp
// ❌ POTENTIAL DEADLOCK in FX system
public class UnsafeTransferService
{
    public void Transfer(Account from, Account to, decimal amount)
    {
        lock (from)  // Lock source account
        {
            lock (to)  // Lock destination account
            {
                from.Withdraw(amount);
                to.Deposit(amount);
            }
        }
    }
}

// Thread A: Transfer(AccountX, AccountY, 100)
// Thread B: Transfer(AccountY, AccountX, 50)
// DEADLOCK!

// ✅ FIXED: Consistent lock ordering by account ID
public class SafeTransferService
{
    public void Transfer(Account from, Account to, decimal amount)
    {
        // Always lock lower ID first
        Account first = from.Id < to.Id ? from : to;
        Account second = from.Id < to.Id ? to : from;

        lock (first)
        {
            lock (second)
            {
                from.Withdraw(amount);
                to.Deposit(amount);
            }
        }
    }
}

public class Account
{
    public int Id { get; set; }
    public decimal Balance { get; private set; }

    public void Withdraw(decimal amount) => Balance -= amount;
    public void Deposit(decimal amount) => Balance += amount;
}
```

---

## 9. Async/Await vs Threads

### 9.1 Key Differences

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    THREADS vs ASYNC/AWAIT                               │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  THREADS                              ASYNC/AWAIT                       │
│  ───────                              ───────────                       │
│  • CPU-bound work                     • I/O-bound work                  │
│  • Parallel execution                 • Concurrent (not parallel)       │
│  • Uses OS threads                    • Uses thread pool efficiently    │
│  • Heavy resource usage               • Lightweight                     │
│  • Good for computation               • Good for I/O (DB, network)      │
│                                                                         │
│  THREAD (Parallel):                                                     │
│  Thread 1: ████████████████████████                                     │
│  Thread 2: ████████████████████████                                     │
│  Thread 3: ████████████████████████                                     │
│  (Multiple threads running simultaneously)                              │
│                                                                         │
│  ASYNC/AWAIT (Concurrent):                                              │
│  Thread 1: ████░░░░░░░░░████░░░░░████                                   │
│            work  wait   work wait work                                  │
│  (Single thread, releases during await)                                 │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 9.2 When to Use What

```csharp
// Use THREADS for CPU-bound work
public class CpuBoundWork
{
    public void CalculateRiskMetrics(Portfolio portfolio)
    {
        // Heavy computation - use parallel threads
        Parallel.ForEach(portfolio.Positions, position =>
        {
            position.VaR = CalculateVaR(position);
            position.Greeks = CalculateGreeks(position);
        });
    }

    private decimal CalculateVaR(Position p) => /* complex calculation */;
    private Greeks CalculateGreeks(Position p) => /* complex calculation */;
}

// Use ASYNC for I/O-bound work
public class IoBoundWork
{
    public async Task<Quote> GetQuoteAsync(string pair)
    {
        // I/O operations - thread released during await
        using var client = new HttpClient();
        var response = await client.GetStringAsync($"api/quotes/{pair}");
        return JsonSerializer.Deserialize<Quote>(response);
    }

    public async Task SaveTradeAsync(Trade trade)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync("INSERT INTO Trades ...", trade);
    }
}
```

### 9.3 Common Mistakes

```csharp
public class AsyncMistakes
{
    // ❌ MISTAKE 1: Using Task.Run for I/O
    public async Task BadAsync()
    {
        // Don't wrap I/O in Task.Run - wastes thread pool thread
        await Task.Run(async () =>
        {
            await File.ReadAllTextAsync("data.txt");
        });
    }

    // ✅ CORRECT: Direct async I/O
    public async Task GoodAsync()
    {
        await File.ReadAllTextAsync("data.txt");
    }

    // ❌ MISTAKE 2: Blocking on async (sync-over-async)
    public void BadBlocking()
    {
        // Can cause deadlocks in certain contexts (UI, ASP.NET)!
        var result = GetDataAsync().Result;  // Don't do this!
        var result2 = GetDataAsync().GetAwaiter().GetResult();  // Still blocking!
    }

    // ✅ CORRECT: Async all the way
    public async Task GoodNonBlocking()
    {
        var result = await GetDataAsync();
    }

    // ❌ MISTAKE 3: async void (except event handlers)
    public async void BadAsyncVoid()  // Can't be awaited, exceptions lost!
    {
        await Task.Delay(100);
        throw new Exception("This exception is lost!");
    }

    // ✅ CORRECT: async Task
    public async Task GoodAsyncTask()
    {
        await Task.Delay(100);
    }

    private Task<string> GetDataAsync() => Task.FromResult("data");
}
```

### 9.4 Async in FX Rate Streaming

```csharp
public class AsyncRateStream
{
    private readonly HttpClient _client = new();

    // Efficient I/O-bound rate fetching
    public async Task<IEnumerable<Quote>> FetchAllRatesAsync()
    {
        var tasks = new[]
        {
            FetchFromReutersAsync(),
            FetchFromBloombergAsync(),
            FetchFromEbsAsync()
        };

        var results = await Task.WhenAll(tasks);
        return results.SelectMany(r => r);
    }

    // With timeout and cancellation
    public async Task<Quote> FetchWithTimeoutAsync(
        string pair,
        CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(5));  // Timeout

        try
        {
            var response = await _client.GetStringAsync(
                $"api/quotes/{pair}",
                cts.Token);
            return JsonSerializer.Deserialize<Quote>(response);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"Rate fetch for {pair} timed out");
            throw;
        }
    }

    private Task<List<Quote>> FetchFromReutersAsync() => Task.FromResult(new List<Quote>());
    private Task<List<Quote>> FetchFromBloombergAsync() => Task.FromResult(new List<Quote>());
    private Task<List<Quote>> FetchFromEbsAsync() => Task.FromResult(new List<Quote>());
}
```

---

## 10. Concurrent Collections

### 10.1 Overview

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    CONCURRENT COLLECTIONS                               │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Regular Collection        →    Concurrent Alternative                  │
│  ──────────────────────────────────────────────────────                 │
│  Dictionary<K,V>           →    ConcurrentDictionary<K,V>               │
│  Queue<T>                  →    ConcurrentQueue<T>                      │
│  Stack<T>                  →    ConcurrentStack<T>                      │
│  List<T>                   →    ConcurrentBag<T> (unordered)            │
│  N/A                       →    BlockingCollection<T> (producer-consumer)│
│                                                                         │
│  Key features:                                                          │
│  • Thread-safe without external locking                                 │
│  • Use fine-grained locking internally                                  │
│  • Better performance than lock + regular collection                    │
│  • Atomic compound operations (AddOrUpdate, GetOrAdd)                   │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 10.2 ConcurrentDictionary

```csharp
public class ConcurrentDictionaryExample
{
    private readonly ConcurrentDictionary<string, Quote> _quotes = new();

    // Thread-safe add or update
    public void UpdateQuote(string pair, Quote quote)
    {
        _quotes.AddOrUpdate(
            pair,
            quote,  // Value if adding
            (key, existingQuote) => quote  // Value if updating
        );
    }

    // Get or add (useful for caching)
    public Quote GetOrFetchQuote(string pair)
    {
        return _quotes.GetOrAdd(pair, key =>
        {
            // Only called if key doesn't exist
            return FetchQuoteFromProvider(key);
        });
    }

    // Conditional update
    public bool TryUpdateQuote(string pair, Quote newQuote, Quote expectedOldQuote)
    {
        // Only updates if current value equals expected
        return _quotes.TryUpdate(pair, newQuote, expectedOldQuote);
    }

    // Safe enumeration (snapshot)
    public IEnumerable<Quote> GetAllQuotes()
    {
        // Safe to enumerate while other threads modify
        foreach (var kvp in _quotes)
        {
            yield return kvp.Value;
        }
    }

    private Quote FetchQuoteFromProvider(string pair) => new Quote(pair, 1.0m, 1.0001m);
}
```

### 10.3 ConcurrentQueue and ConcurrentStack

```csharp
public class ConcurrentQueueExample
{
    private readonly ConcurrentQueue<Trade> _tradeQueue = new();

    // Producer
    public void EnqueueTrade(Trade trade)
    {
        _tradeQueue.Enqueue(trade);
    }

    // Consumer
    public Trade? TryDequeueTrade()
    {
        if (_tradeQueue.TryDequeue(out var trade))
        {
            return trade;
        }
        return null;
    }

    // Check without removing
    public Trade? PeekNextTrade()
    {
        if (_tradeQueue.TryPeek(out var trade))
        {
            return trade;
        }
        return null;
    }

    public int PendingTradeCount => _tradeQueue.Count;
}

public class ConcurrentStackExample
{
    // LIFO structure - useful for undo operations
    private readonly ConcurrentStack<TradeAction> _undoStack = new();

    public void RecordAction(TradeAction action)
    {
        _undoStack.Push(action);
    }

    public TradeAction? Undo()
    {
        if (_undoStack.TryPop(out var action))
        {
            return action;
        }
        return null;
    }

    // Pop multiple
    public TradeAction[] UndoMultiple(int count)
    {
        var actions = new TradeAction[count];
        int popped = _undoStack.TryPopRange(actions);
        return actions.Take(popped).ToArray();
    }
}

public record Trade(string Id, string Pair, decimal Amount);
public record TradeAction(string Type, Trade Trade);
```

### 10.4 BlockingCollection (Producer-Consumer)

```csharp
public class BlockingCollectionExample
{
    private readonly BlockingCollection<Quote> _quoteBuffer;

    public BlockingCollectionExample(int boundedCapacity = 1000)
    {
        // Bounded capacity prevents memory issues
        _quoteBuffer = new BlockingCollection<Quote>(boundedCapacity);
    }

    // Producer - blocks if buffer is full
    public void ProduceQuote(Quote quote)
    {
        _quoteBuffer.Add(quote);  // Blocks if full
    }

    // Producer - try with timeout
    public bool TryProduceQuote(Quote quote, int timeoutMs)
    {
        return _quoteBuffer.TryAdd(quote, timeoutMs);
    }

    // Consumer - blocks if buffer is empty
    public Quote ConsumeQuote()
    {
        return _quoteBuffer.Take();  // Blocks if empty
    }

    // Consumer - try with timeout
    public Quote? TryConsumeQuote(int timeoutMs)
    {
        if (_quoteBuffer.TryTake(out var quote, timeoutMs))
        {
            return quote;
        }
        return null;
    }

    // Consumer loop (common pattern)
    public void StartConsumerLoop(CancellationToken cancellationToken)
    {
        // GetConsumingEnumerable blocks waiting for items
        foreach (var quote in _quoteBuffer.GetConsumingEnumerable(cancellationToken))
        {
            ProcessQuote(quote);
        }
    }

    // Signal no more items will be added
    public void CompleteAdding()
    {
        _quoteBuffer.CompleteAdding();
    }

    private void ProcessQuote(Quote quote) { }
}

// Full producer-consumer pattern
public class ProducerConsumerExample
{
    private readonly BlockingCollection<Quote> _buffer = new(100);
    private readonly CancellationTokenSource _cts = new();

    public void Start()
    {
        // Start multiple producers
        for (int i = 0; i < 3; i++)
        {
            Task.Run(() => ProducerLoop(_cts.Token));
        }

        // Start multiple consumers
        for (int i = 0; i < 2; i++)
        {
            Task.Run(() => ConsumerLoop(_cts.Token));
        }
    }

    private void ProducerLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var quote = FetchQuoteFromExternalSource();
            _buffer.Add(quote, token);
        }
    }

    private void ConsumerLoop(CancellationToken token)
    {
        foreach (var quote in _buffer.GetConsumingEnumerable(token))
        {
            ProcessAndPublishQuote(quote);
        }
    }

    public void Stop()
    {
        _buffer.CompleteAdding();
        _cts.Cancel();
    }

    private Quote FetchQuoteFromExternalSource() => new Quote("EUR/USD", 1.08m, 1.0801m);
    private void ProcessAndPublishQuote(Quote quote) { }
}

public record Quote(string Pair, decimal Bid, decimal Ask);
```

---

## 11. Task Parallel Library (TPL)

### 11.1 Task Basics

```csharp
public class TaskBasics
{
    // Creating tasks
    public async Task CreateTasksAsync()
    {
        // Method 1: Task.Run (preferred for CPU-bound)
        Task task1 = Task.Run(() => ProcessData());

        // Method 2: Task.Factory.StartNew (more options)
        Task task2 = Task.Factory.StartNew(
            ProcessData,
            CancellationToken.None,
            TaskCreationOptions.LongRunning,  // Hint for long-running work
            TaskScheduler.Default);

        // Method 3: new Task + Start (rarely used)
        Task task3 = new Task(ProcessData);
        task3.Start();

        // With return value
        Task<decimal> task4 = Task.Run(() => CalculateRate());
        decimal result = await task4;

        await Task.WhenAll(task1, task2, task3);
    }

    // Task continuation
    public async Task ContinuationExample()
    {
        var task = Task.Run(() => FetchData())
            .ContinueWith(t => ProcessData(t.Result))
            .ContinueWith(t => SaveData(t.Result));

        await task;

        // Prefer async/await over ContinueWith
        var data = await Task.Run(() => FetchData());
        var processed = ProcessData(data);
        SaveData(processed);
    }

    private void ProcessData() { }
    private decimal CalculateRate() => 1.0850m;
    private string FetchData() => "data";
    private string ProcessData(string data) => data.ToUpper();
    private void SaveData(string data) { }
}
```

### 11.2 Parallel.For and Parallel.ForEach

```csharp
public class ParallelLoops
{
    // Parallel.For
    public void ParallelForExample()
    {
        var results = new decimal[1000];

        Parallel.For(0, 1000, i =>
        {
            results[i] = CalculateValueAt(i);
        });
    }

    // Parallel.ForEach
    public void ParallelForEachExample()
    {
        var pairs = new[] { "EUR/USD", "GBP/USD", "USD/JPY", "AUD/USD" };
        var quotes = new ConcurrentBag<Quote>();

        Parallel.ForEach(pairs, pair =>
        {
            var quote = FetchQuote(pair);
            quotes.Add(quote);
        });
    }

    // With options
    public void ParallelWithOptions()
    {
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount / 2,
            CancellationToken = CancellationToken.None
        };

        Parallel.ForEach(GetPositions(), options, position =>
        {
            CalculateRisk(position);
        });
    }

    // With local state (thread-local accumulator)
    public decimal ParallelSum()
    {
        var values = Enumerable.Range(1, 1000000).Select(i => (decimal)i).ToArray();
        decimal total = 0;
        object lockObj = new object();

        Parallel.ForEach(
            values,
            () => 0m,  // Initialize local state
            (value, state, localSum) => localSum + value,  // Accumulate locally
            localSum =>  // Merge local into global
            {
                lock (lockObj)
                {
                    total += localSum;
                }
            });

        return total;
    }

    private decimal CalculateValueAt(int i) => i * 1.5m;
    private Quote FetchQuote(string pair) => new Quote(pair, 1m, 1.0001m);
    private IEnumerable<Position> GetPositions() => new List<Position>();
    private void CalculateRisk(Position p) { }
}

public record Quote(string Pair, decimal Bid, decimal Ask);
public record Position(string Id);
```

### 11.3 PLINQ (Parallel LINQ)

```csharp
public class PlinqExamples
{
    public void BasicPlinq()
    {
        var numbers = Enumerable.Range(1, 1000000);

        // Sequential LINQ
        var sequentialResult = numbers
            .Where(n => n % 2 == 0)
            .Select(n => n * n)
            .Sum();

        // Parallel LINQ (just add .AsParallel())
        var parallelResult = numbers
            .AsParallel()
            .Where(n => n % 2 == 0)
            .Select(n => n * n)
            .Sum();
    }

    // With options
    public void PlinqWithOptions()
    {
        var data = GetLargeDataSet();

        var results = data
            .AsParallel()
            .WithDegreeOfParallelism(4)  // Limit parallelism
            .WithCancellation(CancellationToken.None)
            .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
            .Where(x => x.IsValid)
            .Select(x => Transform(x))
            .ToList();
    }

    // Preserve order (has performance cost)
    public void PlinqOrdered()
    {
        var results = Enumerable.Range(1, 100)
            .AsParallel()
            .AsOrdered()  // Preserve original order
            .Select(n => ExpensiveCalculation(n))
            .ToList();
    }

    // ForAll - parallel foreach on results
    public void PlinqForAll()
    {
        var data = GetLargeDataSet();

        data.AsParallel()
            .Where(x => x.IsValid)
            .ForAll(x => Process(x));  // Parallel processing
    }

    private IEnumerable<DataItem> GetLargeDataSet() => new List<DataItem>();
    private DataItem Transform(DataItem x) => x;
    private int ExpensiveCalculation(int n) => n * n;
    private void Process(DataItem x) { }
}

public record DataItem(string Id, bool IsValid);
```

---

## 12. Cancellation Tokens

### 12.1 Basic Cancellation

```csharp
public class CancellationExample
{
    public async Task CancellableOperationAsync(CancellationToken cancellationToken)
    {
        for (int i = 0; i < 100; i++)
        {
            // Check for cancellation
            cancellationToken.ThrowIfCancellationRequested();

            // Or check without throwing
            if (cancellationToken.IsCancellationRequested)
            {
                Console.WriteLine("Cancellation requested, cleaning up...");
                CleanUp();
                return;  // Or throw new OperationCanceledException(cancellationToken);
            }

            await ProcessItemAsync(i, cancellationToken);
        }
    }

    public async Task UsageExample()
    {
        using var cts = new CancellationTokenSource();

        // Cancel after 5 seconds
        cts.CancelAfter(TimeSpan.FromSeconds(5));

        try
        {
            await CancellableOperationAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was cancelled");
        }
    }

    // Manual cancellation
    public async Task ManualCancellation()
    {
        using var cts = new CancellationTokenSource();

        var task = LongRunningOperationAsync(cts.Token);

        // Cancel after some condition
        await Task.Delay(2000);
        cts.Cancel();

        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Cancelled!");
        }
    }

    private Task ProcessItemAsync(int i, CancellationToken token) => Task.Delay(100, token);
    private Task LongRunningOperationAsync(CancellationToken token) => Task.Delay(10000, token);
    private void CleanUp() { }
}
```

### 12.2 Linked Cancellation Tokens

```csharp
public class LinkedCancellationExample
{
    public async Task LinkedCancellation(CancellationToken externalToken)
    {
        // Create internal timeout
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        // Link external token with internal timeout
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            externalToken,
            timeoutCts.Token);

        try
        {
            // Cancelled if EITHER external cancellation OR timeout
            await DoWorkAsync(linkedCts.Token);
        }
        catch (OperationCanceledException)
        {
            if (externalToken.IsCancellationRequested)
            {
                Console.WriteLine("Cancelled by user");
            }
            else if (timeoutCts.Token.IsCancellationRequested)
            {
                Console.WriteLine("Operation timed out");
            }
        }
    }

    private Task DoWorkAsync(CancellationToken token) => Task.Delay(60000, token);
}
```

### 12.3 FX Rate Streaming with Cancellation

```csharp
public class CancellableRateStreaming
{
    private CancellationTokenSource _streamingCts;

    public void StartStreaming()
    {
        _streamingCts = new CancellationTokenSource();
        Task.Run(() => StreamRatesAsync(_streamingCts.Token));
    }

    public void StopStreaming()
    {
        _streamingCts?.Cancel();
        _streamingCts?.Dispose();
    }

    private async Task StreamRatesAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var quotes = await FetchLatestQuotesAsync(cancellationToken);
                PublishQuotes(quotes);
                await Task.Delay(100, cancellationToken);  // Rate limit
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Rate streaming stopped");
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching rates: {ex.Message}");
                await Task.Delay(1000, cancellationToken);  // Back off on error
            }
        }
    }

    private Task<List<Quote>> FetchLatestQuotesAsync(CancellationToken token)
        => Task.FromResult(new List<Quote>());
    private void PublishQuotes(List<Quote> quotes) { }
}
```

---

## 13. Best Practices

### 13.1 Thread Safety Best Practices

```csharp
public class ThreadSafetyBestPractices
{
    // 1. Prefer immutable objects
    public record ImmutableQuote(string Pair, decimal Bid, decimal Ask, DateTime Timestamp);

    // 2. Use thread-safe collections
    private readonly ConcurrentDictionary<string, Quote> _quotes = new();

    // 3. Minimize lock scope
    private readonly object _lock = new object();
    private List<Trade> _trades = new();

    public void AddTrade(Trade trade)
    {
        // ✅ GOOD: Short lock
        lock (_lock)
        {
            _trades.Add(trade);
        }

        // Do other work outside lock
        NotifyTradeAdded(trade);
    }

    // 4. Avoid nested locks when possible
    // 5. Use Interlocked for simple counters
    private long _counter = 0;
    public void Increment() => Interlocked.Increment(ref _counter);

    // 6. Use readonly for lock objects
    private readonly object _syncRoot = new object();  // readonly!

    // 7. Consider using Lazy<T> for thread-safe lazy initialization
    private readonly Lazy<ExpensiveResource> _resource =
        new Lazy<ExpensiveResource>(() => new ExpensiveResource());

    public ExpensiveResource Resource => _resource.Value;

    private void NotifyTradeAdded(Trade trade) { }
}

public record Trade(string Id);
public record Quote(string Pair, decimal Bid, decimal Ask);
public class ExpensiveResource { }
```

### 13.2 Async Best Practices

```csharp
public class AsyncBestPractices
{
    // 1. Async all the way - don't mix sync and async
    public async Task<string> GoodAsync()
    {
        var data = await FetchDataAsync();
        return await ProcessDataAsync(data);
    }

    // 2. Use ConfigureAwait(false) in library code
    public async Task LibraryMethodAsync()
    {
        await Task.Delay(100).ConfigureAwait(false);
    }

    // 3. Avoid async void (except event handlers)
    public async Task GoodAsyncMethod() { await Task.Delay(100); }
    // NOT: public async void BadAsyncMethod() { }

    // 4. Use ValueTask for hot paths that often complete synchronously
    private decimal? _cachedRate;
    public ValueTask<decimal> GetRateAsync()
    {
        if (_cachedRate.HasValue)
        {
            return ValueTask.FromResult(_cachedRate.Value);  // No allocation
        }
        return new ValueTask<decimal>(FetchRateAsync());
    }

    // 5. Handle exceptions properly
    public async Task HandleExceptionsAsync()
    {
        try
        {
            await RiskyOperationAsync();
        }
        catch (OperationCanceledException)
        {
            // Expected cancellation - handle gracefully
        }
        catch (Exception ex)
        {
            // Log and handle
            Console.WriteLine($"Error: {ex.Message}");
            throw;  // Re-throw if can't handle
        }
    }

    private Task<string> FetchDataAsync() => Task.FromResult("data");
    private Task<string> ProcessDataAsync(string data) => Task.FromResult(data);
    private Task<decimal> FetchRateAsync() => Task.FromResult(1.0850m);
    private Task RiskyOperationAsync() => Task.CompletedTask;
}
```

### 13.3 Performance Best Practices

```csharp
public class PerformanceBestPractices
{
    // 1. Use SemaphoreSlim instead of Semaphore for in-process
    private readonly SemaphoreSlim _semaphore = new(10);

    // 2. Use ReaderWriterLockSlim for read-heavy scenarios
    private readonly ReaderWriterLockSlim _rwLock = new();

    // 3. Prefer Task.WhenAll over sequential awaits
    public async Task GoodParallelAsync()
    {
        // ✅ GOOD: Parallel
        var tasks = new[]
        {
            FetchFromSource1Async(),
            FetchFromSource2Async(),
            FetchFromSource3Async()
        };
        await Task.WhenAll(tasks);

        // ❌ BAD: Sequential
        // await FetchFromSource1Async();
        // await FetchFromSource2Async();
        // await FetchFromSource3Async();
    }

    // 4. Batch operations when possible
    public async Task BatchInsertAsync(IEnumerable<Trade> trades)
    {
        // ✅ GOOD: Batch insert
        await Database.BulkInsertAsync(trades);

        // ❌ BAD: Individual inserts
        // foreach (var trade in trades)
        //     await Database.InsertAsync(trade);
    }

    // 5. Use object pooling for frequently created objects
    private readonly ObjectPool<StringBuilder> _stringBuilderPool =
        new DefaultObjectPoolProvider().CreateStringBuilderPool();

    public string BuildMessage()
    {
        var sb = _stringBuilderPool.Get();
        try
        {
            sb.Append("Message content");
            return sb.ToString();
        }
        finally
        {
            _stringBuilderPool.Return(sb);
        }
    }

    private Task FetchFromSource1Async() => Task.CompletedTask;
    private Task FetchFromSource2Async() => Task.CompletedTask;
    private Task FetchFromSource3Async() => Task.CompletedTask;
}

// Mock classes
public static class Database
{
    public static Task BulkInsertAsync<T>(IEnumerable<T> items) => Task.CompletedTask;
}
public record Trade(string Id);
```

---

## 14. Interview Questions

### Q1: What is the difference between Process and Thread?

**Answer:**

- **Process**: Independent execution unit with its own memory space. Processes are isolated - one crash doesn't affect others.
- **Thread**: Lightweight execution unit within a process. Threads share memory with other threads in the same process.

Key difference: Threads share memory (heap, code, data), processes don't.

---

### Q2: What is a race condition? How do you prevent it?

**Answer:**
A race condition occurs when program behavior depends on the relative timing of threads accessing shared data.

**Prevention:**

1. **Locks**: Use `lock` statement or `Monitor`
2. **Interlocked**: For simple atomic operations
3. **Concurrent collections**: Use `ConcurrentDictionary`, etc.
4. **Immutability**: Use immutable objects

```csharp
// Race condition
int counter = 0;
counter++;  // Not atomic!

// Fixed
Interlocked.Increment(ref counter);
```

---

### Q3: What is a deadlock? How do you prevent it?

**Answer:**
Deadlock occurs when threads are blocked forever, each waiting for the other to release a lock.

**Prevention:**

1. **Consistent lock ordering**: Always acquire locks in the same order
2. **Lock timeout**: Use `Monitor.TryEnter` with timeout
3. **Avoid nested locks**: Redesign to use single lock if possible
4. **Use concurrent collections**: They handle locking internally

---

### Q4: Difference between `lock` and `Monitor`?

**Answer:**
`lock` is syntactic sugar for `Monitor.Enter/Exit` with proper try-finally handling.

`Monitor` provides additional features:

- `TryEnter` with timeout
- `Wait` and `Pulse` for signaling

```csharp
// lock statement
lock (obj) { }

// Equivalent Monitor code
Monitor.Enter(obj);
try { }
finally { Monitor.Exit(obj); }
```

---

### Q5: When to use `volatile` keyword?

**Answer:**
Use `volatile` when:

1. A field is accessed by multiple threads
2. Only simple read/write operations (not read-modify-write)
3. You need visibility guarantee across threads

```csharp
private volatile bool _running = true;

// Good for flags
while (_running) { }

// NOT good for increment (not atomic)
volatile int counter;
counter++;  // Still race condition!
```

---

### Q6: Difference between `Task.Run` and `new Thread`?

**Answer:**

| Aspect        | Task.Run             | new Thread                   |
| ------------- | -------------------- | ---------------------------- |
| Thread source | Thread pool (reused) | New OS thread (expensive)    |
| Return values | Built-in (`Task<T>`) | Manual                       |
| Exceptions    | Propagated via Task  | Crashes thread               |
| Cancellation  | CancellationToken    | Manual                       |
| Use case      | Short operations     | Long-running, dedicated work |

---

### Q7: What is `SemaphoreSlim` used for?

**Answer:**
`SemaphoreSlim` limits the number of threads that can access a resource concurrently.

Use cases:

- Connection pooling (limit DB connections)
- Rate limiting (limit API calls)
- Resource throttling

```csharp
private readonly SemaphoreSlim _limiter = new(5);  // Max 5 concurrent

async Task AccessResource()
{
    await _limiter.WaitAsync();
    try { /* use resource */ }
    finally { _limiter.Release(); }
}
```

---

### Q8: How does `async/await` differ from multithreading?

**Answer:**

- **Threads**: True parallelism, multiple threads running simultaneously
- **Async/await**: Concurrency, single thread released during I/O wait

Async/await is for I/O-bound work (network, disk), not CPU-bound.
During `await`, the thread is returned to the pool to do other work.

---

### Q9: What is `ConcurrentDictionary.GetOrAdd` and why is it useful?

**Answer:**
`GetOrAdd` atomically checks if a key exists and adds it if not, returning the value.

```csharp
// Thread-safe lazy caching
var quote = _cache.GetOrAdd("EUR/USD", key => FetchQuote(key));
```

Without this, you'd need external locking for check-then-add pattern.

---

### Q10: How do you implement a thread-safe singleton?

**Answer:**

```csharp
// Best: Use Lazy<T>
public class Singleton
{
    private static readonly Lazy<Singleton> _instance =
        new(() => new Singleton());

    public static Singleton Instance => _instance.Value;

    private Singleton() { }
}

// Alternative: Static constructor (guaranteed thread-safe)
public class Singleton
{
    public static readonly Singleton Instance = new();
    private Singleton() { }
}
```

---

_Document created for interview preparation. February 2026_
