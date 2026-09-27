using System;
using System.Collections.Generic;
using System.Linq;

namespace CollectionsDemo.QueueStackDemo;

/// <summary>
/// Comprehensive Queue<T> and Stack<T> examples for interview preparation
/// Run: dotnet run
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("           QUEUE & STACK COMPREHENSIVE DEMO                    ");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");
        
        Demo1_QueueBasics();
        Demo2_QueueSafeOperations();
        Demo3_StackBasics();
        Demo4_StackSafeOperations();
        Demo5_BFSWithQueue();
        Demo6_DFSWithStack();
        Demo7_ValidParentheses();
        Demo8_QueueUsingStacks();
        Demo9_UndoRedoPattern();
        Demo10_FXTradingOrderQueue();
        
        Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
        Console.WriteLine("                    ALL DEMOS COMPLETE                         ");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
    }
    
    static void Demo1_QueueBasics()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 1: Queue<T> Basics - FIFO                              │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        // Create queue
        var queue = new Queue<string>();
        
        // Enqueue - O(1) amortized
        Console.WriteLine("Enqueueing: Order1, Order2, Order3");
        queue.Enqueue("Order1");
        queue.Enqueue("Order2");
        queue.Enqueue("Order3");
        
        Console.WriteLine($"Queue count: {queue.Count}");
        Console.WriteLine($"Queue contents: {string.Join(" → ", queue)}");
        
        // Peek - O(1) - look at first without removing
        Console.WriteLine($"\nPeek (front): {queue.Peek()}");
        Console.WriteLine($"Count after Peek: {queue.Count}");
        
        // Dequeue - O(1) - remove and return first
        Console.WriteLine($"\nDequeue: {queue.Dequeue()}");
        Console.WriteLine($"Queue after Dequeue: {string.Join(" → ", queue)}");
        
        // Process all
        Console.WriteLine("\nProcessing remaining queue (FIFO order):");
        while (queue.Count > 0)
        {
            Console.WriteLine($"  Processing: {queue.Dequeue()}");
        }
        
        // Create from collection
        var items = new[] { "A", "B", "C" };
        var queue2 = new Queue<string>(items);
        Console.WriteLine($"\nQueue from array: {string.Join(" → ", queue2)}");
        
        Console.WriteLine();
    }
    
    static void Demo2_QueueSafeOperations()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 2: Queue<T> Safe Operations                            │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        var queue = new Queue<string>();
        queue.Enqueue("Item1");
        
        // TryDequeue - safe dequeue
        Console.WriteLine("TryDequeue tests:");
        if (queue.TryDequeue(out string? item1))
        {
            Console.WriteLine($"  Dequeued: {item1}");
        }
        
        // Try on empty queue
        if (queue.TryDequeue(out string? item2))
        {
            Console.WriteLine($"  Dequeued: {item2}");
        }
        else
        {
            Console.WriteLine($"  Queue empty, TryDequeue returned false");
        }
        
        // TryPeek - safe peek
        queue.Enqueue("Item2");
        Console.WriteLine("\nTryPeek tests:");
        if (queue.TryPeek(out string? peeked))
        {
            Console.WriteLine($"  Peeked: {peeked}");
        }
        
        // Contains - O(n)
        Console.WriteLine($"\nContains 'Item2': {queue.Contains("Item2")}");
        Console.WriteLine($"Contains 'Item1': {queue.Contains("Item1")}");
        
        // ToArray
        queue.Enqueue("Item3");
        var array = queue.ToArray();
        Console.WriteLine($"ToArray: [{string.Join(", ", array)}]");
        
        // Clear
        queue.Clear();
        Console.WriteLine($"After Clear, count: {queue.Count}");
        
        Console.WriteLine();
    }
    
    static void Demo3_StackBasics()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 3: Stack<T> Basics - LIFO                              │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        // Create stack
        var stack = new Stack<string>();
        
        // Push - O(1) amortized
        Console.WriteLine("Pushing: Main → ProcessOrder → Validate");
        stack.Push("Main");
        stack.Push("ProcessOrder");
        stack.Push("Validate");
        
        Console.WriteLine($"Stack count: {stack.Count}");
        Console.WriteLine($"Stack (top to bottom): {string.Join(" → ", stack)}");
        
        // Peek - O(1) - look at top without removing
        Console.WriteLine($"\nPeek (top): {stack.Peek()}");
        Console.WriteLine($"Count after Peek: {stack.Count}");
        
        // Pop - O(1) - remove and return top
        Console.WriteLine($"\nPop: {stack.Pop()}");
        Console.WriteLine($"Stack after Pop: {string.Join(" → ", stack)}");
        
        // Process all
        Console.WriteLine("\nProcessing remaining stack (LIFO order):");
        while (stack.Count > 0)
        {
            Console.WriteLine($"  Processing: {stack.Pop()}");
        }
        
        // Create from collection (NOTE: reverses order!)
        var items = new[] { "A", "B", "C" };
        var stack2 = new Stack<string>(items);
        Console.WriteLine($"\nStack from array [A,B,C]: {string.Join(" → ", stack2)}");
        Console.WriteLine("(Notice: C is on top - order reversed!)");
        
        Console.WriteLine();
    }
    
    static void Demo4_StackSafeOperations()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 4: Stack<T> Safe Operations                            │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        var stack = new Stack<string>();
        stack.Push("Item1");
        
        // TryPop - safe pop
        Console.WriteLine("TryPop tests:");
        if (stack.TryPop(out string? item1))
        {
            Console.WriteLine($"  Popped: {item1}");
        }
        
        // Try on empty stack
        if (stack.TryPop(out string? item2))
        {
            Console.WriteLine($"  Popped: {item2}");
        }
        else
        {
            Console.WriteLine($"  Stack empty, TryPop returned false");
        }
        
        // TryPeek - safe peek
        stack.Push("Item2");
        Console.WriteLine("\nTryPeek tests:");
        if (stack.TryPeek(out string? peeked))
        {
            Console.WriteLine($"  Peeked: {peeked}");
        }
        
        // Contains - O(n)
        stack.Push("Item3");
        Console.WriteLine($"\nContains 'Item2': {stack.Contains("Item2")}");
        Console.WriteLine($"Contains 'Item1': {stack.Contains("Item1")}");
        
        // ToArray (top to bottom order)
        var array = stack.ToArray();
        Console.WriteLine($"ToArray: [{string.Join(", ", array)}]");
        
        Console.WriteLine();
    }
    
    static void Demo5_BFSWithQueue()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 5: BFS with Queue - Level Order Traversal              │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        // Build a simple binary tree
        //       1
        //      / \
        //     2   3
        //    / \   \
        //   4   5   6
        
        var root = new TreeNode(1,
            new TreeNode(2, new TreeNode(4), new TreeNode(5)),
            new TreeNode(3, null, new TreeNode(6))
        );
        
        Console.WriteLine("Tree structure:");
        Console.WriteLine("       1");
        Console.WriteLine("      / \\");
        Console.WriteLine("     2   3");
        Console.WriteLine("    / \\   \\");
        Console.WriteLine("   4   5   6");
        
        Console.WriteLine("\nBFS (level order) traversal:");
        BFS(root);
        
        Console.WriteLine("\nBFS by level:");
        BFSByLevel(root);
        
        Console.WriteLine();
    }
    
    static void BFS(TreeNode root)
    {
        if (root == null) return;
        
        var queue = new Queue<TreeNode>();
        queue.Enqueue(root);
        var result = new List<int>();
        
        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            result.Add(node.Value);
            
            if (node.Left != null) queue.Enqueue(node.Left);
            if (node.Right != null) queue.Enqueue(node.Right);
        }
        
        Console.WriteLine($"  Order: {string.Join(" → ", result)}");
    }
    
    static void BFSByLevel(TreeNode root)
    {
        if (root == null) return;
        
        var queue = new Queue<TreeNode>();
        queue.Enqueue(root);
        int level = 0;
        
        while (queue.Count > 0)
        {
            int levelSize = queue.Count;
            var levelNodes = new List<int>();
            
            for (int i = 0; i < levelSize; i++)
            {
                var node = queue.Dequeue();
                levelNodes.Add(node.Value);
                
                if (node.Left != null) queue.Enqueue(node.Left);
                if (node.Right != null) queue.Enqueue(node.Right);
            }
            
            Console.WriteLine($"  Level {level}: [{string.Join(", ", levelNodes)}]");
            level++;
        }
    }
    
    static void Demo6_DFSWithStack()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 6: DFS with Stack - Pre-order Traversal                │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        var root = new TreeNode(1,
            new TreeNode(2, new TreeNode(4), new TreeNode(5)),
            new TreeNode(3, null, new TreeNode(6))
        );
        
        Console.WriteLine("DFS (pre-order) traversal using Stack:");
        DFS(root);
        
        Console.WriteLine("\nDFS recursive (for comparison):");
        var result = new List<int>();
        DFSRecursive(root, result);
        Console.WriteLine($"  Order: {string.Join(" → ", result)}");
        
        Console.WriteLine();
    }
    
    static void DFS(TreeNode root)
    {
        if (root == null) return;
        
        var stack = new Stack<TreeNode>();
        stack.Push(root);
        var result = new List<int>();
        
        while (stack.Count > 0)
        {
            var node = stack.Pop();
            result.Add(node.Value);
            
            // Push right first so left is processed first
            if (node.Right != null) stack.Push(node.Right);
            if (node.Left != null) stack.Push(node.Left);
        }
        
        Console.WriteLine($"  Order: {string.Join(" → ", result)}");
    }
    
    static void DFSRecursive(TreeNode? node, List<int> result)
    {
        if (node == null) return;
        
        result.Add(node.Value);
        DFSRecursive(node.Left, result);
        DFSRecursive(node.Right, result);
    }
    
    static void Demo7_ValidParentheses()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 7: Valid Parentheses - Classic Stack Problem           │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        var testCases = new[]
        {
            "()",
            "()[]{}",
            "(]",
            "([)]",
            "{[()]}",
            "((()))",
            "({[}])"
        };
        
        Console.WriteLine("Validating bracket strings:");
        foreach (var test in testCases)
        {
            bool valid = IsValidParentheses(test);
            Console.WriteLine($"  \"{test}\" → {(valid ? "✓ Valid" : "✗ Invalid")}");
        }
        
        Console.WriteLine();
    }
    
    static bool IsValidParentheses(string s)
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
    
    static void Demo8_QueueUsingStacks()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 8: Implement Queue using Two Stacks                    │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        var myQueue = new MyQueue<string>();
        
        Console.WriteLine("Enqueue: A, B, C");
        myQueue.Enqueue("A");
        myQueue.Enqueue("B");
        myQueue.Enqueue("C");
        
        Console.WriteLine($"Peek: {myQueue.Peek()}");
        Console.WriteLine($"Dequeue: {myQueue.Dequeue()}");
        Console.WriteLine($"Dequeue: {myQueue.Dequeue()}");
        
        Console.WriteLine("\nEnqueue: D, E");
        myQueue.Enqueue("D");
        myQueue.Enqueue("E");
        
        Console.WriteLine("Dequeue remaining:");
        while (!myQueue.IsEmpty)
        {
            Console.WriteLine($"  Dequeue: {myQueue.Dequeue()}");
        }
        
        Console.WriteLine();
    }
    
    static void Demo9_UndoRedoPattern()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 9: Undo/Redo Pattern with Stacks                       │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        var editor = new TextEditor();
        
        Console.WriteLine("Typing operations:");
        editor.Type("Hello");
        Console.WriteLine($"  Type 'Hello' → \"{editor.Text}\"");
        
        editor.Type("Hello World");
        Console.WriteLine($"  Type 'Hello World' → \"{editor.Text}\"");
        
        editor.Type("Hello World!");
        Console.WriteLine($"  Type 'Hello World!' → \"{editor.Text}\"");
        
        Console.WriteLine("\nUndo operations:");
        editor.Undo();
        Console.WriteLine($"  Undo → \"{editor.Text}\"");
        
        editor.Undo();
        Console.WriteLine($"  Undo → \"{editor.Text}\"");
        
        Console.WriteLine("\nRedo operations:");
        editor.Redo();
        Console.WriteLine($"  Redo → \"{editor.Text}\"");
        
        Console.WriteLine("\nNew type after undo (clears redo stack):");
        editor.Type("Hello Universe");
        Console.WriteLine($"  Type 'Hello Universe' → \"{editor.Text}\"");
        
        Console.WriteLine("  Try Redo:");
        editor.Redo();
        Console.WriteLine($"    Redo → \"{editor.Text}\" (no change, redo stack was cleared)");
        
        Console.WriteLine();
    }
    
    static void Demo10_FXTradingOrderQueue()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 10: FX Trading - Order Queue System                    │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        var orderProcessor = new OrderProcessor();
        
        // Submit orders
        Console.WriteLine("Submitting orders:");
        orderProcessor.SubmitOrder(new Order("O001", "EUR/USD", 100000m, "BUY"));
        orderProcessor.SubmitOrder(new Order("O002", "GBP/USD", 50000m, "SELL"));
        orderProcessor.SubmitOrder(new Order("O003", "USD/JPY", 200000m, "BUY"));
        orderProcessor.SubmitOrder(new Order("O004", "EUR/USD", 75000m, "SELL"));
        
        Console.WriteLine($"\nPending orders: {orderProcessor.PendingCount}");
        
        // Process orders
        Console.WriteLine("\nProcessing orders (FIFO):");
        while (orderProcessor.HasPendingOrders)
        {
            var order = orderProcessor.ProcessNextOrder();
            Console.WriteLine($"  Processed: {order}");
        }
        
        // Priority queue example (if .NET 6+)
        Console.WriteLine("\n--- Priority Queue (VIP orders first) ---");
        
        var priorityProcessor = new PriorityOrderProcessor();
        
        priorityProcessor.SubmitOrder(new Order("O101", "EUR/USD", 100000m, "BUY"), priority: 5);  // Normal
        priorityProcessor.SubmitOrder(new Order("O102", "GBP/USD", 500000m, "BUY"), priority: 1);  // VIP
        priorityProcessor.SubmitOrder(new Order("O103", "USD/JPY", 200000m, "SELL"), priority: 3); // Medium
        priorityProcessor.SubmitOrder(new Order("O104", "EUR/GBP", 1000000m, "BUY"), priority: 1); // VIP
        
        Console.WriteLine("\nProcessing by priority (lower = higher priority):");
        while (priorityProcessor.HasPendingOrders)
        {
            var order = priorityProcessor.ProcessNextOrder();
            Console.WriteLine($"  Processed: {order}");
        }
        
        Console.WriteLine();
    }
}

// Supporting classes

public class TreeNode
{
    public int Value { get; set; }
    public TreeNode? Left { get; set; }
    public TreeNode? Right { get; set; }
    
    public TreeNode(int value, TreeNode? left = null, TreeNode? right = null)
    {
        Value = value;
        Left = left;
        Right = right;
    }
}

public class MyQueue<T>
{
    private Stack<T> _inbox = new Stack<T>();
    private Stack<T> _outbox = new Stack<T>();
    
    public void Enqueue(T item)
    {
        _inbox.Push(item);
    }
    
    public T Dequeue()
    {
        EnsureOutbox();
        return _outbox.Pop();
    }
    
    public T Peek()
    {
        EnsureOutbox();
        return _outbox.Peek();
    }
    
    public bool IsEmpty => _inbox.Count == 0 && _outbox.Count == 0;
    
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

public class TextEditor
{
    private Stack<string> _undoStack = new Stack<string>();
    private Stack<string> _redoStack = new Stack<string>();
    
    public string Text { get; private set; } = "";
    
    public void Type(string newText)
    {
        _undoStack.Push(Text);
        Text = newText;
        _redoStack.Clear();
    }
    
    public void Undo()
    {
        if (_undoStack.Count > 0)
        {
            _redoStack.Push(Text);
            Text = _undoStack.Pop();
        }
    }
    
    public void Redo()
    {
        if (_redoStack.Count > 0)
        {
            _undoStack.Push(Text);
            Text = _redoStack.Pop();
        }
    }
}

public record Order(string OrderId, string CurrencyPair, decimal Amount, string Side)
{
    public override string ToString() => $"{OrderId}: {Side} {Amount:N0} {CurrencyPair}";
}

public class OrderProcessor
{
    private Queue<Order> _orderQueue = new Queue<Order>();
    
    public void SubmitOrder(Order order)
    {
        _orderQueue.Enqueue(order);
        Console.WriteLine($"  Submitted: {order}");
    }
    
    public Order ProcessNextOrder()
    {
        return _orderQueue.Dequeue();
    }
    
    public bool HasPendingOrders => _orderQueue.Count > 0;
    public int PendingCount => _orderQueue.Count;
}

public class PriorityOrderProcessor
{
    private PriorityQueue<Order, int> _priorityQueue = new PriorityQueue<Order, int>();
    
    public void SubmitOrder(Order order, int priority)
    {
        _priorityQueue.Enqueue(order, priority);
        Console.WriteLine($"  Submitted (priority {priority}): {order}");
    }
    
    public Order ProcessNextOrder()
    {
        return _priorityQueue.Dequeue();
    }
    
    public bool HasPendingOrders => _priorityQueue.Count > 0;
}
