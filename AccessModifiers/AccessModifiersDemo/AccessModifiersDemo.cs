// AccessModifiersDemo.cs
// Demonstrates all C# access modifiers with explanations and examples

using System;

namespace AccessModifiersDemo
{
    // Public class: accessible from anywhere
    public class PublicClass
    {
        public void Show() => Console.WriteLine("PublicClass: Accessible from anywhere");
    }

    // Internal class: accessible only within the same assembly
    internal class InternalClass
    {
        public void Show() => Console.WriteLine("InternalClass: Accessible within the same assembly");
    }

    public class ModifierExamples
    {
        // Public: accessible from anywhere
        public string PublicField = "Public field";

        // Private: accessible only within this class
        private string PrivateField = "Private field";

        // Protected: accessible in this class and derived classes
        protected string ProtectedField = "Protected field";

        // Internal: accessible within the same assembly
        internal string InternalField = "Internal field";

        // Protected Internal: accessible in derived classes or same assembly
        protected internal string ProtectedInternalField = "Protected Internal field";

        // Private Protected: accessible in derived classes within the same assembly
        private protected string PrivateProtectedField = "Private Protected field";

        public void ShowFields()
        {
            Console.WriteLine(PublicField);
            Console.WriteLine(PrivateField);
            Console.WriteLine(ProtectedField);
            Console.WriteLine(InternalField);
            Console.WriteLine(ProtectedInternalField);
            Console.WriteLine(PrivateProtectedField);
        }
    }

    // Derived class to show protected, protected internal, and private protected
    public class Derived : ModifierExamples
    {
        public void ShowInheritedFields()
        {
            Console.WriteLine(PublicField); // OK
            // Console.WriteLine(PrivateField); // Error: not accessible
            Console.WriteLine(ProtectedField); // OK
            Console.WriteLine(InternalField); // OK (same assembly)
            Console.WriteLine(ProtectedInternalField); // OK
            Console.WriteLine(PrivateProtectedField); // OK (same assembly)
        }
    }

}

/*
Access Modifiers Summary:
- public: Accessible from anywhere.
- private: Accessible only within the containing class.
- protected: Accessible within the containing class and derived classes.
- internal: Accessible within the same assembly/project.
- protected internal: Accessible within the same assembly OR from derived classes.
- private protected: Accessible within the same assembly AND from derived classes.
*/
