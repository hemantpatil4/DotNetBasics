/*
╔══════════════════════════════════════════════════════════════════════════════╗
║                 DICTIONARY<K,V> PRACTICE - TEST INPUTS                        ║
║                                                                               ║
║  Ready-to-use test cases for all Dictionary practice problems                 ║
║  Run with: dotnet run                                                         ║
╚══════════════════════════════════════════════════════════════════════════════╝
*/

using System;
using System.Collections.Generic;
using System.Linq;

public class DictionaryTestInputs
{
    public static void Main(string[] args)
    {
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("           DICTIONARY<K,V> PRACTICE - TEST ALL PROBLEMS         ");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        TestProblem1_TwoSum();
        TestProblem2_FirstUniqueChar();
        TestProblem3_GroupAnagrams();
        TestProblem4_SubarraySumEqualsK();
        TestProblem5_LRUCache();
        TestProblem6_RateTracker();
        TestProblem7_WordFrequency();
        TestProblem8_ValidParentheses();
        TestProblem9_TradePositions();
        TestProblem10_IsomorphicStrings();
        TestProblem11_LongestSubstring();

        Console.WriteLine("\n✓ All tests completed!");
    }

    // ════════════════════════════════════════════════════════════════════
    // Problem 1: Two Sum
    // ════════════════════════════════════════════════════════════════════
    static void TestProblem1_TwoSum()
    {
        Console.WriteLine("═══ Problem 1: Two Sum ═══");
        
        var testCases = new[]
        {
            (nums: new int[] { 2, 7, 11, 15 }, target: 9, expected: "[0, 1]"),
            (nums: new int[] { 3, 2, 4 }, target: 6, expected: "[1, 2]"),
            (nums: new int[] { 3, 3 }, target: 6, expected: "[0, 1]"),
            (nums: new int[] { 1, 5, 8, 3, 9, 2 }, target: 11, expected: "[2, 3] (8+3=11)"),
            (nums: new int[] { -1, -2, -3, -4, -5 }, target: -8, expected: "[2, 4] (-3+-5=-8)"),
            (nums: new int[] { 0, 4, 3, 0 }, target: 0, expected: "[0, 3] (0+0=0)"),
            (nums: new int[] { 1, 2, 3, 4, 5 }, target: 100, expected: "[] (no solution)"),
        };

        for (int i = 0; i < testCases.Length; i++)
        {
            var tc = testCases[i];
            Console.WriteLine($"\n  Test {i + 1}:");
            Console.WriteLine($"    Input:    nums = [{string.Join(", ", tc.nums)}], target = {tc.target}");
            Console.WriteLine($"    Expected: {tc.expected}");
        }
        Console.WriteLine();
    }

    // ════════════════════════════════════════════════════════════════════
    // Problem 2: First Non-Repeating Character
    // ════════════════════════════════════════════════════════════════════
    static void TestProblem2_FirstUniqueChar()
    {
        Console.WriteLine("═══ Problem 2: First Non-Repeating Character ═══");
        
        var testCases = new[]
        {
            (input: "fxtrading", expected: "'f'"),
            (input: "leetcode", expected: "'l'"),
            (input: "loveleetcode", expected: "'v'"),
            (input: "aabbcc", expected: "null (all repeat)"),
            (input: "abcabc", expected: "null (all repeat)"),
            (input: "a", expected: "'a'"),
            (input: "aadadaad", expected: "null"),
            (input: "dddccdbba", expected: "'a'"),
        };

        for (int i = 0; i < testCases.Length; i++)
        {
            var tc = testCases[i];
            Console.WriteLine($"\n  Test {i + 1}:");
            Console.WriteLine($"    Input:    \"{tc.input}\"");
            Console.WriteLine($"    Expected: {tc.expected}");
        }
        Console.WriteLine();
    }

    // ════════════════════════════════════════════════════════════════════
    // Problem 3: Group Anagrams
    // ════════════════════════════════════════════════════════════════════
    static void TestProblem3_GroupAnagrams()
    {
        Console.WriteLine("═══ Problem 3: Group Anagrams ═══");
        
        Console.WriteLine("\n  Test 1:");
        Console.WriteLine("    Input:    [\"eat\", \"tea\", \"tan\", \"ate\", \"nat\", \"bat\"]");
        Console.WriteLine("    Expected: [[\"eat\",\"tea\",\"ate\"], [\"tan\",\"nat\"], [\"bat\"]]");

        Console.WriteLine("\n  Test 2:");
        Console.WriteLine("    Input:    [\"\"]");
        Console.WriteLine("    Expected: [[\"\"]]");

        Console.WriteLine("\n  Test 3:");
        Console.WriteLine("    Input:    [\"a\"]");
        Console.WriteLine("    Expected: [[\"a\"]]");

        Console.WriteLine("\n  Test 4:");
        Console.WriteLine("    Input:    [\"abc\", \"bca\", \"cab\", \"xyz\", \"zyx\", \"yxz\"]");
        Console.WriteLine("    Expected: [[\"abc\",\"bca\",\"cab\"], [\"xyz\",\"zyx\",\"yxz\"]]");

        Console.WriteLine("\n  Test 5:");
        Console.WriteLine("    Input:    [\"listen\", \"silent\", \"enlist\", \"google\", \"goggles\"]");
        Console.WriteLine("    Expected: [[\"listen\",\"silent\",\"enlist\"], [\"google\"], [\"goggles\"]]");

        Console.WriteLine("\n  Test 6 (FX Domain):");
        Console.WriteLine("    Input:    [\"EURUSD\", \"USDEUR\", \"GBPUSD\", \"USDGBP\", \"USDJPY\"]");
        Console.WriteLine("    Expected: [[\"EURUSD\",\"USDEUR\"], [\"GBPUSD\",\"USDGBP\"], [\"USDJPY\"]]");

        Console.WriteLine();
    }

    // ════════════════════════════════════════════════════════════════════
    // Problem 4: Subarray Sum Equals K
    // ════════════════════════════════════════════════════════════════════
    static void TestProblem4_SubarraySumEqualsK()
    {
        Console.WriteLine("═══ Problem 4: Subarray Sum Equals K ═══");
        
        var testCases = new[]
        {
            (nums: new int[] { 1, 1, 1 }, k: 2, expected: 2, explanation: "[1,1] at 0-1 and 1-2"),
            (nums: new int[] { 1, 2, 3 }, k: 3, expected: 2, explanation: "[1,2] and [3]"),
            (nums: new int[] { 1, -1, 0 }, k: 0, expected: 3, explanation: "[1,-1], [-1,0,1? no], [0], [1,-1,0]"),
            (nums: new int[] { 3, 4, 7, 2, -3, 1, 4, 2 }, k: 7, expected: 4, explanation: "[3,4], [7], [7,2,-3,1], [2,-3,1,4,2,1? check]"),
            (nums: new int[] { 1 }, k: 0, expected: 0, explanation: "no subarray"),
            (nums: new int[] { -1, -1, 1 }, k: 0, expected: 1, explanation: "[-1,-1,1,1? no] [-1,1]"),
        };

        for (int i = 0; i < testCases.Length; i++)
        {
            var tc = testCases[i];
            Console.WriteLine($"\n  Test {i + 1}:");
            Console.WriteLine($"    Input:    nums = [{string.Join(", ", tc.nums)}], k = {tc.k}");
            Console.WriteLine($"    Expected: {tc.expected} subarrays");
            Console.WriteLine($"    Note:     {tc.explanation}");
        }
        Console.WriteLine();
    }

    // ════════════════════════════════════════════════════════════════════
    // Problem 5: LRU Cache
    // ════════════════════════════════════════════════════════════════════
    static void TestProblem5_LRUCache()
    {
        Console.WriteLine("═══ Problem 5: LRU Cache ═══");
        
        Console.WriteLine("\n  Test 1: Basic Operations (capacity = 2)");
        Console.WriteLine("    Operations:");
        Console.WriteLine("      Put(1, 1)        → cache: {1=1}");
        Console.WriteLine("      Put(2, 2)        → cache: {1=1, 2=2}");
        Console.WriteLine("      Get(1)           → returns 1, cache: {2=2, 1=1}");
        Console.WriteLine("      Put(3, 3)        → evicts 2, cache: {1=1, 3=3}");
        Console.WriteLine("      Get(2)           → returns -1 (not found)");
        Console.WriteLine("      Put(4, 4)        → evicts 1, cache: {3=3, 4=4}");
        Console.WriteLine("      Get(1)           → returns -1");
        Console.WriteLine("      Get(3)           → returns 3");
        Console.WriteLine("      Get(4)           → returns 4");

        Console.WriteLine("\n  Test 2: Update Existing Key (capacity = 2)");
        Console.WriteLine("    Operations:");
        Console.WriteLine("      Put(1, 1)        → cache: {1=1}");
        Console.WriteLine("      Put(2, 2)        → cache: {1=1, 2=2}");
        Console.WriteLine("      Put(1, 10)       → update 1, cache: {2=2, 1=10}");
        Console.WriteLine("      Put(3, 3)        → evicts 2, cache: {1=10, 3=3}");
        Console.WriteLine("      Get(1)           → returns 10");
        Console.WriteLine("      Get(2)           → returns -1");

        Console.WriteLine("\n  Test 3: Single Capacity (capacity = 1)");
        Console.WriteLine("    Operations:");
        Console.WriteLine("      Put(1, 1)        → cache: {1=1}");
        Console.WriteLine("      Put(2, 2)        → evicts 1, cache: {2=2}");
        Console.WriteLine("      Get(1)           → returns -1");
        Console.WriteLine("      Get(2)           → returns 2");

        Console.WriteLine();
    }

    // ════════════════════════════════════════════════════════════════════
    // Problem 6: FX Rate History Tracker
    // ════════════════════════════════════════════════════════════════════
    static void TestProblem6_RateTracker()
    {
        Console.WriteLine("═══ Problem 6: FX Rate History Tracker ═══");
        
        Console.WriteLine("\n  Test 1: Track EUR/USD rates");
        Console.WriteLine("    Operations:");
        Console.WriteLine("      UpdateRate(\"EUR/USD\", 1.0850)  at t=0");
        Console.WriteLine("      UpdateRate(\"EUR/USD\", 1.0855)  at t=1");
        Console.WriteLine("      UpdateRate(\"EUR/USD\", 1.0840)  at t=2");
        Console.WriteLine("      UpdateRate(\"GBP/USD\", 1.2500)  at t=3");
        Console.WriteLine("      ");
        Console.WriteLine("      GetLatestRate(\"EUR/USD\")       → 1.0840");
        Console.WriteLine("      GetLatestRate(\"GBP/USD\")       → 1.2500");
        Console.WriteLine("      GetLatestRate(\"USD/JPY\")       → null");
        Console.WriteLine("      ");
        Console.WriteLine("      GetRateHistory(\"EUR/USD\")      → [(t0, 1.0850), (t1, 1.0855), (t2, 1.0840)]");
        Console.WriteLine("      GetAverageRate(\"EUR/USD\", 5)   → 1.0848 (average of all 3)");

        Console.WriteLine("\n  Test 2: Empty tracker");
        Console.WriteLine("    Operations:");
        Console.WriteLine("      GetLatestRate(\"EUR/USD\")       → null");
        Console.WriteLine("      GetRateHistory(\"EUR/USD\")      → []");
        Console.WriteLine("      GetAverageRate(\"EUR/USD\", 5)   → null");

        Console.WriteLine();
    }

    // ════════════════════════════════════════════════════════════════════
    // Problem 7: Word Frequency Counter
    // ════════════════════════════════════════════════════════════════════
    static void TestProblem7_WordFrequency()
    {
        Console.WriteLine("═══ Problem 7: Word Frequency Counter ═══");
        
        var testCases = new[]
        {
            (input: "The quick brown fox jumps over the lazy dog. The dog barks.",
             expected: "[(the, 3), (dog, 2), (barks, 1), (brown, 1), (fox, 1), ...]"),
            (input: "hello hello hello world",
             expected: "[(hello, 3), (world, 1)]"),
            (input: "A a A a A",
             expected: "[(a, 5)] (case-insensitive)"),
            (input: "FX trading FX rates FX market rates",
             expected: "[(fx, 3), (rates, 2), (market, 1), (trading, 1)]"),
            (input: "",
             expected: "[] (empty)"),
        };

        for (int i = 0; i < testCases.Length; i++)
        {
            var tc = testCases[i];
            Console.WriteLine($"\n  Test {i + 1}:");
            Console.WriteLine($"    Input:    \"{tc.input}\"");
            Console.WriteLine($"    Expected: {tc.expected}");
        }
        Console.WriteLine();
    }

    // ════════════════════════════════════════════════════════════════════
    // Problem 8: Valid Parentheses
    // ════════════════════════════════════════════════════════════════════
    static void TestProblem8_ValidParentheses()
    {
        Console.WriteLine("═══ Problem 8: Valid Parentheses ═══");
        
        var testCases = new[]
        {
            (input: "()", expected: true),
            (input: "()[]{}", expected: true),
            (input: "(]", expected: false),
            (input: "([)]", expected: false),
            (input: "{[]}", expected: true),
            (input: "{[()]}", expected: true),
            (input: "((()))", expected: true),
            (input: "((())", expected: false),
            (input: "", expected: true),
            (input: "[", expected: false),
            (input: "]", expected: false),
            (input: "{[}]", expected: false),
        };

        for (int i = 0; i < testCases.Length; i++)
        {
            var tc = testCases[i];
            Console.WriteLine($"\n  Test {i + 1}:");
            Console.WriteLine($"    Input:    \"{tc.input}\"");
            Console.WriteLine($"    Expected: {tc.expected}");
        }
        Console.WriteLine();
    }

    // ════════════════════════════════════════════════════════════════════
    // Problem 9: Trade Position Calculator
    // ════════════════════════════════════════════════════════════════════
    static void TestProblem9_TradePositions()
    {
        Console.WriteLine("═══ Problem 9: Trade Position Calculator ═══");
        
        Console.WriteLine("\n  Test 1: Mixed BUY/SELL");
        Console.WriteLine("    Input Trades:");
        Console.WriteLine("      { Pair: \"EUR/USD\", Amount: 100000, Side: \"BUY\" }");
        Console.WriteLine("      { Pair: \"EUR/USD\", Amount: 50000,  Side: \"SELL\" }");
        Console.WriteLine("      { Pair: \"GBP/USD\", Amount: 200000, Side: \"BUY\" }");
        Console.WriteLine("    Expected: { \"EUR/USD\": 50000, \"GBP/USD\": 200000 }");

        Console.WriteLine("\n  Test 2: Flat Position");
        Console.WriteLine("    Input Trades:");
        Console.WriteLine("      { Pair: \"EUR/USD\", Amount: 100000, Side: \"BUY\" }");
        Console.WriteLine("      { Pair: \"EUR/USD\", Amount: 100000, Side: \"SELL\" }");
        Console.WriteLine("    Expected: { } (EUR/USD is flat, removed)");

        Console.WriteLine("\n  Test 3: All Sells (Short Position)");
        Console.WriteLine("    Input Trades:");
        Console.WriteLine("      { Pair: \"USD/JPY\", Amount: 50000, Side: \"SELL\" }");
        Console.WriteLine("      { Pair: \"USD/JPY\", Amount: 30000, Side: \"SELL\" }");
        Console.WriteLine("    Expected: { \"USD/JPY\": -80000 }");

        Console.WriteLine("\n  Test 4: Multiple Pairs");
        Console.WriteLine("    Input Trades:");
        Console.WriteLine("      { Pair: \"EUR/USD\", Amount: 100000, Side: \"BUY\" }");
        Console.WriteLine("      { Pair: \"GBP/USD\", Amount: 50000,  Side: \"BUY\" }");
        Console.WriteLine("      { Pair: \"USD/JPY\", Amount: 200000, Side: \"SELL\" }");
        Console.WriteLine("      { Pair: \"EUR/USD\", Amount: 25000,  Side: \"SELL\" }");
        Console.WriteLine("      { Pair: \"GBP/USD\", Amount: 75000,  Side: \"BUY\" }");
        Console.WriteLine("    Expected: { \"EUR/USD\": 75000, \"GBP/USD\": 125000, \"USD/JPY\": -200000 }");

        Console.WriteLine();
    }

    // ════════════════════════════════════════════════════════════════════
    // Problem 10: Isomorphic Strings
    // ════════════════════════════════════════════════════════════════════
    static void TestProblem10_IsomorphicStrings()
    {
        Console.WriteLine("═══ Problem 10: Isomorphic Strings ═══");
        
        var testCases = new[]
        {
            (s: "egg", t: "add", expected: true, reason: "e→a, g→d"),
            (s: "foo", t: "bar", expected: false, reason: "o maps to both a and r"),
            (s: "paper", t: "title", expected: true, reason: "p→t, a→i, e→l, r→e"),
            (s: "ab", t: "aa", expected: false, reason: "a→a, b→a (duplicate mapping)"),
            (s: "abc", t: "def", expected: true, reason: "a→d, b→e, c→f"),
            (s: "", t: "", expected: true, reason: "empty strings"),
            (s: "a", t: "a", expected: true, reason: "same char"),
            (s: "badc", t: "baba", expected: false, reason: "d→b, c→a but b already maps to b"),
        };

        for (int i = 0; i < testCases.Length; i++)
        {
            var tc = testCases[i];
            Console.WriteLine($"\n  Test {i + 1}:");
            Console.WriteLine($"    Input:    s = \"{tc.s}\", t = \"{tc.t}\"");
            Console.WriteLine($"    Expected: {tc.expected} ({tc.reason})");
        }
        Console.WriteLine();
    }

    // ════════════════════════════════════════════════════════════════════
    // Problem 11: Longest Substring Without Repeating Characters
    // ════════════════════════════════════════════════════════════════════
    static void TestProblem11_LongestSubstring()
    {
        Console.WriteLine("═══ Problem 11: Longest Substring Without Repeating Characters ═══");
        
        var testCases = new[]
        {
            (input: "abcabcbb", expected: 3, substring: "abc"),
            (input: "bbbbb", expected: 1, substring: "b"),
            (input: "pwwkew", expected: 3, substring: "wke"),
            (input: "", expected: 0, substring: ""),
            (input: " ", expected: 1, substring: " "),
            (input: "au", expected: 2, substring: "au"),
            (input: "dvdf", expected: 3, substring: "vdf"),
            (input: "abcdefg", expected: 7, substring: "abcdefg (all unique)"),
            (input: "aab", expected: 2, substring: "ab"),
            (input: "tmmzuxt", expected: 5, substring: "mzuxt"),
        };

        for (int i = 0; i < testCases.Length; i++)
        {
            var tc = testCases[i];
            Console.WriteLine($"\n  Test {i + 1}:");
            Console.WriteLine($"    Input:    \"{tc.input}\"");
            Console.WriteLine($"    Expected: {tc.expected} (substring: \"{tc.substring}\")");
        }
        Console.WriteLine();
    }
}

// ════════════════════════════════════════════════════════════════════
// Trade record for Problem 9
// ════════════════════════════════════════════════════════════════════
public record DictTrade(string Pair, decimal Amount, string Side);
