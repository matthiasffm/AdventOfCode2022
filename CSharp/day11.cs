namespace AdventOfCode2022;

using NUnit.Framework;
using FluentAssertions;

[TestFixture]
public class Day11
{
    interface IWorryOperation { long Calc(long old); }

    private readonly struct AddWorryOp : IWorryOperation
    {
        private readonly long _toAdd;
        public AddWorryOp(long toAdd) { _toAdd = toAdd; }
        public long Calc(long oldWorryLevel) => _toAdd + oldWorryLevel;
    }

    private readonly struct MultiplyWorryOp : IWorryOperation
    {
        private readonly long _factor;
        public MultiplyWorryOp(long factor) { _factor = factor; }
        public long Calc(long oldWorryLevel) => _factor * oldWorryLevel;
    }

    private readonly struct SquareWorryOp : IWorryOperation
    {
        public SquareWorryOp() { }
        public long Calc(long oldWorryLevel) => oldWorryLevel * oldWorryLevel;
    }

    record struct Test(long DivisibleBy, int ThrowToMonkeyIfTrue, int ThrowToMonkeyIfFalse)
    {
        public int ThrowToNext(long worryLevel) => (worryLevel % DivisibleBy == 0) ? ThrowToMonkeyIfTrue : ThrowToMonkeyIfFalse;
    }

    record class Monkey(int Id, Queue<long> Items, IWorryOperation Op, Test Test)
    {
        public Monkey CloneWithQueue() => this with { Items = new Queue<long>(this.Items)};
    }

    private static IEnumerable<Monkey> ParseData(string data, string separator)
    {
        List<Monkey> monkeys = new();

        foreach(var monkeySpec in data.Split(separator + separator))
        {
            var monkeyLines = monkeySpec.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            var id = int.Parse(monkeyLines[0][7..8]); // only single digit ids

            var items = monkeyLines[1][18..].Split(", ")
                                            .Select(idx => long.Parse(idx));

            IWorryOperation op = monkeyLines[2][23..] switch {
                "* old"   => new SquareWorryOp(),
                ['*', ..] => new MultiplyWorryOp(int.Parse(monkeyLines[2][25..])),
                _         => new AddWorryOp(int.Parse(monkeyLines[2][25..])),
            };

            var test = new Test(int.Parse(monkeyLines[3][21..]),
                                int.Parse(monkeyLines[4][29..]),
                                int.Parse(monkeyLines[5][30..]));

            monkeys.Add(new Monkey(id, new Queue<long>(items), op, test));
        }

        return monkeys;
    }

    [Test]
    public void TestSamples()
    {
        var data = """
            Monkey 0:
              Starting items: 79, 98
              Operation: new = old * 19
              Test: divisible by 23
                If true: throw to monkey 2
                If false: throw to monkey 3

            Monkey 1:
              Starting items: 54, 65, 75, 74
              Operation: new = old + 6
              Test: divisible by 19
                If true: throw to monkey 2
                If false: throw to monkey 0

            Monkey 2:
              Starting items: 79, 60, 97
              Operation: new = old * old
              Test: divisible by 13
                If true: throw to monkey 1
                If false: throw to monkey 3

            Monkey 3:
              Starting items: 74
              Operation: new = old + 3
              Test: divisible by 17
                If true: throw to monkey 0
                If false: throw to monkey 1
            """;

        var monkeys = ParseData(data, Environment.NewLine);

        Puzzle1(monkeys).Should().Be(10605);
        Puzzle2(monkeys).Should().Be(2713310158L);
    }

    [Test]
    public void TestAocInput()
    {
        var data = FileUtils.ReadAllText(this);
        var monkeys = ParseData(data, "\n");

        Puzzle1(monkeys).Should().Be(99840);
        Puzzle2(monkeys).Should().Be(20683044837);
    }

    // Monkeys are playing Keep Away with your missing things! In this play the monkeys operate based on how worried you are about each item.
    // Each monkey has several attributes:
    // - 'Starting items' lists your worry level for each item the monkey is currently holding in the order they will be inspected.
    // - 'Operation' shows how your worry level changes as that monkey inspects an item.
    // - 'Test' shows how the monkey uses your worry level to decide where to throw an item next.
    // After each monkey inspects an item but before it tests your worry level, your relief that the monkey's inspection didn't damage the item causes
    // your worry level to be divided by three and rounded down to the nearest integer.
    // The monkeys take turns inspecting and throwing items. On a single monkey's turn, it inspects and throws all of the items it is holding one at a
    // time and in the order listed. Monkey 0 goes first, then monkey 1, and so on until each monkey has had one turn. The process of each monkey taking
    // a single turn is called a round.
    // Chasing all of the monkeys at once is impossible; focus on the two most active monkeys if you want any hope of getting your stuff back. Multiplying
    // their level is called the monkey business in this situation.
    //
    // Puzzle == What is the level of monkey business after 20 rounds of stuff-slinging simian shenanigans?
    private static long Puzzle1(IEnumerable<Monkey> monkeys)
        => MonkeyBusiness(monkeys.Select(m => m.CloneWithQueue()).ToArray(), 3, 20);

    // You're worried you might not ever get your items back. So worried, in fact, that your relief that a monkey's inspection didn't damage an item no
    // longer causes your worry level to be divided by three.
    //
    // Puzzle == What is the level of monkey business after 10000 rounds?
    private static long Puzzle2(IEnumerable<Monkey> monkeys)
        => MonkeyBusiness(monkeys.Select(m => m.CloneWithQueue()).ToArray(), 1, 10000);

    private static long MonkeyBusiness(Monkey[] monkeys, long div, int rounds)
    {
        var inspected = Enumerable.Repeat(0L, monkeys.Length).ToArray();

        // all divide by tests for the monkeys are prime
        // so to limit the growth of the worry levels to manageable numbers just do all calculations
        // inside the ring of the product of DivisibleBy of all monkeys

        long worryRing = monkeys.Aggregate(1L, (p, m) => p * m.Test.DivisibleBy);

        for(int round = 1; round <= rounds; round++)
        {
            for(int m = 0; m < monkeys.Length; m++)
            {
                var monkey = monkeys[m];

                while(monkey.Items.Count > 0)
                {
                    inspected[m]++;

                    var worryLevelOfItem = monkey.Items.Dequeue();
                    worryLevelOfItem = (monkey.Op.Calc(worryLevelOfItem) / div) % worryRing;

                    var throwTo = monkey.Test.ThrowToNext(worryLevelOfItem);
                    monkeys[throwTo].Items.Enqueue(worryLevelOfItem);
                }
            }
        }

        var mostActiveMonkeys = inspected.OrderByDescending(i => i).Take(2);
        return mostActiveMonkeys.First() * mostActiveMonkeys.Last();
    }
}
