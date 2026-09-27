using System;
using System.Collections.Generic;

namespace OOPFundamentals
{
    // ============================================================
    // SECTION 1: Four Pillars of OOP Demo
    // ============================================================
    
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║        OOP FUNDAMENTALS - COMPREHENSIVE DEMO                 ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");
            
            // Demo 1: Encapsulation
            Demo1_Encapsulation();
            
            // Demo 2: Inheritance
            Demo2_Inheritance();
            
            // Demo 3: Polymorphism - Compile Time (Overloading)
            Demo3_CompileTimePolymorphism();
            
            // Demo 4: Polymorphism - Runtime (Override)
            Demo4_RuntimePolymorphism();
            
            // Demo 5: Virtual and Override
            Demo5_VirtualOverride();
            
            // Demo 6: Abstract Classes
            Demo6_AbstractClasses();
            
            // Demo 7: Interfaces
            Demo7_Interfaces();
            
            // Demo 8: Multiple Interface Implementation
            Demo8_MultipleInterfaces();
            
            // Demo 9: Explicit Interface Implementation
            Demo9_ExplicitInterface();
            
            // Demo 10: Abstract vs Interface
            Demo10_AbstractVsInterface();
            
            // Demo 11: Method Hiding (new keyword)
            Demo11_MethodHiding();
            
            // Demo 12: Sealed Classes and Methods
            Demo12_Sealed();
            
            // Demo 13: Static Members
            Demo13_Static();
            
            // Demo 14: readonly vs const
            Demo14_ReadonlyVsConst();
            
            Console.WriteLine("\n╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    DEMO COMPLETED!                           ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        }
        
        // ============================================================
        // DEMO 1: Encapsulation
        // ============================================================
        static void Demo1_Encapsulation()
        {
            PrintHeader("DEMO 1: Encapsulation");
            
            var account = new BankAccount("John Doe");
            Console.WriteLine($"  Initial balance: ${account.Balance}");
            
            account.Deposit(1000);
            Console.WriteLine($"  After deposit $1000: ${account.Balance}");
            
            account.Withdraw(300);
            Console.WriteLine($"  After withdraw $300: ${account.Balance}");
            
            try
            {
                account.Withdraw(1000);  // Will fail
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"  Withdraw $1000 failed: {ex.Message}");
            }
            
            // Cannot directly modify balance:
            // account.Balance = 1000000;  // COMPILE ERROR - no setter
            // account._balance = 1000000; // COMPILE ERROR - private
            
            PrintFooter();
        }
        
        // ============================================================
        // DEMO 2: Inheritance
        // ============================================================
        static void Demo2_Inheritance()
        {
            PrintHeader("DEMO 2: Inheritance");
            
            var animal = new Animal { Name = "Generic Animal" };
            animal.Eat();
            animal.Sleep();
            
            Console.WriteLine();
            
            var dog = new Dog { Name = "Rex", Breed = "German Shepherd" };
            dog.Eat();    // Inherited from Animal
            dog.Sleep();  // Inherited from Animal
            dog.Bark();   // Defined in Dog
            
            Console.WriteLine();
            
            var cat = new Cat { Name = "Whiskers" };
            cat.Eat();    // Inherited
            cat.Meow();   // Defined in Cat
            
            PrintFooter();
        }
        
        // ============================================================
        // DEMO 3: Compile-Time Polymorphism (Overloading)
        // ============================================================
        static void Demo3_CompileTimePolymorphism()
        {
            PrintHeader("DEMO 3: Compile-Time Polymorphism (Method Overloading)");
            
            var calc = new Calculator();
            
            Console.WriteLine("  Same method name, different parameters:");
            Console.WriteLine($"    Add(5, 3) = {calc.Add(5, 3)}");
            Console.WriteLine($"    Add(5, 3, 2) = {calc.Add(5, 3, 2)}");
            Console.WriteLine($"    Add(5.5, 3.3) = {calc.Add(5.5, 3.3)}");
            Console.WriteLine($"    Add(\"Hello\", \"World\") = {calc.Add("Hello", "World")}");
            
            Console.WriteLine("\n  Compiler decides which method at COMPILE time");
            
            PrintFooter();
        }
        
        // ============================================================
        // DEMO 4: Runtime Polymorphism (Override)
        // ============================================================
        static void Demo4_RuntimePolymorphism()
        {
            PrintHeader("DEMO 4: Runtime Polymorphism (Method Overriding)");
            
            // Array of base type references, holding derived objects
            Shape[] shapes = 
            {
                new Circle { Radius = 5 },
                new Rectangle { Width = 4, Height = 6 },
                new Triangle { Base = 3, Height = 4 }
            };
            
            Console.WriteLine("  Same method call, different behavior:");
            foreach (var shape in shapes)
            {
                // GetArea() is resolved at RUNTIME based on actual object type
                Console.WriteLine($"    {shape.GetType().Name}: Area = {shape.GetArea():F2}");
            }
            
            Console.WriteLine("\n  CLR looks at ACTUAL object type at RUNTIME");
            
            PrintFooter();
        }
        
        // ============================================================
        // DEMO 5: Virtual and Override
        // ============================================================
        static void Demo5_VirtualOverride()
        {
            PrintHeader("DEMO 5: Virtual and Override Keywords");
            
            Animal animal = new Animal { Name = "Generic" };
            Animal dog = new Dog { Name = "Rex" };      // Base reference, derived object
            Animal cat = new Cat { Name = "Whiskers" };
            
            Console.WriteLine("  Calling Speak() - virtual method:");
            Console.WriteLine($"    animal.Speak(): ");
            animal.Speak();
            Console.WriteLine($"    dog.Speak(): ");
            dog.Speak();
            Console.WriteLine($"    cat.Speak(): ");
            cat.Speak();
            
            Console.WriteLine("\n  Key: virtual + override = polymorphic behavior");
            Console.WriteLine("  The ACTUAL object type determines which method runs");
            
            // Calling base implementation
            Console.WriteLine("\n  Calling base implementation from override:");
            var bulldog = new Bulldog { Name = "Bruno" };
            bulldog.Speak();
            
            PrintFooter();
        }
        
        // ============================================================
        // DEMO 6: Abstract Classes
        // ============================================================
        static void Demo6_AbstractClasses()
        {
            PrintHeader("DEMO 6: Abstract Classes");
            
            // Cannot instantiate abstract class:
            // var vehicle = new Vehicle();  // COMPILE ERROR!
            
            Vehicle car = new Car("Toyota", "Camry", 200);
            Vehicle motorcycle = new Motorcycle("Harley", "Davidson", 180);
            
            Console.WriteLine("  Abstract class with abstract and concrete methods:");
            
            Console.WriteLine("\n  Car:");
            car.DisplayInfo();   // Concrete method from abstract class
            car.Start();         // Abstract method - implemented in Car
            car.Accelerate();    // Virtual method - overridden in Car
            
            Console.WriteLine("\n  Motorcycle:");
            motorcycle.DisplayInfo();
            motorcycle.Start();
            motorcycle.Accelerate();  // Uses default from abstract class
            
            PrintFooter();
        }
        
        // ============================================================
        // DEMO 7: Interfaces
        // ============================================================
        static void Demo7_Interfaces()
        {
            PrintHeader("DEMO 7: Interfaces");
            
            // Interface as type
            IPlayable musicPlayer = new MusicPlayer();
            IPlayable videoPlayer = new VideoPlayer();
            
            Console.WriteLine("  Interface defines contract - classes implement:");
            
            Console.WriteLine("\n  MusicPlayer:");
            musicPlayer.Play();
            musicPlayer.Pause();
            musicPlayer.Stop();
            
            Console.WriteLine("\n  VideoPlayer:");
            videoPlayer.Play();
            videoPlayer.Pause();
            videoPlayer.Stop();
            
            Console.WriteLine("\n  Both implement IPlayable but with different behavior");
            
            PrintFooter();
        }
        
        // ============================================================
        // DEMO 8: Multiple Interface Implementation
        // ============================================================
        static void Demo8_MultipleInterfaces()
        {
            PrintHeader("DEMO 8: Multiple Interface Implementation");
            
            var duck = new Duck { Name = "Donald" };
            
            Console.WriteLine($"  Duck '{duck.Name}' can do multiple things:");
            
            // Use as different interface types
            IFlyable flyable = duck;
            ISwimmable swimmable = duck;
            IWalkable walkable = duck;
            
            flyable.Fly();
            swimmable.Swim();
            walkable.Walk();
            
            Console.WriteLine("\n  C# allows multiple interface inheritance!");
            Console.WriteLine("  class Duck : IFlyable, ISwimmable, IWalkable");
            
            PrintFooter();
        }
        
        // ============================================================
        // DEMO 9: Explicit Interface Implementation
        // ============================================================
        static void Demo9_ExplicitInterface()
        {
            PrintHeader("DEMO 9: Explicit Interface Implementation");
            
            var device = new MultiFunctionDevice();
            
            Console.WriteLine("  When two interfaces have same method signature:");
            
            Console.WriteLine("\n  Calling through class reference:");
            device.Print();  // Default implementation
            
            Console.WriteLine("\n  Calling through IPrinter reference:");
            IPrinter printer = device;
            printer.Print();  // IPrinter-specific implementation
            
            Console.WriteLine("\n  Calling through IScanner reference:");
            IScanner scanner = device;
            scanner.Print();  // IScanner-specific implementation
            
            Console.WriteLine("\n  Explicit implementation resolves ambiguity!");
            
            PrintFooter();
        }
        
        // ============================================================
        // DEMO 10: Abstract Class vs Interface
        // ============================================================
        static void Demo10_AbstractVsInterface()
        {
            PrintHeader("DEMO 10: Abstract Class vs Interface");
            
            Console.WriteLine("  ┌────────────────────────────────────────────────────────┐");
            Console.WriteLine("  │ Feature            │ Abstract Class │ Interface       │");
            Console.WriteLine("  ├────────────────────┼────────────────┼─────────────────┤");
            Console.WriteLine("  │ Multiple inherit   │ ❌ No          │ ✅ Yes          │");
            Console.WriteLine("  │ Fields             │ ✅ Yes         │ ❌ No           │");
            Console.WriteLine("  │ Constructors       │ ✅ Yes         │ ❌ No           │");
            Console.WriteLine("  │ Default impl       │ ✅ Yes         │ ✅ Yes (C# 8+)  │");
            Console.WriteLine("  │ Access modifiers   │ ✅ Any         │ ✅ Any (C# 8+)  │");
            Console.WriteLine("  │ Purpose            │ IS-A relation  │ CAN-DO ability  │");
            Console.WriteLine("  └────────────────────────────────────────────────────────┘");
            
            // Practical example
            var employee = new Employee(1, "John", "Engineering");
            employee.Work();
            employee.TakeBreak();
            
            var serialized = employee.Serialize();
            Console.WriteLine($"\n  Employee serialized: {serialized}");
            Console.WriteLine($"  Employee valid: {employee.IsValid()}");
            
            PrintFooter();
        }
        
        // ============================================================
        // DEMO 11: Method Hiding (new keyword)
        // ============================================================
        static void Demo11_MethodHiding()
        {
            PrintHeader("DEMO 11: Method Hiding (new keyword)");
            
            Parent parent = new Parent();
            Parent childAsParent = new Child();  // Parent reference, Child object
            Child child = new Child();
            
            Console.WriteLine("  Without virtual/override (using 'new'):");
            Console.WriteLine($"    parent.Display(): ");
            parent.Display();
            Console.WriteLine($"    childAsParent.Display(): ");
            childAsParent.Display();  // Calls Parent.Display!
            Console.WriteLine($"    child.Display(): ");
            child.Display();
            
            Console.WriteLine("\n  ⚠️  Notice: childAsParent calls Parent.Display!");
            Console.WriteLine("  'new' HIDES the method, doesn't override it");
            Console.WriteLine("  Reference type determines which method is called");
            
            Console.WriteLine("\n  Compare with virtual/override:");
            Parent virtualChild = new VirtualChild();
            Console.WriteLine($"    virtualChild.VirtualDisplay(): ");
            virtualChild.VirtualDisplay();  // Calls Child's version!
            
            PrintFooter();
        }
        
        // ============================================================
        // DEMO 12: Sealed Classes and Methods
        // ============================================================
        static void Demo12_Sealed()
        {
            PrintHeader("DEMO 12: Sealed Classes and Methods");
            
            // Sealed class example
            var final = new FinalClass();
            final.DoSomething();
            
            Console.WriteLine("  FinalClass is sealed - cannot be inherited");
            Console.WriteLine("  // class Derived : FinalClass { }  // ERROR!");
            
            // Sealed method example
            var level3 = new Level3();
            level3.Method();
            
            Console.WriteLine("\n  Level2.Method() is sealed override");
            Console.WriteLine("  Level3 cannot override Method() anymore");
            
            Console.WriteLine("\n  Uses of sealed:");
            Console.WriteLine("  • Security: Prevent tampering");
            Console.WriteLine("  • Performance: JIT optimization");
            Console.WriteLine("  • Design: Class not meant for extension");
            
            PrintFooter();
        }
        
        // ============================================================
        // DEMO 13: Static Members
        // ============================================================
        static void Demo13_Static()
        {
            PrintHeader("DEMO 13: Static Members");
            
            Console.WriteLine("  Creating instances of Counter:");
            var c1 = new Counter("First");
            var c2 = new Counter("Second");
            var c3 = new Counter("Third");
            
            Console.WriteLine($"\n  Total instances created: {Counter.TotalCount}");
            Console.WriteLine($"  Each instance has unique ID: {c1.InstanceId}, {c2.InstanceId}, {c3.InstanceId}");
            
            Console.WriteLine("\n  Static vs Instance:");
            Console.WriteLine("  • Static: Shared across ALL instances");
            Console.WriteLine("  • Instance: Unique to each object");
            
            // Static class
            Console.WriteLine($"\n  Static class MathHelper.Square(5) = {MathHelper.Square(5)}");
            Console.WriteLine($"  Static class MathHelper.CircleArea(3) = {MathHelper.CircleArea(3):F2}");
            
            PrintFooter();
        }
        
        // ============================================================
        // DEMO 14: readonly vs const
        // ============================================================
        static void Demo14_ReadonlyVsConst()
        {
            PrintHeader("DEMO 14: readonly vs const");
            
            Console.WriteLine("  const values (compile-time):");
            Console.WriteLine($"    MathConstants.Pi = {MathConstants.Pi}");
            Console.WriteLine($"    MathConstants.E = {MathConstants.E}");
            
            Console.WriteLine("\n  readonly values (runtime):");
            var config1 = new AppConfiguration("Connection1");
            var config2 = new AppConfiguration("Connection2");
            Console.WriteLine($"    config1.ConnectionString = {config1.ConnectionString}");
            Console.WriteLine($"    config2.ConnectionString = {config2.ConnectionString}");
            Console.WriteLine($"    AppConfiguration.StartTime = {AppConfiguration.StartTime}");
            
            Console.WriteLine("\n  Key differences:");
            Console.WriteLine("  ┌──────────────────────────────────────────────────────┐");
            Console.WriteLine("  │ const              │ readonly                        │");
            Console.WriteLine("  ├────────────────────┼─────────────────────────────────┤");
            Console.WriteLine("  │ Compile-time only  │ Runtime values OK               │");
            Console.WriteLine("  │ Implicitly static  │ Can be instance or static       │");
            Console.WriteLine("  │ Inlined at use     │ Stored in memory                │");
            Console.WriteLine("  │ Change = recompile │ Change = just redeploy          │");
            Console.WriteLine("  └──────────────────────────────────────────────────────┘");
            
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
            Console.WriteLine("─────────────────────────────────────────────────────────────────");
        }
    }
    
    // ============================================================
    // SUPPORTING CLASSES
    // ============================================================
    
    // --- Demo 1: Encapsulation ---
    public class BankAccount
    {
        private decimal _balance;  // Private field - encapsulated
        
        public string Owner { get; }
        public decimal Balance => _balance;  // Read-only property
        
        public BankAccount(string owner)
        {
            Owner = owner;
            _balance = 0;
        }
        
        public void Deposit(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive");
            _balance += amount;
        }
        
        public void Withdraw(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive");
            if (amount > _balance)
                throw new InvalidOperationException("Insufficient funds");
            _balance -= amount;
        }
    }
    
    // --- Demo 2 & 5: Inheritance ---
    public class Animal
    {
        public string Name { get; set; }
        
        public void Eat() => Console.WriteLine($"    {Name} is eating");
        public void Sleep() => Console.WriteLine($"    {Name} is sleeping");
        
        public virtual void Speak()
        {
            Console.WriteLine($"    {Name} makes a sound");
        }
    }
    
    public class Dog : Animal
    {
        public string Breed { get; set; }
        
        public void Bark() => Console.WriteLine($"    {Name} barks: Woof!");
        
        public override void Speak()
        {
            Console.WriteLine($"    {Name} says: Woof woof!");
        }
    }
    
    public class Cat : Animal
    {
        public void Meow() => Console.WriteLine($"    {Name} meows");
        
        public override void Speak()
        {
            Console.WriteLine($"    {Name} says: Meow!");
        }
    }
    
    public class Bulldog : Dog
    {
        public override void Speak()
        {
            base.Speak();  // Call base implementation first
            Console.WriteLine($"    {Name} also growls!");
        }
    }
    
    // --- Demo 3: Method Overloading ---
    public class Calculator
    {
        public int Add(int a, int b) => a + b;
        public int Add(int a, int b, int c) => a + b + c;
        public double Add(double a, double b) => a + b;
        public string Add(string a, string b) => a + " " + b;
    }
    
    // --- Demo 4: Polymorphism ---
    public abstract class Shape
    {
        public abstract double GetArea();
    }
    
    public class Circle : Shape
    {
        public double Radius { get; set; }
        public override double GetArea() => Math.PI * Radius * Radius;
    }
    
    public class Rectangle : Shape
    {
        public double Width { get; set; }
        public double Height { get; set; }
        public override double GetArea() => Width * Height;
    }
    
    public class Triangle : Shape
    {
        public double Base { get; set; }
        public double Height { get; set; }
        public override double GetArea() => 0.5 * Base * Height;
    }
    
    // --- Demo 6: Abstract Classes ---
    public abstract class Vehicle
    {
        protected string Brand { get; }
        protected string Model { get; }
        protected int MaxSpeed { get; }
        
        protected Vehicle(string brand, string model, int maxSpeed)
        {
            Brand = brand;
            Model = model;
            MaxSpeed = maxSpeed;
        }
        
        // Abstract method - must be implemented
        public abstract void Start();
        
        // Virtual method - can be overridden
        public virtual void Accelerate()
        {
            Console.WriteLine($"    Accelerating up to {MaxSpeed} km/h");
        }
        
        // Concrete method - shared implementation
        public void DisplayInfo()
        {
            Console.WriteLine($"    {Brand} {Model} (Max: {MaxSpeed} km/h)");
        }
    }
    
    public class Car : Vehicle
    {
        public Car(string brand, string model, int maxSpeed) 
            : base(brand, model, maxSpeed) { }
        
        public override void Start()
        {
            Console.WriteLine("    Car engine starting... vroom!");
        }
        
        public override void Accelerate()
        {
            Console.WriteLine($"    Car smoothly accelerating to {MaxSpeed} km/h");
        }
    }
    
    public class Motorcycle : Vehicle
    {
        public Motorcycle(string brand, string model, int maxSpeed) 
            : base(brand, model, maxSpeed) { }
        
        public override void Start()
        {
            Console.WriteLine("    Motorcycle engine revving!");
        }
        // Uses default Accelerate from Vehicle
    }
    
    // --- Demo 7: Interfaces ---
    public interface IPlayable
    {
        void Play();
        void Pause();
        void Stop();
    }
    
    public class MusicPlayer : IPlayable
    {
        public void Play() => Console.WriteLine("    ♪ Playing music...");
        public void Pause() => Console.WriteLine("    ♪ Music paused");
        public void Stop() => Console.WriteLine("    ♪ Music stopped");
    }
    
    public class VideoPlayer : IPlayable
    {
        public void Play() => Console.WriteLine("    ▶ Playing video...");
        public void Pause() => Console.WriteLine("    ⏸ Video paused");
        public void Stop() => Console.WriteLine("    ⏹ Video stopped");
    }
    
    // --- Demo 8: Multiple Interfaces ---
    public interface IFlyable { void Fly(); }
    public interface ISwimmable { void Swim(); }
    public interface IWalkable { void Walk(); }
    
    public class Duck : IFlyable, ISwimmable, IWalkable
    {
        public string Name { get; set; }
        
        public void Fly() => Console.WriteLine($"    {Name} is flying through the air!");
        public void Swim() => Console.WriteLine($"    {Name} is swimming in the pond!");
        public void Walk() => Console.WriteLine($"    {Name} is waddling around!");
    }
    
    // --- Demo 9: Explicit Interface ---
    public interface IPrinter
    {
        void Print();
    }
    
    public interface IScanner
    {
        void Print();  // Same signature as IPrinter!
    }
    
    public class MultiFunctionDevice : IPrinter, IScanner
    {
        // Default implementation
        public void Print()
        {
            Console.WriteLine("    Default print operation");
        }
        
        // Explicit IPrinter implementation
        void IPrinter.Print()
        {
            Console.WriteLine("    [IPrinter] Printing document to paper...");
        }
        
        // Explicit IScanner implementation
        void IScanner.Print()
        {
            Console.WriteLine("    [IScanner] Printing scan preview...");
        }
    }
    
    // --- Demo 10: Abstract vs Interface ---
    public interface ISerializable
    {
        string Serialize();
    }
    
    public interface IValidatable
    {
        bool IsValid();
    }
    
    public abstract class Entity
    {
        public int Id { get; }
        public string Name { get; set; }
        
        protected Entity(int id, string name)
        {
            Id = id;
            Name = name;
        }
        
        public abstract void Work();
    }
    
    public class Employee : Entity, ISerializable, IValidatable
    {
        public string Department { get; set; }
        
        public Employee(int id, string name, string department) 
            : base(id, name)
        {
            Department = department;
        }
        
        public override void Work()
        {
            Console.WriteLine($"    {Name} is working in {Department}");
        }
        
        public void TakeBreak()
        {
            Console.WriteLine($"    {Name} is taking a break");
        }
        
        public string Serialize() => $"{{Id:{Id}, Name:{Name}, Dept:{Department}}}";
        public bool IsValid() => Id > 0 && !string.IsNullOrEmpty(Name);
    }
    
    // --- Demo 11: Method Hiding ---
    public class Parent
    {
        public void Display()
        {
            Console.WriteLine("    Parent.Display()");
        }
        
        public virtual void VirtualDisplay()
        {
            Console.WriteLine("    Parent.VirtualDisplay()");
        }
    }
    
    public class Child : Parent
    {
        // 'new' hides the base method - NOT polymorphic!
        public new void Display()
        {
            Console.WriteLine("    Child.Display()");
        }
    }
    
    public class VirtualChild : Parent
    {
        // 'override' - polymorphic behavior
        public override void VirtualDisplay()
        {
            Console.WriteLine("    VirtualChild.VirtualDisplay()");
        }
    }
    
    // --- Demo 12: Sealed ---
    public sealed class FinalClass
    {
        public void DoSomething()
        {
            Console.WriteLine("    FinalClass doing something");
        }
    }
    // Cannot inherit: class Derived : FinalClass { }
    
    public class Level1
    {
        public virtual void Method()
        {
            Console.WriteLine("    Level1.Method()");
        }
    }
    
    public class Level2 : Level1
    {
        public sealed override void Method()  // Sealed here
        {
            Console.WriteLine("    Level2.Method() - sealed, cannot be overridden further");
        }
    }
    
    public class Level3 : Level2
    {
        // Cannot override: public override void Method() { } - ERROR!
    }
    
    // --- Demo 13: Static ---
    public class Counter
    {
        private static int _count = 0;  // Shared across all instances
        
        public static int TotalCount => _count;
        
        public int InstanceId { get; }  // Unique to each instance
        public string Name { get; }
        
        public Counter(string name)
        {
            _count++;
            InstanceId = _count;
            Name = name;
            Console.WriteLine($"    Created {Name} with ID {InstanceId}");
        }
        
        public static void ResetCount() => _count = 0;
    }
    
    public static class MathHelper
    {
        public static double Pi => 3.14159;
        
        public static int Square(int x) => x * x;
        
        public static double CircleArea(double radius) => Pi * radius * radius;
    }
    
    // --- Demo 14: readonly vs const ---
    public class MathConstants
    {
        public const double Pi = 3.14159265359;
        public const double E = 2.71828182845;
        // const is implicitly static
    }
    
    public class AppConfiguration
    {
        // readonly can be set at declaration or in constructor
        public readonly string ConnectionString;
        
        // static readonly - like const but for runtime values
        public static readonly DateTime StartTime = DateTime.Now;
        
        public AppConfiguration(string connectionString)
        {
            ConnectionString = connectionString;  // Set in constructor
        }
        
        public void SomeMethod()
        {
            // Cannot modify after construction:
            // ConnectionString = "new";  // ERROR!
        }
    }
}
