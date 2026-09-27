# Exception Handling – Practice Problems

> Practice implementing proper exception handling patterns  
> Focus on real-world scenarios and interview questions

---

## Problem 1: Input Validation with Specific Exceptions

Create a `UserValidator` class that validates user input and throws appropriate exceptions.

```csharp
public class UserValidator
{
    // Implement these methods:

    // Throws ArgumentNullException if email is null
    // Throws ArgumentException if email is empty or invalid format
    public void ValidateEmail(string email) { }

    // Throws ArgumentOutOfRangeException if age < 0 or > 150
    public void ValidateAge(int age) { }

    // Throws ArgumentException if password doesn't meet requirements:
    // - At least 8 characters
    // - Contains uppercase, lowercase, and digit
    public void ValidatePassword(string password) { }
}
```

<details>
<summary>💡 Solution</summary>

```csharp
public class UserValidator
{
    public void ValidateEmail(string email)
    {
        ArgumentNullException.ThrowIfNull(email);

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (!email.Contains('@') || !email.Contains('.'))
            throw new ArgumentException("Invalid email format", nameof(email));
    }

    public void ValidateAge(int age)
    {
        if (age < 0 || age > 150)
            throw new ArgumentOutOfRangeException(nameof(age), age,
                "Age must be between 0 and 150");
    }

    public void ValidatePassword(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        var errors = new List<string>();

        if (password.Length < 8)
            errors.Add("at least 8 characters");
        if (!password.Any(char.IsUpper))
            errors.Add("an uppercase letter");
        if (!password.Any(char.IsLower))
            errors.Add("a lowercase letter");
        if (!password.Any(char.IsDigit))
            errors.Add("a digit");

        if (errors.Any())
            throw new ArgumentException(
                $"Password must contain {string.Join(", ", errors)}",
                nameof(password));
    }
}
```

</details>

---

## Problem 2: Custom Exception Hierarchy

Design an exception hierarchy for a banking system:

```
BankingException (base)
├── AccountException
│   ├── AccountNotFoundException
│   └── AccountClosedException
├── TransactionException
│   ├── InsufficientBalanceException
│   ├── TransactionLimitExceededException
│   └── DuplicateTransactionException
└── AuthenticationException
    └── InvalidCredentialsException
```

Each exception should have relevant properties (AccountId, TransactionId, etc.)

<details>
<summary>💡 Solution</summary>

```csharp
// Base exception
public class BankingException : Exception
{
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    public BankingException() { }
    public BankingException(string message) : base(message) { }
    public BankingException(string message, Exception inner) : base(message, inner) { }
}

// Account exceptions
public class AccountException : BankingException
{
    public string AccountId { get; }

    public AccountException(string message, string accountId) : base(message)
    {
        AccountId = accountId;
    }
}

public class AccountNotFoundException : AccountException
{
    public AccountNotFoundException(string accountId)
        : base($"Account not found: {accountId}", accountId) { }
}

public class AccountClosedException : AccountException
{
    public DateTime ClosedDate { get; }

    public AccountClosedException(string accountId, DateTime closedDate)
        : base($"Account {accountId} was closed on {closedDate:d}", accountId)
    {
        ClosedDate = closedDate;
    }
}

// Transaction exceptions
public class TransactionException : BankingException
{
    public string TransactionId { get; }
    public string AccountId { get; }

    public TransactionException(string message, string transactionId, string accountId)
        : base(message)
    {
        TransactionId = transactionId;
        AccountId = accountId;
    }
}

public class InsufficientBalanceException : TransactionException
{
    public decimal Required { get; }
    public decimal Available { get; }

    public InsufficientBalanceException(string transactionId, string accountId,
        decimal required, decimal available)
        : base($"Insufficient balance: need {required:C}, have {available:C}",
            transactionId, accountId)
    {
        Required = required;
        Available = available;
    }
}

public class TransactionLimitExceededException : TransactionException
{
    public decimal Amount { get; }
    public decimal Limit { get; }
    public string LimitType { get; }  // "Daily", "Single", etc.

    public TransactionLimitExceededException(string transactionId, string accountId,
        decimal amount, decimal limit, string limitType)
        : base($"{limitType} limit exceeded: {amount:C} > {limit:C}",
            transactionId, accountId)
    {
        Amount = amount;
        Limit = limit;
        LimitType = limitType;
    }
}

public class DuplicateTransactionException : TransactionException
{
    public string OriginalTransactionId { get; }

    public DuplicateTransactionException(string transactionId, string accountId,
        string originalId)
        : base($"Duplicate of transaction {originalId}", transactionId, accountId)
    {
        OriginalTransactionId = originalId;
    }
}

// Authentication exceptions
public class AuthenticationException : BankingException
{
    public string Username { get; }

    public AuthenticationException(string message, string username) : base(message)
    {
        Username = username;
    }
}

public class InvalidCredentialsException : AuthenticationException
{
    public int FailedAttempts { get; }

    public InvalidCredentialsException(string username, int failedAttempts)
        : base($"Invalid credentials for {username}. Attempt {failedAttempts}/3", username)
    {
        FailedAttempts = failedAttempts;
    }
}
```

</details>

---

## Problem 3: Retry with Exception Handling

Implement a retry mechanism that:

- Retries up to 3 times for transient exceptions
- Has exponential backoff (1s, 2s, 4s)
- Throws immediately for non-transient exceptions
- Returns the result on success

```csharp
public class RetryHelper
{
    public T ExecuteWithRetry<T>(Func<T> operation, Func<Exception, bool> isTransient)
    {
        // Implement retry logic
    }
}
```

<details>
<summary>💡 Solution</summary>

```csharp
public class RetryHelper
{
    private readonly int _maxRetries;
    private readonly TimeSpan _baseDelay;

    public RetryHelper(int maxRetries = 3, TimeSpan? baseDelay = null)
    {
        _maxRetries = maxRetries;
        _baseDelay = baseDelay ?? TimeSpan.FromSeconds(1);
    }

    public T ExecuteWithRetry<T>(Func<T> operation, Func<Exception, bool> isTransient)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(isTransient);

        int attempt = 0;
        var exceptions = new List<Exception>();

        while (true)
        {
            attempt++;
            try
            {
                return operation();
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);

                // Non-transient: fail immediately
                if (!isTransient(ex))
                {
                    throw;
                }

                // Max retries reached
                if (attempt >= _maxRetries)
                {
                    throw new AggregateException(
                        $"Operation failed after {_maxRetries} attempts",
                        exceptions);
                }

                // Wait with exponential backoff
                var delay = TimeSpan.FromTicks(_baseDelay.Ticks * (long)Math.Pow(2, attempt - 1));
                Console.WriteLine($"Attempt {attempt} failed, retrying in {delay.TotalSeconds}s...");
                Thread.Sleep(delay);
            }
        }
    }

    public async Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> operation,
        Func<Exception, bool> isTransient,
        CancellationToken cancellationToken = default)
    {
        int attempt = 0;
        var exceptions = new List<Exception>();

        while (true)
        {
            attempt++;
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);

                if (!isTransient(ex) || attempt >= _maxRetries)
                {
                    if (attempt >= _maxRetries)
                        throw new AggregateException(
                            $"Operation failed after {_maxRetries} attempts", exceptions);
                    throw;
                }

                var delay = TimeSpan.FromTicks(_baseDelay.Ticks * (long)Math.Pow(2, attempt - 1));
                await Task.Delay(delay, cancellationToken);
            }
        }
    }
}

// Usage:
var retry = new RetryHelper(maxRetries: 3);
var result = retry.ExecuteWithRetry(
    () => CallExternalService(),
    ex => ex is TimeoutException || ex is HttpRequestException);
```

</details>

---

## Problem 4: Exception Filter Challenge

Use exception filters to implement different handling based on exception properties:

```csharp
// Given this exception:
public class ApiException : Exception
{
    public int StatusCode { get; set; }
    public string ErrorCode { get; set; }
    public bool IsRetryable { get; set; }
}

// Write a method that:
// - Logs and retries for IsRetryable = true
// - Returns default value for StatusCode 404
// - Throws custom exception for StatusCode 401/403
// - Re-throws for other cases
```

<details>
<summary>💡 Solution</summary>

```csharp
public class ApiClient
{
    private readonly ILogger _logger;
    private readonly RetryHelper _retry;

    public T CallApi<T>(Func<T> apiCall, T defaultValue)
    {
        try
        {
            return _retry.ExecuteWithRetry(
                apiCall,
                ex => ex is ApiException api && api.IsRetryable);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            // Not found - return default
            _logger.LogWarning("Resource not found, returning default");
            return defaultValue;
        }
        catch (ApiException ex) when (ex.StatusCode == 401)
        {
            // Unauthorized
            throw new AuthenticationRequiredException(
                "Authentication required to access this resource", ex);
        }
        catch (ApiException ex) when (ex.StatusCode == 403)
        {
            // Forbidden
            throw new AccessDeniedException(
                "You don't have permission to access this resource", ex);
        }
        catch (ApiException ex) when (LogAndReturnFalse(ex))
        {
            // Log all other API exceptions (never actually catches)
            throw; // Unreachable
        }
        // Other ApiExceptions and non-ApiExceptions propagate up
    }

    private bool LogAndReturnFalse(Exception ex)
    {
        _logger.LogError(ex, "Unhandled API exception");
        return false;
    }
}

public class AuthenticationRequiredException : Exception
{
    public AuthenticationRequiredException(string message, Exception inner)
        : base(message, inner) { }
}

public class AccessDeniedException : Exception
{
    public AccessDeniedException(string message, Exception inner)
        : base(message, inner) { }
}
```

</details>

---

## Problem 5: Safe Resource Handling

Implement a `ResourceManager` that safely handles multiple disposable resources:

```csharp
public class ResourceManager
{
    // Open multiple files, process them, ensure ALL are closed even if one fails
    public void ProcessFiles(string[] filePaths, Action<StreamReader[]> processor)
    {
        // Implement safe handling
    }
}
```

<details>
<summary>💡 Solution</summary>

```csharp
public class ResourceManager
{
    public void ProcessFiles(string[] filePaths, Action<StreamReader[]> processor)
    {
        ArgumentNullException.ThrowIfNull(filePaths);
        ArgumentNullException.ThrowIfNull(processor);

        if (filePaths.Length == 0)
            throw new ArgumentException("At least one file required", nameof(filePaths));

        var readers = new List<StreamReader>();
        Exception? firstException = null;

        try
        {
            // Open all files
            foreach (var path in filePaths)
            {
                readers.Add(new StreamReader(path));
            }

            // Process
            processor(readers.ToArray());
        }
        catch (Exception ex)
        {
            firstException = ex;
            throw;
        }
        finally
        {
            // Close ALL files, collect any exceptions
            var closeExceptions = new List<Exception>();

            foreach (var reader in readers)
            {
                try
                {
                    reader.Dispose();
                }
                catch (Exception ex)
                {
                    closeExceptions.Add(ex);
                }
            }

            // If we had close exceptions and no processing exception,
            // throw aggregate
            if (closeExceptions.Any() && firstException == null)
            {
                throw new AggregateException(
                    "Errors occurred while closing files",
                    closeExceptions);
            }
        }
    }

    // Alternative using LINQ and IDisposable
    public async Task ProcessFilesAsync(
        string[] filePaths,
        Func<StreamReader[], Task> processor)
    {
        var readers = new List<StreamReader>();

        try
        {
            foreach (var path in filePaths)
            {
                readers.Add(new StreamReader(path));
            }

            await processor(readers.ToArray());
        }
        finally
        {
            // Dispose in reverse order
            foreach (var reader in readers.AsEnumerable().Reverse())
            {
                await reader.DisposeAsync();
            }
        }
    }
}
```

</details>

---

## Problem 6: Global Exception Handler

Implement an exception handler for a trading system that:

- Logs all exceptions
- Sends alerts for critical exceptions
- Converts exceptions to user-friendly messages
- Tracks exception statistics

```csharp
public interface IExceptionHandler
{
    ExceptionResult Handle(Exception ex);
}

public record ExceptionResult(
    bool IsHandled,
    string UserMessage,
    string ErrorCode,
    bool ShouldRetry);
```

<details>
<summary>💡 Solution</summary>

```csharp
public class TradingExceptionHandler : IExceptionHandler
{
    private readonly ILogger _logger;
    private readonly IAlertService _alertService;
    private readonly ConcurrentDictionary<string, int> _exceptionCounts = new();

    public TradingExceptionHandler(ILogger logger, IAlertService alertService)
    {
        _logger = logger;
        _alertService = alertService;
    }

    public ExceptionResult Handle(Exception ex)
    {
        // Track statistics
        var exType = ex.GetType().Name;
        _exceptionCounts.AddOrUpdate(exType, 1, (_, count) => count + 1);

        // Log all exceptions
        _logger.LogError(ex, "Exception occurred: {Type}", exType);

        // Handle specific types
        return ex switch
        {
            // Validation errors - user can fix
            ArgumentException argEx => new ExceptionResult(
                IsHandled: true,
                UserMessage: $"Invalid input: {argEx.Message}",
                ErrorCode: "VALIDATION_ERROR",
                ShouldRetry: false),

            // Insufficient funds - user can fix
            InsufficientFundsException fundsEx => new ExceptionResult(
                IsHandled: true,
                UserMessage: $"Insufficient funds. Available: {fundsEx.Available:C}",
                ErrorCode: "INSUFFICIENT_FUNDS",
                ShouldRetry: false),

            // Rate not found - temporary, can retry
            RateNotFoundException rateEx => new ExceptionResult(
                IsHandled: true,
                UserMessage: $"Rate temporarily unavailable for {rateEx.CurrencyPair}",
                ErrorCode: "RATE_UNAVAILABLE",
                ShouldRetry: true),

            // Timeout - can retry
            TimeoutException => HandleTransient(ex, "Request timed out"),

            // Network error - can retry
            HttpRequestException => HandleTransient(ex, "Network error"),

            // Critical - alert support
            OutOfMemoryException or StackOverflowException => HandleCritical(ex),

            // Unknown - log and generic message
            _ => new ExceptionResult(
                IsHandled: false,
                UserMessage: "An unexpected error occurred. Please try again.",
                ErrorCode: "UNKNOWN_ERROR",
                ShouldRetry: false)
        };
    }

    private ExceptionResult HandleTransient(Exception ex, string message)
    {
        return new ExceptionResult(
            IsHandled: true,
            UserMessage: $"{message}. Please try again.",
            ErrorCode: "TRANSIENT_ERROR",
            ShouldRetry: true);
    }

    private ExceptionResult HandleCritical(Exception ex)
    {
        // Send alert
        _alertService.SendCriticalAlert(
            $"CRITICAL: {ex.GetType().Name}",
            ex.ToString());

        return new ExceptionResult(
            IsHandled: false,
            UserMessage: "A critical error occurred. Support has been notified.",
            ErrorCode: "CRITICAL_ERROR",
            ShouldRetry: false);
    }

    public Dictionary<string, int> GetStatistics() =>
        _exceptionCounts.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
}
```

</details>

---

## Problem 7: Exception-Safe Iterator

Implement an iterator that continues even if some items fail:

```csharp
public class SafeEnumerator
{
    // Process all items, collect failures, return results
    public ProcessingResult<TResult> ProcessAll<TSource, TResult>(
        IEnumerable<TSource> items,
        Func<TSource, TResult> processor)
    {
        // Return successful results AND failures
    }
}

public record ProcessingResult<T>(
    List<T> Successes,
    List<(object Item, Exception Error)> Failures);
```

<details>
<summary>💡 Solution</summary>

```csharp
public class SafeEnumerator
{
    public ProcessingResult<TResult> ProcessAll<TSource, TResult>(
        IEnumerable<TSource> items,
        Func<TSource, TResult> processor)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(processor);

        var successes = new List<TResult>();
        var failures = new List<(object Item, Exception Error)>();

        foreach (var item in items)
        {
            try
            {
                var result = processor(item);
                successes.Add(result);
            }
            catch (Exception ex)
            {
                failures.Add((item!, ex));
            }
        }

        return new ProcessingResult<TResult>(successes, failures);
    }

    // Async version with cancellation
    public async Task<ProcessingResult<TResult>> ProcessAllAsync<TSource, TResult>(
        IEnumerable<TSource> items,
        Func<TSource, CancellationToken, Task<TResult>> processor,
        CancellationToken cancellationToken = default,
        int maxDegreeOfParallelism = 4)
    {
        var successes = new ConcurrentBag<TResult>();
        var failures = new ConcurrentBag<(object Item, Exception Error)>();

        var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);
        var tasks = items.Select(async item =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var result = await processor(item, cancellationToken);
                successes.Add(result);
            }
            catch (OperationCanceledException)
            {
                throw; // Don't catch cancellation
            }
            catch (Exception ex)
            {
                failures.Add((item!, ex));
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);

        return new ProcessingResult<TResult>(
            successes.ToList(),
            failures.ToList());
    }
}

// Usage:
var enumerator = new SafeEnumerator();
var trades = new[] { trade1, trade2, trade3 };

var result = enumerator.ProcessAll(trades, t => ExecuteTrade(t));

Console.WriteLine($"Successful: {result.Successes.Count}");
Console.WriteLine($"Failed: {result.Failures.Count}");

foreach (var (item, error) in result.Failures)
{
    Console.WriteLine($"  {item}: {error.Message}");
}
```

</details>

---

## Problem 8: Circuit Breaker Pattern

Implement a circuit breaker that:

- Opens after N consecutive failures
- Stays open for a timeout period
- Half-opens to test if service recovered
- Closes on success

```csharp
public enum CircuitState { Closed, Open, HalfOpen }

public class CircuitBreaker
{
    // Implement circuit breaker logic
}
```

<details>
<summary>💡 Solution</summary>

```csharp
public class CircuitBreaker
{
    private readonly int _failureThreshold;
    private readonly TimeSpan _openDuration;
    private readonly object _lock = new();

    private CircuitState _state = CircuitState.Closed;
    private int _failureCount;
    private DateTime _openedAt;
    private Exception? _lastException;

    public CircuitState State => _state;

    public CircuitBreaker(int failureThreshold = 5, TimeSpan? openDuration = null)
    {
        _failureThreshold = failureThreshold;
        _openDuration = openDuration ?? TimeSpan.FromSeconds(30);
    }

    public T Execute<T>(Func<T> operation)
    {
        EnsureNotOpen();

        try
        {
            var result = operation();
            OnSuccess();
            return result;
        }
        catch (Exception ex)
        {
            OnFailure(ex);
            throw;
        }
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        EnsureNotOpen();

        try
        {
            var result = await operation();
            OnSuccess();
            return result;
        }
        catch (Exception ex)
        {
            OnFailure(ex);
            throw;
        }
    }

    private void EnsureNotOpen()
    {
        lock (_lock)
        {
            if (_state == CircuitState.Open)
            {
                // Check if we should transition to half-open
                if (DateTime.UtcNow - _openedAt >= _openDuration)
                {
                    _state = CircuitState.HalfOpen;
                    Console.WriteLine("Circuit: Open → HalfOpen (testing...)");
                }
                else
                {
                    throw new CircuitBreakerOpenException(
                        "Circuit breaker is open",
                        _lastException!,
                        _openedAt + _openDuration - DateTime.UtcNow);
                }
            }
        }
    }

    private void OnSuccess()
    {
        lock (_lock)
        {
            if (_state == CircuitState.HalfOpen)
            {
                Console.WriteLine("Circuit: HalfOpen → Closed (service recovered)");
            }

            _failureCount = 0;
            _state = CircuitState.Closed;
        }
    }

    private void OnFailure(Exception ex)
    {
        lock (_lock)
        {
            _lastException = ex;
            _failureCount++;

            if (_state == CircuitState.HalfOpen)
            {
                // Failed during test - reopen
                _state = CircuitState.Open;
                _openedAt = DateTime.UtcNow;
                Console.WriteLine("Circuit: HalfOpen → Open (still failing)");
            }
            else if (_failureCount >= _failureThreshold)
            {
                _state = CircuitState.Open;
                _openedAt = DateTime.UtcNow;
                Console.WriteLine($"Circuit: Closed → Open (after {_failureCount} failures)");
            }
        }
    }
}

public class CircuitBreakerOpenException : Exception
{
    public TimeSpan RetryAfter { get; }

    public CircuitBreakerOpenException(string message, Exception lastException, TimeSpan retryAfter)
        : base(message, lastException)
    {
        RetryAfter = retryAfter;
    }
}

// Usage:
var circuitBreaker = new CircuitBreaker(failureThreshold: 3, openDuration: TimeSpan.FromSeconds(10));

for (int i = 0; i < 10; i++)
{
    try
    {
        var result = circuitBreaker.Execute(() => CallUnreliableService());
        Console.WriteLine($"Success: {result}");
    }
    catch (CircuitBreakerOpenException ex)
    {
        Console.WriteLine($"Circuit open, retry after {ex.RetryAfter.TotalSeconds}s");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed: {ex.Message}");
    }
}
```

</details>

---

## Interview Challenge Questions

### Challenge 1: Stack Trace Puzzle

What's wrong with this code?

```csharp
try
{
    DoSomething();
}
catch (Exception ex)
{
    Log(ex);
    throw ex;  // What's the problem?
}
```

**Answer:** `throw ex;` resets the stack trace. Use `throw;` to preserve it.

---

### Challenge 2: Finally Trap

What does this return?

```csharp
int GetNumber()
{
    try
    {
        return 1;
    }
    finally
    {
        return 2;  // Compile error in C#!
    }
}
```

**Answer:** This doesn't compile in C# - you cannot return from a finally block. (Java allows it but C# doesn't)

---

### Challenge 3: Exception Filter vs Catch-Rethrow

What's the difference?

```csharp
// Option A
catch (Exception ex) when (ShouldHandle(ex)) { Handle(ex); }

// Option B
catch (Exception ex) { if (!ShouldHandle(ex)) throw; Handle(ex); }
```

**Answer:** Option A (filter) doesn't unwind the stack if condition is false - better for debugging. Option B unwinds stack before checking condition.

---

### Challenge 4: Async Exception

Where does this exception go?

```csharp
async void FireAndForget()
{
    throw new Exception("Oops!");
}

FireAndForget();  // What happens?
```

**Answer:** The exception crashes the process! `async void` exceptions can't be caught. Use `async Task` instead.

---

_Complete these exercises to master C# exception handling!_ 🎯
