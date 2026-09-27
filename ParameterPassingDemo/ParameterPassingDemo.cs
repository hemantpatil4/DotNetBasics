// ParameterPassingDemo.cs
// Comprehensive runnable examples for C# parameter passing mechanisms
// Run with: dotnet run

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ParameterPassingDemo
{
    #region Basic Classes and Structs
    
    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public override string ToString() => $"Person({Name}, {Age})";
    }

    public struct Point
    {
        public int X;
        public int Y;
        public Point(int x, int y) { X = x; Y = y; }
        public override string ToString() => $"({X}, {Y})";
    }

    // Large struct for performance demos (80 bytes)
    public struct LargeStruct
    {
        public long A, B, C, D, E, F, G, H;  // 64 bytes
        public decimal Value;                 // 16 bytes
        
        public LargeStruct(long val)
        {
            A = B = C = D = E = F = G = H = val;
            Value = val;
        }
    }

    // Readonly struct (avoids defensive copies)
    public readonly struct ImmutablePoint
    {
        public readonly int X;
        public readonly int Y;
        
        public ImmutablePoint(int x, int y) { X = x; Y = y; }
        public double Distance => Math.Sqrt(X * X + Y * Y);
        public override string ToString() => $"({X}, {Y})";
    }

    #endregion

    #region 1. Value Type Examples
    
    public static class ValueTypeExamples
    {
        static void PassByValue(int x)
        {
            x = 999;
            Console.WriteLine($"  Inside PassByValue: x = {x}");
        }

        static void PassByRef(ref int x)
        {
            x = 999;
            Console.WriteLine($"  Inside PassByRef: x = {x}");
        }

        static void PassByOut(out int x)
        {
            x = 999;
            Console.WriteLine($"  Inside PassByOut: x = {x}");
        }

        static void PassByIn(in int x)
        {
            // x = 999; // ERROR: Cannot assign to 'in' parameter
            Console.WriteLine($"  Inside PassByIn: x = {x}");
        }

        public static void Demo()
        {
            Console.WriteLine("=== VALUE TYPE EXAMPLES ===\n");
            
            int num = 10;
            Console.WriteLine($"Original: {num}");
            
            PassByValue(num);
            Console.WriteLine($"After PassByValue: {num}\n");
            
            PassByRef(ref num);
            Console.WriteLine($"After PassByRef: {num}\n");
            
            num = 10; // Reset
            PassByOut(out num);
            Console.WriteLine($"After PassByOut: {num}\n");
            
            num = 10; // Reset
            PassByIn(in num);
            Console.WriteLine($"After PassByIn: {num}\n");
        }
    }

    #endregion

    #region 2. Reference Type Examples
    
    public static class ReferenceTypeExamples
    {
        // Modify object through reference (works!)
        static void ModifyObject(Person p)
        {
            p.Name = "Modified";
            p.Age = 99;
            Console.WriteLine($"  Inside ModifyObject: {p}");
        }

        // Try to replace object (doesn't work without ref!)
        static void ReplaceObject(Person p)
        {
            p = new Person { Name = "Replaced", Age = 0 };
            Console.WriteLine($"  Inside ReplaceObject: {p}");
        }

        // Replace object WITH ref (works!)
        static void ReplaceObjectRef(ref Person p)
        {
            p = new Person { Name = "Replaced", Age = 0 };
            Console.WriteLine($"  Inside ReplaceObjectRef: {p}");
        }

        // Out parameter with reference type
        static void CreatePerson(out Person p)
        {
            p = new Person { Name = "Created", Age = 25 };
            Console.WriteLine($"  Inside CreatePerson: {p}");
        }

        public static void Demo()
        {
            Console.WriteLine("=== REFERENCE TYPE EXAMPLES ===\n");
            
            Person person = new Person { Name = "Original", Age = 30 };
            Console.WriteLine($"Original: {person}");

            ModifyObject(person);
            Console.WriteLine($"After ModifyObject: {person}");
            Console.WriteLine("  ^ Object WAS modified!\n");

            person = new Person { Name = "Original", Age = 30 }; // Reset
            ReplaceObject(person);
            Console.WriteLine($"After ReplaceObject: {person}");
            Console.WriteLine("  ^ Still original! Replacement was local only.\n");

            ReplaceObjectRef(ref person);
            Console.WriteLine($"After ReplaceObjectRef: {person}");
            Console.WriteLine("  ^ NOW points to new object!\n");

            Person newPerson;
            CreatePerson(out newPerson);
            Console.WriteLine($"After CreatePerson: {newPerson}\n");
        }
    }

    #endregion

    #region 3. Struct Examples
    
    public static class StructExamples
    {
        static void ModifyStruct(Point p)
        {
            p.X = 999;
            p.Y = 999;
            Console.WriteLine($"  Inside ModifyStruct: {p}");
        }

        static void ModifyStructRef(ref Point p)
        {
            p.X = 999;
            p.Y = 999;
            Console.WriteLine($"  Inside ModifyStructRef: {p}");
        }

        static void ReadStructIn(in Point p)
        {
            // p.X = 999; // ERROR: Cannot modify 'in' parameter
            Console.WriteLine($"  Inside ReadStructIn: {p}");
        }

        public static void Demo()
        {
            Console.WriteLine("=== STRUCT EXAMPLES ===\n");
            
            Point point = new Point(10, 20);
            Console.WriteLine($"Original: {point}");

            ModifyStruct(point);
            Console.WriteLine($"After ModifyStruct: {point}");
            Console.WriteLine("  ^ UNCHANGED! Copy was modified.\n");

            ModifyStructRef(ref point);
            Console.WriteLine($"After ModifyStructRef: {point}");
            Console.WriteLine("  ^ MODIFIED via ref!\n");

            point = new Point(10, 20); // Reset
            ReadStructIn(in point);
            Console.WriteLine($"After ReadStructIn: {point}");
            Console.WriteLine("  ^ Read-only access, no modification.\n");
        }
    }

    #endregion

    #region 4. String Examples (Immutable Reference Type)
    
    public static class StringExamples
    {
        static void ModifyString(string s)
        {
            s = "Modified";
            Console.WriteLine($"  Inside ModifyString: {s}");
        }

        static void ModifyStringRef(ref string s)
        {
            s = "Modified";
            Console.WriteLine($"  Inside ModifyStringRef: {s}");
        }

        static void ConcatenateString(string s)
        {
            s += " World";
            Console.WriteLine($"  Inside ConcatenateString: {s}");
        }

        public static void Demo()
        {
            Console.WriteLine("=== STRING EXAMPLES (Immutable Reference Type) ===\n");
            
            string str = "Hello";
            Console.WriteLine($"Original: {str}");

            ModifyString(str);
            Console.WriteLine($"After ModifyString: {str}");
            Console.WriteLine("  ^ UNCHANGED! String is immutable, new object created locally.\n");

            ModifyStringRef(ref str);
            Console.WriteLine($"After ModifyStringRef: {str}");
            Console.WriteLine("  ^ CHANGED via ref!\n");

            str = "Hello"; // Reset
            ConcatenateString(str);
            Console.WriteLine($"After ConcatenateString: {str}");
            Console.WriteLine("  ^ UNCHANGED! New string created inside method.\n");
        }
    }

    #endregion

    #region 5. Edge Cases
    
    public static class EdgeCases
    {
        // Edge Case 1: Reassigning reference types
        static void Reassign(StringBuilder sb)
        {
            sb.Append(" World");  // Modifies original object
            Console.WriteLine($"  After Append: {sb}");
            
            sb = new StringBuilder("Completely New");  // Local reassignment only
            sb.Append("!!!");
            Console.WriteLine($"  After reassignment: {sb}");
        }

        // Edge Case 2: Modify vs Replace with List
        static void ModifyVsReplace(List<int> list)
        {
            list.Add(100);
            list.Add(200);
            Console.WriteLine($"  After Add: [{string.Join(", ", list)}]");
            
            list = new List<int> { 999 };
            list.Add(888);
            Console.WriteLine($"  After replace (local): [{string.Join(", ", list)}]");
        }

        // Edge Case 3: Boxing
        static void ModifyBoxed(object obj)
        {
            if (obj is int i)
            {
                i = 999;
                Console.WriteLine($"  Inside (unboxed copy): {i}");
            }
        }

        static void ModifyBoxedRef(ref object obj)
        {
            obj = 999;
            Console.WriteLine($"  Inside (new boxed value): {obj}");
        }

        public static void Demo()
        {
            Console.WriteLine("=== EDGE CASES ===\n");
            
            // Edge Case 1
            Console.WriteLine("Edge Case 1: Reassigning StringBuilder");
            StringBuilder sb = new StringBuilder("Hello");
            Console.WriteLine($"Original: {sb}");
            Reassign(sb);
            Console.WriteLine($"After Reassign: {sb}");
            Console.WriteLine("  ^ 'Hello World' - Append worked, but reassignment was local.\n");

            // Edge Case 2
            Console.WriteLine("Edge Case 2: List Add vs Replace");
            List<int> myList = new List<int> { 1, 2, 3 };
            Console.WriteLine($"Original: [{string.Join(", ", myList)}]");
            ModifyVsReplace(myList);
            Console.WriteLine($"After ModifyVsReplace: [{string.Join(", ", myList)}]");
            Console.WriteLine("  ^ Add worked, but replacement was local.\n");

            // Edge Case 3
            Console.WriteLine("Edge Case 3: Boxing");
            int num = 10;
            object boxed = num;
            Console.WriteLine($"Original boxed: {boxed}");
            ModifyBoxed(boxed);
            Console.WriteLine($"After ModifyBoxed: {boxed}");
            Console.WriteLine("  ^ Still 10 - unboxed copy was modified.\n");
            
            ModifyBoxedRef(ref boxed);
            Console.WriteLine($"After ModifyBoxedRef: {boxed}");
            Console.WriteLine("  ^ Now 999 - new boxed value via ref.\n");
        }
    }

    #endregion

    #region 6. Advanced: ref return and ref local
    
    public static class AdvancedRefExamples
    {
        private static int[] _array = { 1, 2, 3, 4, 5 };

        // ref return
        public static ref int GetElementRef(int index)
        {
            return ref _array[index];
        }

        // ref readonly return
        public static ref readonly int GetElementReadOnly(int index)
        {
            return ref _array[index];
        }

        public static void Demo()
        {
            Console.WriteLine("=== ADVANCED: ref return & ref local ===\n");
            
            Console.WriteLine($"Original array: [{string.Join(", ", _array)}]");
            
            // ref local
            ref int element = ref GetElementRef(2);
            Console.WriteLine($"Got ref to element[2]: {element}");
            
            element = 999;
            Console.WriteLine($"After modifying through ref: [{string.Join(", ", _array)}]");
            Console.WriteLine("  ^ Element at index 2 changed directly!\n");
            
            // ref readonly
            ref readonly int readOnlyRef = ref GetElementReadOnly(0);
            Console.WriteLine($"ReadOnly ref to element[0]: {readOnlyRef}");
            // readOnlyRef = 100; // ERROR: Cannot assign to readonly reference
            Console.WriteLine("  ^ Can read but cannot modify.\n");
            
            // Reset for other demos
            _array = new[] { 1, 2, 3, 4, 5 };
        }
    }

    #endregion

    #region 7. Performance: Large Struct with 'in'
    
    public static class LargeStructPerformance
    {
        // BAD: Copies 80 bytes every call
        static decimal ProcessByValue(LargeStruct data)
        {
            return data.Value * 2;
        }

        // GOOD: Passes 8-byte pointer, no copy
        static decimal ProcessByIn(in LargeStruct data)
        {
            return data.Value * 2;
        }

        // When modification needed
        static void ModifyByRef(ref LargeStruct data)
        {
            data.Value *= 2;
        }

        public static void Demo()
        {
            Console.WriteLine("=== LARGE STRUCT PERFORMANCE ===\n");
            
            LargeStruct data = new LargeStruct(100);
            Console.WriteLine($"Struct size: {Marshal.SizeOf<LargeStruct>()} bytes");
            Console.WriteLine($"Pointer size: {IntPtr.Size} bytes");
            
            var result1 = ProcessByValue(data);
            Console.WriteLine($"ProcessByValue result: {result1} (copied {Marshal.SizeOf<LargeStruct>()} bytes)");
            
            var result2 = ProcessByIn(in data);
            Console.WriteLine($"ProcessByIn result: {result2} (passed {IntPtr.Size}-byte pointer)");
            
            Console.WriteLine($"\nOriginal Value: {data.Value}");
            ModifyByRef(ref data);
            Console.WriteLine($"After ModifyByRef: {data.Value}");
            Console.WriteLine("  ^ Modified in place via ref.\n");
        }
    }

    #endregion

    #region 8. TryParse Pattern (out usage)
    
    public static class TryParsePatternDemo
    {
        public static bool TryParsePoint(string input, out Point result)
        {
            result = default;
            
            if (string.IsNullOrEmpty(input))
                return false;
                
            var parts = input.Split(',');
            if (parts.Length != 2)
                return false;
                
            if (int.TryParse(parts[0].Trim(), out int x) &&
                int.TryParse(parts[1].Trim(), out int y))
            {
                result = new Point(x, y);
                return true;
            }
            
            return false;
        }

        public static void Demo()
        {
            Console.WriteLine("=== TRY-PARSE PATTERN (out usage) ===\n");
            
            string[] inputs = { "10, 20", "invalid", "5,5", "", "1,2,3" };
            
            foreach (var input in inputs)
            {
                if (TryParsePoint(input, out Point point))
                {
                    Console.WriteLine($"Parsed '{input}' -> {point}");
                }
                else
                {
                    Console.WriteLine($"Failed to parse '{input}'");
                }
            }
            Console.WriteLine();
        }
    }

    #endregion

    #region 9. Swap Example
    
    public static class SwapDemo
    {
        // WRONG: Only swaps local copies
        static void BadSwap(int a, int b)
        {
            int temp = a;
            a = b;
            b = temp;
        }

        // CORRECT: Swaps original values
        static void GoodSwap(ref int a, ref int b)
        {
            int temp = a;
            a = b;
            b = temp;
        }

        // Generic swap
        static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }

        public static void Demo()
        {
            Console.WriteLine("=== SWAP EXAMPLE ===\n");
            
            int x = 10, y = 20;
            Console.WriteLine($"Before BadSwap: x={x}, y={y}");
            BadSwap(x, y);
            Console.WriteLine($"After BadSwap: x={x}, y={y}");
            Console.WriteLine("  ^ No change! Local copies swapped.\n");
            
            Console.WriteLine($"Before GoodSwap: x={x}, y={y}");
            GoodSwap(ref x, ref y);
            Console.WriteLine($"After GoodSwap: x={x}, y={y}");
            Console.WriteLine("  ^ Swapped successfully!\n");
            
            string s1 = "Hello", s2 = "World";
            Console.WriteLine($"Before generic Swap: s1={s1}, s2={s2}");
            Swap(ref s1, ref s2);
            Console.WriteLine($"After generic Swap: s1={s1}, s2={s2}\n");
        }
    }

    #endregion

    #region Program Entry Point

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║        C# PARAMETER PASSING DEMO - COMPREHENSIVE             ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");

            ValueTypeExamples.Demo();
            Console.WriteLine(new string('-', 60) + "\n");

            ReferenceTypeExamples.Demo();
            Console.WriteLine(new string('-', 60) + "\n");

            StructExamples.Demo();
            Console.WriteLine(new string('-', 60) + "\n");

            StringExamples.Demo();
            Console.WriteLine(new string('-', 60) + "\n");

            EdgeCases.Demo();
            Console.WriteLine(new string('-', 60) + "\n");

            AdvancedRefExamples.Demo();
            Console.WriteLine(new string('-', 60) + "\n");

            LargeStructPerformance.Demo();
            Console.WriteLine(new string('-', 60) + "\n");

            TryParsePatternDemo.Demo();
            Console.WriteLine(new string('-', 60) + "\n");

            SwapDemo.Demo();

            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    DEMO COMPLETED                            ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        }
    }

    #endregion
}
