# Queue & Stack Practice Problems

> Interview-focused coding challenges for Queue and Stack operations  
> Difficulty: ⭐ Easy | ⭐⭐ Medium | ⭐⭐⭐ Hard

---

## Problem 1: Valid Parentheses (⭐⭐)

**Classic Stack Problem!**

Check if brackets are properly matched.

```csharp
// Input: "()[]{}"
// Output: true

// Input: "([)]"
// Output: false

public bool IsValid(string s)
{
    // Your solution here
}
```

<details>
<summary>Solution</summary>

```csharp
public bool IsValid(string s)
{
    var stack = new Stack<char>();
    var pairs = new Dictionary<char, char>
    {
        { ')', '(' },
        { ']', '[' },
        { '}', '{' }
    };

    foreach (char c in s)
    {
        if (pairs.ContainsKey(c))
        {
            if (stack.Count == 0 || stack.Pop() != pairs[c])
                return false;
        }
        else
        {
            stack.Push(c);
        }
    }

    return stack.Count == 0;
}
```

**Time Complexity:** O(n)  
**Space Complexity:** O(n)

</details>

---

## Problem 2: Implement Queue using Stacks (⭐⭐)

**Popular Interview Problem!**

Implement a queue using only two stacks.

```csharp
public class MyQueue
{
    public void Push(int x) { }  // Enqueue
    public int Pop() { }         // Dequeue
    public int Peek() { }        // Front element
    public bool Empty() { }
}

// Operations should be amortized O(1)
```

<details>
<summary>Solution</summary>

```csharp
public class MyQueue
{
    private Stack<int> _inbox = new Stack<int>();
    private Stack<int> _outbox = new Stack<int>();

    public void Push(int x)
    {
        _inbox.Push(x);
    }

    public int Pop()
    {
        EnsureOutbox();
        return _outbox.Pop();
    }

    public int Peek()
    {
        EnsureOutbox();
        return _outbox.Peek();
    }

    public bool Empty()
    {
        return _inbox.Count == 0 && _outbox.Count == 0;
    }

    private void EnsureOutbox()
    {
        if (_outbox.Count == 0)
        {
            while (_inbox.Count > 0)
            {
                _outbox.Push(_inbox.Pop());
            }
        }
    }
}
```

**Time Complexity:** Amortized O(1) for all operations  
**Space Complexity:** O(n)

</details>

---

## Problem 3: Implement Stack using Queues (⭐⭐)

Implement a stack using only queues.

```csharp
public class MyStack
{
    public void Push(int x) { }
    public int Pop() { }
    public int Top() { }
    public bool Empty() { }
}
```

<details>
<summary>Solution</summary>

```csharp
public class MyStack
{
    private Queue<int> _queue = new Queue<int>();

    public void Push(int x)
    {
        _queue.Enqueue(x);

        // Rotate queue so newest element is at front
        for (int i = 0; i < _queue.Count - 1; i++)
        {
            _queue.Enqueue(_queue.Dequeue());
        }
    }

    public int Pop()
    {
        return _queue.Dequeue();
    }

    public int Top()
    {
        return _queue.Peek();
    }

    public bool Empty()
    {
        return _queue.Count == 0;
    }
}
```

**Time Complexity:** Push O(n), Pop O(1)  
**Space Complexity:** O(n)

</details>

---

## Problem 4: Min Stack (⭐⭐)

**Popular Interview Problem!**

Design a stack that supports push, pop, top, and retrieving minimum in O(1) time.

```csharp
public class MinStack
{
    public void Push(int val) { }
    public void Pop() { }
    public int Top() { }
    public int GetMin() { }  // O(1)!
}
```

**Hint:** Use auxiliary stack to track minimums.

<details>
<summary>Solution</summary>

```csharp
public class MinStack
{
    private Stack<int> _stack = new Stack<int>();
    private Stack<int> _minStack = new Stack<int>();

    public void Push(int val)
    {
        _stack.Push(val);

        // Push to minStack if empty or val <= current min
        if (_minStack.Count == 0 || val <= _minStack.Peek())
        {
            _minStack.Push(val);
        }
    }

    public void Pop()
    {
        int popped = _stack.Pop();

        // If popped value was the min, remove from minStack too
        if (popped == _minStack.Peek())
        {
            _minStack.Pop();
        }
    }

    public int Top()
    {
        return _stack.Peek();
    }

    public int GetMin()
    {
        return _minStack.Peek();
    }
}
```

**Time Complexity:** O(1) for all operations  
**Space Complexity:** O(n)

</details>

---

## Problem 5: Evaluate Reverse Polish Notation (⭐⭐)

Evaluate expression in Reverse Polish Notation (postfix).

```csharp
// Input: tokens = ["2","1","+","3","*"]
// Output: 9 ((2 + 1) * 3 = 9)

// Input: tokens = ["4","13","5","/","+"]
// Output: 6 (4 + (13 / 5) = 6)

public int EvalRPN(string[] tokens)
{
    // Your solution here
}
```

<details>
<summary>Solution</summary>

```csharp
public int EvalRPN(string[] tokens)
{
    var stack = new Stack<int>();
    var operators = new HashSet<string> { "+", "-", "*", "/" };

    foreach (string token in tokens)
    {
        if (operators.Contains(token))
        {
            int b = stack.Pop();  // Second operand (popped first!)
            int a = stack.Pop();  // First operand

            int result = token switch
            {
                "+" => a + b,
                "-" => a - b,
                "*" => a * b,
                "/" => a / b,
                _ => throw new ArgumentException()
            };

            stack.Push(result);
        }
        else
        {
            stack.Push(int.Parse(token));
        }
    }

    return stack.Pop();
}
```

**Time Complexity:** O(n)  
**Space Complexity:** O(n)

</details>

---

## Problem 6: Daily Temperatures (⭐⭐)

**Monotonic Stack Problem!**

Find days until warmer temperature. Return 0 if no warmer day.

```csharp
// Input: temperatures = [73, 74, 75, 71, 69, 72, 76, 73]
// Output: [1, 1, 4, 2, 1, 1, 0, 0]

// 73 → wait 1 day for 74
// 75 → wait 4 days for 76

public int[] DailyTemperatures(int[] temperatures)
{
    // Your solution here
}
```

**Hint:** Use stack to store indices of waiting temperatures.

<details>
<summary>Solution</summary>

```csharp
public int[] DailyTemperatures(int[] temperatures)
{
    int n = temperatures.Length;
    int[] result = new int[n];

    // Stack stores INDICES of temperatures waiting for warmer day
    var stack = new Stack<int>();

    for (int i = 0; i < n; i++)
    {
        // While current temp is warmer than top of stack
        while (stack.Count > 0 && temperatures[i] > temperatures[stack.Peek()])
        {
            int prevIndex = stack.Pop();
            result[prevIndex] = i - prevIndex;  // Days waited
        }

        stack.Push(i);
    }

    // Remaining in stack have no warmer day (result stays 0)
    return result;
}
```

**Time Complexity:** O(n)  
**Space Complexity:** O(n)

</details>

---

## Problem 7: Number of Recent Calls (⭐)

Count calls in last 3000 milliseconds.

```csharp
public class RecentCounter
{
    public int Ping(int t)
    {
        // Return number of calls in range [t - 3000, t]
    }
}

// recentCounter.ping(1);    // [1] → 1
// recentCounter.ping(100);  // [1, 100] → 2
// recentCounter.ping(3001); // [1, 100, 3001] → 3
// recentCounter.ping(3002); // [100, 3001, 3002] → 3 (1 is out of range)
```

<details>
<summary>Solution</summary>

```csharp
public class RecentCounter
{
    private Queue<int> _queue = new Queue<int>();

    public int Ping(int t)
    {
        _queue.Enqueue(t);

        // Remove calls outside the window
        while (_queue.Peek() < t - 3000)
        {
            _queue.Dequeue();
        }

        return _queue.Count;
    }
}
```

**Time Complexity:** Amortized O(1)  
**Space Complexity:** O(3000) = O(1)

</details>

---

## Problem 8: FX Order Queue Processor (⭐⭐)

**Domain: FX Trading**

Process trading orders with priority levels.

```csharp
public enum Priority { High = 1, Medium = 2, Low = 3 }

public record Order(string Id, string Pair, decimal Amount, Priority Priority);

public class FXOrderProcessor
{
    // Submit order to queue
    public void Submit(Order order) { }

    // Process next order (highest priority first, then FIFO within same priority)
    public Order? ProcessNext() { }

    // Get count of pending orders
    public int PendingCount { get; }

    // Cancel order by ID
    public bool Cancel(string orderId) { }
}
```

<details>
<summary>Solution</summary>

```csharp
public class FXOrderProcessor
{
    // Queue for each priority level
    private Dictionary<Priority, Queue<Order>> _queues = new()
    {
        { Priority.High, new Queue<Order>() },
        { Priority.Medium, new Queue<Order>() },
        { Priority.Low, new Queue<Order>() }
    };

    // Track orders for cancellation
    private HashSet<string> _cancelledIds = new HashSet<string>();

    public void Submit(Order order)
    {
        _queues[order.Priority].Enqueue(order);
    }

    public Order? ProcessNext()
    {
        // Try each priority in order
        foreach (Priority priority in new[] { Priority.High, Priority.Medium, Priority.Low })
        {
            var queue = _queues[priority];

            while (queue.Count > 0)
            {
                var order = queue.Dequeue();

                // Skip if cancelled
                if (_cancelledIds.Contains(order.Id))
                {
                    _cancelledIds.Remove(order.Id);
                    continue;
                }

                return order;
            }
        }

        return null;
    }

    public int PendingCount =>
        _queues.Values.Sum(q => q.Count) - _cancelledIds.Count;

    public bool Cancel(string orderId)
    {
        // Check if order exists in any queue
        foreach (var queue in _queues.Values)
        {
            if (queue.Any(o => o.Id == orderId))
            {
                _cancelledIds.Add(orderId);
                return true;
            }
        }
        return false;
    }
}
```

</details>

---

## Problem 9: Simplify Path (⭐⭐)

Convert Unix path to canonical form.

```csharp
// Input: "/home//foo/"
// Output: "/home/foo"

// Input: "/a/./b/../../c/"
// Output: "/c"

// Input: "/../"
// Output: "/"

public string SimplifyPath(string path)
{
    // Your solution here
}
```

**Hint:** Use stack to handle directory navigation.

<details>
<summary>Solution</summary>

```csharp
public string SimplifyPath(string path)
{
    var stack = new Stack<string>();

    string[] parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

    foreach (string part in parts)
    {
        if (part == "..")
        {
            if (stack.Count > 0)
                stack.Pop();
        }
        else if (part != ".")
        {
            stack.Push(part);
        }
    }

    // Build result (stack is reversed)
    if (stack.Count == 0) return "/";

    var result = new Stack<string>(stack);  // Reverse
    return "/" + string.Join("/", result);
}
```

**Time Complexity:** O(n)  
**Space Complexity:** O(n)

</details>

---

## Problem 10: Next Greater Element (⭐⭐)

**Monotonic Stack Pattern!**

Find next greater element for each element.

```csharp
// Input: nums = [1, 3, 4, 2]
// Output: [3, 4, -1, -1]

// 1 → next greater is 3
// 3 → next greater is 4
// 4 → no next greater = -1
// 2 → no next greater = -1

public int[] NextGreaterElement(int[] nums)
{
    // Your solution here
}
```

<details>
<summary>Solution</summary>

```csharp
public int[] NextGreaterElement(int[] nums)
{
    int n = nums.Length;
    int[] result = new int[n];
    Array.Fill(result, -1);

    // Stack stores INDICES
    var stack = new Stack<int>();

    for (int i = 0; i < n; i++)
    {
        while (stack.Count > 0 && nums[i] > nums[stack.Peek()])
        {
            int prevIndex = stack.Pop();
            result[prevIndex] = nums[i];
        }

        stack.Push(i);
    }

    return result;
}
```

**Time Complexity:** O(n)  
**Space Complexity:** O(n)

</details>

---

## Problem 11: Decode String (⭐⭐⭐)

Decode encoded string with nested brackets.

```csharp
// Input: s = "3[a]2[bc]"
// Output: "aaabcbc"

// Input: s = "3[a2[c]]"
// Output: "accaccacc"

// Input: s = "2[abc]3[cd]ef"
// Output: "abcabccdcdcdef"

public string DecodeString(string s)
{
    // Your solution here
}
```

**Hint:** Use two stacks - one for counts, one for strings.

<details>
<summary>Solution</summary>

```csharp
public string DecodeString(string s)
{
    var countStack = new Stack<int>();
    var stringStack = new Stack<string>();

    string current = "";
    int count = 0;

    foreach (char c in s)
    {
        if (char.IsDigit(c))
        {
            count = count * 10 + (c - '0');
        }
        else if (c == '[')
        {
            // Save current state
            countStack.Push(count);
            stringStack.Push(current);

            // Reset
            count = 0;
            current = "";
        }
        else if (c == ']')
        {
            // Pop and repeat
            int repeatCount = countStack.Pop();
            string prev = stringStack.Pop();

            current = prev + string.Concat(Enumerable.Repeat(current, repeatCount));
        }
        else
        {
            current += c;
        }
    }

    return current;
}
```

**Time Complexity:** O(n \* maxK) where maxK is max repeat count  
**Space Complexity:** O(n)

</details>

---

## Problem 12: Sliding Window Maximum (⭐⭐⭐)

**Monotonic Deque Problem!**

Find max in each sliding window of size k.

```csharp
// Input: nums = [1,3,-1,-3,5,3,6,7], k = 3
// Output: [3,3,5,5,6,7]

// Window [1,3,-1] → max 3
// Window [3,-1,-3] → max 3
// Window [-1,-3,5] → max 5
// ...

public int[] MaxSlidingWindow(int[] nums, int k)
{
    // Your solution here
}
```

**Hint:** Use deque to store indices in decreasing order of values.

<details>
<summary>Solution</summary>

```csharp
public int[] MaxSlidingWindow(int[] nums, int k)
{
    int n = nums.Length;
    int[] result = new int[n - k + 1];

    // Deque stores INDICES, front is max, in decreasing order
    var deque = new LinkedList<int>();

    for (int i = 0; i < n; i++)
    {
        // Remove indices outside window
        while (deque.Count > 0 && deque.First!.Value <= i - k)
        {
            deque.RemoveFirst();
        }

        // Remove indices with smaller values (they can't be max)
        while (deque.Count > 0 && nums[deque.Last!.Value] <= nums[i])
        {
            deque.RemoveLast();
        }

        deque.AddLast(i);

        // Start recording results when we have full window
        if (i >= k - 1)
        {
            result[i - k + 1] = nums[deque.First!.Value];
        }
    }

    return result;
}
```

**Time Complexity:** O(n)  
**Space Complexity:** O(k)

</details>

---

## Big-O Quick Reference

| Problem              | Time      | Space |
| -------------------- | --------- | ----- |
| Valid Parentheses    | O(n)      | O(n)  |
| Queue using Stacks   | O(1)\*    | O(n)  |
| Stack using Queues   | O(n) push | O(n)  |
| Min Stack            | O(1) all  | O(n)  |
| Eval RPN             | O(n)      | O(n)  |
| Daily Temperatures   | O(n)      | O(n)  |
| Recent Calls         | O(1)\*    | O(1)  |
| Simplify Path        | O(n)      | O(n)  |
| Next Greater Element | O(n)      | O(n)  |
| Decode String        | O(n·k)    | O(n)  |
| Sliding Window Max   | O(n)      | O(k)  |

\*Amortized

---

_Practice makes perfect! Try solving these problems without looking at solutions first._
