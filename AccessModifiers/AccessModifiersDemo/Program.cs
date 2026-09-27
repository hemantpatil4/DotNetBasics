// Demo runner

using AccessModifiersDemo;
namespace AccessModifiersDemo1
{
    public class Program
    {
        public static void Main()
        {
            var pub = new PublicClass();
            pub.Show();

            var internalClass = new InternalClass();
            internalClass.Show();

            var mod = new ModifierExamples();
            mod.ShowFields();

            var derived = new Derived();
            derived.ShowInheritedFields();
        }
    }
}