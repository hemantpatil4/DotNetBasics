using System;

namespace demo
{

    public class Student
    {
        internal int Id { get; set; }
        internal string Name { get; set; }

        public string ToString()
        {
            return $"Id is {this.Id} and Name is {this.Name}.";
        }
        
    }
    public class PracticeRefInOut
    {

        public static Student  ModifyStudent( Student s)
        {
            s = new Student { Id = 0, Name = "Prasad" };
            return s;
        }
        public static void Main()
        {
            Student s = new Student { Id = 0, Name = "Hemant" };
            Console.WriteLine(s.Name);
            Console.WriteLine(ModifyStudent( s).ToString());
            Console.WriteLine(s.Name);
        }
    }
}