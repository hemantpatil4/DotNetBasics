# Queue<T> and Stack<T> Complete Guide – C# Collections

> **Interview Focus:** FIFO vs LIFO, Internals, When to use which  
> **Java Equivalent:** Queue<E> (LinkedList impl), Stack<E>

---

## Queue<T> - FIFO (First In, First Out)

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                        QUEUE<T> - FIFO STRUCTURE                              ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║   Queue<string> orderQueue = new Queue<string>();                             ║
║   orderQueue.Enqueue("Order1");                                               ║
║   orderQueue.Enqueue("Order2");                                               ║
║   orderQueue.Enqueue("Order3");                                               ║
║                                                                               ║
║   INTERNAL: Circular buffer (array-based)                                     ║
║   ┌───────┬───────┬───────┬───────┬───────┬───────┐                          ║
║   │   0   │   1   │   2   │   3   │   4   │   5   │                          ║
║   │Order1 │Order2 │Order3 │       │       │       │                          ║
║   └───────┴───────┴───────┴───────┴───────┴───────┘                          ║
║       ↑                       ↑                                               ║
║     HEAD                    TAIL                                              ║
║   (Dequeue)              (Enqueue)                                            ║
║                                                                               ║
║   Dequeue(): Returns "Order1", HEAD moves to index 1                          ║
║   Enqueue("Order4"): Adds at TAIL (index 3)                                   ║
║                                                                               ║
║   ┌───────┬───────┬───────┬───────┬───────┬───────┐                          ║
║   │   0   │   1   │   2   │   3   │   4   │   5   │                          ║
║   │(empty)│Order2 │Order3 │Order4 │       │       │                          ║
║   └───────┴───────┴───────┴───────┴───────┴───────┘                          ║
║               ↑               ↑                                               ║
║             HEAD            TAIL                                              ║
║                                                                               ║
║   CIRCULAR: When TAIL reaches end, wraps to beginning                         ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

---

## Stack<T> - LIFO (Last In, First Out)

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                        STACK<T> - LIFO STRUCTURE                              ║
╠═══════════════════════════════════════════════════════════════════════════════╣
║                                                                               ║
║   Stack<string> callStack = new Stack<string>();                              ║
║   callStack.Push("Main");                                                     ║
║   callStack.Push("ProcessTrade");                                             ║
║   callStack.Push("ValidateOrder");                                            ║
║                                                                               ║
║   INTERNAL: Array-based (grows from bottom)                                   ║
║                                                                               ║
║         │               │                                                     ║
║         │               │                                                     ║
║         │ValidateOrder  │  ← TOP (index 2) - Pop returns this                 ║
║         ├───────────────┤                                                     ║
║         │ProcessTrade   │  ← index 1                                          ║
║         ├───────────────┤                                                     ║
║         │Main           │  ← BOTTOM (index 0)                                 ║
║         └───────────────┘                                                     ║
║                                                                               ║
║   Pop(): Returns "ValidateOrder", TOP moves to index 1                        ║
║   Push("SendConfirmation"): Adds at TOP (index 2)                             ║
║                                                                               ║
║         │               │                                                     ║
║         │SendConfirm... │  ← TOP (index 2)                                    ║
║         ├───────────────┤                                                     ║
║         │ProcessTrade   │                                                     ║
║         ├───────────────┤                                                     ║
║         │Main           │                                                     ║
║         └───────────────┘                                                     ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

---

## Queue<T> Operations

### Creation & Basic Operations

```csharp
// Create queue
Queue<string> queue = new Queue<string>();

// Enqueue - O(1) amortized
queue.Enqueue("Order1");
queue.Enqueue("Order2");
queue.Enqueue("Order3");

// Dequeue - O(1) - removes and returns first item
string first = queue.Dequeue();  // "Order1"

// Peek - O(1) - returns first without removing
string next = queue.Peek();  // "Order2"

// Count
int count = queue.Count;  // 2

// Clear
queue.Clear();

// TryDequeue - Safe dequeue (no exception)
if (queue.TryDequeue(out string? item))
{
    Console.WriteLine($"Dequeued: {item}");
}

// TryPeek - Safe peek
if (queue.TryPeek(out string? peeked))
{
    Console.WriteLine($"Next: {peeked}");
}
```

### Queue from Collection

```csharp
// Create from existing collection
var orders = new List<string> { "O1", "O2", "O3" };
var queue = new Queue<string>(orders);

// Convert to array
string[] array = queue.ToArray();

// Iterate (doesn't remove items)
foreach (var item in queue)
{
    Console.WriteLine(item);
}

// Contains - O(n)
bool hasO2 = queue.Contains("O2");
```

---

## Stack<T> Operations

### Creation & Basic Operations

```csharp
// Create stack
Stack<string> stack = new Stack<string>();

// Push - O(1) amortized
stack.Push("Main");
stack.Push("ProcessOrder");
stack.Push("Validate");

// Pop - O(1) - removes and returns top item
string top = stack.Pop();  // "Validate"

// Peek - O(1) - returns top without removing
string current = stack.Peek();  // "ProcessOrder"

// Count
int count = stack.Count;  // 2

// Clear
stack.Clear();

// TryPop - Safe pop (no exception)
if (stack.TryPop(out string? item))
{
    Console.WriteLine($"Popped: {item}");
}

// TryPeek - Safe peek
if (stack.TryPeek(out string? peeked))
{
    Console.WriteLine($"Top: {peeked}");
}
```

### Stack from Collection

```csharp
// Create from collection (note: reverses order!)
var items = new List<string> { "A", "B", "C" };
var stack = new Stack<string>(items);  // C is on top!

// Pop order: C, B, A

// Convert to array (top to bottom)
string[] array = stack.ToArray();  // ["C", "B", "A"]

// Contains - O(n)
bool hasB = stack.Contains("B");
```

---

## Big-O Comparison

```
╔══════════════════════════════════════════════════════════════════════════╗
║                    QUEUE vs STACK - OPERATIONS                           ║
╠══════════════════════════════════════════════════════════════════════════╣
║                                                                          ║
║   Operation        │ Queue<T>    │ Stack<T>    │ Notes                   ║
║   ─────────────────┼─────────────┼─────────────┼─────────────────────────║
║   Enqueue/Push     │ O(1)*       │ O(1)*       │ *Amortized              ║
║   Dequeue/Pop      │ O(1)        │ O(1)        │                         ║
║   Peek             │ O(1)        │ O(1)        │                         ║
║   Contains         │ O(n)        │ O(n)        │ Linear search           ║
║   Count            │ O(1)        │ O(1)        │                         ║
║   Clear            │ O(n)        │ O(n)        │                         ║
║                                                                          ║
╚══════════════════════════════════════════════════════════════════════════╝
```

---

## When to Use What

```
╔══════════════════════════════════════════════════════════════════════════╗
║                    WHEN TO USE QUEUE vs STACK                            ║
╠══════════════════════════════════════════════════════════════════════════╣
║                                                                          ║
║   USE QUEUE when:                                                        ║
║   ✓ Processing in order received (FIFO)                                 ║
║   ✓ Message queues, job queues                                          ║
║   ✓ BFS (Breadth-First Search)                                          ║
║   ✓ Order processing systems                                            ║
║   ✓ Print spooling, task scheduling                                     ║
║   ✓ FX: Trade order queue, quote streaming                              ║
║                                                                          ║
║   USE STACK when:                                                        ║
║   ✓ Processing in reverse order (LIFO)                                  ║
║   ✓ Undo/Redo functionality                                             ║
║   ✓ DFS (Depth-First Search)                                            ║
║   ✓ Expression evaluation, parsing                                      ║
║   ✓ Backtracking algorithms                                             ║
║   ✓ Call stack simulation                                               ║
║   ✓ Bracket matching                                                    ║
║                                                                          ║
╚══════════════════════════════════════════════════════════════════════════╝
```

---

## Common Patterns

### 1. BFS with Queue

```csharp
// Breadth-First Search - level by level traversal
public void BFS(TreeNode root)
{
    if (root == null) return;

    var queue = new Queue<TreeNode>();
    queue.Enqueue(root);

    while (queue.Count > 0)
    {
        var node = queue.Dequeue();
        Console.WriteLine(node.Value);

        if (node.Left != null) queue.Enqueue(node.Left);
        if (node.Right != null) queue.Enqueue(node.Right);
    }
}
```

### 2. DFS with Stack

```csharp
// Depth-First Search - go deep first
public void DFS(TreeNode root)
{
    if (root == null) return;

    var stack = new Stack<TreeNode>();
    stack.Push(root);

    while (stack.Count > 0)
    {
        var node = stack.Pop();
        Console.WriteLine(node.Value);

        // Push right first so left is processed first
        if (node.Right != null) stack.Push(node.Right);
        if (node.Left != null) stack.Push(node.Left);
    }
}
```

### 3. Valid Parentheses

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

### 4. Implement Queue using Stacks

```csharp
public class MyQueue
{
    private Stack<int> _inbox = new Stack<int>();
    private Stack<int> _outbox = new Stack<int>();

    public void Enqueue(int x)
    {
        _inbox.Push(x);
    }

    public int Dequeue()
    {
        if (_outbox.Count == 0)
        {
            // Transfer all from inbox to outbox
            while (_inbox.Count > 0)
            {
                _outbox.Push(_inbox.Pop());
            }
        }
        return _outbox.Pop();
    }

    public int Peek()
    {
        if (_outbox.Count == 0)
        {
            while (_inbox.Count > 0)
            {
                _outbox.Push(_inbox.Pop());
            }
        }
        return _outbox.Peek();
    }
}
```

### 5. Undo/Redo with Stacks

```csharp
public class TextEditor
{
    private string _text = "";
    private Stack<string> _undoStack = new Stack<string>();
    private Stack<string> _redoStack = new Stack<string>();

    public void Type(string newText)
    {
        _undoStack.Push(_text);
        _text = newText;
        _redoStack.Clear();  // Clear redo on new action
    }

    public void Undo()
    {
        if (_undoStack.Count > 0)
        {
            _redoStack.Push(_text);
            _text = _undoStack.Pop();
        }
    }

    public void Redo()
    {
        if (_redoStack.Count > 0)
        {
            _undoStack.Push(_text);
            _text = _redoStack.Pop();
        }
    }
}
```

---

## Priority Queue (Heap)

.NET 6+ provides `PriorityQueue<TElement, TPriority>`:

```csharp
// Priority queue - dequeues lowest priority first
var pq = new PriorityQueue<string, int>();

// Enqueue with priority
pq.Enqueue("Low priority task", 10);
pq.Enqueue("High priority task", 1);
pq.Enqueue("Medium priority task", 5);

// Dequeue returns item with LOWEST priority first
while (pq.Count > 0)
{
    var task = pq.Dequeue();
    Console.WriteLine(task);
}
// Output: High priority task, Medium priority task, Low priority task

// For max-heap behavior, negate priorities
pq.Enqueue("Important", -100);
```

### FX Example: Order Priority Queue

```csharp
// Orders prioritized by arrival time (earlier = higher priority)
var orderQueue = new PriorityQueue<Order, DateTime>();

orderQueue.Enqueue(new Order("O1", "EUR/USD", 100000), DateTime.Parse("10:00:01"));
orderQueue.Enqueue(new Order("O2", "GBP/USD", 50000), DateTime.Parse("10:00:00"));
orderQueue.Enqueue(new Order("O3", "USD/JPY", 200000), DateTime.Parse("10:00:02"));

// Process in order of arrival
while (orderQueue.Count > 0)
{
    var order = orderQueue.Dequeue();
    Console.WriteLine(order);  // O2, O1, O3
}
```

---

## Interview Questions

**Q: Difference between Queue and Stack?**

- Queue: FIFO (First In, First Out) - like a line at a store
- Stack: LIFO (Last In, First Out) - like a stack of plates

**Q: Internal implementation of Queue<T>?**

- Circular buffer (array-based)
- Head and tail pointers
- Wraps around when reaching end

**Q: What happens when Queue/Stack runs out of capacity?**

- Creates new array (typically 2x size)
- Copies elements to new array
- O(n) for resize, but amortized O(1)

**Q: Implement Queue using two Stacks?**

- Use inbox stack for Enqueue
- Use outbox stack for Dequeue
- Transfer from inbox to outbox when outbox is empty

**Q: Time complexity of contains in Queue/Stack?**

- O(n) - must search through all elements

---

_See QueueStackDemo.cs for runnable examples and QueueStackPractice.md for coding challenges._
