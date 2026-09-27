# C# Class and Object Demo

This demo covers:

- Fields and properties (getter/setter)
- Constructors (including base/derived chaining)
- How objects are created and initialized

## Key Concepts

- **Field**: Variable inside a class (usually private)
- **Property**: Public getter/setter for controlled access
- **Constructor**: Special method to initialize objects
- **Base/Derived Constructor**: Derived class can call base class constructor using `: base()`

## Example Structure

- `Animal`: Base class with field, property, and constructor
- `Dog`: Derived class with its own property and constructor chaining
- `Program`: Shows object creation and usage

## How to Run

```sh
dotnet run ClassAndObjectDemo/ClassAndObjectDemo.cs
```

## Output Example

```
Animal constructor: Generic Animal
Animal Name: Generic Animal
Animal constructor: Buddy
Dog constructor: Golden Retriever
Dog Name: Buddy, Breed: Golden Retriever
Dog New Name: Max
```

## Notes

- Constructors run base first, then derived.
- Properties are the C# way to do getter/setter.
- You can set properties after object creation.

---

Use this as a reference for interviews or further practice!
