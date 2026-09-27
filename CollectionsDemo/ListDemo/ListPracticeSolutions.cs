// Practice Solutions for List<T> Problems
// Write your solutions here - IntelliSense will work!

public class ListPracticeSolutions
{
    // Problem 1: Two Sum
    // Given an array of integers and a target, return indices of two numbers that add up to target
    public static int[] TwoSum(int[] nums, int target)
    {
        Dictionary<int, int> dict = new Dictionary<int, int>();
        int[]result=new int[2] { -1,-1};
        for (int i = 0; i < nums.Length; i++)
        {
            if (dict.TryGetValue(target - nums[i], out int index))
            {
                result[0] = index;
                result[1] = i;
            }
            else
            {
                dict.Add(nums[i], i);
            }
        }
        return result;
    }

    // Problem 2: Remove Duplicates from Sorted Array
    // Remove duplicates in-place and return new length
    public static int RemoveDuplicates(int[] nums)
    {
        List<> list = nums.ToList();
        
    }

    // Problem 3: Rotate Array
    // Rotate array to the right by k steps
    public static void Rotate(int[] nums, int k)
    {
        // Your solution here
        throw new NotImplementedException();
    }

    // Problem 4: Find Maximum Subarray Sum (Kadane's Algorithm)
    public static int MaxSubArray(int[] nums)
    {
        // Your solution here
        throw new NotImplementedException();
    }

    // Problem 5: Merge Two Sorted Lists
    public static List<int> MergeSortedLists(List<int> list1, List<int> list2)
    {
        // Your solution here
        throw new NotImplementedException();
    }

    // Add more solutions as you practice...

    public static void Main(string[] args)
    {
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("              LIST PRACTICE SOLUTIONS - TEST RUNNER             ");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        // ════════════════════════════════════════════════════════════════════
        // Problem 1: Two Sum - TEST CASES
        // ════════════════════════════════════════════════════════════════════
        Console.WriteLine("═══ Problem 1: Two Sum ═══");
        
        // Test 1: Basic case
        int[] nums1 = { 2, 7, 11, 15 };
        int target1 = 9;
        var result1 = TwoSum(nums1, target1);
        Console.WriteLine($"  Test 1: nums=[2,7,11,15], target=9");
        Console.WriteLine($"    Expected: [0, 1]  |  Got: [{result1[0]}, {result1[1]}]");
        
        // Test 2
        int[] nums2 = { 3, 2, 4 };
        int target2 = 6;
        var result2 = TwoSum(nums2, target2);
        Console.WriteLine($"  Test 2: nums=[3,2,4], target=6");
        Console.WriteLine($"    Expected: [1, 2]  |  Got: [{result2[0]}, {result2[1]}]");
        
        // Test 3: Duplicate values
        int[] nums3 = { 3, 3 };
        int target3 = 6;
        var result3 = TwoSum(nums3, target3);
        Console.WriteLine($"  Test 3: nums=[3,3], target=6");
        Console.WriteLine($"    Expected: [0, 1]  |  Got: [{result3[0]}, {result3[1]}]");
        
        // Test 4: Negative numbers
        int[] nums4 = { -1, -2, -3, -4, -5 };
        int target4 = -8;
        var result4 = TwoSum(nums4, target4);
        Console.WriteLine($"  Test 4: nums=[-1,-2,-3,-4,-5], target=-8");
        Console.WriteLine($"    Expected: [2, 4]  |  Got: [{result4[0]}, {result4[1]}]");

        Console.WriteLine();

        // ════════════════════════════════════════════════════════════════════
        // Problem 2: Remove Duplicates - TEST CASES
        // ════════════════════════════════════════════════════════════════════
        Console.WriteLine("═══ Problem 2: Remove Duplicates (Sorted Array) ═══");
        Console.WriteLine("  Uncomment tests after implementing\n");
        
        
        // Test 1
        int[] dup1 = { 1, 1, 2 };
        int len1 = RemoveDuplicates(dup1);
        Console.WriteLine($"  Test 1: [1,1,2]");
        Console.WriteLine($"    Expected: length=2, [1,2]  |  Got: length={len1}");
        
        // Test 2
        int[] dup2 = { 0, 0, 1, 1, 1, 2, 2, 3, 3, 4 };
        int len2 = RemoveDuplicates(dup2);
        Console.WriteLine($"  Test 2: [0,0,1,1,1,2,2,3,3,4]");
        Console.WriteLine($"    Expected: length=5, [0,1,2,3,4]  |  Got: length={len2}");
        
        // Test 3: No duplicates
        int[] dup3 = { 1, 2, 3, 4, 5 };
        int len3 = RemoveDuplicates(dup3);
        Console.WriteLine($"  Test 3: [1,2,3,4,5]");
        Console.WriteLine($"    Expected: length=5  |  Got: length={len3}");
        
        // Test 4: All same
        int[] dup4 = { 1, 1, 1, 1, 1 };
        int len4 = RemoveDuplicates(dup4);
        Console.WriteLine($"  Test 4: [1,1,1,1,1]");
        Console.WriteLine($"    Expected: length=1  |  Got: length={len4}");
        

        // ════════════════════════════════════════════════════════════════════
        // Problem 3: Rotate Array - TEST CASES
        // ════════════════════════════════════════════════════════════════════
        Console.WriteLine("═══ Problem 3: Rotate Array ═══");
        Console.WriteLine("  Uncomment tests after implementing\n");
        
        /*
        // Test 1
        int[] rot1 = { 1, 2, 3, 4, 5 };
        Rotate(rot1, 2);
        Console.WriteLine($"  Test 1: [1,2,3,4,5], k=2");
        Console.WriteLine($"    Expected: [4,5,1,2,3]  |  Got: [{string.Join(",", rot1)}]");
        
        // Test 2
        int[] rot2 = { -1, -100, 3, 99 };
        Rotate(rot2, 2);
        Console.WriteLine($"  Test 2: [-1,-100,3,99], k=2");
        Console.WriteLine($"    Expected: [3,99,-1,-100]  |  Got: [{string.Join(",", rot2)}]");
        
        // Test 3: k > length
        int[] rot3 = { 1, 2, 3 };
        Rotate(rot3, 4);  // k=4 on length=3 is same as k=1
        Console.WriteLine($"  Test 3: [1,2,3], k=4");
        Console.WriteLine($"    Expected: [3,1,2]  |  Got: [{string.Join(",", rot3)}]");
        
        // Test 4: k = 0
        int[] rot4 = { 1, 2, 3 };
        Rotate(rot4, 0);
        Console.WriteLine($"  Test 4: [1,2,3], k=0");
        Console.WriteLine($"    Expected: [1,2,3]  |  Got: [{string.Join(",", rot4)}]");
        */

        // ════════════════════════════════════════════════════════════════════
        // Problem 4: Max Subarray Sum (Kadane's) - TEST CASES
        // ════════════════════════════════════════════════════════════════════
        Console.WriteLine("═══ Problem 4: Max Subarray Sum (Kadane's Algorithm) ═══");
        Console.WriteLine("  Uncomment tests after implementing\n");
        
        /*
        // Test 1
        int[] sub1 = { -2, 1, -3, 4, -1, 2, 1, -5, 4 };
        Console.WriteLine($"  Test 1: [-2,1,-3,4,-1,2,1,-5,4]");
        Console.WriteLine($"    Expected: 6 (subarray [4,-1,2,1])  |  Got: {MaxSubArray(sub1)}");
        
        // Test 2
        int[] sub2 = { 1 };
        Console.WriteLine($"  Test 2: [1]");
        Console.WriteLine($"    Expected: 1  |  Got: {MaxSubArray(sub2)}");
        
        // Test 3
        int[] sub3 = { 5, 4, -1, 7, 8 };
        Console.WriteLine($"  Test 3: [5,4,-1,7,8]");
        Console.WriteLine($"    Expected: 23  |  Got: {MaxSubArray(sub3)}");
        
        // Test 4: All negative
        int[] sub4 = { -2, -3, -1, -5 };
        Console.WriteLine($"  Test 4: [-2,-3,-1,-5]");
        Console.WriteLine($"    Expected: -1 (best single element)  |  Got: {MaxSubArray(sub4)}");
        */

        // ════════════════════════════════════════════════════════════════════
        // Problem 5: Merge Sorted Lists - TEST CASES
        // ════════════════════════════════════════════════════════════════════
        Console.WriteLine("═══ Problem 5: Merge Sorted Lists ═══");
        Console.WriteLine("  Uncomment tests after implementing\n");
        
        /*
        // Test 1
        var merge1a = new List<int> { 1, 3, 5 };
        var merge1b = new List<int> { 2, 4, 6 };
        var merged1 = MergeSortedLists(merge1a, merge1b);
        Console.WriteLine($"  Test 1: [1,3,5] + [2,4,6]");
        Console.WriteLine($"    Expected: [1,2,3,4,5,6]  |  Got: [{string.Join(",", merged1)}]");
        
        // Test 2
        var merge2a = new List<int> { 1, 2, 4 };
        var merge2b = new List<int> { 1, 3, 4 };
        var merged2 = MergeSortedLists(merge2a, merge2b);
        Console.WriteLine($"  Test 2: [1,2,4] + [1,3,4]");
        Console.WriteLine($"    Expected: [1,1,2,3,4,4]  |  Got: [{string.Join(",", merged2)}]");
        
        // Test 3: One empty
        var merge3a = new List<int> { };
        var merge3b = new List<int> { 1, 2, 3 };
        var merged3 = MergeSortedLists(merge3a, merge3b);
        Console.WriteLine($"  Test 3: [] + [1,2,3]");
        Console.WriteLine($"    Expected: [1,2,3]  |  Got: [{string.Join(",", merged3)}]");
        
        // Test 4: Both empty
        var merge4a = new List<int> { };
        var merge4b = new List<int> { };
        var merged4 = MergeSortedLists(merge4a, merge4b);
        Console.WriteLine($"  Test 4: [] + []");
        Console.WriteLine($"    Expected: []  |  Got: [{string.Join(",", merged4)}]");
        */

        Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
        Console.WriteLine("  💡 Uncomment test blocks as you implement each solution!");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
    }
}
