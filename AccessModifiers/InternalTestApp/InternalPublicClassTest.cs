// Program.cs
// This program tries to use InternalClass from another assembly
using System;
using AccessModifiersDemo;

namespace InternalTestApp
{
    class Program
    {
        public static void Main()
        {
            var publicClass = new PublicClass();
            //var internalClass = new InternalClass(); will failed due to different assembly
            //internalClass.Show();
        }
    }
}
