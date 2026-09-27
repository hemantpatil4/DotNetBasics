# Task in C# - Complete In-Depth Guide

> **This is THE most important topic for .NET interviews!**  
> Task is the foundation of modern async programming in C#

---

## Table of Contents

1. [What is Task?](#1-what-is-task)
2. [Task vs Thread](#2-task-vs-thread)
3. [Creating Tasks](#3-creating-tasks)
4. [Task&lt;T&gt; - Returning Values](#4-taskt---returning-values)
5. [Task Status & Lifecycle](#5-task-status--lifecycle)
6. [Waiting for Tasks](#6-waiting-for-tasks)
7. [Task.WhenAll vs Task.WhenAny](#7-taskwhenall-vs-taskwhenany)
8. [Task Continuations](#8-task-continuations)
9. [Exception Handling](#9-exception-handling)
10. [Task.Run vs Task.Factory.StartNew](#10-taskrun-vs-taskfactorystartnew)
11. [ValueTask vs Task](#11-valuetask-vs-task)
12. [TaskCompletionSource](#12-taskcompletionsource)
13. [Common Patterns & Best Practices](#13-common-patterns--best-practices)
14. [Interview Questions](#14-interview-questions)

---

## 1. What is Task?

### Definition

`Task` represents an **asynchronous operation** that may or may not return a value. It's part of the Task Parallel Library (TPL) introduced in .NET 4.0.

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           TASK OVERVIEW                                  │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Task = Promise/Future pattern in C#                                    │
│                                                                         │
│  ┌─────────────┐                        ┌─────────────────────────┐    │
│  │   Task      │   represents          │  An operation that      │    │
│  │             │ ──────────────────►   │  - runs asynchronously  │    │
│  │             │                        │  - may return a value   │    │
│  │             │                        │  - can be awaited       │    │
│  └─────────────┘                        └─────────────────────────┘    │
│                                                                         │
│  Two Types:                                                             │
│  ┌────────────────────┐    ┌────────────────────────────┐              │
│  │ Task               │    │ Task<TResult>              │              │
│  │ (no return value)  │    │ (returns TResult)          │              │
│  │ like void async    │    │ like T async               │              │
│  └────────────────────┘    └────────────────────────────┘              │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### Why Task?

```csharp
// ❌ OLD WAY: Manual Thread Management
Thread thread = new Thread(() => DoWork());
thread.Start();
thread.Join(); // Block until complete
// Problem: No easy way to get result, handle exceptions, compose

// ✅ MODERN WAY: Task
Task task = Task.Run(() => DoWork());
await task; // Non-blocking wait
// Benefits: Composition, exception handling, cancellation, return values
```

---

## 2. Task vs Thread

### Key Differences

```
┌─────────────────────────────────────────────────────────────────────────┐
│                      TASK vs THREAD                                      │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  THREAD                              TASK                               │
│  ══════                              ════                               │
│  • Low-level OS concept              • High-level abstraction           │
│  • 1 MB stack per thread             • Uses ThreadPool (efficient)      │
│  • Manual lifecycle management       • Automatic management             │
│  • No built-in return value          • Task<T> returns value            │
│  • No built-in composition           • WhenAll, WhenAny, ContinueWith   │
│  • Exception crashes if unhandled    • Exceptions captured in Task      │
│  • No built-in cancellation          • CancellationToken support        │
│                                                                         │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │  Think of it this way:                                          │   │
│  │                                                                  │   │
│  │  Thread = A worker                                              │   │
│  │  Task   = A unit of work (may be done by any available worker)  │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### Code Comparison

```csharp
// ═══════════════════════════════════════════════════════════════
// THREAD WAY (Old, Avoid)
// ═══════════════════════════════════════════════════════════════
public class ThreadExample
{
    public void RunWithThread()
    {
        decimal result = 0;
        Exception? error = null;

        Thread thread = new Thread(() =>
        {
            try
            {
                result = CalculateRate("EUR/USD");
            }
            catch (Exception ex)
            {
                error = ex;
            }
        });

        thread.Start();
        thread.Join(); // ❌ Blocks the calling thread!

        if (error != null) throw error;
        Console.WriteLine(result);
    }

    private decimal CalculateRate(string pair) => 1.0850m;
}

// ═══════════════════════════════════════════════════════════════
// TASK WAY (Modern, Preferred)
// ═══════════════════════════════════════════════════════════════
public class TaskExample
{
    public async Task RunWithTaskAsync()
    {
        // ✅ Clean, readable, non-blocking
        decimal result = await Task.Run(() => CalculateRate("EUR/USD"));
        Console.WriteLine(result);
        // Exceptions automatically propagate!
    }

    private decimal CalculateRate(string pair) => 1.0850m;
}
```

### When to use Thread directly?

```csharp
// Only use Thread when you need:
// 1. A long-running dedicated thread
// 2. Control over thread priority/name
// 3. STA (Single-Threaded Apartment) thread for COM

Thread dedicatedThread = new Thread(LongRunningService)
{
    Name = "RateStreamingThread",
    IsBackground = true,
    Priority = ThreadPriority.AboveNormal
};
dedicatedThread.Start();
```

---

## 3. Creating Tasks

### All Ways to Create a Task

```csharp
public class TaskCreation
{
    // ═══════════════════════════════════════════════════════════════
    // METHOD 1: Task.Run (MOST COMMON - CPU-bound work)
    // ═══════════════════════════════════════════════════════════════
    public async Task Method1_TaskRun()
    {
        // Fire and forget (don't do this normally!)
        Task task1 = Task.Run(() => Console.WriteLine("Hello"));

        // Await the task
        await Task.Run(() => ProcessData());

        // With return value
        int result = await Task.Run(() => Calculate());
    }

    // ═══════════════════════════════════════════════════════════════
    // METHOD 2: Task.Factory.StartNew (More control)
    // ═══════════════════════════════════════════════════════════════
    public async Task Method2_FactoryStartNew()
    {
        // Basic usage
        Task task1 = Task.Factory.StartNew(() => ProcessData());

        // With options
        Task task2 = Task.Factory.StartNew(
            () => LongRunningWork(),
            CancellationToken.None,
            TaskCreationOptions.LongRunning,  // Uses dedicated thread
            TaskScheduler.Default
        );

        await Task.WhenAll(task1, task2);
    }

    // ═══════════════════════════════════════════════════════════════
    // METHOD 3: new Task() + Start() (AVOID - rarely needed)
    // ═══════════════════════════════════════════════════════════════
    public void Method3_NewTask()
    {
        // ❌ Not recommended - use Task.Run instead
        Task task = new Task(() => ProcessData());
        task.Start();
        task.Wait();
    }

    // ═══════════════════════════════════════════════════════════════
    // METHOD 4: Task.FromResult (Already completed task)
    // ═══════════════════════════════════════════════════════════════
    public Task<decimal> Method4_FromResult()
    {
        // Useful for:
        // - Cached values
        // - Implementing interfaces that return Task
        // - Unit testing

        decimal cachedRate = 1.0850m;
        return Task.FromResult(cachedRate);  // No async overhead!
    }

    // ═══════════════════════════════════════════════════════════════
    // METHOD 5: Task.CompletedTask (Completed void task)
    // ═══════════════════════════════════════════════════════════════
    public Task Method5_CompletedTask()
    {
        // When you need to return Task but work is already done
        if (_cache.ContainsKey("EUR/USD"))
        {
            return Task.CompletedTask;
        }
        return FetchAndCacheAsync("EUR/USD");
    }

    // ═══════════════════════════════════════════════════════════════
    // METHOD 6: async/await (I/O-bound work)
    // ═══════════════════════════════════════════════════════════════
    public async Task Method6_AsyncAwait()
    {
        // For I/O operations - NO Task.Run needed!
        string data = await File.ReadAllTextAsync("rates.json");
        var response = await _httpClient.GetAsync("https://api.rates.com");
    }

    private void ProcessData() { }
    private void LongRunningWork() { Thread.Sleep(10000); }
    private int Calculate() => 42;
    private Dictionary<string, decimal> _cache = new();
    private Task FetchAndCacheAsync(string pair) => Task.CompletedTask;
    private HttpClient _httpClient = new();
}
```

### Visual: When to Use What?

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    WHICH TASK CREATION TO USE?                          │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │ Is it I/O-bound? (File, Network, Database)                      │   │
│  │                                                                  │   │
│  │    YES ──► Just use async/await                                 │   │
│  │            await File.ReadAllTextAsync(...)                     │   │
│  │            await httpClient.GetAsync(...)                       │   │
│  │                                                                  │   │
│  │    NO (CPU-bound) ──► Task.Run()                                │   │
│  │                       await Task.Run(() => HeavyComputation())  │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │ Special Cases:                                                   │   │
│  │                                                                  │   │
│  │  • Return cached value?     ──► Task.FromResult(value)          │   │
│  │  • Return completed void?   ──► Task.CompletedTask              │   │
│  │  • Long-running dedicated?  ──► Task.Factory.StartNew           │   │
│  │                                  with LongRunning option        │   │
│  │  • Wrap callback API?       ──► TaskCompletionSource            │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 4. Task&lt;T&gt; - Returning Values

```csharp
public class TaskWithResults
{
    // ═══════════════════════════════════════════════════════════════
    // Task<T> returns a value of type T
    // ═══════════════════════════════════════════════════════════════

    public async Task<decimal> GetRateAsync(string pair)
    {
        // Simulating API call
        await Task.Delay(100);
        return 1.0850m;
    }

    public async Task UseTaskWithResult()
    {
        // Method 1: await directly
        decimal rate = await GetRateAsync("EUR/USD");

        // Method 2: Get Task first, then await
        Task<decimal> rateTask = GetRateAsync("EUR/USD");
        // ... do other work ...
        decimal rate2 = await rateTask;

        // Method 3: Multiple parallel calls
        Task<decimal> eurTask = GetRateAsync("EUR/USD");
        Task<decimal> gbpTask = GetRateAsync("GBP/USD");
        Task<decimal> jpyTask = GetRateAsync("USD/JPY");

        decimal[] rates = await Task.WhenAll(eurTask, gbpTask, jpyTask);
        // rates[0] = EUR/USD, rates[1] = GBP/USD, rates[2] = USD/JPY
    }

    // ═══════════════════════════════════════════════════════════════
    // Accessing Result (be careful!)
    // ═══════════════════════════════════════════════════════════════

    public void ResultProperty()
    {
        Task<decimal> task = GetRateAsync("EUR/USD");

        // ❌ DANGER: .Result blocks the calling thread!
        // Can cause deadlocks in UI/ASP.NET contexts
        decimal rate = task.Result;  // AVOID!

        // ❌ DANGER: .GetAwaiter().GetResult() also blocks
        decimal rate2 = task.GetAwaiter().GetResult();  // AVOID!

        // ✅ CORRECT: Use await
        // decimal rate3 = await task;
    }
}
```

---

## 5. Task Status & Lifecycle

```csharp
public class TaskLifecycle
{
    public async Task DemonstrateTaskStatus()
    {
        var cts = new CancellationTokenSource();

        // Create task
        Task task = Task.Run(async () =>
        {
            await Task.Delay(1000);
        });

        // Check status at different points
        Console.WriteLine($"After creation: {task.Status}");  // WaitingToRun or Running

        await Task.Delay(100);
        Console.WriteLine($"During execution: {task.Status}");  // Running

        await task;
        Console.WriteLine($"After completion: {task.Status}");  // RanToCompletion
    }
}
```

### Task Status Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        TASK STATUS LIFECYCLE                            │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│                        ┌──────────────┐                                 │
│                        │   Created    │  (new Task())                   │
│                        └──────┬───────┘                                 │
│                               │ .Start()                                │
│                               ▼                                         │
│                        ┌──────────────┐                                 │
│                        │WaitingToRun  │  (Queued in ThreadPool)         │
│                        └──────┬───────┘                                 │
│                               │                                         │
│                               ▼                                         │
│                        ┌──────────────┐                                 │
│                        │   Running    │  (Executing)                    │
│                        └──────┬───────┘                                 │
│                               │                                         │
│              ┌────────────────┼────────────────┐                        │
│              │                │                │                        │
│              ▼                ▼                ▼                        │
│     ┌────────────────┐ ┌────────────┐ ┌─────────────────┐              │
│     │RanToCompletion │ │  Faulted   │ │    Canceled     │              │
│     │   (Success)    │ │  (Error)   │ │ (Cancellation)  │              │
│     └────────────────┘ └────────────┘ └─────────────────┘              │
│                                                                         │
│  Properties:                                                            │
│  • task.IsCompleted     - true for all 3 final states                  │
│  • task.IsCompletedSuccessfully - true only for RanToCompletion        │
│  • task.IsFaulted       - true if exception occurred                    │
│  • task.IsCanceled      - true if canceled                             │
│  • task.Exception       - AggregateException if faulted                │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### Checking Task Status

```csharp
public class TaskStatusChecking
{
    public async Task CheckAllStatuses()
    {
        // Successful task
        Task successTask = Task.Run(() => { });
        await successTask;
        Console.WriteLine($"Success - Status: {successTask.Status}");           // RanToCompletion
        Console.WriteLine($"IsCompleted: {successTask.IsCompleted}");           // True
        Console.WriteLine($"IsCompletedSuccessfully: {successTask.IsCompletedSuccessfully}"); // True

        // Faulted task
        Task faultedTask = Task.Run(() => throw new Exception("Oops!"));
        try { await faultedTask; } catch { }
        Console.WriteLine($"Faulted - Status: {faultedTask.Status}");           // Faulted
        Console.WriteLine($"IsFaulted: {faultedTask.IsFaulted}");               // True
        Console.WriteLine($"Exception: {faultedTask.Exception?.InnerException?.Message}"); // "Oops!"

        // Canceled task
        var cts = new CancellationTokenSource();
        cts.Cancel();
        Task canceledTask = Task.Run(() => { }, cts.Token);
        try { await canceledTask; } catch (OperationCanceledException) { }
        Console.WriteLine($"Canceled - Status: {canceledTask.Status}");         // Canceled
        Console.WriteLine($"IsCanceled: {canceledTask.IsCanceled}");            // True
    }
}
```

---

## 6. Waiting for Tasks

### Different Ways to Wait

```csharp
public class WaitingForTasks
{
    // ═══════════════════════════════════════════════════════════════
    // METHOD 1: await (BEST - Non-blocking)
    // ═══════════════════════════════════════════════════════════════
    public async Task Method1_Await()
    {
        Task task = DoWorkAsync();
        await task;  // ✅ Non-blocking, releases thread
    }

    // ═══════════════════════════════════════════════════════════════
    // METHOD 2: Wait() - BLOCKING!
    // ═══════════════════════════════════════════════════════════════
    public void Method2_Wait()
    {
        Task task = DoWorkAsync();
        task.Wait();  // ❌ Blocks thread until complete

        // With timeout
        bool completed = task.Wait(TimeSpan.FromSeconds(5));
        if (!completed)
            Console.WriteLine("Task timed out!");
    }

    // ═══════════════════════════════════════════════════════════════
    // METHOD 3: Result property - BLOCKING!
    // ═══════════════════════════════════════════════════════════════
    public void Method3_Result()
    {
        Task<int> task = GetValueAsync();
        int value = task.Result;  // ❌ Blocks until complete
    }

    // ═══════════════════════════════════════════════════════════════
    // METHOD 4: GetAwaiter().GetResult() - BLOCKING!
    // ═══════════════════════════════════════════════════════════════
    public void Method4_GetAwaiterGetResult()
    {
        Task<int> task = GetValueAsync();
        int value = task.GetAwaiter().GetResult();  // ❌ Blocks

        // Slightly better than .Result because it unwraps AggregateException
        // But still AVOID in async contexts!
    }

    // ═══════════════════════════════════════════════════════════════
    // COMPARISON TABLE
    // ═══════════════════════════════════════════════════════════════
    /*
    ┌──────────────────────────────┬───────────┬───────────────────────┐
    │ Method                       │ Blocking? │ When to Use           │
    ├──────────────────────────────┼───────────┼───────────────────────┤
    │ await task                   │ No ✅     │ Always (in async)     │
    │ task.Wait()                  │ Yes ❌    │ Console apps, tests   │
    │ task.Result                  │ Yes ❌    │ Avoid                 │
    │ task.GetAwaiter().GetResult()│ Yes ❌    │ Avoid                 │
    └──────────────────────────────┴───────────┴───────────────────────┘
    */

    private Task DoWorkAsync() => Task.Delay(100);
    private Task<int> GetValueAsync() => Task.FromResult(42);
}
```

### ⚠️ Deadlock Warning!

```csharp
public class DeadlockExample
{
    // ═══════════════════════════════════════════════════════════════
    // THIS WILL DEADLOCK in UI/ASP.NET!
    // ═══════════════════════════════════════════════════════════════

    // ASP.NET Controller (BAD)
    public string GetData()
    {
        // ❌ DEADLOCK!
        // .Result blocks the thread
        // await needs the same thread to continue
        // = Deadlock!
        return GetDataAsync().Result;
    }

    // ✅ CORRECT WAY
    public async Task<string> GetDataCorrect()
    {
        return await GetDataAsync();
    }

    private async Task<string> GetDataAsync()
    {
        await Task.Delay(100);
        return "data";
    }
}
```

---

## 7. Task.WhenAll vs Task.WhenAny

### Task.WhenAll - Wait for ALL tasks

```csharp
public class WhenAllExamples
{
    // ═══════════════════════════════════════════════════════════════
    // WhenAll - Completes when ALL tasks complete
    // ═══════════════════════════════════════════════════════════════

    public async Task FetchAllRatesAsync()
    {
        // Start all tasks simultaneously
        Task<decimal> eurTask = GetRateAsync("EUR/USD");
        Task<decimal> gbpTask = GetRateAsync("GBP/USD");
        Task<decimal> jpyTask = GetRateAsync("USD/JPY");
        Task<decimal> audTask = GetRateAsync("AUD/USD");

        // Wait for all to complete
        decimal[] rates = await Task.WhenAll(eurTask, gbpTask, jpyTask, audTask);

        // rates = [1.08, 1.25, 150.5, 0.65] (in order!)
        Console.WriteLine($"EUR/USD: {rates[0]}");
        Console.WriteLine($"GBP/USD: {rates[1]}");
        Console.WriteLine($"USD/JPY: {rates[2]}");
        Console.WriteLine($"AUD/USD: {rates[3]}");
    }

    // Dynamic list of tasks
    public async Task ProcessAllOrdersAsync(List<Order> orders)
    {
        // Create tasks for all orders
        var tasks = orders.Select(order => ProcessOrderAsync(order));

        // Wait for all
        await Task.WhenAll(tasks);
    }

    // ═══════════════════════════════════════════════════════════════
    // WhenAll with exception handling
    // ═══════════════════════════════════════════════════════════════

    public async Task WhenAllWithExceptions()
    {
        var tasks = new[]
        {
            Task.Run(() => { throw new Exception("Task 1 failed"); }),
            Task.Run(() => { throw new Exception("Task 2 failed"); }),
            Task.Run(() => Console.WriteLine("Task 3 succeeded"))
        };

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            // Only first exception is thrown
            Console.WriteLine($"Caught: {ex.Message}");  // "Task 1 failed"

            // To get ALL exceptions:
            var allExceptions = tasks
                .Where(t => t.IsFaulted)
                .SelectMany(t => t.Exception!.InnerExceptions);

            foreach (var e in allExceptions)
                Console.WriteLine($"Exception: {e.Message}");
        }
    }

    private Task<decimal> GetRateAsync(string pair) => Task.FromResult(1.0m);
    private Task ProcessOrderAsync(Order order) => Task.CompletedTask;
}
public record Order(int Id);
```

### Task.WhenAny - Wait for FIRST task

```csharp
public class WhenAnyExamples
{
    // ═══════════════════════════════════════════════════════════════
    // WhenAny - Completes when ANY task completes (first one wins)
    // ═══════════════════════════════════════════════════════════════

    // Use Case 1: First response wins (redundant calls)
    public async Task<decimal> GetRateFromFastestSourceAsync()
    {
        Task<decimal> reuters = GetRateFromReutersAsync();
        Task<decimal> bloomberg = GetRateFromBloombergAsync();
        Task<decimal> ebs = GetRateFromEBSAsync();

        // Return first successful response
        Task<decimal> firstCompleted = await Task.WhenAny(reuters, bloomberg, ebs);
        return await firstCompleted;
    }

    // Use Case 2: Timeout pattern
    public async Task<decimal?> GetRateWithTimeoutAsync(string pair, TimeSpan timeout)
    {
        Task<decimal> rateTask = GetRateAsync(pair);
        Task timeoutTask = Task.Delay(timeout);

        Task completedTask = await Task.WhenAny(rateTask, timeoutTask);

        if (completedTask == timeoutTask)
        {
            Console.WriteLine("Request timed out!");
            return null;
        }

        return await rateTask;
    }

    // Use Case 3: Process tasks as they complete
    public async Task ProcessAsCompletedAsync()
    {
        var tasks = new List<Task<decimal>>
        {
            GetRateAsync("EUR/USD"),
            GetRateAsync("GBP/USD"),
            GetRateAsync("USD/JPY")
        };

        while (tasks.Count > 0)
        {
            Task<decimal> completed = await Task.WhenAny(tasks);
            tasks.Remove(completed);

            decimal rate = await completed;
            Console.WriteLine($"Got rate: {rate}");
        }
    }

    private Task<decimal> GetRateFromReutersAsync() => Task.FromResult(1.0850m);
    private Task<decimal> GetRateFromBloombergAsync() => Task.FromResult(1.0851m);
    private Task<decimal> GetRateFromEBSAsync() => Task.FromResult(1.0849m);
    private Task<decimal> GetRateAsync(string pair) => Task.FromResult(1.0m);
}
```

### Visual Comparison

```
┌─────────────────────────────────────────────────────────────────────────┐
│                   WhenAll vs WhenAny                                    │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Task.WhenAll                        Task.WhenAny                       │
│  ════════════                        ════════════                       │
│                                                                         │
│  Task1 ████████████░░░░░░ (8s)      Task1 ████████████░░░░░░ (8s)      │
│  Task2 ████████░░░░░░░░░░ (5s)      Task2 ████████░░░░░░░░░░ (5s)      │
│  Task3 ████████████████░░ (10s)     Task3 ████████████████░░ (10s)     │
│                       ▲                      ▲                          │
│                       │                      │                          │
│              WhenAll completes       WhenAny completes                  │
│              when ALL done (10s)     when FIRST done (5s)               │
│                                                                         │
│  Returns: T[]                        Returns: Task<T>                   │
│  (results of all tasks)              (the completed task itself)        │
│                                                                         │
│  Use Cases:                          Use Cases:                         │
│  • Parallel independent work         • First-wins racing                │
│  • Batch processing                  • Timeout implementation           │
│  • Multiple API calls                • Process as completed             │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 8. Task Continuations

### ContinueWith (Old way)

```csharp
public class TaskContinuations
{
    // ═══════════════════════════════════════════════════════════════
    // ContinueWith - Old style, still useful sometimes
    // ═══════════════════════════════════════════════════════════════

    public void ContinueWithExample()
    {
        Task.Run(() => FetchData())
            .ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    Console.WriteLine($"Error: {t.Exception?.InnerException?.Message}");
                    return null;
                }
                return ProcessData(t.Result);
            })
            .ContinueWith(t =>
            {
                if (t.Result != null)
                    SaveData(t.Result);
            });
    }

    // With continuation options
    public void ContinueWithOptions()
    {
        Task.Run(() => FetchData())
            .ContinueWith(
                t => ProcessData(t.Result),
                TaskContinuationOptions.OnlyOnRanToCompletion)  // Only if success
            .ContinueWith(
                t => HandleError(t.Exception),
                TaskContinuationOptions.OnlyOnFaulted);  // Only if failed
    }

    // ═══════════════════════════════════════════════════════════════
    // ✅ PREFER async/await over ContinueWith
    // ═══════════════════════════════════════════════════════════════

    public async Task BetterWayAsync()
    {
        try
        {
            var data = await Task.Run(() => FetchData());
            var processed = ProcessData(data);
            SaveData(processed);
        }
        catch (Exception ex)
        {
            HandleError(ex);
        }
    }

    private string FetchData() => "data";
    private string ProcessData(string data) => data.ToUpper();
    private void SaveData(string data) { }
    private void HandleError(Exception? ex) { }
}
```

---

## 9. Exception Handling

```csharp
public class TaskExceptionHandling
{
    // ═══════════════════════════════════════════════════════════════
    // Exception handling with await
    // ═══════════════════════════════════════════════════════════════

    public async Task HandleSingleException()
    {
        try
        {
            await Task.Run(() => throw new InvalidOperationException("Failed!"));
        }
        catch (InvalidOperationException ex)
        {
            // Exception is unwrapped automatically with await
            Console.WriteLine($"Caught: {ex.Message}");
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // Multiple exceptions with WhenAll
    // ═══════════════════════════════════════════════════════════════

    public async Task HandleMultipleExceptions()
    {
        var tasks = new[]
        {
            Task.Run(() => { throw new Exception("Error 1"); }),
            Task.Run(() => { throw new Exception("Error 2"); }),
            Task.FromResult(42)  // This one succeeds
        };

        Task allTask = Task.WhenAll(tasks);

        try
        {
            await allTask;
        }
        catch
        {
            // Only FIRST exception is thrown when using await
            // To get ALL exceptions:
            if (allTask.Exception != null)
            {
                foreach (var ex in allTask.Exception.InnerExceptions)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // Fire and forget - exceptions are LOST!
    // ═══════════════════════════════════════════════════════════════

    public void DangerousFireAndForget()
    {
        // ❌ Exception is LOST! App might crash unexpectedly
        Task.Run(() => throw new Exception("This is lost!"));

        // ✅ If you must fire-and-forget, handle exceptions
        _ = SafeFireAndForgetAsync();
    }

    private async Task SafeFireAndForgetAsync()
    {
        try
        {
            await Task.Run(() => throw new Exception("Handled!"));
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine($"Background task failed: {ex.Message}");
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // AggregateException with .Wait() or .Result
    // ═══════════════════════════════════════════════════════════════

    public void AggregateExceptionExample()
    {
        Task task = Task.Run(() => throw new InvalidOperationException("Failed!"));

        try
        {
            task.Wait();  // Blocking wait
        }
        catch (AggregateException ae)
        {
            // .Wait() and .Result wrap exceptions in AggregateException
            foreach (var ex in ae.InnerExceptions)
            {
                Console.WriteLine($"Inner: {ex.Message}");
            }

            // Or flatten nested AggregateExceptions
            foreach (var ex in ae.Flatten().InnerExceptions)
            {
                Console.WriteLine($"Flattened: {ex.Message}");
            }
        }
    }
}
```

---

## 10. Task.Run vs Task.Factory.StartNew

```csharp
public class TaskRunVsFactoryStartNew
{
    // ═══════════════════════════════════════════════════════════════
    // Task.Run - Simple, preferred for most cases
    // ═══════════════════════════════════════════════════════════════

    public async Task UseTaskRun()
    {
        // Simple CPU-bound work
        await Task.Run(() => ProcessData());

        // With return value
        int result = await Task.Run(() => Calculate());

        // With cancellation
        var cts = new CancellationTokenSource();
        await Task.Run(() => ProcessData(), cts.Token);
    }

    // ═══════════════════════════════════════════════════════════════
    // Task.Factory.StartNew - More options
    // ═══════════════════════════════════════════════════════════════

    public async Task UseFactoryStartNew()
    {
        // Long-running task (gets dedicated thread, not from pool)
        await Task.Factory.StartNew(
            () => LongRunningWork(),
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default
        );

        // With custom scheduler
        await Task.Factory.StartNew(
            () => ProcessData(),
            CancellationToken.None,
            TaskCreationOptions.None,
            TaskScheduler.FromCurrentSynchronizationContext()  // UI thread
        );
    }

    // ═══════════════════════════════════════════════════════════════
    // ⚠️ GOTCHA: async lambdas with StartNew
    // ═══════════════════════════════════════════════════════════════

    public async Task AsyncLambdaGotcha()
    {
        // ❌ WRONG: This returns Task<Task>!
        Task outer = Task.Factory.StartNew(async () =>
        {
            await Task.Delay(1000);
        });
        await outer;  // Returns immediately! Inner task still running

        // ✅ CORRECT: Unwrap the nested task
        await Task.Factory.StartNew(async () =>
        {
            await Task.Delay(1000);
        }).Unwrap();

        // ✅ OR: Just use Task.Run (handles async lambdas correctly)
        await Task.Run(async () =>
        {
            await Task.Delay(1000);
        });
    }

    private void ProcessData() { }
    private int Calculate() => 42;
    private void LongRunningWork() { Thread.Sleep(60000); }
}
```

### Comparison Table

```
┌─────────────────────────────────────────────────────────────────────────┐
│                Task.Run vs Task.Factory.StartNew                        │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Feature              │ Task.Run          │ Task.Factory.StartNew       │
│  ─────────────────────┼───────────────────┼─────────────────────────────│
│  Simplicity           │ ✅ Simple         │ More verbose                │
│  Async lambda         │ ✅ Unwraps auto   │ ❌ Returns Task<Task>       │
│  LongRunning option   │ ❌ Not available  │ ✅ Available                │
│  Custom scheduler     │ ❌ Not available  │ ✅ Available                │
│  Nested tasks         │ ✅ Auto unwrap    │ ❌ Manual Unwrap()          │
│  Default scheduler    │ ThreadPool        │ ThreadPool                  │
│                                                                         │
│  RECOMMENDATION:                                                        │
│  • Use Task.Run for 99% of cases                                       │
│  • Use StartNew only when you need LongRunning or custom scheduler     │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 11. ValueTask vs Task

```csharp
public class ValueTaskVsTask
{
    private readonly Dictionary<string, decimal> _cache = new();

    // ═══════════════════════════════════════════════════════════════
    // Task - allocates on heap (small overhead)
    // ═══════════════════════════════════════════════════════════════

    public async Task<decimal> GetRateAsync_Task(string pair)
    {
        if (_cache.TryGetValue(pair, out var rate))
        {
            return rate;  // Even cached path creates Task object!
        }

        return await FetchRateFromApiAsync(pair);
    }

    // ═══════════════════════════════════════════════════════════════
    // ValueTask - struct, no allocation when sync
    // ═══════════════════════════════════════════════════════════════

    public ValueTask<decimal> GetRateAsync_ValueTask(string pair)
    {
        if (_cache.TryGetValue(pair, out var rate))
        {
            return new ValueTask<decimal>(rate);  // No allocation!
        }

        return new ValueTask<decimal>(FetchRateFromApiAsync(pair));
    }

    // ═══════════════════════════════════════════════════════════════
    // ⚠️ ValueTask RESTRICTIONS
    // ═══════════════════════════════════════════════════════════════

    public async Task ValueTaskRestrictions()
    {
        ValueTask<decimal> vt = GetRateAsync_ValueTask("EUR/USD");

        // ❌ CAN'T await multiple times
        // decimal r1 = await vt;
        // decimal r2 = await vt;  // WRONG!

        // ❌ CAN'T use WhenAll directly
        // await Task.WhenAll(vt);  // Won't compile

        // ✅ Convert to Task if needed
        Task<decimal> task = vt.AsTask();
        await Task.WhenAll(task);

        // ✅ Or await once immediately
        decimal result = await vt;
    }

    private Task<decimal> FetchRateFromApiAsync(string pair) => Task.FromResult(1.0850m);
}
```

### When to use ValueTask?

```
┌─────────────────────────────────────────────────────────────────────────┐
│                   WHEN TO USE ValueTask?                                │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Use ValueTask when:                                                    │
│  ✅ Method often completes synchronously (cached results)              │
│  ✅ Called in tight loops / hot paths                                  │
│  ✅ Performance is critical                                            │
│                                                                         │
│  Use Task when:                                                         │
│  ✅ Method usually does async work                                     │
│  ✅ Result needs to be awaited multiple times                          │
│  ✅ Need to use Task.WhenAll / WhenAny                                 │
│  ✅ Simplicity is preferred                                            │
│                                                                         │
│  Rule of Thumb:                                                         │
│  • Start with Task (simpler, safer)                                    │
│  • Switch to ValueTask only after profiling shows benefit              │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 12. TaskCompletionSource

Used to create a Task that you control manually - perfect for wrapping callback-based APIs.

```csharp
public class TaskCompletionSourceExamples
{
    // ═══════════════════════════════════════════════════════════════
    // Wrapping callback-based API to Task
    // ═══════════════════════════════════════════════════════════════

    // Old callback-based API
    public void OldApi_GetRate(string pair, Action<decimal> onSuccess, Action<Exception> onError)
    {
        // Simulating async callback
        ThreadPool.QueueUserWorkItem(_ =>
        {
            try
            {
                Thread.Sleep(100);  // Simulate work
                onSuccess(1.0850m);
            }
            catch (Exception ex)
            {
                onError(ex);
            }
        });
    }

    // Wrap with TaskCompletionSource
    public Task<decimal> GetRateAsync(string pair)
    {
        var tcs = new TaskCompletionSource<decimal>();

        OldApi_GetRate(
            pair,
            rate => tcs.SetResult(rate),           // Success
            ex => tcs.SetException(ex)             // Error
        );

        return tcs.Task;
    }

    // ═══════════════════════════════════════════════════════════════
    // Manual timeout implementation
    // ═══════════════════════════════════════════════════════════════

    public Task TimeoutAfter(TimeSpan timeout)
    {
        var tcs = new TaskCompletionSource<bool>();

        var timer = new Timer(_ =>
        {
            tcs.TrySetResult(true);
        }, null, timeout, Timeout.InfiniteTimeSpan);

        return tcs.Task;
    }

    // ═══════════════════════════════════════════════════════════════
    // Signaling completion from another thread
    // ═══════════════════════════════════════════════════════════════

    private TaskCompletionSource<decimal>? _rateTcs;

    public Task<decimal> WaitForRateUpdateAsync()
    {
        _rateTcs = new TaskCompletionSource<decimal>();
        return _rateTcs.Task;
    }

    // Called from another thread when rate arrives
    public void OnRateReceived(decimal rate)
    {
        _rateTcs?.TrySetResult(rate);
    }

    // ═══════════════════════════════════════════════════════════════
    // TrySet methods (safe, won't throw if already set)
    // ═══════════════════════════════════════════════════════════════

    public void TrySetMethods()
    {
        var tcs = new TaskCompletionSource<int>();

        // These return bool - safe to call multiple times
        tcs.TrySetResult(42);
        tcs.TrySetException(new Exception("Error"));
        tcs.TrySetCanceled();

        // These throw if already completed
        // tcs.SetResult(42);
        // tcs.SetException(new Exception("Error"));
        // tcs.SetCanceled();
    }
}
```

---

## 13. Common Patterns & Best Practices

### Pattern 1: Parallel Execution

```csharp
public class ParallelPatterns
{
    // ✅ Run independent tasks in parallel
    public async Task<(decimal eur, decimal gbp, decimal jpy)> GetAllRatesAsync()
    {
        // Start all at once
        var eurTask = GetRateAsync("EUR/USD");
        var gbpTask = GetRateAsync("GBP/USD");
        var jpyTask = GetRateAsync("USD/JPY");

        // Wait for all
        await Task.WhenAll(eurTask, gbpTask, jpyTask);

        return (eurTask.Result, gbpTask.Result, jpyTask.Result);
    }

    // ❌ DON'T do this - Sequential!
    public async Task<(decimal, decimal, decimal)> GetAllRatesSequentialAsync()
    {
        var eur = await GetRateAsync("EUR/USD");  // Wait
        var gbp = await GetRateAsync("GBP/USD");  // Then wait
        var jpy = await GetRateAsync("USD/JPY"); // Then wait
        return (eur, gbp, jpy);
    }

    private Task<decimal> GetRateAsync(string pair) => Task.FromResult(1.0m);
}
```

### Pattern 2: Retry with Exponential Backoff

```csharp
public class RetryPattern
{
    public async Task<T> RetryAsync<T>(
        Func<Task<T>> operation,
        int maxRetries = 3,
        int baseDelayMs = 1000)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (i < maxRetries - 1)
            {
                int delay = baseDelayMs * (int)Math.Pow(2, i);  // 1s, 2s, 4s
                Console.WriteLine($"Attempt {i + 1} failed. Retrying in {delay}ms...");
                await Task.Delay(delay);
            }
        }

        throw new Exception("All retries failed");
    }

    public async Task UseRetry()
    {
        var rate = await RetryAsync(() => GetRateFromApiAsync("EUR/USD"));
    }

    private Task<decimal> GetRateFromApiAsync(string pair) => Task.FromResult(1.0850m);
}
```

### Pattern 3: Timeout

```csharp
public class TimeoutPattern
{
    public async Task<T?> WithTimeoutAsync<T>(Task<T> task, TimeSpan timeout)
    {
        var timeoutTask = Task.Delay(timeout);
        var completedTask = await Task.WhenAny(task, timeoutTask);

        if (completedTask == timeoutTask)
        {
            throw new TimeoutException($"Operation timed out after {timeout}");
        }

        return await task;
    }

    public async Task UseTimeout()
    {
        try
        {
            var rate = await WithTimeoutAsync(
                GetRateAsync("EUR/USD"),
                TimeSpan.FromSeconds(5));
        }
        catch (TimeoutException)
        {
            Console.WriteLine("Rate fetch timed out!");
        }
    }

    private Task<decimal> GetRateAsync(string pair) => Task.FromResult(1.0850m);
}
```

### Pattern 4: Semaphore for Throttling

```csharp
public class ThrottlingPattern
{
    private readonly SemaphoreSlim _semaphore = new(maxCount: 5);  // Max 5 concurrent

    public async Task ProcessAllAsync(List<string> pairs)
    {
        var tasks = pairs.Select(pair => ProcessWithThrottleAsync(pair));
        await Task.WhenAll(tasks);
    }

    private async Task<decimal> ProcessWithThrottleAsync(string pair)
    {
        await _semaphore.WaitAsync();  // Wait for slot
        try
        {
            return await GetRateAsync(pair);
        }
        finally
        {
            _semaphore.Release();  // Release slot
        }
    }

    private Task<decimal> GetRateAsync(string pair) => Task.FromResult(1.0850m);
}
```

### Best Practices Summary

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    TASK BEST PRACTICES                                  │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ✅ DO:                                                                 │
│  • Use async/await all the way (don't mix with .Result/.Wait())        │
│  • Use Task.Run for CPU-bound work only                                │
│  • Use ConfigureAwait(false) in libraries                              │
│  • Use CancellationToken for cancellation                              │
│  • Return Task.CompletedTask instead of Task.FromResult(void)          │
│  • Start multiple independent tasks before awaiting                     │
│  • Handle all exceptions from fire-and-forget tasks                    │
│                                                                         │
│  ❌ DON'T:                                                              │
│  • Use .Result or .Wait() in async code (deadlocks!)                   │
│  • Use Task.Run for I/O-bound operations                               │
│  • Use async void (except for event handlers)                          │
│  • Ignore task return values                                           │
│  • Create tasks that are never awaited                                  │
│  • Use Task.Factory.StartNew without understanding it                  │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 14. Interview Questions

### Q1: What is the difference between Task and Thread?

**Answer:**
| Aspect | Thread | Task |
|--------|--------|------|
| Level | OS-level | High-level abstraction |
| Resource | 1MB stack each | Uses ThreadPool (shared) |
| Return value | No direct support | Task<T> returns value |
| Exception | Crashes if unhandled | Captured in Task |
| Composition | Manual | WhenAll, WhenAny, ContinueWith |
| Cancellation | Manual | CancellationToken built-in |

---

### Q2: When would you use Task.Run vs async/await directly?

**Answer:**

```csharp
// Task.Run → CPU-bound work (offload to thread pool)
await Task.Run(() => HeavyCalculation());

// Direct async/await → I/O-bound work (no extra thread needed)
await File.ReadAllTextAsync("file.txt");
await httpClient.GetAsync("https://api.example.com");
```

---

### Q3: What's the danger of using .Result or .Wait()?

**Answer:**

```csharp
// In ASP.NET or UI apps, this can DEADLOCK:
public string GetData()
{
    return GetDataAsync().Result;  // ❌ DEADLOCK!
}

// The await needs to resume on the same context,
// but .Result is blocking that context
```

---

### Q4: How do you handle exceptions in Task.WhenAll?

**Answer:**

```csharp
var tasks = new[] { Task1(), Task2(), Task3() };
var allTask = Task.WhenAll(tasks);

try
{
    await allTask;
}
catch
{
    // Only first exception thrown with await
    // Get ALL exceptions:
    var exceptions = allTask.Exception?.InnerExceptions;
    foreach (var ex in exceptions ?? Enumerable.Empty<Exception>())
        Console.WriteLine(ex.Message);
}
```

---

### Q5: What is TaskCompletionSource and when would you use it?

**Answer:**
TaskCompletionSource lets you create a Task that you control manually. Use cases:

1. Wrapping callback-based APIs to Task-based
2. Signaling completion from another thread
3. Creating custom async primitives

```csharp
public Task<decimal> WrapCallbackApi(string pair)
{
    var tcs = new TaskCompletionSource<decimal>();

    CallbackApi.GetRate(pair,
        rate => tcs.SetResult(rate),
        ex => tcs.SetException(ex));

    return tcs.Task;
}
```

---

### Q6: What's the difference between Task.WhenAll and Parallel.ForEach?

**Answer:**
| WhenAll | Parallel.ForEach |
|---------|------------------|
| Async I/O friendly | Blocks calling thread |
| Non-blocking | Blocking |
| Best for I/O-bound | Best for CPU-bound |
| Returns Task | Returns void (synchronous) |

---

### Q7: Why use ValueTask over Task?

**Answer:**

- `ValueTask` is a struct (no heap allocation)
- Useful when method often completes synchronously (cached results)
- Restrictions: Can only await once, can't use with WhenAll directly

```csharp
public ValueTask<decimal> GetCachedRate(string pair)
{
    if (_cache.TryGetValue(pair, out var rate))
        return new ValueTask<decimal>(rate);  // No allocation!

    return new ValueTask<decimal>(FetchAsync(pair));
}
```

---

### Q8: How do you implement a timeout for an async operation?

**Answer:**

```csharp
public async Task<T> WithTimeout<T>(Task<T> task, TimeSpan timeout)
{
    var completed = await Task.WhenAny(task, Task.Delay(timeout));

    if (completed != task)
        throw new TimeoutException();

    return await task;
}
```

---

### Q9: What happens to exceptions in fire-and-forget tasks?

**Answer:**

```csharp
// ❌ Exception is LOST
Task.Run(() => throw new Exception("Lost!"));

// ✅ Safe fire-and-forget
_ = Task.Run(async () =>
{
    try
    {
        await RiskyOperationAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Background task failed");
    }
});
```

---

### Q10: Explain ConfigureAwait(false) - when and why?

**Answer:**

```csharp
// In LIBRARY code:
public async Task<string> GetDataAsync()
{
    var result = await httpClient.GetStringAsync(url)
        .ConfigureAwait(false);  // Don't capture context

    return result;
}

// Why?
// - Avoids deadlocks when library is called with .Result
// - Better performance (no context switch back)
// - NOT needed in app code (ASP.NET Core has no SynchronizationContext)
```

---

## Quick Reference Card

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     TASK QUICK REFERENCE                                │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  CREATE:                                                                │
│    Task.Run(() => Work())              // CPU-bound                     │
│    await DoAsync()                      // I/O-bound                    │
│    Task.FromResult(value)               // Already complete             │
│    Task.CompletedTask                   // Void complete                │
│    new TaskCompletionSource<T>()        // Manual control               │
│                                                                         │
│  WAIT:                                                                  │
│    await task                           // ✅ Non-blocking             │
│    task.Wait()                          // ❌ Blocking                 │
│    task.Result                          // ❌ Blocking                 │
│                                                                         │
│  COMBINE:                                                               │
│    await Task.WhenAll(t1, t2, t3)      // Wait for ALL                 │
│    await Task.WhenAny(t1, t2, t3)      // Wait for FIRST               │
│                                                                         │
│  STATUS:                                                                │
│    task.Status                          // Created/Running/Completed   │
│    task.IsCompleted                     // true if done (any state)    │
│    task.IsCompletedSuccessfully         // true if RanToCompletion     │
│    task.IsFaulted                       // true if exception           │
│    task.IsCanceled                      // true if canceled            │
│                                                                         │
│  EXCEPTIONS:                                                            │
│    try { await task; } catch { }       // Unwrapped exception          │
│    task.Exception?.InnerExceptions     // All exceptions               │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

**Author:** Interview Prep Guide  
**Last Updated:** February 2026  
**Framework:** .NET 8
