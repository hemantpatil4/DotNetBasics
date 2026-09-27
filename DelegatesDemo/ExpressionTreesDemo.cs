/*
╔══════════════════════════════════════════════════════════════════════════════╗
║                    EXPRESSION TREES - COMPREHENSIVE DEMO                      ║
║                                                                               ║
║  Understanding Code as Data                                                   ║
║  Run with: dotnet run ExpressionTreesDemo.cs                                  ║
╚══════════════════════════════════════════════════════════════════════════════╝
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ExpressionTreesDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║           EXPRESSION TREES - COMPREHENSIVE DEMO              ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");

            Demo1_DelegateVsExpression();
             Demo2_InspectingExpressionTree();
            // Demo3_BuildingExpressionManually();
            // Demo4_BuildingPropertyAccess();
            // Demo5_CompilingExpressions();
            // Demo6_DynamicFilterBuilder();
            // Demo7_CombiningFilters();
            // Demo8_ExpressionVisitorDemo();
            // Demo9_GetPropertyNamePattern();
            // Demo10_FXTradingExample();

            Console.WriteLine("\n✓ All demos completed!");
        }

        // ════════════════════════════════════════════════════════════════════
        // DEMO 1: Delegate vs Expression Tree
        // ════════════════════════════════════════════════════════════════════
        static void Demo1_DelegateVsExpression()
        {
            PrintHeader("DEMO 1: Delegate vs Expression Tree");

            // Delegate - compiled code (black box)
            Func<int, int> delegateFunc = x => x * 2;
            Console.WriteLine($"  Delegate type: {delegateFunc.GetType().Name}");
            Console.WriteLine($"  Delegate result for 5: {delegateFunc(5)}");
            Console.WriteLine($"  Can inspect? NO - it's compiled IL code\n");

            // Expression Tree - data structure (can inspect)
            Expression<Func<int, int>> expressionTree = x => x * 2;
            Console.WriteLine($"  Expression type: {expressionTree.GetType().Name}");
            Console.WriteLine($"  Expression body: {expressionTree.Body}");
            Console.WriteLine($"  Expression body type: {expressionTree.Body.GetType().Name}");
            Console.WriteLine($"  Node type: {expressionTree.Body.NodeType}");
            Console.WriteLine($"  Can inspect? YES - it's a data structure!");

            Console.WriteLine();
        }

        // ════════════════════════════════════════════════════════════════════
        // DEMO 2: Inspecting Expression Tree Structure
        // ════════════════════════════════════════════════════════════════════
        static void Demo2_InspectingExpressionTree()
        {
            PrintHeader("DEMO 2: Inspecting Expression Tree Structure");

            Expression<Func<int, bool>> expr = x => x > 5 && x < 10;
            Console.WriteLine($"  Expression: {expr}");
            Console.WriteLine($"\n  Tree Structure:");

            // The body is a BinaryExpression (AndAlso)
            if (expr.Body is BinaryExpression andAlso)
            {
                Console.WriteLine($"    Root: {andAlso.NodeType}");

                // Left side: x > 5
                if (andAlso.Left is BinaryExpression left)
                {
                    Console.WriteLine($"      Left: {left.NodeType}");
                    Console.WriteLine($"        Left operand: {left.Left} ({left.Left.NodeType})");
                    Console.WriteLine($"        Right operand: {left.Right} ({left.Right.NodeType})");
                }

                // Right side: x < 10
                if (andAlso.Right is BinaryExpression right)
                {
                    Console.WriteLine($"      Right: {right.NodeType}");
                    Console.WriteLine($"        Left operand: {right.Left} ({right.Left.NodeType})");
                    Console.WriteLine($"        Right operand: {right.Right} ({right.Right.NodeType})");
                }
            }

            Console.WriteLine();
        }

        // ════════════════════════════════════════════════════════════════════
        // DEMO 3: Building Expression Tree Manually
        // ════════════════════════════════════════════════════════════════════
        static void Demo3_BuildingExpressionManually()
        {
            PrintHeader("DEMO 3: Building Expression Tree Manually");

            Console.WriteLine("  Building: x => x * 2 + 1\n");

            // Step 1: Create parameter
            ParameterExpression x = Expression.Parameter(typeof(int), "x");
            Console.WriteLine($"  Step 1 - Parameter: {x}");

            // Step 2: Create constant 2
            ConstantExpression two = Expression.Constant(2, typeof(int));
            Console.WriteLine($"  Step 2 - Constant: {two}");

            // Step 3: Create multiply (x * 2)
            BinaryExpression multiply = Expression.Multiply(x, two);
            Console.WriteLine($"  Step 3 - Multiply: {multiply}");

            // Step 4: Create constant 1
            ConstantExpression one = Expression.Constant(1, typeof(int));
            Console.WriteLine($"  Step 4 - Constant: {one}");

            // Step 5: Create add ((x * 2) + 1)
            BinaryExpression add = Expression.Add(multiply, one);
            Console.WriteLine($"  Step 5 - Add: {add}");

            // Step 6: Create lambda
            var lambda = Expression.Lambda<Func<int, int>>(add, x);
            Console.WriteLine($"\n  Final Lambda: {lambda}");

            // Compile and test
            var compiled = lambda.Compile();
            Console.WriteLine($"  Result for x=5: {compiled(5)}");  // Should be 11

            Console.WriteLine();
        }

        // ════════════════════════════════════════════════════════════════════
        // DEMO 4: Building Property Access Expression
        // ════════════════════════════════════════════════════════════════════
        static void Demo4_BuildingPropertyAccess()
        {
            PrintHeader("DEMO 4: Building Property Access Expression");

            Console.WriteLine("  Building: trade => trade.Amount > 100000\n");

            // Parameter
            ParameterExpression trade = Expression.Parameter(typeof(Trade), "trade");
            Console.WriteLine($"  Parameter: {trade}");

            // Property access: trade.Amount
            MemberExpression amountProperty = Expression.Property(trade, nameof(Trade.Amount));
            Console.WriteLine($"  Property access: {amountProperty}");

            // Constant
            ConstantExpression threshold = Expression.Constant(100000m, typeof(decimal));
            Console.WriteLine($"  Constant: {threshold}");

            // Comparison
            BinaryExpression comparison = Expression.GreaterThan(amountProperty, threshold);
            Console.WriteLine($"  Comparison: {comparison}");

            // Lambda
            var lambda = Expression.Lambda<Func<Trade, bool>>(comparison, trade);
            Console.WriteLine($"\n  Final Lambda: {lambda}");

            // Test
            var filter = lambda.Compile();
            var largeTrade = new Trade("EUR/USD", 150000m);
            var smallTrade = new Trade("GBP/USD", 50000m);

            Console.WriteLine($"\n  Testing:");
            Console.WriteLine($"    Trade EUR/USD 150000: {filter(largeTrade)}");  // true
            Console.WriteLine($"    Trade GBP/USD 50000: {filter(smallTrade)}");   // false

            Console.WriteLine();
        }

        // ════════════════════════════════════════════════════════════════════
        // DEMO 5: Compiling Expressions
        // ════════════════════════════════════════════════════════════════════
        static void Demo5_CompilingExpressions()
        {
            PrintHeader("DEMO 5: Compiling Expressions");

            Expression<Func<int, int, int>> addExpr = (a, b) => a + b;
            Console.WriteLine($"  Expression: {addExpr}");

            // Compile to delegate
            Func<int, int, int> addFunc = addExpr.Compile();
            Console.WriteLine($"\n  Compiled successfully!");
            Console.WriteLine($"  Result of 10 + 20: {addFunc(10, 20)}");

            // Performance note
            Console.WriteLine("\n  ⚠️ Performance Note:");
            Console.WriteLine("    - Compile() is expensive (JIT compilation)");
            Console.WriteLine("    - Compile ONCE, reuse many times");
            Console.WriteLine("    - DON'T compile in loops!");

            Console.WriteLine();
        }

        // ════════════════════════════════════════════════════════════════════
        // DEMO 6: Dynamic Filter Builder
        // ════════════════════════════════════════════════════════════════════
        static void Demo6_DynamicFilterBuilder()
        {
            PrintHeader("DEMO 6: Dynamic Filter Builder");

            Console.WriteLine("  Building filters at runtime based on user input:\n");

            // Simulate runtime filter criteria
            var filters = new[]
            {
                ("Amount", ">", (object)100000m),
                ("CurrencyPair", "==", (object)"EUR/USD"),
            };

            foreach (var (property, op, value) in filters)
            {
                var filter = BuildFilter<Trade>(property, op, value);
                Console.WriteLine($"  Filter: {filter}");
            }

            // Test with sample data
            var trades = new List<Trade>
            {
                new("EUR/USD", 150000m),
                new("EUR/USD", 50000m),
                new("GBP/USD", 200000m),
            };

            Console.WriteLine("\n  Testing Amount > 100000:");
            var amountFilter = BuildFilter<Trade>("Amount", ">", 100000m).Compile();
            foreach (var trade in trades)
            {
                Console.WriteLine($"    {trade.CurrencyPair} {trade.Amount}: {amountFilter(trade)}");
            }

            Console.WriteLine();
        }

        static Expression<Func<T, bool>> BuildFilter<T>(string propertyName, string operation, object value)
        {
            var param = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(param, propertyName);
            var constant = Expression.Constant(value);

            Expression comparison = operation switch
            {
                "==" => Expression.Equal(property, constant),
                "!=" => Expression.NotEqual(property, constant),
                ">" => Expression.GreaterThan(property, constant),
                ">=" => Expression.GreaterThanOrEqual(property, constant),
                "<" => Expression.LessThan(property, constant),
                "<=" => Expression.LessThanOrEqual(property, constant),
                _ => throw new ArgumentException($"Unknown operation: {operation}")
            };

            return Expression.Lambda<Func<T, bool>>(comparison, param);
        }

        // ════════════════════════════════════════════════════════════════════
        // DEMO 7: Combining Filters (AND/OR)
        // ════════════════════════════════════════════════════════════════════
        static void Demo7_CombiningFilters()
        {
            PrintHeader("DEMO 7: Combining Filters (AND/OR)");

            Expression<Func<Trade, bool>> filter1 = t => t.Amount > 100000;
            Expression<Func<Trade, bool>> filter2 = t => t.CurrencyPair == "EUR/USD";

            Console.WriteLine($"  Filter 1: {filter1}");
            Console.WriteLine($"  Filter 2: {filter2}");

            // Combine with AND
            var combinedAnd = CombineAnd(filter1, filter2);
            Console.WriteLine($"\n  Combined (AND): {combinedAnd}");

            // Combine with OR
            var combinedOr = CombineOr(filter1, filter2);
            Console.WriteLine($"  Combined (OR): {combinedOr}");

            // Test
            var trades = new List<Trade>
            {
                new("EUR/USD", 150000m),  // Both true
                new("EUR/USD", 50000m),   // Only pair matches
                new("GBP/USD", 200000m),  // Only amount matches
                new("GBP/USD", 50000m),   // Neither matches
            };

            Console.WriteLine("\n  Testing Combined Filters:");
            var andFunc = combinedAnd.Compile();
            var orFunc = combinedOr.Compile();

            foreach (var trade in trades)
            {
                Console.WriteLine($"    {trade.CurrencyPair} {trade.Amount,-10} AND:{andFunc(trade),-6} OR:{orFunc(trade)}");
            }

            Console.WriteLine();
        }

        static Expression<Func<T, bool>> CombineAnd<T>(
            Expression<Func<T, bool>> left,
            Expression<Func<T, bool>> right)
        {
            var param = left.Parameters[0];
            var rightBody = new ParameterReplacer(right.Parameters[0], param).Visit(right.Body);
            var body = Expression.AndAlso(left.Body, rightBody);
            return Expression.Lambda<Func<T, bool>>(body, param);
        }

        static Expression<Func<T, bool>> CombineOr<T>(
            Expression<Func<T, bool>> left,
            Expression<Func<T, bool>> right)
        {
            var param = left.Parameters[0];
            var rightBody = new ParameterReplacer(right.Parameters[0], param).Visit(right.Body);
            var body = Expression.OrElse(left.Body, rightBody);
            return Expression.Lambda<Func<T, bool>>(body, param);
        }

        // ════════════════════════════════════════════════════════════════════
        // DEMO 8: ExpressionVisitor Demo
        // ════════════════════════════════════════════════════════════════════
        static void Demo8_ExpressionVisitorDemo()
        {
            PrintHeader("DEMO 8: ExpressionVisitor - Analyzing Expression Trees");

            Expression<Func<Trade, bool>> expr = t => t.Amount > 100000 && t.CurrencyPair == "EUR/USD";
            Console.WriteLine($"  Expression: {expr}\n");
            Console.WriteLine("  Tree Structure (using ExpressionVisitor):\n");

            var printer = new ExpressionPrinter();
            printer.Print(expr);

            Console.WriteLine();
        }

        // ════════════════════════════════════════════════════════════════════
        // DEMO 9: Get Property Name Pattern
        // ════════════════════════════════════════════════════════════════════
        static void Demo9_GetPropertyNamePattern()
        {
            PrintHeader("DEMO 9: Get Property Name Pattern (Used by EF, AutoMapper, etc.)");

            Console.WriteLine("  This pattern extracts property names from expressions.\n");
            Console.WriteLine("  Used by:");
            Console.WriteLine("    - EF Core: .Include(x => x.Orders)");
            Console.WriteLine("    - FluentValidation: RuleFor(x => x.Name)");
            Console.WriteLine("    - AutoMapper: ForMember(x => x.Name, ...)\n");

            // Examples
            string prop1 = GetPropertyName<Trade, decimal>(t => t.Amount);
            string prop2 = GetPropertyName<Trade, string>(t => t.CurrencyPair);

            Console.WriteLine($"  GetPropertyName(t => t.Amount): \"{prop1}\"");
            Console.WriteLine($"  GetPropertyName(t => t.CurrencyPair): \"{prop2}\"");

            Console.WriteLine();
        }

        static string GetPropertyName<T, TProperty>(Expression<Func<T, TProperty>> propertyExpression)
        {
            if (propertyExpression.Body is MemberExpression member)
            {
                return member.Member.Name;
            }

            if (propertyExpression.Body is UnaryExpression unary &&
                unary.Operand is MemberExpression unaryMember)
            {
                return unaryMember.Member.Name;
            }

            throw new ArgumentException("Expression must be a property access");
        }

        // ════════════════════════════════════════════════════════════════════
        // DEMO 10: FX Trading Real-World Example
        // ════════════════════════════════════════════════════════════════════
        static void Demo10_FXTradingExample()
        {
            PrintHeader("DEMO 10: FX Trading - Building Dynamic Search");

            var trades = new List<Trade>
            {
                new("EUR/USD", 100000m) { Status = "Executed" },
                new("EUR/USD", 250000m) { Status = "Pending" },
                new("GBP/USD", 150000m) { Status = "Executed" },
                new("USD/JPY", 500000m) { Status = "Cancelled" },
                new("EUR/USD", 75000m) { Status = "Executed" },
            };

            Console.WriteLine("  All Trades:");
            foreach (var t in trades)
                Console.WriteLine($"    {t.CurrencyPair} | {t.Amount,-10:N0} | {t.Status}");

            // Simulate user search criteria (runtime values)
            string? searchPair = "EUR/USD";
            decimal? minAmount = 100000m;
            string? searchStatus = "Executed";

            Console.WriteLine($"\n  Search Criteria:");
            Console.WriteLine($"    Pair: {searchPair ?? "Any"}");
            Console.WriteLine($"    Min Amount: {minAmount?.ToString("N0") ?? "Any"}");
            Console.WriteLine($"    Status: {searchStatus ?? "Any"}");

            // Build dynamic filter
            var predicate = PredicateBuilder.True<Trade>();

            if (!string.IsNullOrEmpty(searchPair))
                predicate = CombineAnd(predicate, t => t.CurrencyPair == searchPair);

            if (minAmount.HasValue)
                predicate = CombineAnd(predicate, t => t.Amount >= minAmount.Value);

            if (!string.IsNullOrEmpty(searchStatus))
                predicate = CombineAnd(predicate, t => t.Status == searchStatus);

            Console.WriteLine($"\n  Generated Filter: {predicate}");

            // Execute
            var results = trades.AsQueryable().Where(predicate).ToList();

            Console.WriteLine($"\n  Results ({results.Count} found):");
            foreach (var t in results)
                Console.WriteLine($"    {t.CurrencyPair} | {t.Amount,-10:N0} | {t.Status}");

            Console.WriteLine();
        }

        // ════════════════════════════════════════════════════════════════════
        // HELPER
        // ════════════════════════════════════════════════════════════════════
        static void PrintHeader(string title)
        {
            Console.WriteLine($"═══ {title} ═══\n");
        }
    }

    // ════════════════════════════════════════════════════════════════════
    // Trade model
    // ════════════════════════════════════════════════════════════════════
    public class Trade
    {
        public string CurrencyPair { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending";

        public Trade(string pair, decimal amount)
        {
            CurrencyPair = pair;
            Amount = amount;
        }
    }

    // ════════════════════════════════════════════════════════════════════
    // ExpressionVisitor for printing tree structure
    // ════════════════════════════════════════════════════════════════════
    public class ExpressionPrinter : ExpressionVisitor
    {
        private int _indent = 0;

        public void Print(Expression expression) => Visit(expression);

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            PrintIndented($"Lambda ({node.Parameters.Count} params)");
            _indent++;
            foreach (var param in node.Parameters)
                PrintIndented($"Parameter: {param.Name} ({param.Type.Name})");
            Visit(node.Body);
            _indent--;
            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            PrintIndented($"Binary: {node.NodeType}");
            _indent++;
            Visit(node.Left);
            Visit(node.Right);
            _indent--;
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            PrintIndented($"Constant: {node.Value} ({node.Type.Name})");
            return node;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            PrintIndented($"Parameter: {node.Name}");
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            PrintIndented($"Member: {node.Member.Name}");
            _indent++;
            if (node.Expression != null)
                Visit(node.Expression);
            _indent--;
            return node;
        }

        private void PrintIndented(string text)
        {
            Console.WriteLine(new string(' ', _indent * 2) + text);
        }
    }

    // ════════════════════════════════════════════════════════════════════
    // Parameter Replacer (for combining expressions)
    // ════════════════════════════════════════════════════════════════════
    public class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParam;
        private readonly ParameterExpression _newParam;

        public ParameterReplacer(ParameterExpression oldParam, ParameterExpression newParam)
        {
            _oldParam = oldParam;
            _newParam = newParam;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _oldParam ? _newParam : base.VisitParameter(node);
        }
    }

    // ════════════════════════════════════════════════════════════════════
    // PredicateBuilder helper
    // ════════════════════════════════════════════════════════════════════
    public static class PredicateBuilder
    {
        public static Expression<Func<T, bool>> True<T>() => x => true;
        public static Expression<Func<T, bool>> False<T>() => x => false;
    }
}
