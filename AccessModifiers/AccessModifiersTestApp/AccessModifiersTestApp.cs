// Program.cs
// This file demonstrates which access modifiers are accessible from another assembly.

using System;
using AccessModifiersDemo; // Reference to the original assembly (add project reference!)

namespace AccessModifiersTestApp
{
    class Program
    {
        static void Main()
        {
            var mod = new ModifierExamples();
            Console.WriteLine(mod.PublicField); // OK: public
            // Console.WriteLine(mod.PrivateField); // Error: private, not accessible
            Console.WriteLine(mod.ProtectedField); // Error: protected, not accessible
            Console.WriteLine(mod.InternalField); // Error: internal, not accessible from another assembly
            Console.WriteLine(mod.ProtectedInternalField); // OK: protected internal (accessible from another assembly if derived, but not here)
            // Console.WriteLine(mod.PrivateProtectedField); // Error: private protected, not accessible
        }
    }
}

/*
Expected:
- Only public members are accessible directly.
- protected, private, internal, private protected are NOT accessible.
- protected internal is accessible ONLY if you inherit the class in the new assembly.
*/
