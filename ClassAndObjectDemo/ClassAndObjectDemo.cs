// ClassAndObjectDemo.cs
// Demonstrates fields, constructors, getter/setter, base/derived constructors in C#

using System;

namespace ClassAndObjectDemo
{
    // Base class (superclass)
    public class Animal
    {
        // Field
        private string name;

        // Property (getter/setter)
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        // Constructor

        public Animal(){
            Console.WriteLine("Inside def ctor of base class");
        }
        public Animal(string name)
        {
            this.name = name;
            Console.WriteLine($"Animal constructor: {name}");
        }
    }

    // Derived class (subclass)
    public class Dog : Animal
    {
        public string Breed { get; set; }

        // Default constructor chaining to base default constructor
        public Dog() : base()
        {
            Console.WriteLine("Inside def ctor of Dog");
        }

        // Constructor chaining to base class
        public Dog(string name, string breed) : base(name)
        {
            Breed = breed;
            Console.WriteLine($"Dog constructor: {breed}");
        }

        // Constructor chaining within the derived class
        public Dog(string breed) : this("Unnamed", breed)
        {
            Console.WriteLine("Dog ctor with only breed called, chained to Dog(name, breed)");
        }
    }

    public class Program
    {
        public static void Main()
        {
            // // Object creation using constructor
            // Animal animal = new Animal("Generic Animal");
            // Console.WriteLine($"Animal Name: {animal.Name}");

            // // Object creation using derived class
            // Dog dog = new Dog("Buddy", "Golden Retriever");
            // Console.WriteLine($"Dog Name: {dog.Name}, Breed: {dog.Breed}");

            // // Using setter
            // dog.Name = "Max";
            // Console.WriteLine($"Dog New Name: {dog.Name}");

            // // Demonstrate default constructor chaining
            // Dog dog2 = new Dog();
            // Demonstrate derived class chaining
            Dog dog3 = new Dog("Beagle");
            Console.WriteLine($"Dog3 Name: {dog3.Name}, Breed: {dog3.Breed}");
        }
    }
}

/*
Key Points:
- Fields store data inside a class.
- Properties (getter/setter) provide controlled access to fields.
- Constructors initialize objects; derived constructors can call base constructors.
- Object creation triggers constructor calls (base first, then derived).
*/
