// C# Exceptions - Comprehensive Demo
// Run: dotnet run

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

#region Domain Classes and Custom Exceptions

// FX Trading Domain Classes
public record Trade(string TradeId, string CurrencyPair, decimal Amount, string Side);
public record TradeResult(string TradeId, string Status, decimal ExecutedRate);

// Custom Exception Hierarchy for FX Trading
public class TradingException : Exception
{
    public string? TradeId { get; }
    public int ErrorCode { get; }
    
    public TradingException() { }
    public TradingException(string message) : base(message) { }
    public TradingException(string message, Exception inner) : base(message, inner) { }
    public TradingException(string message, string tradeId, int errorCode) : base(message)
    {
        TradeId = tradeId;
        ErrorCode = errorCode;
    }
}

public class InsufficientFundsException : TradingException
{
    public decimal Required { get; }
    public decimal Available { get; }
    public string Currency { get; }

    public InsufficientFundsException(decimal required, decimal available, string currency)
        : base($"Insufficient funds: required {required:N2} {currency}, available {available:N2} {currency}")
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
        : base($"Trade amount {amount:N2} exceeds limit {limit:N2}")
    {
        TradeAmount = amount;
        Limit = limit;
    }
}

public class TradeValidationException : TradingException
{
    public List<string> ValidationErrors { get; }

    public TradeValidationException(List<string> errors)
        : base($"Trade validation failed: {string.Join(", ", errors)}")
    {
        ValidationErrors = errors;
    }
}

#endregion

class ExceptionsDemo
{
    private static readonly Dictionary<string, decimal> _rates = new()
    {
        ["EUR/USD"] = 1.0850m,
        ["GBP/USD"] = 1.2650m,
        ["USD/JPY"] = 149.50m
    };
    
    private static decimal _accountBalance = 100_000m;
    private const decimal MAX_TRADE_SIZE = 1_000_000m;

    static void Main(string[] args)
    {
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("                    C# EXCEPTIONS DEMO");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Demo1_BasicTryCatch();
        Demo2_MultipleCatchBlocks();
        Demo3_FinallyBlock();
        Demo4_ThrowVsThrowEx();
        Demo5_CustomExceptions();
        Demo6_ExceptionFilters();
        Demo7_NestedExceptions();
        Demo8_ExceptionProperties();
        Demo9_UsingStatement();
        Demo10_TryPattern();

        Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
        Console.WriteLine("                    DEMO COMPLETED");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
    }

    // ═══════════════════════════════════════════════════════════════
    // DEMO 1: Basic Try-Catch
    // ═══════════════════════════════════════════════════════════════
    static void Demo1_BasicTryCatch()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ DEMO 1: Basic Try-Catch                                     │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");

        // Example 1: Division by zero
        try
        {
            int divisor = 0;
            int result = 10 / divisor;
            Console.WriteLine($"Result: {result}");
        }
        catch (DivideByZeroException ex)
        {
            Console.WriteLine($"✓ Caught DivideByZeroException: {ex.Message}");
        }

        // Example 2: Null reference
        try
        {
            string? name = null;
            int length = name!.Length;  // Will throw
        }
        catch (NullReferenceException ex)
        {
            Console.WriteLine($"✓ Caught NullReferenceException: {ex.Message}");
        }

        // Example 3: Index out of range
        try
        {
            int[] numbers = { 1, 2, 3 };
            int value = numbers[10];  // Will throw
        }
        catch (IndexOutOfRangeException ex)
        {
            Console.WriteLine($"✓ Caught IndexOutOfRangeException: {ex.Message}");
        }

        // Example 4: Key not found
        try
        {
            var dict = new Dictionary<string, int> { ["a"] = 1 };
            int value = dict["nonexistent"];  // Will throw
        }
        catch (KeyNotFoundException ex)
        {
            Console.WriteLine($"✓ Caught KeyNotFoundException: {ex.Message}");
        }

        Console.WriteLine();
    }

    // ═══════════════════════════════════════════════════════════════
    // DEMO 2: Multiple Catch Blocks (Most Specific First!)
    // ═══════════════════════════════════════════════════════════════
    static void Demo2_MultipleCatchBlocks()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ DEMO 2: Multiple Catch Blocks (Most Specific First!)       │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");

        void ProcessInput(string? input)
        {
            try
            {
                // Guard clause
                if (input == null)
                    throw new ArgumentNullException(nameof(input));
                
                if (string.IsNullOrWhiteSpace(input))
                    throw new ArgumentException("Input cannot be empty", nameof(input));
                
                int value = int.Parse(input);
                
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(input), value, "Value must be non-negative");
                
                Console.WriteLine($"  Processed value: {value}");
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine($"  ✗ Null input: {ex.ParamName} was null");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine($"  ✗ Out of range: {ex.ActualValue} (must be >= 0)");
            }
            catch (ArgumentException ex)  // Parent of above - catches remaining
            {
                Console.WriteLine($"  ✗ Invalid argument: {ex.Message}");
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"  ✗ Format error: {ex.Message}");
            }
            catch (Exception ex)  // Catch-all (LAST!)
            {
                Console.WriteLine($"  ✗ Unexpected error: {ex.GetType().Name} - {ex.Message}");
            }
        }

        Console.WriteLine("Testing various inputs:");
        ProcessInput(null);          // ArgumentNullException
        ProcessInput("");            // ArgumentException
        ProcessInput("abc");         // FormatException
        ProcessInput("-5");          // ArgumentOutOfRangeException
        ProcessInput("42");          // Success!

        Console.WriteLine();
    }

    // ═══════════════════════════════════════════════════════════════
    // DEMO 3: Finally Block (Always Runs!)
    // ═══════════════════════════════════════════════════════════════
    static void Demo3_FinallyBlock()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ DEMO 3: Finally Block (Always Runs!)                       │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");

        string SimulateResourceOperation(bool shouldThrow)
        {
            string resource = "Resource acquired";
            Console.WriteLine($"  1. {resource}");
            
            try
            {
                Console.WriteLine("  2. Performing operation...");
                
                if (shouldThrow)
                {
                    throw new InvalidOperationException("Operation failed!");
                }
                
                Console.WriteLine("  3. Operation succeeded");
                return "Success";
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"  3. Caught: {ex.Message}");
                return "Failed";
            }
            finally
            {
                // This ALWAYS runs - even after return!
                Console.WriteLine("  4. Finally: Releasing resource (ALWAYS runs!)");
            }
            // Code here would be after the return - unreachable
        }

        Console.WriteLine("Scenario A - No exception:");
        string resultA = SimulateResourceOperation(shouldThrow: false);
        Console.WriteLine($"  Result: {resultA}\n");

        Console.WriteLine("Scenario B - With exception:");
        string resultB = SimulateResourceOperation(shouldThrow: true);
        Console.WriteLine($"  Result: {resultB}");

        Console.WriteLine();
    }

    // ═══════════════════════════════════════════════════════════════
    // DEMO 4: throw vs throw ex (Stack Trace Preservation)
    // ═══════════════════════════════════════════════════════════════
    static void Demo4_ThrowVsThrowEx()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ DEMO 4: throw vs throw ex (Stack Trace Preservation)       │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");

        void InnerMethod()
        {
            throw new InvalidOperationException("Error in InnerMethod!");
        }

        void MiddleMethod_Throw()
        {
            try
            {
                InnerMethod();
            }
            catch (Exception)
            {
                // ✓ CORRECT: Preserves original stack trace
                throw;
            }
        }

        void MiddleMethod_ThrowEx()
        {
            try
            {
                InnerMethod();
            }
            catch (Exception ex)
            {
                // ✗ WRONG: Resets stack trace to HERE
                throw ex;
            }
        }

        // Test with throw; (correct)
        Console.WriteLine("Using 'throw;' (CORRECT - preserves stack trace):");
        try
        {
            MiddleMethod_Throw();
        }
        catch (Exception ex)
        {
            // Count stack frames to show it includes InnerMethod
            int frameCount = ex.StackTrace?.Split('\n').Length ?? 0;
            Console.WriteLine($"  Stack trace has {frameCount} frames (includes InnerMethod)");
            Console.WriteLine($"  Contains 'InnerMethod': {ex.StackTrace?.Contains("InnerMethod") ?? false}");
        }

        // Test with throw ex; (wrong)
        Console.WriteLine("\nUsing 'throw ex;' (WRONG - loses original location):");
        try
        {
            MiddleMethod_ThrowEx();
        }
        catch (Exception ex)
        {
            int frameCount = ex.StackTrace?.Split('\n').Length ?? 0;
            Console.WriteLine($"  Stack trace has {frameCount} frames (InnerMethod may be hidden)");
            Console.WriteLine($"  Contains 'InnerMethod': {ex.StackTrace?.Contains("InnerMethod") ?? false}");
        }

        Console.WriteLine();
    }

    // ═══════════════════════════════════════════════════════════════
    // DEMO 5: Custom Exceptions (FX Trading Domain)
    // ═══════════════════════════════════════════════════════════════
    static void Demo5_CustomExceptions()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ DEMO 5: Custom Exceptions (FX Trading Domain)              │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");

        void ExecuteTrade(Trade trade)
        {
            // Validate trade
            var errors = new List<string>();
            if (string.IsNullOrEmpty(trade.TradeId))
                errors.Add("TradeId is required");
            if (trade.Amount <= 0)
                errors.Add("Amount must be positive");
            if (trade.Side != "BUY" && trade.Side != "SELL")
                errors.Add("Side must be BUY or SELL");
            
            if (errors.Count > 0)
                throw new TradeValidationException(errors);
            
            // Check rate exists
            if (!_rates.ContainsKey(trade.CurrencyPair))
                throw new RateNotFoundException(trade.CurrencyPair);
            
            // Check trade limit
            if (trade.Amount > MAX_TRADE_SIZE)
                throw new TradeLimitExceededException(trade.Amount, MAX_TRADE_SIZE);
            
            // Check funds (simplified - assume 1:1 for demo)
            if (trade.Amount > _accountBalance)
                throw new InsufficientFundsException(trade.Amount, _accountBalance, "USD");
            
            // Execute (simplified)
            decimal rate = _rates[trade.CurrencyPair];
            Console.WriteLine($"  ✓ Executed {trade.Side} {trade.Amount:N0} {trade.CurrencyPair} @ {rate}");
        }

        void TryTrade(Trade trade)
        {
            Console.WriteLine($"  Attempting: {trade.Side} {trade.Amount:N0} {trade.CurrencyPair}");
            try
            {
                ExecuteTrade(trade);
            }
            catch (TradeValidationException ex)
            {
                Console.WriteLine($"  ✗ Validation failed: {string.Join(", ", ex.ValidationErrors)}");
            }
            catch (RateNotFoundException ex)
            {
                Console.WriteLine($"  ✗ Unknown pair: {ex.CurrencyPair}");
            }
            catch (TradeLimitExceededException ex)
            {
                Console.WriteLine($"  ✗ Limit exceeded: {ex.TradeAmount:N0} > {ex.Limit:N0}");
            }
            catch (InsufficientFundsException ex)
            {
                Console.WriteLine($"  ✗ Need {ex.Required - ex.Available:N0} more {ex.Currency}");
            }
            catch (TradingException ex)
            {
                // Catch-all for trading exceptions
                Console.WriteLine($"  ✗ Trading error: {ex.Message}");
            }
        }

        Console.WriteLine("Testing various trade scenarios:\n");
        
        // Invalid trade
        TryTrade(new Trade("", "EUR/USD", -100, "INVALID"));
        Console.WriteLine();
        
        // Unknown currency pair
        TryTrade(new Trade("T001", "XXX/YYY", 10000, "BUY"));
        Console.WriteLine();
        
        // Exceeds limit
        TryTrade(new Trade("T002", "EUR/USD", 5_000_000, "BUY"));
        Console.WriteLine();
        
        // Insufficient funds
        TryTrade(new Trade("T003", "EUR/USD", 500_000, "BUY"));
        Console.WriteLine();
        
        // Valid trade
        TryTrade(new Trade("T004", "EUR/USD", 50_000, "BUY"));

        Console.WriteLine();
    }

    // ═══════════════════════════════════════════════════════════════
    // DEMO 6: Exception Filters (when clause)
    // ═══════════════════════════════════════════════════════════════
    static void Demo6_ExceptionFilters()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ DEMO 6: Exception Filters (when clause)                    │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");

        void ProcessWithErrorCode(int errorCode)
        {
            throw new TradingException($"Trade failed with error code {errorCode}", "T001", errorCode);
        }

        void HandleTrade(int errorCode)
        {
            Console.WriteLine($"  Processing with error code: {errorCode}");
            try
            {
                ProcessWithErrorCode(errorCode);
            }
            catch (TradingException ex) when (ex.ErrorCode == 100)
            {
                Console.WriteLine("  → Handled: Temporary error (100) - will retry");
            }
            catch (TradingException ex) when (ex.ErrorCode == 200)
            {
                Console.WriteLine("  → Handled: Validation error (200) - fix and resubmit");
            }
            catch (TradingException ex) when (ex.ErrorCode >= 500)
            {
                Console.WriteLine($"  → Handled: Critical error ({ex.ErrorCode}) - alerting support");
            }
            catch (TradingException ex)
            {
                Console.WriteLine($"  → Handled: Unknown error code ({ex.ErrorCode})");
            }
        }

        HandleTrade(100);  // Temporary
        HandleTrade(200);  // Validation
        HandleTrade(500);  // Critical
        HandleTrade(300);  // Unknown

        // Logging without handling (filter returns false)
        Console.WriteLine("\n  Using filter for logging without catching:");
        try
        {
            throw new InvalidOperationException("Test exception");
        }
        catch (Exception ex) when (LogAndReturnFalse(ex))
        {
            // Never reached - filter returns false
            Console.WriteLine("  This never prints");
        }
        catch (Exception)
        {
            Console.WriteLine("  → Caught by second handler (first just logged)");
        }

        Console.WriteLine();
    }

    static bool LogAndReturnFalse(Exception ex)
    {
        Console.WriteLine($"  [LOG] Exception occurred: {ex.Message}");
        return false;  // Don't actually catch
    }

    // ═══════════════════════════════════════════════════════════════
    // DEMO 7: Nested Exceptions (InnerException)
    // ═══════════════════════════════════════════════════════════════
    static void Demo7_NestedExceptions()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ DEMO 7: Nested Exceptions (InnerException)                 │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");

        void DatabaseOperation()
        {
            throw new InvalidOperationException("Database connection failed");
        }

        void ServiceOperation()
        {
            try
            {
                DatabaseOperation();
            }
            catch (Exception ex)
            {
                // Wrap with context, preserve original
                throw new TradingException(
                    "Failed to load exchange rates from database",
                    "N/A",
                    500)
                {
                    // Data dictionary for additional context
                };
            }
        }

        void ApplicationOperation()
        {
            try
            {
                ServiceOperation();
            }
            catch (Exception ex)
            {
                // Wrap again at application layer
                throw new ApplicationException("Rate service unavailable", ex);
            }
        }

        try
        {
            ApplicationOperation();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception chain (unwinding):");
            Console.WriteLine($"  Level 1: {ex.GetType().Name} - {ex.Message}");
            
            var inner = ex.InnerException;
            int level = 2;
            while (inner != null)
            {
                Console.WriteLine($"  Level {level}: {inner.GetType().Name} - {inner.Message}");
                inner = inner.InnerException;
                level++;
            }
        }

        Console.WriteLine();
    }

    // ═══════════════════════════════════════════════════════════════
    // DEMO 8: Exception Properties
    // ═══════════════════════════════════════════════════════════════
    static void Demo8_ExceptionProperties()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ DEMO 8: Exception Properties                               │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");

        try
        {
            var ex = new TradingException("Trade execution failed", "TRADE-12345", 500);
            ex.HelpLink = "https://docs.trading.com/errors/500";
            ex.Data["Timestamp"] = DateTime.UtcNow;
            ex.Data["Server"] = "PROD-1";
            ex.Data["UserId"] = "trader42";
            throw ex;
        }
        catch (TradingException ex)
        {
            Console.WriteLine("Exception Properties:");
            Console.WriteLine($"  Message:      {ex.Message}");
            Console.WriteLine($"  TradeId:      {ex.TradeId}");
            Console.WriteLine($"  ErrorCode:    {ex.ErrorCode}");
            Console.WriteLine($"  HelpLink:     {ex.HelpLink}");
            Console.WriteLine($"  Source:       {ex.Source}");
            Console.WriteLine($"  TargetSite:   {ex.TargetSite}");
            
            Console.WriteLine("\n  Data Dictionary:");
            foreach (var key in ex.Data.Keys)
            {
                Console.WriteLine($"    {key}: {ex.Data[key]}");
            }
            
            Console.WriteLine($"\n  StackTrace (first 2 lines):");
            var lines = ex.StackTrace?.Split('\n') ?? Array.Empty<string>();
            foreach (var line in lines.Take(2))
            {
                Console.WriteLine($"   {line.Trim()}");
            }
        }

        Console.WriteLine();
    }

    // ═══════════════════════════════════════════════════════════════
    // DEMO 9: Using Statement (Auto-Dispose)
    // ═══════════════════════════════════════════════════════════════
    static void Demo9_UsingStatement()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ DEMO 9: Using Statement (Auto-Dispose)                     │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");

        // Simulated disposable resource
        Console.WriteLine("Classic using block:");
        using (var resource = new DisposableResource("Resource1"))
        {
            Console.WriteLine("  Inside using block");
            resource.DoWork();
        }  // Dispose called here
        Console.WriteLine("  After using block\n");

        Console.WriteLine("C# 8 using declaration:");
        {
            using var resource2 = new DisposableResource("Resource2");
            Console.WriteLine("  Inside scope");
            resource2.DoWork();
        }  // Dispose called when scope ends
        Console.WriteLine("  After scope\n");

        Console.WriteLine("Using with exception:");
        try
        {
            using var resource3 = new DisposableResource("Resource3");
            Console.WriteLine("  About to throw...");
            throw new InvalidOperationException("Something went wrong!");
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine("  Exception caught - but Dispose was still called!");
        }

        Console.WriteLine();
    }

    // ═══════════════════════════════════════════════════════════════
    // DEMO 10: Try Pattern (Like TryParse)
    // ═══════════════════════════════════════════════════════════════
    static void Demo10_TryPattern()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ DEMO 10: Try Pattern (Like TryParse)                       │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");

        // Standard parsing - throws
        Console.WriteLine("Standard int.Parse (throws on invalid):");
        try
        {
            int value = int.Parse("abc");
        }
        catch (FormatException)
        {
            Console.WriteLine("  ✗ FormatException thrown\n");
        }

        // TryParse - no exception
        Console.WriteLine("int.TryParse (no exception):");
        if (int.TryParse("abc", out int result1))
        {
            Console.WriteLine($"  ✓ Parsed: {result1}");
        }
        else
        {
            Console.WriteLine("  ✗ Parse failed - no exception!\n");
        }

        // Custom Try pattern for trading
        bool TryGetRate(string currencyPair, out decimal rate)
        {
            return _rates.TryGetValue(currencyPair, out rate);
        }

        bool TryExecuteTrade(Trade trade, out TradeResult? result)
        {
            result = null;
            
            if (!_rates.TryGetValue(trade.CurrencyPair, out decimal rate))
                return false;
            
            if (trade.Amount > _accountBalance)
                return false;
            
            if (trade.Amount > MAX_TRADE_SIZE)
                return false;
            
            // Success
            result = new TradeResult(trade.TradeId, "EXECUTED", rate);
            return true;
        }

        Console.WriteLine("Custom TryGetRate:");
        if (TryGetRate("EUR/USD", out decimal eurRate))
        {
            Console.WriteLine($"  ✓ EUR/USD rate: {eurRate}");
        }
        
        if (!TryGetRate("XXX/YYY", out _))
        {
            Console.WriteLine("  ✗ XXX/YYY not found - no exception!\n");
        }

        Console.WriteLine("Custom TryExecuteTrade:");
        var goodTrade = new Trade("T100", "EUR/USD", 10000, "BUY");
        if (TryExecuteTrade(goodTrade, out var tradeResult))
        {
            Console.WriteLine($"  ✓ Trade executed: {tradeResult}");
        }
        
        var badTrade = new Trade("T101", "XXX/YYY", 10000, "BUY");
        if (!TryExecuteTrade(badTrade, out _))
        {
            Console.WriteLine("  ✗ Trade failed - no exception!");
        }

        Console.WriteLine();
    }
}

// Helper class for using statement demo
class DisposableResource : IDisposable
{
    private readonly string _name;
    private bool _disposed = false;

    public DisposableResource(string name)
    {
        _name = name;
        Console.WriteLine($"  [{_name}] Created");
    }

    public void DoWork()
    {
        if (_disposed)
            throw new ObjectDisposedException(_name);
        Console.WriteLine($"  [{_name}] Working...");
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Console.WriteLine($"  [{_name}] Disposed");
            _disposed = true;
        }
    }
}
