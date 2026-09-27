# Delegates in C# – From Basics to Advanced (Interview-Level Guide)

> **Target Audience:** 3+ years .NET backend developer preparing for product-based company interviews (NVIDIA, Microsoft, Google-level)

---

## Table of Contents

1. [Conceptual Foundation](#1-conceptual-foundation)
2. [Internal Working - Deep Dive](#2-internal-working---deep-dive)
3. [Basic Syntax with Examples](#3-basic-syntax-with-examples)
4. [Multicast Delegates](#4-multicast-delegates)
5. [Built-in Delegates](#5-built-in-delegates)
6. [Anonymous Methods and Lambda Expressions](#6-anonymous-methods-and-lambda-expressions)
7. [Delegates vs Events](#7-delegates-vs-events)
8. [Delegates vs Interfaces](#8-delegates-vs-interfaces)
9. [Real-World Scenarios](#9-real-world-scenarios)
10. [Advanced Topics](#10-advanced-topics)
11. [Common Interview Questions](#11-common-interview-questions)
12. [Comparison Tables](#12-comparison-tables)
13. [Code Examples with Explanation](#13-code-examples-with-explanation)

---

## 1. Conceptual Foundation

### 1.1 What is a Delegate?

A **delegate** is a type that represents references to methods with a particular parameter list and return type. It is a **type-safe, object-oriented function pointer**.

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        DELEGATE CONCEPT                                 │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   Traditional Function Pointer (C/C++):                                 │
│   • Raw memory address                                                  │
│   • No type safety                                                      │
│   • Can point to any function                                           │
│   • Runtime crashes if signature mismatch                               │
│                                                                         │
│   C# Delegate:                                                          │
│   • Type-safe wrapper                                                   │
│   • Compile-time signature checking                                     │
│   • Object-oriented (can reference instance methods)                    │
│   • Supports multicast (multiple methods)                               │
│   • Garbage collected                                                   │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 1.2 Why Delegates Were Introduced?

1. **Callback Mechanism**: Pass methods as parameters to other methods
2. **Event-Driven Programming**: Foundation for events in .NET
3. **Decoupling**: Separate "what to do" from "when to do it"
4. **Functional Programming**: Enable higher-order functions in C#
5. **Extensibility**: Allow framework code to call user-defined code

```csharp
// Without delegates - tightly coupled
class Button
{
    public void Click()
    {
        // Hardcoded action - can't change without modifying class
        Console.WriteLine("Button clicked!");
    }
}

// With delegates - loosely coupled
class Button
{
    public Action OnClick;

    public void Click()
    {
        OnClick?.Invoke(); // Caller decides what happens
    }
}
```

### 1.3 How Delegates Differ from Methods?

| Aspect               | Method                    | Delegate                        |
| -------------------- | ------------------------- | ------------------------------- |
| **Nature**           | Code block that executes  | Object that references a method |
| **Storage**          | Part of a type            | Can be stored in variables      |
| **Passing**          | Cannot be passed directly | Can be passed as parameter      |
| **Multiple targets** | Single implementation     | Can reference multiple methods  |
| **Runtime binding**  | Compile-time bound        | Can change at runtime           |

### 1.4 Delegate as Type-Safe Function Pointer

```csharp
// Delegate declaration defines the "contract"
delegate int Calculator(int x, int y);

// Only methods matching this signature can be assigned
int Add(int a, int b) => a + b;
int Multiply(int a, int b) => a * b;
string Concat(string a, string b) => a + b; // INCOMPATIBLE!

Calculator calc = Add;       // ✅ OK - signature matches
calc = Multiply;             // ✅ OK - signature matches
// calc = Concat;            // ❌ COMPILE ERROR - signature mismatch
```

### 1.5 Historical Context (Before Lambdas)

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    DELEGATE EVOLUTION IN C#                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  C# 1.0 (2002): Named Methods Only                                      │
│  ─────────────────────────────────                                      │
│  delegate void MyDelegate(string s);                                    │
│  void PrintMessage(string msg) { Console.WriteLine(msg); }              │
│  MyDelegate d = new MyDelegate(PrintMessage);                           │
│                                                                         │
│  C# 2.0 (2005): Anonymous Methods                                       │
│  ─────────────────────────────────                                      │
│  MyDelegate d = delegate(string msg) { Console.WriteLine(msg); };       │
│                                                                         │
│  C# 3.0 (2007): Lambda Expressions                                      │
│  ─────────────────────────────────                                      │
│  MyDelegate d = msg => Console.WriteLine(msg);                          │
│                                                                         │
│  C# 3.0+: Built-in Delegates (Action, Func, Predicate)                  │
│  ─────────────────────────────────────────────────────                  │
│  Action<string> d = msg => Console.WriteLine(msg);                      │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Internal Working - Deep Dive

### 2.1 How Delegates are Implemented in CLR

Every delegate in C# inherits from `System.MulticastDelegate`, which inherits from `System.Delegate`.

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    DELEGATE CLASS HIERARCHY                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│                      System.Object                                      │
│                           │                                             │
│                           ▼                                             │
│                    System.Delegate                                      │
│                    ┌─────────────┐                                      │
│                    │ _target     │ → Object instance (or null)          │
│                    │ _methodPtr  │ → Pointer to method                  │
│                    └─────────────┘                                      │
│                           │                                             │
│                           ▼                                             │
│               System.MulticastDelegate                                  │
│               ┌───────────────────┐                                     │
│               │ _invocationList   │ → Array of delegates                │
│               │ _invocationCount  │ → Number of delegates               │
│               └───────────────────┘                                     │
│                           │                                             │
│                           ▼                                             │
│                Your Custom Delegate                                     │
│               (e.g., MyDelegate)                                        │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 2.2 Delegate Object Internal Fields

```csharp
// When you write:
delegate int MathOp(int x, int y);

// Compiler generates a class like:
public sealed class MathOp : System.MulticastDelegate
{
    // Constructor
    public MathOp(object target, IntPtr methodPtr);

    // Invoke method (synchronous call)
    public virtual int Invoke(int x, int y);

    // BeginInvoke/EndInvoke (async pattern - legacy)
    public virtual IAsyncResult BeginInvoke(int x, int y,
        AsyncCallback callback, object state);
    public virtual int EndInvoke(IAsyncResult result);
}
```

### 2.3 Memory Layout of a Delegate

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    SINGLE DELEGATE MEMORY LAYOUT                        │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  STACK                              HEAP                                │
│  ┌────────────┐                     ┌─────────────────────────────┐    │
│  │ myDelegate │────────────────────>│ Delegate Object             │    │
│  └────────────┘                     ├─────────────────────────────┤    │
│                                     │ _target: 0x2000 ─────────┐  │    │
│                                     │ _methodPtr: 0x3000       │  │    │
│                                     │ _invocationList: null    │  │    │
│                                     │ _invocationCount: 1      │  │    │
│                                     └──────────────────────────┼──┘    │
│                                                                │       │
│                                     ┌──────────────────────────▼──┐    │
│                                     │ Target Object Instance      │    │
│                                     │ (for instance methods)      │    │
│                                     └─────────────────────────────┘    │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                 MULTICAST DELEGATE MEMORY LAYOUT                        │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  STACK                              HEAP                                │
│  ┌────────────┐                     ┌─────────────────────────────┐    │
│  │ combined   │────────────────────>│ Delegate Object             │    │
│  └────────────┘                     ├─────────────────────────────┤    │
│                                     │ _target: null               │    │
│                                     │ _methodPtr: (internal)      │    │
│                                     │ _invocationList: ──────┐    │    │
│                                     │ _invocationCount: 3    │    │    │
│                                     └────────────────────────┼────┘    │
│                                                              │         │
│                                     ┌────────────────────────▼────┐    │
│                                     │ Delegate[] (Invocation List)│    │
│                                     ├─────────────────────────────┤    │
│                                     │ [0] → Delegate A            │    │
│                                     │ [1] → Delegate B            │    │
│                                     │ [2] → Delegate C            │    │
│                                     └─────────────────────────────┘    │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 2.4 How Delegate Invocation Works Internally

```csharp
// When you call:
int result = myDelegate(5, 3);

// CLR internally does:
// 1. Check if delegate is null (throws NullReferenceException if so)
// 2. Get the invocation list
// 3. For each delegate in the list:
//    a. Load target object (if instance method)
//    b. Call the method pointer with arguments
//    c. Store return value (only last one kept)
// 4. Return the final result
```

**Pseudocode of Internal Invocation:**

```csharp
// Simplified internal logic
public int Invoke(int x, int y)
{
    if (_invocationList == null)
    {
        // Single delegate
        return _methodPtr.Invoke(_target, x, y);
    }
    else
    {
        // Multicast delegate
        int result = default;
        foreach (var del in _invocationList)
        {
            result = del.Invoke(x, y); // Only last result kept!
        }
        return result;
    }
}
```

### 2.5 Memory Allocation Behavior

```csharp
// SCENARIO 1: Static method - no target allocation
static int Add(int x, int y) => x + y;
Func<int, int, int> d1 = Add;
// Allocates: 1 delegate object on heap

// SCENARIO 2: Instance method - target reference stored
class Calculator
{
    public int Add(int x, int y) => x + y;
}
var calc = new Calculator();
Func<int, int, int> d2 = calc.Add;
// Allocates: 1 delegate object (holds reference to calc)

// SCENARIO 3: Lambda without capture - may be cached
Func<int, int, int> d3 = (x, y) => x + y;
// Allocates: 1 delegate object (compiler may cache this)

// SCENARIO 4: Lambda WITH capture - closure allocation!
int factor = 10;
Func<int, int> d4 = x => x * factor;
// Allocates:
//   1. Closure object (holds 'factor')
//   2. Delegate object (points to closure method)
```

### 2.6 Heap Allocation Implications

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    GC PRESSURE FROM DELEGATES                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  LOW GC PRESSURE:                                                       │
│  • Static method delegates (can be cached)                              │
│  • Reused delegate instances                                            │
│  • Non-capturing lambdas (compiler may optimize)                        │
│                                                                         │
│  HIGH GC PRESSURE:                                                      │
│  • Capturing lambdas created in loops                                   │
│  • Combining delegates frequently (creates new objects)                 │
│  • Event handlers created per request                                   │
│                                                                         │
│  Example - BAD (creates delegate every iteration):                      │
│  for (int i = 0; i < 1000000; i++)                                      │
│  {                                                                      │
│      int local = i;                                                     │
│      Process(x => x + local);  // 1 million closures + delegates!      │
│  }                                                                      │
│                                                                         │
│  Example - GOOD (reuse delegate):                                       │
│  Func<int, int> addOne = x => x + 1;                                    │
│  for (int i = 0; i < 1000000; i++)                                      │
│  {                                                                      │
│      Process(addOne);  // Same delegate reused                          │
│  }                                                                      │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 3. Basic Syntax with Examples

### 3.1 Declaring a Delegate

```csharp
// Syntax: delegate <return-type> <delegate-name>(<parameters>);

// No parameters, no return
delegate void SimpleDelegate();

// With parameters
delegate void MessageDelegate(string message);

// With return type
delegate int MathOperation(int a, int b);

// Generic delegate
delegate T Transformer<T>(T input);
```

### 3.2 Instantiating a Delegate

```csharp
// Method 1: Using new keyword (C# 1.0 style)
MathOperation op1 = new MathOperation(Add);

// Method 2: Direct assignment (C# 2.0+)
MathOperation op2 = Add;

// Method 3: Anonymous method (C# 2.0)
MathOperation op3 = delegate(int x, int y) { return x + y; };

// Method 4: Lambda expression (C# 3.0+)
MathOperation op4 = (x, y) => x + y;

// Method 5: Lambda with block body
MathOperation op5 = (x, y) =>
{
    Console.WriteLine($"Adding {x} and {y}");
    return x + y;
};

// Target method
static int Add(int a, int b) => a + b;
```

### 3.3 Invoking a Delegate

```csharp
MathOperation op = (x, y) => x + y;

// Method 1: Direct invocation (throws if null)
int result1 = op(5, 3);

// Method 2: Invoke method (same as direct)
int result2 = op.Invoke(5, 3);

// Method 3: Null-safe invocation (C# 6.0+)
int? result3 = op?.Invoke(5, 3);

// Method 4: Check for null explicitly
if (op != null)
{
    int result4 = op(5, 3);
}
```

### 3.4 Passing Delegate as Parameter

```csharp
// Higher-order function that accepts a delegate
void ProcessNumbers(int[] numbers, Func<int, int> transform)
{
    for (int i = 0; i < numbers.Length; i++)
    {
        numbers[i] = transform(numbers[i]);
    }
}

// Usage
int[] data = { 1, 2, 3, 4, 5 };

// Pass named method
ProcessNumbers(data, Square);

// Pass lambda
ProcessNumbers(data, x => x * 2);

// Pass anonymous method
ProcessNumbers(data, delegate(int x) { return x + 10; });

static int Square(int x) => x * x;
```

### 3.5 Returning Delegate from Method

```csharp
// Factory method that returns delegates
Func<int, int, int> GetOperation(char op)
{
    return op switch
    {
        '+' => (a, b) => a + b,
        '-' => (a, b) => a - b,
        '*' => (a, b) => a * b,
        '/' => (a, b) => b != 0 ? a / b : throw new DivideByZeroException(),
        _ => throw new ArgumentException($"Unknown operator: {op}")
    };
}

// Usage
var add = GetOperation('+');
var multiply = GetOperation('*');

Console.WriteLine(add(5, 3));      // 8
Console.WriteLine(multiply(5, 3)); // 15
```

---

## 4. Multicast Delegates

### 4.1 Understanding Multicast

All delegates in C# are multicast (derived from `MulticastDelegate`). A multicast delegate maintains an **invocation list** - an ordered collection of delegates.

```csharp
// Every delegate can hold multiple method references
Action<string> log = null;

// Add methods to invocation list
log += msg => Console.WriteLine($"[Console] {msg}");
log += msg => File.AppendAllText("log.txt", msg + "\n");
log += msg => Debug.WriteLine($"[Debug] {msg}");

// Invoking calls ALL methods in order
log("Application started");
```

### 4.2 Combining Delegates

```csharp
Action action1 = () => Console.WriteLine("First");
Action action2 = () => Console.WriteLine("Second");
Action action3 = () => Console.WriteLine("Third");

// Method 1: Using + operator
Action combined = action1 + action2 + action3;

// Method 2: Using += operator
Action combined2 = action1;
combined2 += action2;
combined2 += action3;

// Method 3: Using Delegate.Combine
Action combined3 = (Action)Delegate.Combine(action1, action2, action3);

// All produce the same result
combined();
// Output:
// First
// Second
// Third
```

### 4.3 Removing Delegates

```csharp
Action a = () => Console.WriteLine("A");
Action b = () => Console.WriteLine("B");
Action c = () => Console.WriteLine("C");

Action all = a + b + c;
all(); // A, B, C

// Remove using - operator
all -= b;
all(); // A, C

// Remove using Delegate.Remove
all = (Action)Delegate.Remove(all, a);
all(); // C

// Remove all (set to null or remove last)
all -= c;
all?.Invoke(); // Nothing (null-safe)
```

### 4.4 What Happens if One Delegate Throws Exception?

```csharp
Action action1 = () => Console.WriteLine("First - OK");
Action action2 = () => throw new Exception("Second - FAILED!");
Action action3 = () => Console.WriteLine("Third - Never reached");

Action combined = action1 + action2 + action3;

try
{
    combined(); // Exception stops execution!
}
catch (Exception ex)
{
    Console.WriteLine($"Caught: {ex.Message}");
}
// Output:
// First - OK
// Caught: Second - FAILED!
// (Third never executes!)
```

**Solution: Manual Iteration with GetInvocationList()**

```csharp
Action combined = action1 + action2 + action3;

foreach (Action handler in combined.GetInvocationList())
{
    try
    {
        handler();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Handler failed: {ex.Message}");
        // Continue to next handler
    }
}
// Output:
// First - OK
// Handler failed: Second - FAILED!
// Third - Never reached (but DOES execute now!)
```

### 4.5 Return Values in Multicast Delegates

**IMPORTANT**: Only the **last** delegate's return value is returned!

```csharp
Func<int> getNumber = () => { Console.WriteLine("Returning 1"); return 1; };
getNumber += () => { Console.WriteLine("Returning 2"); return 2; };
getNumber += () => { Console.WriteLine("Returning 3"); return 3; };

int result = getNumber();
// Output:
// Returning 1
// Returning 2
// Returning 3
// result = 3 (only last value!)

// To get all return values:
var results = getNumber.GetInvocationList()
    .Cast<Func<int>>()
    .Select(f => f())
    .ToList();
// results = [1, 2, 3]
```

### 4.6 Execution Order

Methods are invoked in the **order they were added**.

```csharp
Action sequence = null;
sequence += () => Console.Write("1");
sequence += () => Console.Write("2");
sequence += () => Console.Write("3");

sequence(); // Output: 123

// Insert at beginning? Not directly possible.
// Must rebuild the delegate:
Action first = () => Console.Write("0");
sequence = first + sequence;
sequence(); // Output: 0123
```

---

## 5. Built-in Delegates

### 5.1 Action<T> - For Methods Returning Void

```csharp
// Action - no parameters
Action greet = () => Console.WriteLine("Hello!");

// Action<T> - one parameter
Action<string> print = msg => Console.WriteLine(msg);

// Action<T1, T2> - two parameters
Action<string, int> repeat = (msg, times) =>
{
    for (int i = 0; i < times; i++)
        Console.WriteLine(msg);
};

// Up to Action<T1, T2, ..., T16>
Action<int, int, int, int> fourParams = (a, b, c, d) =>
    Console.WriteLine(a + b + c + d);
```

### 5.2 Func<T> - For Methods With Return Value

```csharp
// Func<TResult> - no parameters, returns TResult
Func<int> getRandomNumber = () => new Random().Next();

// Func<T, TResult> - one parameter
Func<int, int> square = x => x * x;

// Func<T1, T2, TResult> - two parameters
Func<int, int, int> add = (x, y) => x + y;

// Func<T1, T2, ..., T16, TResult> - up to 16 parameters
Func<int, int, int, int, int> sum4 = (a, b, c, d) => a + b + c + d;

// Last type parameter is ALWAYS the return type
Func<string, int, bool> validateLength = (str, maxLen) => str.Length <= maxLen;
```

### 5.3 Predicate<T> - For Boolean Conditions

```csharp
// Always takes one parameter, always returns bool
Predicate<int> isPositive = x => x > 0;
Predicate<string> isNotEmpty = s => !string.IsNullOrEmpty(s);
Predicate<Person> isAdult = p => p.Age >= 18;

// Common usage in List<T> methods
List<int> numbers = new List<int> { -2, -1, 0, 1, 2, 3 };

// Find first match
int firstPositive = numbers.Find(isPositive); // 1

// Find all matches
List<int> positives = numbers.FindAll(isPositive); // [1, 2, 3]

// Check if any match
bool hasPositive = numbers.Exists(isPositive); // true

// Check if all match
bool allPositive = numbers.TrueForAll(isPositive); // false

// Remove matches
numbers.RemoveAll(x => x < 0); // numbers = [0, 1, 2, 3]
```

### 5.4 Differences and Use Cases

```
┌─────────────────────────────────────────────────────────────────────────┐
│                 BUILT-IN DELEGATES COMPARISON                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Action<T>                                                              │
│  ┌─────────────────────────────────────────────────────────────┐       │
│  │ Signature: void Method(T1 arg1, T2 arg2, ...)               │       │
│  │ Return: void (no return value)                              │       │
│  │ Parameters: 0 to 16                                         │       │
│  │ Use cases: Logging, notifications, side effects, callbacks  │       │
│  └─────────────────────────────────────────────────────────────┘       │
│                                                                         │
│  Func<T>                                                                │
│  ┌─────────────────────────────────────────────────────────────┐       │
│  │ Signature: TResult Method(T1 arg1, T2 arg2, ...)            │       │
│  │ Return: TResult (last type parameter)                       │       │
│  │ Parameters: 0 to 16                                         │       │
│  │ Use cases: LINQ, transformations, calculations, factories   │       │
│  └─────────────────────────────────────────────────────────────┘       │
│                                                                         │
│  Predicate<T>                                                           │
│  ┌─────────────────────────────────────────────────────────────┐       │
│  │ Signature: bool Method(T arg)                               │       │
│  │ Return: bool (always)                                       │       │
│  │ Parameters: exactly 1                                       │       │
│  │ Use cases: Filtering, validation, conditions, List methods  │       │
│  └─────────────────────────────────────────────────────────────┘       │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 5.5 When to Use Custom Delegate vs Func/Action

**Use Built-in (Func/Action):**

```csharp
// ✅ Simple callbacks
void ProcessData(Action<string> onComplete)

// ✅ LINQ-style operations
IEnumerable<T> Where<T>(Func<T, bool> predicate)

// ✅ Ad-hoc, one-off scenarios
button.Click += () => Console.WriteLine("Clicked");
```

**Use Custom Delegate:**

```csharp
// ✅ Self-documenting API
public delegate void OrderProcessedHandler(Order order, DateTime processedAt);

// ✅ Events (convention uses EventHandler pattern)
public delegate void DataReceivedEventHandler(object sender, DataEventArgs e);

// ✅ Complex signatures used in multiple places
public delegate ValidationResult Validator<T>(T value, ValidationContext context);

// ✅ Covariance/contravariance needs
public delegate TOutput Converter<in TInput, out TOutput>(TInput input);
```

---

## 6. Anonymous Methods and Lambda Expressions

### 6.1 Evolution of Inline Method Definition

```csharp
delegate int Operation(int x, int y);

// C# 1.0: Named method only
int Add(int a, int b) => a + b;
Operation op1 = new Operation(Add);

// C# 2.0: Anonymous method
Operation op2 = delegate(int a, int b)
{
    return a + b;
};

// C# 3.0: Lambda expression
Operation op3 = (a, b) => a + b;

// C# 3.0: Lambda with statement body
Operation op4 = (a, b) =>
{
    Console.WriteLine($"Adding {a} and {b}");
    return a + b;
};
```

### 6.2 How Lambda Connects to Delegate

The compiler transforms lambdas into delegate instances:

```csharp
// What you write:
Func<int, int> square = x => x * x;

// What compiler generates (simplified):
[CompilerGenerated]
private static class <>c
{
    public static readonly <>c <>9 = new <>c();
    public static Func<int, int> <>9__0_0;

    internal int <Main>b__0_0(int x)
    {
        return x * x;
    }
}

// And uses it as:
Func<int, int> square = <>c.<>9__0_0 ??
    (<>c.<>9__0_0 = new Func<int, int>(<>c.<>9.<Main>b__0_0));
```

### 6.3 Closure Concept - Deep Dive

A **closure** occurs when a lambda captures variables from its enclosing scope.

```csharp
void CreateClosures()
{
    int outerVariable = 10;  // Variable in enclosing scope

    // This lambda "captures" outerVariable
    Func<int, int> multiplier = x => x * outerVariable;

    Console.WriteLine(multiplier(5));  // 50

    outerVariable = 20;  // Change outer variable
    Console.WriteLine(multiplier(5));  // 100 (uses current value!)
}
```

### 6.4 Captured Variables - What Really Happens

```csharp
// What you write:
void Example()
{
    int counter = 0;
    Action increment = () => counter++;
    increment();
    Console.WriteLine(counter); // 1
}

// What compiler generates:
[CompilerGenerated]
private sealed class <>c__DisplayClass0_0
{
    public int counter;  // Captured variable becomes a FIELD

    internal void <Example>b__0()
    {
        counter++;  // Lambda becomes a method on the closure class
    }
}

void Example()
{
    var closure = new <>c__DisplayClass0_0();
    closure.counter = 0;
    Action increment = closure.<Example>b__0;
    increment();
    Console.WriteLine(closure.counter);
}
```

**Memory Diagram:**

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    CLOSURE MEMORY LAYOUT                                │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  WITHOUT CLOSURE:                        WITH CLOSURE:                  │
│  ┌──────────────────┐                   ┌──────────────────┐           │
│  │ Stack Frame      │                   │ Stack Frame      │           │
│  │ ┌──────────────┐ │                   │ ┌──────────────┐ │           │
│  │ │ counter = 0  │ │                   │ │ closure ref  │─┼──┐       │
│  │ │ increment    │ │                   │ │ increment    │ │  │       │
│  │ └──────────────┘ │                   │ └──────────────┘ │  │       │
│  └──────────────────┘                   └──────────────────┘  │       │
│                                                                │       │
│                                         ┌─────────────────────▼───┐   │
│                                         │ HEAP: Closure Object    │   │
│                                         │ ┌─────────────────────┐ │   │
│                                         │ │ counter = 0         │ │   │
│                                         │ │ <Method>b__0()      │ │   │
│                                         │ └─────────────────────┘ │   │
│                                         └─────────────────────────┘   │
│                                                                         │
│  Variable on STACK                      Variable moved to HEAP!         │
│  (destroyed when method returns)        (lives as long as delegate)     │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

╔══════════════════════════════════════════════════════════════════════════════╗
║ CLOSURE MEMORY MODEL - COMPILER TRANSFORMATION ║
╠══════════════════════════════════════════════════════════════════════════════╣
║ ║
║ YOUR CODE: WHAT COMPILER GENERATES: ║
║ ───────── ───────────────────────── ║
║ ║
║ int counter = 0; // Compiler creates a hidden class ║
║ ║
║ Action increment = class <>c\_\_DisplayClass0 ║
║ () => counter++; { ║
║ public int counter; // MOVED HERE! ║
║ Func<int> getCounter = ║
║ () => counter; public void increment_method() ║
║ { ║
║ counter++; // Uses field ║
║ } ║
║ ║
║ public int getCounter_method() ║
║ { ║
║ return counter; // Uses SAME ║
║ } ║
║ } ║
║ ║
╚══════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════╗
║ MEMORY LAYOUT ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║ ║
║ STACK (Demo9_Closures method) HEAP ║
║ ───────────────────────────── ──── ║
║ ║
║ ┌─────────────────────┐ ┌───────────────────────────────┐ ║
║ │ increment │──────────────►│ <>c**DisplayClass0 instance │ ║
║ │ (Action delegate) │ │ ┌─────────────────────────┐ │ ║
║ └─────────────────────┘ ┌───►│ │ counter = 0 │ │ ║
║ ┌─────────────────────┐ │ │ │ (shared field!) │ │ ║
║ │ getCounter │──────────┘ │ └─────────────────────────┘ │ ║
║ │ (Func<int> delegate)│ │ │ ║
║ └─────────────────────┘ │ Methods: │ ║
║ │ - increment_method() │ ║
║ NOTE: counter is NO │ - getCounter_method() │ ║
║ LONGER on stack! └───────────────────────────────┘ ║
║ ║
║ BOTH delegates point to the SAME <>c**DisplayClass0 instance! ║
║ That's why they share the same counter! ║
║ ║
╚═══════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════╗
║ EXECUTION TRACE ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║ ║
║ Step 1: int counter = 0; ║
║ └─► Compiler creates DisplayClass instance with counter = 0 ║
║ DisplayClass.counter = 0 ║
║ ║
║ Step 2: Action increment = () => counter++; ║
║ └─► Creates delegate pointing to DisplayClass.increment_method ║
║ ║
║ Step 3: Func<int> getCounter = () => counter; ║
║ └─► Creates delegate pointing to DisplayClass.getCounter_method ║
║ SAME DisplayClass instance! (not a copy) ║
║ ║
║ Step 4: getCounter() → returns 0 (reads DisplayClass.counter) ║
║ ║
║ Step 5: increment() → DisplayClass.counter becomes 1 ║
║ ║
║ Step 6: getCounter() → returns 1 (reads SAME DisplayClass.counter) ║
║ ║
║ Step 7: increment() → DisplayClass.counter becomes 2 ║
║ Step 8: increment() → DisplayClass.counter becomes 3 ║
║ ║
║ Step 9: getCounter() → returns 3 ║
║ ║
║ Step 10: counter = 100; → DisplayClass.counter = 100 ║
║ ║
║ Step 11: getCounter() → returns 100 ║
║ ║
╚═══════════════════════════════════════════════════════════════════════════════╝

When a lambda captures a variable, the compiler "lifts" that variable from the stack to a compiler-generated class on the heap. All lambdas that capture the same variable share the SAME instance of that class, so they share the SAME variable.

                    WITHOUT CLOSURE                    WITH CLOSURE
                    ──────────────                    ────────────

                    STACK                             STACK              HEAP
                    ┌──────────┐                      ┌──────────┐      ┌────────────┐
                    │ counter  │                      │ ptr ─────┼─────►│ counter    │
                    │ = 0      │                      └──────────┘      │ = 0        │
                    └──────────┘                                        └────────────┘
                                                                            ▲    ▲
                    counter lives on stack                                  │    │
                    and dies when method exits                          increment  getCounter
                                                                        (both point here!)

### 6.5 Memory Impact of Closures

```csharp
// PROBLEM: Closure created in loop
List<Action> actions = new List<Action>();
for (int i = 0; i < 5; i++)
{
    // All lambdas share the SAME closure (same 'i' variable)!
    actions.Add(() => Console.WriteLine(i));
}

foreach (var action in actions)
    action();
// Output: 5, 5, 5, 5, 5 (NOT 0, 1, 2, 3, 4!)

// SOLUTION: Create new variable in each iteration
List<Action> actionsFixed = new List<Action>();
for (int i = 0; i < 5; i++)
{
    int capturedI = i;  // New variable each iteration
    actionsFixed.Add(() => Console.WriteLine(capturedI));
}

foreach (var action in actionsFixed)
    action();
// Output: 0, 1, 2, 3, 4 ✅
```

### 6.6 Closure Performance Considerations

```csharp
// ❌ BAD: Creates new closure object every call
void ProcessMany(List<int> items, int factor)
{
    items.ForEach(x => Console.WriteLine(x * factor));
    // 'factor' is captured - closure allocated!
}

// ✅ BETTER: Avoid capture when possible
void ProcessManyOptimized(List<int> items, int factor)
{
    foreach (var item in items)
    {
        Console.WriteLine(item * factor);
        // No lambda, no closure
    }
}

// ✅ ALSO GOOD: Cache the delegate if reusing
class Processor
{
    private readonly Func<int, int> _transform;
    private readonly int _factor;

    public Processor(int factor)
    {
        _factor = factor;
        _transform = x => x * _factor;  // Created once
    }

    public void Process(List<int> items)
    {
        items.ForEach(x => Console.WriteLine(_transform(x)));
    }
}
```

---

## 7. Delegates vs Events

### 7.1 The Problem with Raw Delegates

```csharp
public class Button
{
    // Raw delegate - anyone can do ANYTHING with it
    public Action OnClick;
}

var button = new Button();

// ✅ Subscribe - OK
button.OnClick += () => Console.WriteLine("Clicked!");

// ❌ Invoke from outside - WRONG! Only Button should invoke
button.OnClick?.Invoke();  // This compiles!

// ❌ Replace all handlers - DANGEROUS!
button.OnClick = () => Console.WriteLine("I replaced everything!");

// ❌ Set to null - BREAKS other subscribers!
button.OnClick = null;
```

### 7.2 Events Solve This Problem

```csharp
public class Button
{
    // Event wraps the delegate
    public event Action OnClick;

    public void Click()
    {
        OnClick?.Invoke();  // Only Button can invoke
    }
}

var button = new Button();

// ✅ Subscribe - OK
button.OnClick += () => Console.WriteLine("Clicked!");

// ❌ Invoke from outside - COMPILE ERROR!
// button.OnClick?.Invoke();

// ❌ Replace all handlers - COMPILE ERROR!
// button.OnClick = () => Console.WriteLine("Replaced");

// ❌ Set to null - COMPILE ERROR!
// button.OnClick = null;

// ✅ Only -= is allowed externally
button.OnClick -= myHandler;
```

### 7.3 Event Internals

```csharp
// What you write:
public event Action OnClick;

// What compiler generates:
private Action _onClick;  // Backing field (private!)

public event Action OnClick
{
    add { _onClick += value; }      // += calls add
    remove { _onClick -= value; }   // -= calls remove
}

// Custom event accessors
public event Action OnClick
{
    add
    {
        lock (_lockObject)
        {
            _onClick += value;
        }
    }
    remove
    {
        lock (_lockObject)
        {
            _onClick -= value;
        }
    }
}
```

### 7.4 Side-by-Side Comparison

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    DELEGATE vs EVENT                                    │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Operation           │ Delegate (public field) │ Event                  │
│  ────────────────────┼─────────────────────────┼───────────────────────│
│  += (add)            │ ✅ Anywhere             │ ✅ Anywhere            │
│  -= (remove)         │ ✅ Anywhere             │ ✅ Anywhere            │
│  = (assign)          │ ✅ Anywhere             │ ❌ Only in class       │
│  Invoke()            │ ✅ Anywhere             │ ❌ Only in class       │
│  = null              │ ✅ Anywhere             │ ❌ Only in class       │
│  Read invocation list│ ✅ Anywhere             │ ❌ Only in class       │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 7.5 Standard Event Pattern

```csharp
// Standard .NET event pattern
public class DataProcessor
{
    // 1. Define custom EventArgs (optional)
    public class DataProcessedEventArgs : EventArgs
    {
        public string Data { get; }
        public DateTime ProcessedAt { get; }

        public DataProcessedEventArgs(string data)
        {
            Data = data;
            ProcessedAt = DateTime.Now;
        }
    }

    // 2. Declare event using EventHandler<T>
    public event EventHandler<DataProcessedEventArgs> DataProcessed;

    // 3. Protected virtual method to raise event
    protected virtual void OnDataProcessed(DataProcessedEventArgs e)
    {
        DataProcessed?.Invoke(this, e);
    }

    // 4. Method that triggers the event
    public void Process(string data)
    {
        // Do processing...
        OnDataProcessed(new DataProcessedEventArgs(data));
    }
}

// Usage
var processor = new DataProcessor();
processor.DataProcessed += (sender, e) =>
{
    Console.WriteLine($"Processed: {e.Data} at {e.ProcessedAt}");
};
processor.Process("Hello");
```

---

## 8. Delegates vs Interfaces

### 8.1 Conceptual Difference

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    DELEGATE vs INTERFACE                                │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  DELEGATE = "I need a method with this signature"                       │
│  ────────────────────────────────────────────────                       │
│  • Focus on BEHAVIOR (single method)                                    │
│  • Anonymous/lambda implementation                                       │
│  • Can be multicast                                                     │
│  • Functional programming style                                         │
│                                                                         │
│  INTERFACE = "I need an object that can do these things"                │
│  ────────────────────────────────────────────────────                   │
│  • Focus on CONTRACT (multiple methods)                                 │
│  • Named class implementation                                           │
│  • Single implementation per object                                     │
│  • Object-oriented programming style                                    │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 8.2 When to Use Delegate

```csharp
// ✅ Single method callback
void ProcessAsync(Action<Result> onComplete);

// ✅ Strategy pattern with simple behavior
void Sort(Comparison<T> comparison);

// ✅ Event handling
public event EventHandler Click;

// ✅ LINQ operations
list.Where(x => x > 5).Select(x => x * 2);

// ✅ Lightweight, ad-hoc behavior
button.OnClick = () => Console.WriteLine("Clicked");
```

### 8.3 When to Use Interface

```csharp
// ✅ Multiple related methods
public interface IRepository<T>
{
    T GetById(int id);
    IEnumerable<T> GetAll();
    void Add(T entity);
    void Update(T entity);
    void Delete(int id);
}

// ✅ Stateful operations
public interface IConnection
{
    bool IsOpen { get; }
    void Open();
    void Close();
    void Execute(string command);
}

// ✅ Dependency injection
public interface ILogger
{
    void Log(LogLevel level, string message);
}

// ✅ Testability with mocks
public interface IEmailService
{
    Task SendAsync(string to, string subject, string body);
}
```

### 8.4 Combined Usage Example

```csharp
// Interface for the contract
public interface IValidator<T>
{
    ValidationResult Validate(T item);
}

// Delegate for customization within interface implementation
public class FlexibleValidator<T> : IValidator<T>
{
    private readonly Func<T, ValidationResult> _validateFunc;

    public FlexibleValidator(Func<T, ValidationResult> validateFunc)
    {
        _validateFunc = validateFunc;
    }

    public ValidationResult Validate(T item) => _validateFunc(item);
}

// Usage - best of both worlds
IValidator<string> emailValidator = new FlexibleValidator<string>(
    email => email.Contains("@")
        ? ValidationResult.Success
        : ValidationResult.Error("Invalid email"));
```

### 8.5 Performance Considerations

```csharp
// Delegate invocation - indirect call through delegate object
Func<int, int> square = x => x * x;
int result = square(5);  // Virtual call through delegate

// Interface method - virtual dispatch
ICalculator calc = new Calculator();
int result = calc.Square(5);  // Virtual call through vtable

// Direct method call - fastest
var calc = new Calculator();
int result = calc.Square(5);  // Direct call (when type is known)

// Performance ranking (fastest to slowest):
// 1. Direct method call (inlinable)
// 2. Interface call (vtable lookup)
// 3. Delegate call (delegate object + method pointer)
// 4. Multicast delegate (iterate invocation list)

// NOTE: In most applications, this difference is negligible.
// Optimize only after profiling!
```

---

## 9. Real-World Scenarios

### 9.1 Callback Patterns

```csharp
// Async operation with callbacks
public class FileDownloader
{
    public void DownloadAsync(string url,
        Action<byte[]> onSuccess,
        Action<Exception> onError,
        Action<int> onProgress)
    {
        Task.Run(async () =>
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                var totalBytes = response.Content.Headers.ContentLength ?? -1;

                using var stream = await response.Content.ReadAsStreamAsync();
                using var ms = new MemoryStream();

                var buffer = new byte[8192];
                int bytesRead;
                int totalRead = 0;

                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, bytesRead);
                    totalRead += bytesRead;

                    if (totalBytes > 0)
                        onProgress?.Invoke((int)((totalRead * 100) / totalBytes));
                }

                onSuccess?.Invoke(ms.ToArray());
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
            }
        });
    }
}

// Usage
var downloader = new FileDownloader();
downloader.DownloadAsync(
    "https://example.com/file.zip",
    data => Console.WriteLine($"Downloaded {data.Length} bytes"),
    error => Console.WriteLine($"Error: {error.Message}"),
    progress => Console.WriteLine($"Progress: {progress}%")
);
```

### 9.2 Strategy Pattern

```csharp
// Payment processing with strategy pattern using delegates
public class PaymentProcessor
{
    private readonly Dictionary<string, Func<decimal, PaymentResult>> _strategies = new();

    public void RegisterStrategy(string method, Func<decimal, PaymentResult> processor)
    {
        _strategies[method] = processor;
    }

    public PaymentResult Process(string method, decimal amount)
    {
        if (!_strategies.TryGetValue(method, out var strategy))
            throw new NotSupportedException($"Payment method '{method}' not supported");

        return strategy(amount);
    }
}

// Setup
var processor = new PaymentProcessor();

processor.RegisterStrategy("credit_card", amount =>
{
    // Process credit card payment
    return new PaymentResult { Success = true, TransactionId = Guid.NewGuid().ToString() };
});

processor.RegisterStrategy("paypal", amount =>
{
    // Process PayPal payment
    return new PaymentResult { Success = true, TransactionId = $"PP-{DateTime.Now.Ticks}" };
});

processor.RegisterStrategy("crypto", amount =>
{
    // Process crypto payment
    return new PaymentResult { Success = true, TransactionId = $"BTC-{Guid.NewGuid()}" };
});

// Usage
var result = processor.Process("credit_card", 99.99m);
```

### 9.3 Middleware Pipeline (ASP.NET Core Style)

```csharp
// Simplified middleware pipeline
public delegate Task RequestDelegate(HttpContext context);

public class MiddlewarePipeline
{
    private readonly List<Func<RequestDelegate, RequestDelegate>> _middlewares = new();

    public void Use(Func<RequestDelegate, RequestDelegate> middleware)
    {
        _middlewares.Add(middleware);
    }

    public RequestDelegate Build()
    {
        RequestDelegate app = context =>
        {
            context.Response.StatusCode = 404;
            return Task.CompletedTask;
        };

        // Build pipeline in reverse order
        for (int i = _middlewares.Count - 1; i >= 0; i--)
        {
            app = _middlewares[i](app);
        }

        return app;
    }
}

// Usage
var pipeline = new MiddlewarePipeline();

// Logging middleware
pipeline.Use(next => async context =>
{
    Console.WriteLine($"[{DateTime.Now}] Request: {context.Request.Path}");
    await next(context);
    Console.WriteLine($"[{DateTime.Now}] Response: {context.Response.StatusCode}");
});

// Authentication middleware
pipeline.Use(next => async context =>
{
    if (!context.User.Identity.IsAuthenticated)
    {
        context.Response.StatusCode = 401;
        return;
    }
    await next(context);
});

// Build and execute
var app = pipeline.Build();
await app(new HttpContext());
```

### 9.4 LINQ Usage

```csharp
// LINQ methods are built on delegates
List<Person> people = GetPeople();

// Where uses Func<T, bool>
var adults = people.Where(p => p.Age >= 18);

// Select uses Func<T, TResult>
var names = people.Select(p => p.Name);

// OrderBy uses Func<T, TKey>
var sorted = people.OrderBy(p => p.Age);

// Aggregate uses Func<TAcc, T, TAcc>
var totalAge = people.Aggregate(0, (sum, p) => sum + p.Age);

// Complex query - all delegates!
var result = people
    .Where(p => p.Age >= 18)              // Func<Person, bool>
    .OrderBy(p => p.LastName)              // Func<Person, string>
    .ThenBy(p => p.FirstName)              // Func<Person, string>
    .Select(p => new { p.FullName, p.Age }) // Func<Person, anonymous>
    .Take(10)
    .ToList();
```

### 9.5 Async/Await and Task Continuations

```csharp
// Task continuations use delegates internally
public async Task ProcessDataAsync()
{
    // Each await is a continuation (delegate)
    var data = await FetchDataAsync();      // Continuation 1
    var processed = await ProcessAsync(data); // Continuation 2
    await SaveAsync(processed);              // Continuation 3
}

// Manual continuation with ContinueWith
Task<string> FetchAndProcess()
{
    return FetchDataAsync()
        .ContinueWith(t => ProcessSync(t.Result))  // Func<Task<byte[]>, string>
        .ContinueWith(t => SaveAndReturn(t.Result)); // Func<Task<string>, string>
}

// TaskCompletionSource with callback
public Task<int> ComputeWithCallbackAsync(Action<int> legacyCallback)
{
    var tcs = new TaskCompletionSource<int>();

    // Legacy API that uses callbacks
    LegacyCompute(result =>
    {
        legacyCallback(result);
        tcs.SetResult(result);
    });

    return tcs.Task;
}
```

---

## 10. Advanced Topics

### 10.1 Expression Trees vs Delegates

```csharp
// Delegate - compiled code
Func<int, int, int> addDelegate = (a, b) => a + b;
// Can only EXECUTE

// Expression tree - code as DATA
Expression<Func<int, int, int>> addExpression = (a, b) => a + b;
// Can INSPECT, MODIFY, TRANSLATE

// Inspecting expression tree
var body = (BinaryExpression)addExpression.Body;
Console.WriteLine($"Operation: {body.NodeType}");   // Add
Console.WriteLine($"Left: {body.Left}");            // a
Console.WriteLine($"Right: {body.Right}");          // b

// Compile expression to delegate when needed
Func<int, int, int> compiled = addExpression.Compile();
int result = compiled(5, 3);  // 8
```

### 10.2 Func<T> vs Expression<Func<T>>

```csharp
public class Repository<T>
{
    // Uses Func - filtering happens IN MEMORY (LINQ to Objects)
    public IEnumerable<T> FindInMemory(Func<T, bool> predicate)
    {
        return _data.Where(predicate);
        // ALL data loaded, then filtered in memory
    }

    // Uses Expression - filtering happens IN DATABASE (LINQ to SQL/EF)
    public IQueryable<T> FindInDatabase(Expression<Func<T, bool>> predicate)
    {
        return _dbContext.Set<T>().Where(predicate);
        // Expression translated to SQL WHERE clause!
    }
}

// Usage - same syntax, VERY different behavior
var repo = new Repository<Person>();

// This loads ALL persons, filters in memory - SLOW for large tables
var adults1 = repo.FindInMemory(p => p.Age >= 18);

// This generates SQL: SELECT * FROM Persons WHERE Age >= 18 - FAST
var adults2 = repo.FindInDatabase(p => p.Age >= 18);
```

### 10.3 Delegate Covariance and Contravariance

```csharp
// COVARIANCE (out) - return type can be more derived
delegate Animal AnimalFactory();
delegate Dog DogFactory();

Dog CreateDog() => new Dog();
Animal CreateAnimal() => new Animal();

AnimalFactory factory1 = CreateDog;     // ✅ Covariance: Dog is Animal
// DogFactory factory2 = CreateAnimal;  // ❌ Error: Animal is not Dog

// CONTRAVARIANCE (in) - parameter type can be less derived
delegate void AnimalHandler(Animal a);
delegate void DogHandler(Dog d);

void HandleAnimal(Animal a) => Console.WriteLine("Handling animal");
void HandleDog(Dog d) => Console.WriteLine("Handling dog");

DogHandler handler1 = HandleAnimal;     // ✅ Contravariance: can handle any Animal
// AnimalHandler handler2 = HandleDog;  // ❌ Error: HandleDog can't handle Cat

// Func and Action have variance built-in:
// Func<out TResult> - covariant in return type
// Action<in T> - contravariant in parameter type
// Func<in T, out TResult> - both!

Func<Dog> getDog = () => new Dog();
Func<Animal> getAnimal = getDog;  // ✅ Covariance

Action<Animal> handleAnimal = a => Console.WriteLine(a);
Action<Dog> handleDog = handleAnimal;  // ✅ Contravariance
```

### 10.4 Delegate Caching

```csharp
// ❌ BAD: Creates new delegate every time
void ProcessItems(List<int> items)
{
    // New delegate allocated each call!
    items.ForEach(x => Console.WriteLine(x));
}

// ✅ GOOD: Static lambda - compiler can cache
void ProcessItemsCached(List<int> items)
{
    // Static lambda - no captures, can be cached
    items.ForEach(static x => Console.WriteLine(x));
}

// ✅ GOOD: Cache delegate as field
class Processor
{
    // Cached delegate instance
    private static readonly Action<int> PrintAction = x => Console.WriteLine(x);

    public void ProcessItems(List<int> items)
    {
        items.ForEach(PrintAction);  // Reuses same instance
    }
}

// ✅ GOOD: Method group (compiler may cache)
class BetterProcessor
{
    public void ProcessItems(List<int> items)
    {
        items.ForEach(Console.WriteLine);  // Method group - often cached
    }
}
```

### 10.5 Performance Benchmarks

```csharp
// Benchmark different delegate invocation patterns
[MemoryDiagnoser]
public class DelegateBenchmarks
{
    private readonly Func<int, int> _cachedDelegate = x => x * 2;

    [Benchmark(Baseline = true)]
    public int DirectCall() => Multiply(42);

    [Benchmark]
    public int CachedDelegate() => _cachedDelegate(42);

    [Benchmark]
    public int NewDelegateEveryTime()
    {
        Func<int, int> d = x => x * 2;
        return d(42);
    }

    [Benchmark]
    public int CapturingLambda()
    {
        int factor = 2;
        Func<int, int> d = x => x * factor;  // Closure allocation!
        return d(42);
    }

    private static int Multiply(int x) => x * 2;
}

// Typical results (relative):
// DirectCall:           1.0x (fastest)
// CachedDelegate:       ~1.5x
// NewDelegateEveryTime: ~2x
// CapturingLambda:      ~5x (closure allocation)
```

---

## 11. Common Interview Questions

### Q1: What is a multicast delegate?

**Answer:**
A multicast delegate is a delegate that holds references to more than one method. When invoked, all methods in its invocation list are called sequentially in the order they were added. All delegates in C# are multicast (inherit from `System.MulticastDelegate`).

```csharp
Action multi = () => Console.WriteLine("First");
multi += () => Console.WriteLine("Second");
multi += () => Console.WriteLine("Third");
multi(); // Outputs: First, Second, Third
```

**Important notes:**

- Only the last method's return value is returned (for non-void delegates)
- If one method throws, subsequent methods are not called
- Use `GetInvocationList()` to iterate and handle exceptions individually

---

### Q2: What is a closure? How does it work internally?

**Answer:**
A closure is a function that captures variables from its enclosing scope. When a lambda captures a variable, the compiler:

1. Creates a hidden class (closure class) to hold the captured variables as fields
2. Converts the lambda to an instance method on that class
3. Allocates the closure object on the heap

```csharp
int multiplier = 10;
Func<int, int> multiply = x => x * multiplier;  // Captures 'multiplier'

// Compiler generates:
class <>c__DisplayClass
{
    public int multiplier;
    public int Method(int x) => x * multiplier;
}
```

**Key implications:**

- Captured variables are moved from stack to heap
- The variable's lifetime extends beyond the original scope
- Multiple lambdas can share the same closure object

---

### Q3: What happens internally when a lambda captures a variable?

**Answer:**
When a lambda captures a variable:

1. **Closure class generation**: Compiler creates a class with the captured variable as a field
2. **Variable hoisting**: The captured variable is moved from stack to heap (inside closure object)
3. **Lambda transformation**: The lambda becomes an instance method of the closure class
4. **Reference update**: All references to the original variable now point to the closure's field

```csharp
void Example()
{
    int counter = 0;           // Originally on stack
    Action inc = () => counter++;  // After capture: on heap in closure
    inc();
    Console.WriteLine(counter);  // Reads from closure, not stack
}
```

---

### Q4: Why are delegates reference types?

**Answer:**
Delegates are reference types because:

1. **Dynamic size**: Multicast delegates need to store an invocation list of arbitrary length
2. **Mutability**: Combining delegates (`+`) creates new delegate objects
3. **Closure support**: Captured variables require heap allocation
4. **Target object reference**: Instance method delegates need to store the target object
5. **Garbage collection**: Delegates may outlive their creation scope

Being reference types allows:

- Sharing delegate instances
- Null checks before invocation
- Equality comparisons by reference
- Participation in garbage collection

---

### Q5: Difference between delegate and event?

**Answer:**

| Aspect             | Delegate                         | Event                                 |
| ------------------ | -------------------------------- | ------------------------------------- |
| **Invocation**     | Anyone with reference can invoke | Only declaring class can invoke       |
| **Assignment (=)** | Allowed from anywhere            | Only inside declaring class           |
| **Encapsulation**  | No encapsulation                 | Enforces publisher-subscriber pattern |
| **Purpose**        | General function pointer         | Notification mechanism                |

```csharp
// Delegate - can be invoked and replaced from outside
public Action OnClick;

// Event - can only be subscribed to from outside
public event Action OnClickEvent;
```

Events are essentially delegates with restricted access - they only expose `+=` and `-=` to external code.

---

### Q6: How does LINQ use delegates internally?

**Answer:**
LINQ methods accept delegates to define:

- **Filtering**: `Where(Func<T, bool> predicate)`
- **Projection**: `Select(Func<T, TResult> selector)`
- **Ordering**: `OrderBy(Func<T, TKey> keySelector)`
- **Aggregation**: `Aggregate(Func<TAcc, T, TAcc> func)`

```csharp
// Each lambda becomes a delegate
list.Where(x => x > 5)           // Func<int, bool>
    .Select(x => x * 2)          // Func<int, int>
    .OrderBy(x => x)             // Func<int, int>
    .ToList();
```

**LINQ to Objects**: Delegates are executed directly in memory
**LINQ to SQL/EF**: Uses `Expression<Func<T>>` - delegates are translated to SQL

---

### Q7: What is the difference between Func<T, bool> and Predicate<T>?

**Answer:**
Functionally identical, but:

| Aspect          | Predicate<T>    | Func<T, bool>       |
| --------------- | --------------- | ------------------- |
| **Parameters**  | Exactly 1       | 0-16                |
| **Return type** | Always bool     | Last type param     |
| **Variance**    | None            | Contravariant input |
| **Usage**       | List<T> methods | LINQ methods        |

```csharp
Predicate<int> p = x => x > 0;     // List.Find, FindAll, Exists
Func<int, bool> f = x => x > 0;     // LINQ Where, Any, All

// Can convert:
Func<int, bool> func = p.Invoke;
```

`Func<T, bool>` is preferred in modern code for consistency with LINQ.

---

### Q8: Can delegates be used with async methods?

**Answer:**
Yes, using `Func<Task>` or `Func<Task<T>>`:

```csharp
// Async delegate
Func<Task> asyncAction = async () =>
{
    await Task.Delay(1000);
    Console.WriteLine("Done");
};

Func<int, Task<string>> asyncFunc = async (id) =>
{
    await Task.Delay(100);
    return $"Result for {id}";
};

// Invoke
await asyncAction();
string result = await asyncFunc(42);
```

Note: Standard `Action` and `Func` don't await - they return immediately. Always use `Task`-returning delegates for async operations.

---

### Q9: What is delegate caching and why is it important?

**Answer:**
Delegate caching means reusing delegate instances instead of creating new ones. Important because:

1. **Reduces allocations**: Each `new delegate` allocates on heap
2. **Reduces GC pressure**: Fewer objects to collect
3. **Improves performance**: Especially in hot paths (loops, frequent calls)

```csharp
// ❌ Creates delegate every iteration
for (int i = 0; i < 1000000; i++)
    items.ForEach(x => Process(x));

// ✅ Cached delegate
Action<Item> processor = x => Process(x);
for (int i = 0; i < 1000000; i++)
    items.ForEach(processor);
```

The compiler automatically caches some delegates (static lambdas, method groups), but capturing lambdas always allocate.

---

### Q10: Explain GetInvocationList() and when to use it.

**Answer:**
`GetInvocationList()` returns an array of individual delegates from a multicast delegate. Use it when:

1. **Exception handling**: Continue invoking remaining delegates even if one throws
2. **Collecting return values**: Get results from all delegates, not just the last
3. **Selective invocation**: Invoke only specific delegates

```csharp
Func<int> multi = () => 1;
multi += () => throw new Exception("Oops");
multi += () => 3;

// Without GetInvocationList - stops at exception
// multi(); // Throws, never returns 3

// With GetInvocationList - handle each individually
var results = new List<int>();
foreach (Func<int> d in multi.GetInvocationList())
{
    try
    {
        results.Add(d());
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Handler failed: {ex.Message}");
    }
}
// results = [1, 3], exception logged
```

---

## 12. Comparison Tables

### 12.1 Delegate vs Interface

| Aspect             | Delegate                        | Interface                            |
| ------------------ | ------------------------------- | ------------------------------------ |
| **Purpose**        | Single method reference         | Contract for multiple methods        |
| **Members**        | One method signature            | Multiple methods, properties, events |
| **Implementation** | Lambda, method group, anonymous | Class implementing interface         |
| **State**          | Stateless (method only)         | Can have state                       |
| **Multicast**      | Yes (invocation list)           | No                                   |
| **Variance**       | Covariance/Contravariance       | Covariance only (C# 4.0+)            |
| **Use case**       | Callbacks, events, LINQ         | DI, polymorphism, contracts          |
| **Performance**    | Slightly slower                 | Slightly faster                      |
| **Flexibility**    | Higher (inline definition)      | Lower (need class)                   |

### 12.2 Delegate vs Event

| Aspect                   | Delegate                 | Event                          |
| ------------------------ | ------------------------ | ------------------------------ |
| **Declaration**          | `public Action OnClick;` | `public event Action OnClick;` |
| **+= (subscribe)**       | ✅ Anywhere              | ✅ Anywhere                    |
| **-= (unsubscribe)**     | ✅ Anywhere              | ✅ Anywhere                    |
| **= (assign)**           | ✅ Anywhere              | ❌ Only declaring class        |
| **Invoke()**             | ✅ Anywhere              | ❌ Only declaring class        |
| **Read invocation list** | ✅ Anywhere              | ❌ Only declaring class        |
| **Null assignment**      | ✅ Anywhere              | ❌ Only declaring class        |
| **Encapsulation**        | None                     | Full                           |
| **Pattern**              | General callback         | Publisher-subscriber           |

### 12.3 Func vs Action vs Predicate

| Type                  | Signature                | Return  | Parameters | Primary Use        |
| --------------------- | ------------------------ | ------- | ---------- | ------------------ |
| `Action`              | `void Method()`          | void    | 0          | Side effects       |
| `Action<T>`           | `void Method(T)`         | void    | 1          | Callbacks          |
| `Action<T1,T2,...>`   | `void Method(T1,T2,...)` | void    | 2-16       | Complex callbacks  |
| `Func<TResult>`       | `TResult Method()`       | TResult | 0          | Factories          |
| `Func<T,TResult>`     | `TResult Method(T)`      | TResult | 1          | Transformations    |
| `Func<T1,T2,TResult>` | `TResult Method(T1,T2)`  | TResult | 2-16       | Calculations       |
| `Predicate<T>`        | `bool Method(T)`         | bool    | 1          | Conditions/filters |

### 12.4 Expression<Func<T>> vs Func<T>

| Aspect          | Func<T>              | Expression<Func<T>>             |
| --------------- | -------------------- | ------------------------------- |
| **Nature**      | Compiled code        | Code as data (AST)              |
| **Execution**   | Direct invocation    | Must compile first              |
| **Inspection**  | Cannot inspect logic | Can traverse AST                |
| **Translation** | Not possible         | Can translate to SQL, etc.      |
| **Performance** | Fast (pre-compiled)  | Slower (compilation overhead)   |
| **Use case**    | LINQ to Objects      | LINQ to SQL/EF, query providers |
| **Memory**      | Method pointer       | Full expression tree            |

---

## 13. Code Examples with Explanation

### Example 1: Complete Delegate Lifecycle

```csharp
// 1. DECLARE delegate type
delegate double Calculator(double x, double y);

class Program
{
    static void Main()
    {
        // 2. INSTANTIATE with different methods
        Calculator add = Add;              // Static method
        Calculator subtract = Subtract;    // Static method

        var calc = new AdvancedCalc();
        Calculator multiply = calc.Multiply;  // Instance method

        Calculator divide = (x, y) => y != 0 ? x / y : double.NaN;  // Lambda

        // 3. INVOKE
        Console.WriteLine($"Add: {add(10, 5)}");           // 15
        Console.WriteLine($"Subtract: {subtract(10, 5)}"); // 5
        Console.WriteLine($"Multiply: {multiply(10, 5)}"); // 50
        Console.WriteLine($"Divide: {divide(10, 5)}");     // 2

        // 4. COMBINE (multicast)
        Calculator all = add + subtract + multiply + divide;
        Console.WriteLine($"All (last result): {all(10, 5)}"); // 2 (divide)

        // 5. GET ALL RESULTS
        foreach (Calculator d in all.GetInvocationList())
        {
            Console.WriteLine(d(10, 5));
        }

        // 6. PASS AS PARAMETER
        ExecuteAndPrint("Addition", add, 10, 5);

        // 7. RETURN FROM METHOD
        var power = GetOperation("power");
        Console.WriteLine($"Power: {power(2, 8)}"); // 256
    }

    static double Add(double x, double y) => x + y;
    static double Subtract(double x, double y) => x - y;

    static void ExecuteAndPrint(string name, Calculator op, double x, double y)
    {
        Console.WriteLine($"{name}: {op(x, y)}");
    }

    static Calculator GetOperation(string name) => name switch
    {
        "power" => Math.Pow,
        "mod" => (x, y) => x % y,
        _ => (x, y) => 0
    };
}

class AdvancedCalc
{
    public double Multiply(double x, double y) => x * y;
}
```

### Example 2: Event Pattern with Custom EventArgs

```csharp
// Custom event args
public class StockPriceChangedEventArgs : EventArgs
{
    public string Symbol { get; }
    public decimal OldPrice { get; }
    public decimal NewPrice { get; }
    public decimal ChangePercent => OldPrice != 0
        ? (NewPrice - OldPrice) / OldPrice * 100
        : 0;

    public StockPriceChangedEventArgs(string symbol, decimal oldPrice, decimal newPrice)
    {
        Symbol = symbol;
        OldPrice = oldPrice;
        NewPrice = newPrice;
    }
}

// Publisher
public class StockTicker
{
    private readonly Dictionary<string, decimal> _prices = new();

    // Event declaration
    public event EventHandler<StockPriceChangedEventArgs> PriceChanged;

    // Protected virtual method to raise event
    protected virtual void OnPriceChanged(StockPriceChangedEventArgs e)
    {
        PriceChanged?.Invoke(this, e);
    }

    public void UpdatePrice(string symbol, decimal newPrice)
    {
        decimal oldPrice = _prices.GetValueOrDefault(symbol);
        _prices[symbol] = newPrice;

        if (oldPrice != newPrice)
        {
            OnPriceChanged(new StockPriceChangedEventArgs(symbol, oldPrice, newPrice));
        }
    }
}

// Subscribers
public class AlertService
{
    public void Subscribe(StockTicker ticker)
    {
        ticker.PriceChanged += OnPriceChanged;
    }

    private void OnPriceChanged(object sender, StockPriceChangedEventArgs e)
    {
        if (Math.Abs(e.ChangePercent) > 5)
        {
            Console.WriteLine($"🚨 ALERT: {e.Symbol} changed {e.ChangePercent:F2}%!");
        }
    }
}

public class Logger
{
    public void Subscribe(StockTicker ticker)
    {
        ticker.PriceChanged += (s, e) =>
        {
            Console.WriteLine($"[LOG] {e.Symbol}: ${e.OldPrice} → ${e.NewPrice}");
        };
    }
}

// Usage
var ticker = new StockTicker();
var alertService = new AlertService();
var logger = new Logger();

alertService.Subscribe(ticker);
logger.Subscribe(ticker);

ticker.UpdatePrice("AAPL", 150.00m);
ticker.UpdatePrice("AAPL", 160.00m);  // 6.67% change - triggers alert
ticker.UpdatePrice("GOOGL", 2800.00m);
```

### Example 3: Closure Pitfalls and Solutions

```csharp
class ClosureExamples
{
    // PITFALL 1: Loop variable capture
    public static void LoopCapturePitfall()
    {
        var actions = new List<Action>();

        // ❌ WRONG: All lambdas capture the SAME 'i' variable
        for (int i = 0; i < 5; i++)
        {
            actions.Add(() => Console.WriteLine(i));
        }

        foreach (var action in actions)
            action();  // Outputs: 5 5 5 5 5
    }

    // SOLUTION 1: Copy to local variable
    public static void LoopCaptureFix1()
    {
        var actions = new List<Action>();

        for (int i = 0; i < 5; i++)
        {
            int captured = i;  // Each iteration gets its own variable
            actions.Add(() => Console.WriteLine(captured));
        }

        foreach (var action in actions)
            action();  // Outputs: 0 1 2 3 4 ✅
    }

    // SOLUTION 2: Use foreach (C# 5+)
    public static void LoopCaptureFix2()
    {
        var actions = new List<Action>();
        var numbers = new[] { 0, 1, 2, 3, 4 };

        foreach (var n in numbers)
        {
            actions.Add(() => Console.WriteLine(n));  // Each n is separate
        }

        foreach (var action in actions)
            action();  // Outputs: 0 1 2 3 4 ✅
    }

    // PITFALL 2: Capturing mutable state
    public static void MutableStatePitfall()
    {
        int counter = 0;

        Func<int> getCounter = () => counter;
        Action increment = () => counter++;

        Console.WriteLine(getCounter());  // 0
        increment();
        increment();
        Console.WriteLine(getCounter());  // 2 - closure shares state!
    }
}
```

### Example 4: Building a Pipeline with Delegates

```csharp
public class Pipeline<T>
{
    private readonly List<Func<T, T>> _steps = new();

    public Pipeline<T> AddStep(Func<T, T> step)
    {
        _steps.Add(step);
        return this;  // Fluent API
    }

    public T Execute(T input)
    {
        T result = input;
        foreach (var step in _steps)
        {
            result = step(result);
        }
        return result;
    }

    // Async version
    private readonly List<Func<T, Task<T>>> _asyncSteps = new();

    public Pipeline<T> AddAsyncStep(Func<T, Task<T>> step)
    {
        _asyncSteps.Add(step);
        return this;
    }

    public async Task<T> ExecuteAsync(T input)
    {
        T result = input;
        foreach (var step in _asyncSteps)
        {
            result = await step(result);
        }
        return result;
    }
}

// Usage
var stringPipeline = new Pipeline<string>()
    .AddStep(s => s.Trim())
    .AddStep(s => s.ToLower())
    .AddStep(s => s.Replace(" ", "-"))
    .AddStep(s => $"processed-{s}");

string result = stringPipeline.Execute("  Hello World  ");
Console.WriteLine(result);  // "processed-hello-world"

var numberPipeline = new Pipeline<int>()
    .AddStep(n => n * 2)
    .AddStep(n => n + 10)
    .AddStep(n => n * n);

int numResult = numberPipeline.Execute(5);
Console.WriteLine(numResult);  // ((5 * 2) + 10)² = 400
```

---

## Summary Cheat Sheet

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    C# DELEGATES CHEAT SHEET                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  DECLARATION:                                                           │
│  delegate ReturnType Name(Parameters);                                  │
│                                                                         │
│  BUILT-IN:                                                              │
│  • Action       → void Method(...)                                      │
│  • Action<T>    → void Method(T arg)                                    │
│  • Func<T>      → T Method()                                            │
│  • Func<T,R>    → R Method(T arg)                                       │
│  • Predicate<T> → bool Method(T arg)                                    │
│                                                                         │
│  INSTANTIATION:                                                         │
│  • Named method:     Func<int, int> f = MyMethod;                       │
│  • Lambda:           Func<int, int> f = x => x * 2;                     │
│  • Anonymous:        Func<int, int> f = delegate(int x) { return x; };  │
│                                                                         │
│  MULTICAST:                                                             │
│  • Combine:          combined = d1 + d2;                                │
│  • Remove:           combined -= d1;                                    │
│  • Iterate:          foreach (var d in combined.GetInvocationList())    │
│                                                                         │
│  INVOCATION:                                                            │
│  • Direct:           result = myDelegate(args);                         │
│  • Null-safe:        result = myDelegate?.Invoke(args);                 │
│                                                                         │
│  CLOSURE:                                                               │
│  • Lambda capturing outer variable → heap allocation                    │
│  • Captured variable lives as long as delegate                          │
│  • Loop capture pitfall: use local copy per iteration                   │
│                                                                         │
│  EVENT vs DELEGATE:                                                     │
│  • Event: only declaring class can invoke                               │
│  • Delegate: anyone can invoke                                          │
│                                                                         │
│  PERFORMANCE:                                                           │
│  • Cache delegates when possible                                        │
│  • Avoid capturing lambdas in hot paths                                 │
│  • Use static lambdas (C# 9+) when no capture needed                    │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

_Document created for interview preparation. Last updated: February 2026_
