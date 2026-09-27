using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DelegatesDemo
{
    // ============================================================
    // SECTION 1: Custom Delegate Declarations
    // ============================================================
    
    // Simple delegate - no parameters, no return
    delegate void SimpleDelegate();
    
    // Delegate with parameters
    delegate void MessageDelegate(string message);
    
    // Delegate with return type
    delegate int MathOperation(int a, int b);
    
    // Generic delegate
    delegate T Transformer<T>(T input);
    
    // Delegate for event pattern
    delegate void OrderEventHandler(object sender, OrderEventArgs e);
    
    // ============================================================
    // SECTION 2: Supporting Classes
    // ============================================================
    
    public class OrderEventArgs : EventArgs
    {
        public string OrderId { get; }
        public decimal Amount { get; }
        public DateTime OrderDate { get; }
        
        public OrderEventArgs(string orderId, decimal amount)
        {
            OrderId = orderId;
            Amount = amount;
            OrderDate = DateTime.Now;
        }
    }
    
    public class Calculator
    {
        public int Add(int a, int b) => a + b;
        public int Multiply(int a, int b) => a * b;
    }
    
    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
    
    // ============================================================
    // SECTION 3: Main Demo Program
    // ============================================================
    
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          C# DELEGATES - COMPREHENSIVE DEMO                   ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");
            
            // Demo 1: Basic delegate instantiation
            //Demo1_BasicInstantiation();
            
            // Demo 2: Different ways to create delegates
             //Demo2_CreationMethods();
            
            // // Demo 3: Multicast delegates
             //Demo3_MulticastDelegates();
            
            // // Demo 4: Return values in multicast
            // Demo4_MulticastReturnValues();
            
            // // Demo 5: Exception handling in multicast
            // Demo5_MulticastExceptionHandling();
            
            // // Demo 6: Built-in delegates (Action, Func, Predicate)
            // Demo6_BuiltInDelegates();
            
            // // Demo 7: Passing delegates as parameters
            // Demo7_DelegatesAsParameters();
            
            // // Demo 8: Returning delegates from methods
            // Demo8_ReturningDelegates();
            
            // // Demo 9: Closures and captured variables
             Demo9_Closures();
            
            // // Demo 10: Loop capture pitfall
            // Demo10_LoopCapturePitfall();
            
            // // Demo 11: Events vs Delegates
            // Demo11_EventsVsDelegates();
            
            // // Demo 12: Covariance and Contravariance
            // Demo12_CovarianceContravariance();
            
            // // Demo 13: LINQ with delegates
            // Demo13_LINQWithDelegates();
            
            // // Demo 14: Async delegates
            // Demo14_AsyncDelegates().Wait();
            
            // // Demo 15: Real-world pipeline pattern
            // Demo15_PipelinePattern();
            
            Console.WriteLine("\n╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    DEMO COMPLETED!                           ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        }
        
        // ============================================================
        // DEMO 1: Basic Delegate Instantiation
        // ============================================================
        static void Demo1_BasicInstantiation()
        {
            PrintHeader("DEMO 1: Basic Delegate Instantiation");
            
            // Instantiate delegate with named method
            MathOperation add = Add;
            MathOperation subtract = Subtract;
            
            // Invoke delegates
            Console.WriteLine($"  add(10, 5) = {add(10, 5)}");
            Console.WriteLine($"  subtract(10, 5) = {subtract(10, 5)}");
            
            // Instance method
            var calc = new Calculator();
            MathOperation multiply = calc.Multiply;
            Console.WriteLine($"  multiply(10, 5) = {multiply(10, 5)}");
            
            PrintFooter();
        }
        
        static int Add(int a, int b) => a + b;
        static int Subtract(int a, int b) => a - b;
        
        // ============================================================
        // DEMO 2: Different Ways to Create Delegates
        // ============================================================
        static void Demo2_CreationMethods()
        {
            PrintHeader("DEMO 2: Different Ways to Create Delegates");
            
            // Method 1: Named method (C# 1.0)
            MathOperation op1 = Add;
            Console.WriteLine($"  Named method: {op1(10, 5)}");
            
            // Method 2: Anonymous method (C# 2.0)
            MathOperation op2 = delegate(int a, int b) { return a * b; };
            Console.WriteLine($"  Anonymous method: {op2(10, 5)}");
            
            // Method 3: Lambda expression (C# 3.0)
            MathOperation op3 = (a, b) => a / b;
            Console.WriteLine($"  Lambda expression: {op3(10, 5)}");
            
            // Method 4: Lambda with block body
            MathOperation op4 = (a, b) =>
            {
                int result = a + b;
                return result * 2;
            };
            Console.WriteLine($"  Lambda with block: {op4(10, 5)}");
            
            PrintFooter();
        }
        
        // ============================================================
        // DEMO 3: Multicast Delegates
        // ============================================================
        static void Demo3_MulticastDelegates()
        {
            PrintHeader("DEMO 3: Multicast Delegates");
            
            Action<string> log = null;
            
            // Add multiple handlers
            log += msg => Console.WriteLine($"    [Console] {msg}");
            log += msg => Console.WriteLine($"    [Debug]   {msg}");
            log += msg => Console.WriteLine($"    [File]    Writing '{msg}' to file...");
            
            Console.WriteLine("  Invoking multicast delegate:");
            log("Application started");
            
            Console.WriteLine("\n  Invocation list count: " + log.GetInvocationList().Length);
            
            // Remove one handler
            Action<string> debugLog = msg => Console.WriteLine($"    [Debug]   {msg}");
            // Note: Cannot remove lambda directly - must use same reference
            
            PrintFooter();
        }
        
        // ============================================================
        // DEMO 4: Return Values in Multicast
        // ============================================================
        static void Demo4_MulticastReturnValues()
        {
            PrintHeader("DEMO 4: Return Values in Multicast");
            
            Func<int> getNumber = () => { Console.WriteLine("    Returning 1"); return 1; };
            getNumber += () => { Console.WriteLine("    Returning 2"); return 2; };
            getNumber += () => { Console.WriteLine("    Returning 3"); return 3; };
            
            Console.WriteLine("  Standard invocation (only last value returned):");
            int result = getNumber();
            Console.WriteLine($"    Final result: {result}");
            
            Console.WriteLine("\n  Using GetInvocationList to get ALL results:");
            var allResults = getNumber.GetInvocationList()
                .Cast<Func<int>>()
                .Select(f => f())
                .ToList();
            Console.WriteLine($"    All results: [{string.Join(", ", allResults)}]");
            
            PrintFooter();
        }
        
        // ============================================================
        // DEMO 5: Exception Handling in Multicast
        // ============================================================
        static void Demo5_MulticastExceptionHandling()
        {
            PrintHeader("DEMO 5: Exception Handling in Multicast");
            
            Action action = () => Console.WriteLine("    Handler 1: OK");
            action += () => throw new InvalidOperationException("Handler 2 failed!");
            action += () => Console.WriteLine("    Handler 3: OK");
            
            Console.WriteLine("  Standard invocation (stops on exception):");
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    Caught: {ex.Message}");
            }
            
            Console.WriteLine("\n  Using GetInvocationList (continues after exception):");
            foreach (Action handler in action.GetInvocationList())
            {
                try
                {
                    handler();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"    Caught: {ex.Message}");
                }
            }
            
            PrintFooter();
        }
        
        // ============================================================
        // DEMO 6: Built-in Delegates
        // ============================================================
        static void Demo6_BuiltInDelegates()
        {
            PrintHeader("DEMO 6: Built-in Delegates (Action, Func, Predicate)");
            
            // Action - no return value
            Action greet = () => Console.WriteLine("    Hello, World!");
            Action<string> greetPerson = name => Console.WriteLine($"    Hello, {name}!");
            Action<string, int> greetMultiple = (name, times) =>
            {
                for (int i = 0; i < times; i++)
                    Console.WriteLine($"    Hello, {name}!");
            };
            
            Console.WriteLine("  Action examples:");
            greet();
            greetPerson("Alice");
            
            // Func - has return value
            Func<int> getRandomNumber = () => new Random().Next(1, 100);
            Func<int, int> square = x => x * x;
            Func<int, int, int> add = (a, b) => a + b;
            Func<string, int, bool> validateLength = (str, maxLen) => str.Length <= maxLen;
            
            Console.WriteLine("\n  Func examples:");
            Console.WriteLine($"    Random number: {getRandomNumber()}");
            Console.WriteLine($"    Square of 5: {square(5)}");
            Console.WriteLine($"    Add 10 + 20: {add(10, 20)}");
            Console.WriteLine($"    'Hello' length <= 10: {validateLength("Hello", 10)}");
            
            // Predicate - always returns bool, single parameter
            Predicate<int> isPositive = x => x > 0;
            Predicate<string> isNotEmpty = s => !string.IsNullOrEmpty(s);
            
            Console.WriteLine("\n  Predicate examples:");
            Console.WriteLine($"    isPositive(5): {isPositive(5)}");
            Console.WriteLine($"    isPositive(-3): {isPositive(-3)}");
            Console.WriteLine($"    isNotEmpty(\"Hello\"): {isNotEmpty("Hello")}");
            Console.WriteLine($"    isNotEmpty(\"\"): {isNotEmpty("")}");
            
            // Using Predicate with List<T> methods
            var numbers = new List<int> { -2, -1, 0, 1, 2, 3, 4, 5 };
            Console.WriteLine($"\n  List operations with Predicate:");
            Console.WriteLine($"    Original: [{string.Join(", ", numbers)}]");
            Console.WriteLine($"    Find first positive: {numbers.Find(isPositive)}");
            Console.WriteLine($"    Find all positive: [{string.Join(", ", numbers.FindAll(isPositive))}]");
            Console.WriteLine($"    Any positive: {numbers.Exists(isPositive)}");
            Console.WriteLine($"    All positive: {numbers.TrueForAll(isPositive)}");
            
            PrintFooter();
        }
        
        // ============================================================
        // DEMO 7: Passing Delegates as Parameters
        // ============================================================
        static void Demo7_DelegatesAsParameters()
        {
            PrintHeader("DEMO 7: Passing Delegates as Parameters");
            
            int[] numbers = { 1, 2, 3, 4, 5 };
            
            Console.WriteLine($"  Original array: [{string.Join(", ", numbers)}]");
            
            // Pass different transformations
            Console.WriteLine("\n  Applying different transformations:");
            
            int[] doubled = Transform(numbers, x => x * 2);
            Console.WriteLine($"    Doubled: [{string.Join(", ", doubled)}]");
            
            int[] squared = Transform(numbers, x => x * x);
            Console.WriteLine($"    Squared: [{string.Join(", ", squared)}]");
            
            int[] incremented = Transform(numbers, x => x + 10);
            Console.WriteLine($"    +10: [{string.Join(", ", incremented)}]");
            
            PrintFooter();
        }
        
        static int[] Transform(int[] array, Func<int, int> transformer)
        {
            int[] result = new int[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                result[i] = transformer(array[i]);
            }
            return result;
        }
        
        // ============================================================
        // DEMO 8: Returning Delegates from Methods
        // ============================================================
        static void Demo8_ReturningDelegates()
        {
            PrintHeader("DEMO 8: Returning Delegates from Methods");
            
            var addOp = GetOperation('+');
            var subOp = GetOperation('-');
            var mulOp = GetOperation('*');
            var divOp = GetOperation('/');
            
            Console.WriteLine("  Using returned delegates:");
            Console.WriteLine($"    10 + 5 = {addOp(10, 5)}");
            Console.WriteLine($"    10 - 5 = {subOp(10, 5)}");
            Console.WriteLine($"    10 * 5 = {mulOp(10, 5)}");
            Console.WriteLine($"    10 / 5 = {divOp(10, 5)}");
            
            // Factory that creates multiplier functions
            Console.WriteLine("\n  Multiplier factory:");
            var multiplyBy2 = CreateMultiplier(2);
            var multiplyBy10 = CreateMultiplier(10);
            Console.WriteLine($"    multiplyBy2(5) = {multiplyBy2(5)}");
            Console.WriteLine($"    multiplyBy10(5) = {multiplyBy10(5)}");
            
            PrintFooter();
        }
        
        static Func<int, int, int> GetOperation(char op)
        {
            return op switch
            {
                '+' => (a, b) => a + b,
                '-' => (a, b) => a - b,
                '*' => (a, b) => a * b,
                '/' => (a, b) => b != 0 ? a / b : 0,
                _ => (a, b) => 0
            };
        }
        
        static Func<int, int> CreateMultiplier(int factor)
        {
            // This creates a closure - 'factor' is captured
            return x => x * factor;
        }
        
        // ============================================================
        // DEMO 9: Closures and Captured Variables
        // ============================================================
        static void Demo9_Closures()
        {
            PrintHeader("DEMO 9: Closures and Captured Variables");
            
            int counter = 0;  // Variable to be captured
            
            Action increment = () => counter++;
            Func<int> getCounter = () => counter;
            
            Console.WriteLine($"  Initial counter: {getCounter()}");
            
            increment();
            Console.WriteLine($"  After 1 increment: {getCounter()}");
            
            increment();
            increment();
            Console.WriteLine($"  After 2 more increments: {getCounter()}");
            
            // The closure shares state!
            counter = 100;
            Console.WriteLine($"  After setting counter = 100: {getCounter()}");
            
            Console.WriteLine("\n  Key insight: Both lambdas share the SAME 'counter' variable");
            Console.WriteLine("  The variable was 'captured' and moved to heap memory");
            
            PrintFooter();
        }
        
        // ============================================================
        // DEMO 10: Loop Capture Pitfall
        // ============================================================
        static void Demo10_LoopCapturePitfall()
        {
            PrintHeader("DEMO 10: Loop Capture Pitfall");
            
            Console.WriteLine("  ❌ WRONG - All lambdas capture same variable:");
            var wrongActions = new List<Action>();
            for (int i = 0; i < 5; i++)
            {
                wrongActions.Add(() => Console.Write($"{i} "));
            }
            Console.Write("    ");
            foreach (var action in wrongActions)
                action();
            Console.WriteLine("  <- All print 5!");
            
            Console.WriteLine("\n  ✅ CORRECT - Copy variable in each iteration:");
            var correctActions = new List<Action>();
            for (int i = 0; i < 5; i++)
            {
                int captured = i;  // New variable each iteration
                correctActions.Add(() => Console.Write($"{captured} "));
            }
            Console.Write("    ");
            foreach (var action in correctActions)
                action();
            Console.WriteLine(" <- Prints 0 1 2 3 4");
            
            PrintFooter();
        }
        
        // ============================================================
        // DEMO 11: Events vs Delegates
        // ============================================================
        static void Demo11_EventsVsDelegates()
        {
            PrintHeader("DEMO 11: Events vs Delegates");
            
            // Using raw delegate (problematic)
            var buttonWithDelegate = new ButtonWithDelegate();
            buttonWithDelegate.OnClick = () => Console.WriteLine("    Handler 1");
            buttonWithDelegate.OnClick += () => Console.WriteLine("    Handler 2");
            
            Console.WriteLine("  Raw delegate - can be invoked externally:");
            buttonWithDelegate.OnClick?.Invoke();  // External invocation - BAD!
            
            Console.WriteLine("\n  Raw delegate - can be replaced:");
            buttonWithDelegate.OnClick = () => Console.WriteLine("    I replaced everything!");
            buttonWithDelegate.OnClick();
            
            // Using event (proper encapsulation)
            var buttonWithEvent = new ButtonWithEvent();
            buttonWithEvent.OnClick += () => Console.WriteLine("    Event Handler 1");
            buttonWithEvent.OnClick += () => Console.WriteLine("    Event Handler 2");
            
            Console.WriteLine("\n  Event - can only be invoked internally:");
            // buttonWithEvent.OnClick?.Invoke();  // COMPILE ERROR!
            buttonWithEvent.Click();  // Must use the class's method
            
            // buttonWithEvent.OnClick = () => {};  // COMPILE ERROR!
            Console.WriteLine("\n  Event - cannot be replaced, only += and -= allowed");
            
            PrintFooter();
        }
        
        // ============================================================
        // DEMO 12: Covariance and Contravariance
        // ============================================================
        static void Demo12_CovarianceContravariance()
        {
            PrintHeader("DEMO 12: Covariance and Contravariance");
            
            // Covariance: return type can be more derived
            Console.WriteLine("  Covariance (return type):");
            Func<Animal> getAnimal = GetDog;  // Dog is-a Animal ✓
            Animal animal = getAnimal();
            Console.WriteLine($"    Func<Animal> assigned to GetDog: {animal.GetType().Name}");
            
            // Contravariance: parameter type can be less derived
            Console.WriteLine("\n  Contravariance (parameter type):");
            Action<Dog> handleDog = HandleAnimal;  // HandleAnimal can handle any Animal ✓
            handleDog(new Dog { Name = "Rex" });
            
            Console.WriteLine("\n  Why this works:");
            Console.WriteLine("    Covariance: If you expect Animal, Dog is fine (more specific)");
            Console.WriteLine("    Contravariance: If you need to handle Dog, handler for Animal works");
            
            PrintFooter();
        }
        
        static Dog GetDog() => new Dog { Name = "Buddy" };
        static void HandleAnimal(Animal a) => Console.WriteLine($"    Handling animal: {a.Name}");
        
        // ============================================================
        // DEMO 13: LINQ with Delegates
        // ============================================================
        static void Demo13_LINQWithDelegates()
        {
            PrintHeader("DEMO 13: LINQ with Delegates");
            
            var people = new List<Person>
            {
                new() { Name = "Alice", Age = 30 },
                new() { Name = "Bob", Age = 25 },
                new() { Name = "Charlie", Age = 35 },
                new() { Name = "Diana", Age = 28 },
                new() { Name = "Eve", Age = 22 }
            };
            
            Console.WriteLine("  Original list:");
            people.ForEach(p => Console.WriteLine($"    {p.Name}, Age: {p.Age}"));
            
            // Where - Func<T, bool>
            Console.WriteLine("\n  Where (Age >= 25) - uses Func<Person, bool>:");
            var adults = people.Where(p => p.Age >= 25);
            foreach (var p in adults)
                Console.WriteLine($"    {p.Name}");
            
            // Select - Func<T, TResult>
            Console.WriteLine("\n  Select (names only) - uses Func<Person, string>:");
            var names = people.Select(p => p.Name);
            Console.WriteLine($"    [{string.Join(", ", names)}]");
            
            // OrderBy - Func<T, TKey>
            Console.WriteLine("\n  OrderBy (by age) - uses Func<Person, int>:");
            var sorted = people.OrderBy(p => p.Age);
            foreach (var p in sorted)
                Console.WriteLine($"    {p.Name}: {p.Age}");
            
            // Aggregate - Func<TAcc, T, TAcc>
            Console.WriteLine("\n  Aggregate (sum of ages) - uses Func<int, Person, int>:");
            int totalAge = people.Aggregate(0, (sum, p) => sum + p.Age);
            Console.WriteLine($"    Total age: {totalAge}");
            
            // Complex query
            Console.WriteLine("\n  Complex query combining multiple delegates:");
            var result = people
                .Where(p => p.Age >= 25)           // Filter
                .OrderByDescending(p => p.Age)     // Sort
                .Select(p => $"{p.Name} ({p.Age})")  // Transform
                .Take(3);                          // Limit
            Console.WriteLine($"    Top 3 adults: [{string.Join(", ", result)}]");
            
            PrintFooter();
        }
        
        // ============================================================
        // DEMO 14: Async Delegates
        // ============================================================
        static async Task Demo14_AsyncDelegates()
        {
            PrintHeader("DEMO 14: Async Delegates");
            
            // Async delegate returning Task
            Func<Task> asyncAction = async () =>
            {
                Console.WriteLine("    Starting async operation...");
                await Task.Delay(100);
                Console.WriteLine("    Async operation completed!");
            };
            
            Console.WriteLine("  Invoking Func<Task>:");
            await asyncAction();
            
            // Async delegate returning Task<T>
            Func<int, Task<string>> asyncFunc = async (id) =>
            {
                Console.WriteLine($"    Fetching data for ID: {id}...");
                await Task.Delay(100);
                return $"Data for {id}";
            };
            
            Console.WriteLine("\n  Invoking Func<int, Task<string>>:");
            string result = await asyncFunc(42);
            Console.WriteLine($"    Result: {result}");
            
            // Parallel async operations
            Console.WriteLine("\n  Parallel async operations:");
            var tasks = new[] { 1, 2, 3 }.Select(asyncFunc);
            var results = await Task.WhenAll(tasks);
            Console.WriteLine($"    Results: [{string.Join(", ", results)}]");
            
            PrintFooter();
        }
        
        // ============================================================
        // DEMO 15: Pipeline Pattern
        // ============================================================
        static void Demo15_PipelinePattern()
        {
            PrintHeader("DEMO 15: Real-World Pipeline Pattern");
            
            // String processing pipeline
            var stringPipeline = new Pipeline<string>()
                .AddStep(s => s.Trim())
                .AddStep(s => s.ToLower())
                .AddStep(s => s.Replace(" ", "-"))
                .AddStep(s => $"processed-{s}");
            
            string input = "  Hello World Example  ";
            string output = stringPipeline.Execute(input);
            
            Console.WriteLine("  String pipeline:");
            Console.WriteLine($"    Input:  \"{input}\"");
            Console.WriteLine($"    Output: \"{output}\"");
            
            // Number processing pipeline
            var numberPipeline = new Pipeline<int>()
                .AddStep(n => n * 2)       // Double
                .AddStep(n => n + 10)      // Add 10
                .AddStep(n => n * n);      // Square
            
            int numInput = 5;
            int numOutput = numberPipeline.Execute(numInput);
            
            Console.WriteLine("\n  Number pipeline ((n * 2 + 10)²):");
            Console.WriteLine($"    Input:  {numInput}");
            Console.WriteLine($"    Output: {numOutput}");
            Console.WriteLine($"    Calculation: (({numInput} * 2) + 10)² = (10 + 10)² = 20² = 400");
            
            PrintFooter();
        }
        
        // ============================================================
        // Helper Methods
        // ============================================================
        static void PrintHeader(string title)
        {
            Console.WriteLine($"\n┌──────────────────────────────────────────────────────────────┐");
            Console.WriteLine($"│ {title.PadRight(60)} │");
            Console.WriteLine($"└──────────────────────────────────────────────────────────────┘");
        }
        
        static void PrintFooter()
        {
            Console.WriteLine("└─────────────────────────────────────────────────────────────────┘");
        }
    }
    
    // ============================================================
    // Supporting Classes for Demos
    // ============================================================
    
    // For Demo 11
    class ButtonWithDelegate
    {
        public Action? OnClick;  // Raw delegate - anyone can invoke/replace
    }
    
    class ButtonWithEvent
    {
        public event Action? OnClick;  // Event - encapsulated
        
        public void Click()
        {
            OnClick?.Invoke();  // Only this class can invoke
        }
    }
    
    // For Demo 12
    class Animal
    {
        public string Name { get; set; } = "";
    }
    
    class Dog : Animal { }
    
    // For Demo 15
    class Pipeline<T>
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
    }
}
