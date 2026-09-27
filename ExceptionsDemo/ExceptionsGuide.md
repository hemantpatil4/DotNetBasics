# C# Exceptions – Complete Interview Guide

> **Interview Focus:** Exception hierarchy, try-catch-finally, custom exceptions, best practices  
> **Java Equivalent:** Similar hierarchy but C# has NO checked exceptions

---

## Table of Contents

1. [What is an Exception?](#1-what-is-an-exception)
2. [Exception Hierarchy](#2-exception-hierarchy)
3. [Common Exception Types](#3-common-exception-types)
4. [Try-Catch-Finally](#4-try-catch-finally)
5. [Throwing Exceptions](#5-throwing-exceptions)
6. [Custom Exceptions](#6-custom-exceptions)
7. [Exception Filters](#7-exception-filters)
8. [Best Practices](#8-best-practices)
9. [C# vs Java Exceptions](#9-c-vs-java-exceptions)
10. [Interview Questions](#10-interview-questions)

---

## 1. What is an Exception?

An **exception** is an object that represents an error or unexpected condition during program execution.

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                         EXCEPTION CONCEPT                                     ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║   Normal Flow:                                                                ║
║   ────────────                                                                ║
║   Method A → Method B → Method C → Return → Return → Return                  ║
║                                                                               ║
║   With Exception:                                                             ║
║   ───────────────                                                             ║
║   Method A → Method B → Method C                                              ║
║                              ↓                                                ║
║                         EXCEPTION!                                            ║
║                              ↓                                                ║
║                    ← ← ← Unwind Stack ← ← ←                                  ║
║                              ↓                                                ║
║                    First matching catch block                                 ║
║                              ↓                                                ║
║                    Handle or crash                                            ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

### Exception Object Contains:

```csharp
try
{
    int result = 10 / 0;
}
catch (DivideByZeroException ex)
{
    Console.WriteLine(ex.Message);        // "Attempted to divide by zero."
    Console.WriteLine(ex.StackTrace);     // Where it happened
    Console.WriteLine(ex.Source);         // Assembly name
    Console.WriteLine(ex.InnerException); // Nested exception (if any)
    Console.WriteLine(ex.HelpLink);       // URL for help
    Console.WriteLine(ex.Data);           // Additional key-value data
}
```

---

## 2. Exception Hierarchy

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                    .NET EXCEPTION HIERARCHY                                   ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║                              System.Object                                    ║
║                                   │                                           ║
║                                   ▼                                           ║
║                           System.Exception     ← Base class for ALL          ║
║                                   │                                           ║
║              ┌────────────────────┼────────────────────┐                     ║
║              ▼                    ▼                    ▼                     ║
║     System.SystemException  System.ApplicationException  (Custom)            ║
║              │                    │                                          ║
║              │              (Obsolete - don't use)                           ║
║              │                                                               ║
║     ┌────────┴────────┬──────────────┬──────────────┐                       ║
║     ▼                 ▼              ▼              ▼                       ║
║ ArgumentException  InvalidOperation  NullReference  IOException             ║
║     │              Exception        Exception           │                    ║
║     │                                                   │                    ║
║     ├─ ArgumentNullException                    FileNotFoundException        ║
║     ├─ ArgumentOutOfRangeException              DirectoryNotFoundException   ║
║     └─ ...                                      EndOfStreamException        ║
║                                                                               ║
║   INTERVIEW NOTE:                                                             ║
║   • SystemException = CLR/Runtime exceptions (don't catch broadly)           ║
║   • ApplicationException = Deprecated, don't derive from it                  ║
║   • Derive custom exceptions from Exception directly                         ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

### Detailed Hierarchy Tree

```
System.Exception
│
├── System.SystemException (Runtime/CLR exceptions)
│   │
│   ├── System.ArgumentException
│   │   ├── System.ArgumentNullException
│   │   ├── System.ArgumentOutOfRangeException
│   │   └── System.DuplicateWaitObjectException
│   │
│   ├── System.ArithmeticException
│   │   ├── System.DivideByZeroException
│   │   ├── System.NotFiniteNumberException
│   │   └── System.OverflowException
│   │
│   ├── System.ArrayTypeMismatchException
│   │
│   ├── System.FormatException
│   │
│   ├── System.IndexOutOfRangeException
│   │
│   ├── System.InvalidCastException
│   │
│   ├── System.InvalidOperationException
│   │   └── System.ObjectDisposedException
│   │
│   ├── System.NullReferenceException
│   │
│   ├── System.OutOfMemoryException
│   │
│   ├── System.StackOverflowException (Cannot be caught!)
│   │
│   ├── System.TypeInitializationException
│   │
│   ├── System.NotSupportedException
│   │   └── System.PlatformNotSupportedException
│   │
│   ├── System.NotImplementedException
│   │
│   ├── System.IO.IOException
│   │   ├── System.IO.FileNotFoundException
│   │   ├── System.IO.DirectoryNotFoundException
│   │   ├── System.IO.EndOfStreamException
│   │   ├── System.IO.FileLoadException
│   │   └── System.IO.PathTooLongException
│   │
│   ├── System.Net.WebException
│   │
│   ├── System.Data.DataException
│   │
│   └── System.TimeoutException
│
├── System.ApplicationException (DEPRECATED - Don't use!)
│
└── [Your Custom Exceptions - derive from Exception]
```

---

## 3. Common Exception Types

### Argument Exceptions (Invalid input)

```csharp
// ArgumentNullException - parameter is null
public void ProcessTrade(Trade trade)
{
    if (trade == null)
        throw new ArgumentNullException(nameof(trade));
    // Or use C# 10+:
    ArgumentNullException.ThrowIfNull(trade);
}

// ArgumentException - parameter is invalid
public void SetRate(string currencyPair, decimal rate)
{
    if (string.IsNullOrWhiteSpace(currencyPair))
        throw new ArgumentException("Currency pair cannot be empty", nameof(currencyPair));
}

// ArgumentOutOfRangeException - value outside valid range
public void SetQuantity(int quantity)
{
    if (quantity <= 0)
        throw new ArgumentOutOfRangeException(nameof(quantity), quantity,
            "Quantity must be positive");
}
```

### Invalid Operation Exceptions (Wrong state)

```csharp
// InvalidOperationException - operation not valid for current state
public class TradeProcessor
{
    private bool _isInitialized = false;

    public void ProcessTrade(Trade trade)
    {
        if (!_isInitialized)
            throw new InvalidOperationException("Processor not initialized. Call Initialize() first.");
    }
}

// ObjectDisposedException - using disposed object
public class RateService : IDisposable
{
    private bool _disposed = false;

    public decimal GetRate(string pair)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RateService));
        // ...
    }
}
```

### Null Reference & Type Exceptions

```csharp
// NullReferenceException - accessing member on null
string name = null;
int length = name.Length;  // NullReferenceException!

// InvalidCastException - invalid type conversion
object obj = "hello";
int num = (int)obj;  // InvalidCastException!

// ArrayTypeMismatchException
object[] strings = new string[3];
strings[0] = 42;  // ArrayTypeMismatchException!
```

### Collection Exceptions

```csharp
// IndexOutOfRangeException - array index invalid
int[] arr = { 1, 2, 3 };
int val = arr[10];  // IndexOutOfRangeException!

// KeyNotFoundException - dictionary key not found
var dict = new Dictionary<string, int> { ["a"] = 1 };
int val = dict["b"];  // KeyNotFoundException!
```

### IO Exceptions

```csharp
// FileNotFoundException
string content = File.ReadAllText("nonexistent.txt");

// DirectoryNotFoundException
var files = Directory.GetFiles("/nonexistent/path");

// IOException (base class)
// Includes: file in use, disk full, etc.
```

### Format & Arithmetic Exceptions

```csharp
// FormatException - invalid string format
int num = int.Parse("abc");  // FormatException!

// DivideByZeroException
int result = 10 / 0;  // DivideByZeroException!

// OverflowException (in checked context)
checked
{
    int max = int.MaxValue;
    int overflow = max + 1;  // OverflowException!
}
```

---

## 4. Try-Catch-Finally

### Basic Syntax

```csharp
try
{
    // Code that might throw
    ProcessTrade(trade);
}
catch (ArgumentNullException ex)
{
    // Handle specific exception
    Console.WriteLine($"Null argument: {ex.ParamName}");
}
catch (InvalidOperationException ex)
{
    // Handle another specific exception
    Console.WriteLine($"Invalid state: {ex.Message}");
}
catch (Exception ex)
{
    // Catch-all (last resort)
    Console.WriteLine($"Unexpected error: {ex.Message}");
    throw;  // Re-throw to preserve stack trace
}
finally
{
    // ALWAYS runs (even if exception thrown or return executed)
    // Use for cleanup: close files, release resources
    CleanupResources();
}
```

### Execution Flow

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                    TRY-CATCH-FINALLY EXECUTION                                ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║   SCENARIO 1: No Exception                                                    ║
║   ────────────────────────────                                                ║
║   try { A; B; C; }  →  A → B → C → finally { D; } → continue                 ║
║                                                                               ║
║   SCENARIO 2: Exception caught                                                ║
║   ────────────────────────────                                                ║
║   try { A; B; C; }  →  A → B (throws!) → catch { E; } → finally { D; }       ║
║                                   ↓                                           ║
║                           Skip C, jump to matching catch                      ║
║                                                                               ║
║   SCENARIO 3: Exception not caught                                            ║
║   ────────────────────────────────                                            ║
║   try { A; B; }  →  A → B (throws!) → finally { D; } → propagate up          ║
║                              ↓                                                ║
║                      No matching catch, run finally, then throw               ║
║                                                                               ║
║   SCENARIO 4: Return in try                                                   ║
║   ────────────────────────────                                                ║
║   try { return X; }  →  finally { D; } → then return                         ║
║                              ↓                                                ║
║                      finally STILL runs before return!                        ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

### Order Matters! (Most Specific First)

```csharp
// ✅ CORRECT: Most specific first
try { /* ... */ }
catch (ArgumentNullException ex) { /* ... */ }  // Specific
catch (ArgumentException ex) { /* ... */ }      // Less specific (parent)
catch (Exception ex) { /* ... */ }              // Least specific (base)

// ❌ WRONG: Compiler error - unreachable catch
try { /* ... */ }
catch (Exception ex) { /* ... */ }              // Catches everything!
catch (ArgumentException ex) { /* ... */ }      // UNREACHABLE!
```

### Try-Finally (No Catch)

```csharp
// Ensure cleanup even without handling exception
public void ProcessFile(string path)
{
    FileStream file = null;
    try
    {
        file = File.OpenRead(path);
        // Process file...
    }
    finally
    {
        file?.Dispose();  // Always cleanup
    }
    // If exception thrown, it propagates after finally runs
}

// Better: Use 'using' statement
public void ProcessFile(string path)
{
    using var file = File.OpenRead(path);
    // Process file...
}  // Automatically disposed
```

---

## 5. Throwing Exceptions

### throw vs throw ex

```csharp
// ✅ CORRECT: Preserves original stack trace
try
{
    DoSomething();
}
catch (Exception ex)
{
    LogError(ex);
    throw;  // Re-throws with original stack trace
}

// ❌ WRONG: Loses original stack trace!
try
{
    DoSomething();
}
catch (Exception ex)
{
    LogError(ex);
    throw ex;  // Stack trace starts HERE, not at original error!
}
```

### Stack Trace Comparison

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                    throw vs throw ex                                          ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║   Using "throw;" (CORRECT):                                                   ║
║   ─────────────────────────                                                   ║
║   at ProcessOrder() in Order.cs:line 25      ← Original error location       ║
║   at ValidateOrder() in Order.cs:line 42                                      ║
║   at Main() in Program.cs:line 10                                             ║
║                                                                               ║
║   Using "throw ex;" (WRONG):                                                  ║
║   ──────────────────────────                                                  ║
║   at Main() in Program.cs:line 15            ← Lost original location!       ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

### Wrapping Exceptions (Preserve Inner)

```csharp
try
{
    CallExternalService();
}
catch (HttpRequestException ex)
{
    // Wrap with context, preserve original as InnerException
    throw new TradeServiceException(
        "Failed to fetch exchange rates",
        ex);  // ← InnerException preserves original
}
```

### Conditional Throwing

```csharp
// Guard clauses (fail fast)
public void ExecuteTrade(Trade trade, decimal rate)
{
    ArgumentNullException.ThrowIfNull(trade);  // C# 10+

    if (rate <= 0)
        throw new ArgumentOutOfRangeException(nameof(rate), rate, "Rate must be positive");

    if (!trade.IsValid)
        throw new InvalidOperationException("Trade is not in valid state");

    // Happy path continues...
}
```

---

## 6. Custom Exceptions

### When to Create Custom Exceptions

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                    WHEN TO CREATE CUSTOM EXCEPTIONS                           ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║   ✅ CREATE when:                                                             ║
║   • Need to catch specific business errors                                   ║
║   • Want to add custom properties (TradeId, ErrorCode, etc.)                 ║
║   • Need different handling for different error types                        ║
║   • Building a library/framework                                             ║
║                                                                               ║
║   ❌ DON'T CREATE when:                                                       ║
║   • An existing exception fits perfectly                                     ║
║   • You won't catch it specifically anywhere                                 ║
║   • Just for logging purposes                                                ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

### Custom Exception Template

```csharp
using System;
using System.Runtime.Serialization;

/// <summary>
/// Thrown when a trade operation fails.
/// </summary>
[Serializable]
public class TradeException : Exception
{
    /// <summary>
    /// The trade ID that caused the exception.
    /// </summary>
    public string? TradeId { get; }

    /// <summary>
    /// Application-specific error code.
    /// </summary>
    public int ErrorCode { get; }

    public TradeException()
        : base() { }

    public TradeException(string message)
        : base(message) { }

    public TradeException(string message, Exception innerException)
        : base(message, innerException) { }

    public TradeException(string message, string tradeId, int errorCode)
        : base(message)
    {
        TradeId = tradeId;
        ErrorCode = errorCode;
    }

    public TradeException(string message, string tradeId, int errorCode, Exception innerException)
        : base(message, innerException)
    {
        TradeId = tradeId;
        ErrorCode = errorCode;
    }

    // For serialization (optional in modern .NET)
    protected TradeException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        TradeId = info.GetString(nameof(TradeId));
        ErrorCode = info.GetInt32(nameof(ErrorCode));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(TradeId), TradeId);
        info.AddValue(nameof(ErrorCode), ErrorCode);
    }

    public override string ToString()
    {
        return $"{base.ToString()}, TradeId: {TradeId}, ErrorCode: {ErrorCode}";
    }
}
```

### FX Trading Domain Exceptions

```csharp
// Base exception for trading system
public class TradingException : Exception
{
    public TradingException() { }
    public TradingException(string message) : base(message) { }
    public TradingException(string message, Exception inner) : base(message, inner) { }
}

// Specific exceptions
public class InsufficientFundsException : TradingException
{
    public decimal Required { get; }
    public decimal Available { get; }
    public string Currency { get; }

    public InsufficientFundsException(decimal required, decimal available, string currency)
        : base($"Insufficient funds: required {required} {currency}, available {available} {currency}")
    {
        Required = required;
        Available = available;
        Currency = currency;
    }
}

public class RateNotFoundException : TradingException
{
    public string CurrencyPair { get; }

    public RateNotFoundException(string currencyPair)
        : base($"Rate not found for currency pair: {currencyPair}")
    {
        CurrencyPair = currencyPair;
    }
}

public class TradeLimitExceededException : TradingException
{
    public decimal TradeAmount { get; }
    public decimal Limit { get; }

    public TradeLimitExceededException(decimal amount, decimal limit)
        : base($"Trade amount {amount} exceeds limit {limit}")
    {
        TradeAmount = amount;
        Limit = limit;
    }
}
```

### Using Custom Exceptions

```csharp
public class TradingService
{
    public void ExecuteTrade(string currencyPair, decimal amount, string side)
    {
        // Check rate exists
        if (!_rateService.HasRate(currencyPair))
            throw new RateNotFoundException(currencyPair);

        // Check funds
        decimal required = CalculateRequired(currencyPair, amount);
        decimal available = _accountService.GetBalance();
        if (available < required)
            throw new InsufficientFundsException(required, available, "USD");

        // Check limits
        if (amount > _limits.MaxTradeSize)
            throw new TradeLimitExceededException(amount, _limits.MaxTradeSize);

        // Execute...
    }
}

// Catching custom exceptions
try
{
    tradingService.ExecuteTrade("EUR/USD", 1_000_000, "BUY");
}
catch (InsufficientFundsException ex)
{
    Console.WriteLine($"Need {ex.Required - ex.Available} more {ex.Currency}");
}
catch (RateNotFoundException ex)
{
    Console.WriteLine($"Invalid pair: {ex.CurrencyPair}");
}
catch (TradeLimitExceededException ex)
{
    Console.WriteLine($"Max trade size is {ex.Limit}");
}
catch (TradingException ex)
{
    // Catch-all for trading exceptions
    Console.WriteLine($"Trading error: {ex.Message}");
}
```

---

## 7. Exception Filters (C# 6+)

Exception filters let you catch exceptions conditionally using `when`:

```csharp
try
{
    CallExternalService();
}
catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
{
    // Handle 404 specifically
    Console.WriteLine("Resource not found");
}
catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
{
    // Handle 401 specifically
    Console.WriteLine("Authentication required");
}
catch (HttpRequestException ex) when (IsTransient(ex))
{
    // Retry transient errors
    await RetryAsync();
}
catch (HttpRequestException ex)
{
    // Handle other HTTP errors
    throw;
}

private bool IsTransient(HttpRequestException ex)
{
    return ex.StatusCode == HttpStatusCode.ServiceUnavailable
        || ex.StatusCode == HttpStatusCode.GatewayTimeout;
}
```

### Filter vs Catch and Re-throw

```csharp
// ✅ BETTER: Exception filter (doesn't unwind stack if condition false)
catch (Exception ex) when (ShouldHandle(ex))
{
    Handle(ex);
}

// ❌ WORSE: Catch and re-throw (unwinds stack, then re-throws)
catch (Exception ex)
{
    if (!ShouldHandle(ex))
        throw;  // Stack already unwound!
    Handle(ex);
}
```

### Logging Without Handling

```csharp
// Log all exceptions but don't handle them (filter always returns false)
try
{
    DoSomething();
}
catch (Exception ex) when (LogException(ex))
{
    // Never reaches here because LogException returns false
}

private bool LogException(Exception ex)
{
    _logger.LogError(ex, "Exception occurred");
    return false;  // Don't actually catch it
}
```

---

## 8. Best Practices

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                    EXCEPTION BEST PRACTICES                                   ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║   DO:                                                                         ║
║   ────                                                                        ║
║   ✅ Use specific exception types when catching                              ║
║   ✅ Use "throw;" to re-throw (preserves stack trace)                        ║
║   ✅ Include meaningful messages                                             ║
║   ✅ Use finally or 'using' for cleanup                                      ║
║   ✅ Fail fast with guard clauses                                            ║
║   ✅ Log exceptions with full stack trace                                    ║
║   ✅ Use exception filters (when clause)                                     ║
║   ✅ Create custom exceptions for domain-specific errors                     ║
║                                                                               ║
║   DON'T:                                                                      ║
║   ──────                                                                      ║
║   ❌ Catch Exception broadly (catch-all)                                     ║
║   ❌ Use "throw ex;" (loses stack trace)                                     ║
║   ❌ Swallow exceptions silently (empty catch)                               ║
║   ❌ Use exceptions for flow control                                         ║
║   ❌ Throw from finally block                                                ║
║   ❌ Catch StackOverflowException/OutOfMemoryException                       ║
║   ❌ Derive from ApplicationException (deprecated)                           ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

### Anti-Patterns

```csharp
// ❌ ANTI-PATTERN 1: Empty catch (swallowing)
try
{
    DoSomething();
}
catch
{
    // Silently swallowed - bugs will hide here!
}

// ❌ ANTI-PATTERN 2: Catching Exception too broadly
try
{
    DoSomething();
}
catch (Exception ex)
{
    Console.WriteLine("Error occurred");
    // What kind of error? How to recover?
}

// ❌ ANTI-PATTERN 3: Exceptions for flow control
try
{
    var rate = dict["EUR/USD"];  // Using exception for "not found"
}
catch (KeyNotFoundException)
{
    rate = DefaultRate;
}

// ✅ CORRECT: Use TryGetValue
if (!dict.TryGetValue("EUR/USD", out var rate))
{
    rate = DefaultRate;
}

// ❌ ANTI-PATTERN 4: Throwing in finally
try
{
    DoSomething();
}
finally
{
    throw new Exception("Bad!");  // Masks original exception!
}
```

### Performance Considerations

```csharp
// Exceptions are EXPENSIVE - don't use for normal flow

// ❌ SLOW: Using exception for validation
public bool IsValidEmail_Bad(string email)
{
    try
    {
        var addr = new System.Net.Mail.MailAddress(email);
        return true;
    }
    catch (FormatException)
    {
        return false;  // Exception thrown for every invalid email!
    }
}

// ✅ FAST: Use validation without exceptions
public bool IsValidEmail_Good(string email)
{
    return !string.IsNullOrWhiteSpace(email)
        && email.Contains('@')
        && email.Contains('.');
}
```

---

## 9. C# vs Java Exceptions

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                    C# vs JAVA EXCEPTIONS                                      ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║   Feature                 C#                      Java                        ║
║   ───────────────────────────────────────────────────────────────────────    ║
║   Checked Exceptions      NO (all unchecked)      YES (must handle or throw) ║
║                                                                               ║
║   Base Class              System.Exception        java.lang.Throwable        ║
║                                                                               ║
║   RuntimeException        SystemException         RuntimeException            ║
║   equivalent                                      (unchecked)                 ║
║                                                                               ║
║   throws clause           Not required            Required for checked       ║
║                                                                               ║
║   Exception filters       YES (when clause)       NO                         ║
║                                                                               ║
║   try-with-resources      using statement         try-with-resources         ║
║                                                                               ║
║   Multi-catch             catch (A) when...       catch (A | B ex)           ║
║                           catch (B) when...                                   ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

### No Checked Exceptions in C#!

```java
// JAVA: Must declare or handle checked exceptions
public void readFile() throws IOException {  // MUST declare!
    FileReader fr = new FileReader("file.txt");
}

// Or handle it
public void readFile() {
    try {
        FileReader fr = new FileReader("file.txt");
    } catch (IOException e) {  // MUST catch!
        // handle
    }
}
```

```csharp
// C#: No checked exceptions - all exceptions are "unchecked"
public void ReadFile()
{
    // No 'throws' clause needed!
    // Caller doesn't have to catch
    var content = File.ReadAllText("file.txt");
}

// You CAN catch, but it's optional
public void ReadFileSafe()
{
    try
    {
        var content = File.ReadAllText("file.txt");
    }
    catch (IOException ex)
    {
        // Optional handling
    }
}
```

---

## 10. Interview Questions

### Q1: What's the difference between `throw` and `throw ex`?

**Answer:**

> - `throw;` re-throws the current exception **preserving the original stack trace**
> - `throw ex;` throws the exception but **resets the stack trace** to the current location
>
> Always use `throw;` when re-throwing to keep debugging information intact.

---

### Q2: Can you catch multiple exception types in one catch block?

**Answer:**

> In C#, use exception filters with `when` clause:
>
> ```csharp
> catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
> ```
>
> Or in C# 9+, you can use pattern matching:
>
> ```csharp
> catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
> ```

---

### Q3: What is the purpose of the `finally` block?

**Answer:**

> The `finally` block **always executes**, whether an exception is thrown or not. It's used for:
>
> - Cleanup code (closing files, database connections)
> - Releasing resources
> - Code that must run regardless of success/failure
>
> Even if there's a `return` in the `try` block, `finally` still runs before the return.

---

### Q4: Why doesn't C# have checked exceptions like Java?

**Answer:**

> C# designers chose not to include checked exceptions because:
>
> - They often lead to empty catch blocks (swallowing)
> - They clutter method signatures with `throws` declarations
> - They can force unnecessary handling at wrong abstraction levels
> - Most developers either catch everything or nothing
>
> Instead, C# encourages documenting exceptions in XML comments.

---

### Q5: When should you create a custom exception?

**Answer:**

> Create custom exceptions when:
>
> - You need to catch specific business/domain errors
> - You want to add custom properties (error codes, IDs)
> - Callers need different handling for different error types
> - Building a library/framework
>
> Don't create if an existing exception fits or you won't catch it specifically.

---

### Q6: What exceptions cannot be caught?

**Answer:**

> - `StackOverflowException` - Cannot be caught (process terminates)
> - `OutOfMemoryException` - Can be caught but often shouldn't (unpredictable state)
> - Some corrupted state exceptions like `AccessViolationException`
>
> These indicate severe runtime problems where recovery is usually impossible.

---

### Q7: What's the difference between `ArgumentException` and `ArgumentNullException`?

**Answer:**

> - `ArgumentNullException`: Thrown when a **null** argument is passed where null is not valid
> - `ArgumentException`: Thrown when an argument is **invalid** for other reasons (empty string, invalid format, etc.)
>
> `ArgumentNullException` is more specific and derives from `ArgumentException`.

---

### Q8: How do exception filters work and why are they useful?

**Answer:**

> Exception filters (C# 6+) use the `when` clause:
>
> ```csharp
> catch (Exception ex) when (condition)
> ```
>
> Benefits:
>
> - **Don't unwind stack** if condition is false (better for debugging)
> - Can catch same exception type with different conditions
> - Can log without handling (filter returns false)

---

### Q9: How would you implement the Try pattern (like `int.TryParse`)?

**Answer:**

```csharp
public bool TryExecuteTrade(Trade trade, out TradeResult result)
{
    result = default;
    try
    {
        result = ExecuteTrade(trade);
        return true;
    }
    catch (TradingException)
    {
        return false;
    }
}

// Usage: No exception thrown to caller
if (service.TryExecuteTrade(trade, out var result))
{
    Console.WriteLine($"Success: {result}");
}
else
{
    Console.WriteLine("Trade failed");
}
```

---

### Q10: What is the correct way to handle exceptions in async methods?

**Answer:**

```csharp
// Async method exceptions are captured in the Task
public async Task<decimal> GetRateAsync(string pair)
{
    try
    {
        var response = await _client.GetAsync($"/rates/{pair}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<decimal>();
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "Failed to get rate for {Pair}", pair);
        throw new RateServiceException($"Failed to get rate for {pair}", ex);
    }
}

// Caller must await to observe exception
try
{
    decimal rate = await GetRateAsync("EUR/USD");
}
catch (RateServiceException ex)
{
    // Handle
}
```

---

## Summary Cheat Sheet

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                    EXCEPTIONS CHEAT SHEET                                     ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║   HIERARCHY:                                                                  ║
║   Exception → SystemException → ArgumentException → ArgumentNullException    ║
║                              → InvalidOperationException                     ║
║                              → NullReferenceException                        ║
║                              → IOException → FileNotFoundException           ║
║                                                                               ║
║   KEY RULES:                                                                  ║
║   • Use "throw;" not "throw ex;" (preserve stack trace)                      ║
║   • Catch specific exceptions, not Exception                                 ║
║   • finally always runs (cleanup)                                            ║
║   • Don't use exceptions for flow control                                    ║
║   • Custom exceptions derive from Exception (not ApplicationException)       ║
║                                                                               ║
║   COMMON EXCEPTIONS:                                                          ║
║   • ArgumentNullException - null parameter                                   ║
║   • ArgumentException - invalid parameter                                    ║
║   • InvalidOperationException - wrong state                                  ║
║   • NullReferenceException - null dereference                                ║
║   • KeyNotFoundException - dictionary miss                                   ║
║   • FileNotFoundException - file not found                                   ║
║   • FormatException - parse failure                                          ║
║                                                                               ║
║   C# UNIQUE:                                                                  ║
║   • No checked exceptions                                                    ║
║   • Exception filters: catch (E ex) when (condition)                        ║
║   • using statement = try-finally for IDisposable                           ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

---

_See `ExceptionsDemo.cs` for runnable examples and `ExceptionsPractice.md` for coding challenges!_
