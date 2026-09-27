using System.Collections;
using System.Linq;

namespace LINQPractice1
{
    public record Trade(string TradeId, string Pair, decimal Amount, string Side);

    public record Client(string Id, List<Order> Orders);

    public record Order(string OrderId, decimal Amount);

    public record Employee(
    int Id,
    string Name,
    string Department,
    List<string> Skills,
    decimal Salary
);

    public class Program
    {
        static List<Trade> GetSampleTrades()
        {
            return new List<Trade>
        {
            new Trade("T001", "EUR/USD", 100000m, "BUY"),
            new Trade("T002", "EUR/USD", 150000m, "SELL"),
            new Trade("T003", "GBP/USD", 200000m, "BUY"),
            new Trade("T004", "EUR/USD", 75000m, "BUY"),
            new Trade("T005", "USD/JPY", 300000m, "SELL"),
            new Trade("T006", "GBP/USD", 125000m, "SELL"),
            new Trade("T007", "EUR/USD", 250000m, "BUY"),
            new Trade("T008", "USD/JPY", 180000m, "BUY"),
            new Trade("T009", "AUD/USD", 90000m, "SELL"),
            new Trade("T010", "EUR/USD", 50000m, "SELL"),
            new Trade("T011", "GBP/USD", 175000m, "BUY"),
            new Trade("T012", "USD/JPY", 220000m, "SELL")
        };
        }
        public static void Main()
        {
            var trades = GetSampleTrades();
            // var eurTrades = trades.Where(x => string.Equals(x.Pair, "EUR/USD"));

            // Console.WriteLine(string.Join("\n", eurTrades));
            //System.Console.WriteLine("======================================");

            // var eurTradesWithAmount = trades.Where(x => string.Equals(x.Pair, "EUR/USD") && x.Amount > 125000);
            // Console.WriteLine(string.Join("\n", eurTradesWithAmount));

            //System.Console.WriteLine("======================================");
            // var distinctPairs = trades.DistinctBy(x => x.Pair)
            //                         .Count();
            // System.Console.WriteLine(distinctPairs);
            // System.Console.WriteLine("======================================");


            // var selectDemo = trades.Select(x => string.Concat(x.Pair, "=>", x.Side)).Distinct();
            // Console.WriteLine(string.Join("\n", selectDemo));

            // System.Console.WriteLine("======================================");


            // var selectDemo2 = trades.
            //                     Take(3).
            //                     Select(
            //                         x => new
            //                         {
            //                             x.TradeId,
            //                             x.Amount,
            //                             TransFormmedSide = string.Equals(x.Side, "BUY", StringComparison.OrdinalIgnoreCase) ? "B" : "S",
            //                             TransformedPair = x.Pair.Replace("/", " - ")
            //                         }
            //                     ).ToList();
            // System.Console.WriteLine(selectDemo2[0].GetType());
            // System.Console.WriteLine(string.Join("\n", selectDemo2));

            // var selectDemo3 = trades.Select((x, i) => string.Concat(i, "=>", x.TradeId));
            // System.Console.WriteLine(String.Join(("\n"), selectDemo3));



            //     var clients = new List<Client>
            // {
            //     new Client("C1", new List<Order> { new Order("O1", 1000), new Order("O2", 2000) }),
            //     new Client("C2", new List<Order> { new Order("O3", 3000) }),
            //     new Client("C3", new List<Order> { new Order("O4", 4000), new Order("O5", 5000) })
            // };

            // var numbers = new List<List<int>>() { new List<int>() { 1, 2, 3, 4 }, new List<int>() { 1, 2, 3, 4 }, new List<int>() { 1, 2, 3, 4 } };
            // var flatternNumbers = numbers.SelectMany(x => x);
            // System.Console.WriteLine(string.Join(",", flatternNumbers));

            // var selectDemo4 = clients.SelectMany(x => x.Orders).Count();
            // System.Console.WriteLine(selectDemo4);


            //GroupBy

            // var groupByDemo1 = trades.GroupBy(x => x.Pair);
            // foreach (var group in groupByDemo1)
            // {
            //     Console.WriteLine($"  {group.Key}: {string.Join("\n", group.ToList())}:  trades");
            // }

            // var groupByDemo2 = trades.GroupBy(x => x.Pair)
            // .Select(g =>
            //         new
            //         {
            //             Pair = g.Key,
            //             Count = g.Count(),
            //             Sum = g.Sum(x => x.Amount),
            //             Average = g.Average(x => x.Amount).ToString("F2")
            //         });

            // foreach (var item in groupByDemo2)
            // {
            //     System.Console.WriteLine($"{item}");
            // }

            var groupByDemo3 = trades.GroupBy(x => x.Pair, x => x.Amount);
            foreach (var group in groupByDemo3)
            {
                Console.WriteLine($"  {group.Key}: {string.Join("\n", group.ToList())}:  trades");
            }

            // var groupByDemo4 = trades.ToLookup(x => x.Pair);
            // foreach (var group in groupByDemo4)
            // {
            //     Console.WriteLine($"  {group.Key}: {string.Join("\n", group.ToList())}:  trades");
            // }


            var tradesNew = new[]
            {
            new { TradeId = "T1", Pair = "EUR/USD", Amount = 100000m },
            new { TradeId = "T2", Pair = "GBP/USD", Amount = 50000m },
            new { TradeId = "T3", Pair = "EUR/USD", Amount = 200000m },
            new { TradeId = "T4", Pair = "USD/JPY", Amount = 150000m }
        };

            var rates = new[]
            {
            new { Pair = "EUR/USD", Rate = 1.0850m },
            new { Pair = "GBP/USD", Rate = 1.2650m },
            new { Pair = "USD/JPY", Rate = 149.50m }
        };

            // var jointable = tradesNew.join(
            //     rates,
            //     t => t.Pair,
            //     r => r.Pair,
            //     (trade, rate) =>
            //     new
            //     {
            //         TradeId = trade.TradeId,
            //         Pair = trade.Pair,
            //         Rate = rate.Rate
            //     }
            // );

            // foreach (var item in jointable)
            // {
            //     System.Console.WriteLine(item);
            // }

            var jointable = tradesNew.GroupJoin(
                rates,
                t => t.Pair,
                r => r.Pair,
                (trade, rateGroup) =>
                new
                {
                    TradeId = trade.TradeId,
                    Pair = trade.Pair,
                    Rate = rateGroup
                }
            );

            foreach (var item in jointable)
            {
                System.Console.WriteLine(item);
            }



            var company = new Dictionary<string, Dictionary<string, List<Employee>>>
            {
                ["HDFC"] = new Dictionary<string, List<Employee>>
                {
                    ["IT"] = new List<Employee>
                    {
                        new Employee(1, "Hemant", "IT", new List<string>{"C#", "SQL", "Azure"}, 90000),
                        new Employee(2, "Amit", "IT", new List<string>{"Java", "Spring"}, 80000)
                    },
                    ["HR"] = new List<Employee>
                    {
                        new Employee(3, "Neha", "HR", new List<string>{"Hiring", "Communication"}, 60000)
                    }
                },

                ["ICICI"] = new Dictionary<string, List<Employee>>
                {
                    ["IT"] = new List<Employee>
                    {
                        new Employee(4, "Raj", "IT", new List<string>{"C#", "Angular"}, 95000)
                    },
                    ["Finance"] = new List<Employee>
                    {
                        new Employee(5, "Simran", "Finance", new List<string>{"Accounting", "Excel"}, 70000)
                    }
                }
            };

            // Company
            // ├── HDFC
            // │    ├── IT
            // │    │    ├── Hemant
            // │    │    ├── Amit
            // │    ├── HR
            // │         ├── Neha
            // │
            // ├── ICICI
            //     ├── IT
            //     │    ├── Raj
            //     ├── Finance
            //         ├── Simran


            // // to get all employees form all companies and Departments

            // var names = company
            // .SelectMany(x => x.Value)
            // .SelectMany(x => x.Value).Select(x => x.Name);

            // System.Console.WriteLine(string.Join("\n", names));


            // to get all skills of all employees form all companies and Departments

            // var skills = company
            // .SelectMany(x => x.Value)
            // .SelectMany(x => x.Value)
            // .SelectMany(x => x.Skills)
            // .Distinct();
            // System.Console.WriteLine(string.Join("\n", skills));
        }
    }

}
