using System.Collections;
using System.Text;

namespace practice1
{
    public class practice1
    {
        public static string ReverseString(string s)
        {
            char[] arr = s.ToCharArray();
            for (int i = 0; i < arr.Length / 2; i++)
            {
                char temp = arr[i];
                arr[i] = arr[arr.Length - 1 - i];
                arr[arr.Length - 1 - i] = temp;
            }
            return new String(arr);
        }

        public static string RemoveTheDuplicateCharacters(string s)
        {
            HashSet<char> seen = new();
            List<char> result = new();

            foreach (char c in s)
            {
                if (seen.Add(c))  // Add returns true if new
                    result.Add(c);
            }

            return new string(result.ToArray());
        }

        public static void Main()
        {
            //Reverse String
            //System.Console.WriteLine(ReverseString("patil"));

            //Remove The Duplicate Characters
            // foreach (var item in RemoveTheDuplicateCharacters("Programming"))
            // {
            //     System.Console.WriteLine(item);
            // }
            // string inputString = "killer";

            // // Count the Frequency of The words
            // int[] CharArray = new int[52];
            // System.Console.WriteLine();
            // for (int i = 0; i < inputString.Length; i++)
            // {
            //     CharArray[i] = CharArray[((int)inputString[i]) - 65]++;
            // }
            // for (int i = 0; i < CharArray.Length; i++)
            // {
            //     if (CharArray[i] != 0)
            //     {
            //         System.Console.WriteLine($"Character: {(char)(i + 65)} , Count: {CharArray[i]}");
            //     }
            // }

            int[] numsArray = { 0, 0, 0, 1, 1, 2, 2, 2, 3, 4, 5 };
            int current = 0;
            for (int i = 0; i < numsArray.Length; i++)
            {
                if (numsArray[current] == numsArray[i])
                {

                }
                else
                {
                    current++;
                    numsArray[current] = numsArray[i];
                }
            }
            for (int i = 0; i < numsArray.Length; i++)
            {
                System.Console.WriteLine(numsArray[i]);
            }

            string s = "  hello  world  ";
            StringSplitOptions opt = StringSplitOptions.RemoveEmptyEntries;
            string[] tenmp = s.Split(" ", opt);
            foreach (var item in tenmp)
            {
                System.Console.WriteLine(item);
            }
            string number = "43";

            int.TryParse(number, out int a);
            System.Console.WriteLine(a);

            char c = 'a';
            System.Console.WriteLine(c.ToString()); ;
            Dictionary<char, int> dict = new();
            dict.TryAdd();
            dict.Containske
        }

    }
}