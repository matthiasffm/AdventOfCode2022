namespace AdventOfCode2022;

using NUnit.Framework;
using FluentAssertions;

// TODO: this is a bit overcomplicated, just add a parent relation and put the path from humn to root in a queue
//       then walk it back and recalc the values

[TestFixture]
public class Day21
{
    private interface IMonkey
    {
        public string Name { get; }

        long Eval(IDictionary<string, IMonkey> monkeys);
    }

    private record SimpleMonkey(string Name, int Number) : IMonkey
    {
        public long Eval(IDictionary<string, IMonkey> monkeys)
        {
            return this.Number;
        }
    }

    private record MathMonkey(string Name, string Left, char Operation, string Right) : IMonkey
    {
        private long? _val; // not really immutable anymore like a record should be...

        public long Eval(IDictionary<string, IMonkey> monkeys)
        {
            // or course this works even without this mechanism, but only evaluating nodes one single time is slightly faster
            if(!_val.HasValue)
            {
                var left  = monkeys[Left].Eval(monkeys);
                var right = monkeys[Right].Eval(monkeys);

                _val = Operation switch {
                    '+' => left + right,
                    '-' => left - right,
                    '*' => left * right,
                    '/' => left / right,
                    _   => left,
                };
            }

            return _val.Value;
        }

        // calculates the right value in the equation if result and left value are known
        internal long CalcRight(IDictionary<string, IMonkey> monkeys, long result)
            => Operation switch {
                '+' => result - monkeys[Left].Eval(monkeys),
                '-' => monkeys[Left].Eval(monkeys) - result,
                '*' => result / monkeys[Left].Eval(monkeys),
                '/' => monkeys[Left].Eval(monkeys) / result,
                _   => monkeys[Left].Eval(monkeys),
            };

        // calculates the left value in the equation if result and right value are known
        internal long CalcLeft(IDictionary<string, IMonkey> monkeys, long targetValue)
            => Operation switch {
                '+' => targetValue - monkeys[Right].Eval(monkeys),
                '-' => targetValue + monkeys[Right].Eval(monkeys),
                '*' => targetValue / monkeys[Right].Eval(monkeys),
                '/' => targetValue * monkeys[Right].Eval(monkeys),
                _   => monkeys[Right].Eval(monkeys),
            };
    }

    private static IEnumerable<IMonkey> ParseData(string[] monkeyDeclarations)
        => monkeyDeclarations.Select(md => md.Split(new char[] {' ', ':'}, StringSplitOptions.RemoveEmptyEntries))
                             .Select(splits => splits.Length == 2 ?
                                               (IMonkey)new SimpleMonkey(splits[0], int.Parse(splits[1])) :
                                               (IMonkey)new MathMonkey(splits[0], splits[1], splits[2][0], splits[3]));

    [Test]
    public void TestSamples()
    {
        var data = new [] { 
            "root: pppw + sjmn",
            "dbpl: 5",
            "cczh: sllz + lgvd",
            "zczc: 2",
            "ptdq: humn - dvpt",
            "dvpt: 3",
            "lfqf: 4",
            "humn: 5",
            "ljgn: 2",
            "sjmn: drzm * dbpl",
            "sllz: 4",
            "pppw: cczh / lfqf",
            "lgvd: ljgn * ptdq",
            "drzm: hmdt - zczc",
            "hmdt: 32",
        };
        var monkeys = ParseData(data);

        Puzzle1(monkeys).Should().Be(152L);
        Puzzle2(monkeys).Should().Be(301L);
    }

    [Test]
    public void TestAocInput()
    {
        var data    = FileUtils.ReadAllLines(this);
        var monkeys = ParseData(data);

        Puzzle1(monkeys).Should().Be(21120928600114L);
        Puzzle2(monkeys).Should().Be(3453748220116L);
    }

    // Each monkey is given a job: either to yell a specific number or to yell the result of a math operation.
    // All of the number-yelling monkeys know their number from the start; however, the math operation monkeys need to
    // wait for two other monkeys to yell a number, and those two other monkeys might also be waiting on other monkeys.
    //
    // Puzzle == What number will the monkey named root yell?
    private static long Puzzle1(IEnumerable<IMonkey> monkeys)
    {
        var monkeyLookup = monkeys.ToDictionary(m => m.Name);

        return monkeyLookup["root"].Eval(monkeyLookup);
    }

    // Due to some kind of monkey-elephant-human mistranslation, you seem to have misunderstood a few key details about the riddle.
    // First, you got the wrong job for the monkey named root; specifically, you got the wrong math operation. The correct operation
    // for monkey root should be =, which means that it still listens for two numbers (from the same two monkeys as before), but now
    // checks that the two numbers match.
    // Second, you got the wrong monkey for the job named 'humn'. It isn't a monkey - it's you. Actually, you got the job wrong, too:
    // you need to figure out what number you need to yell so that root's equality check passes.
    //
    // Puzzle == What number do you yell to pass root's equality test?
    private static long Puzzle2(IEnumerable<IMonkey> monkeys)
    {
        // build 2 equation trees, then change the humn-Value in one to
        // determine if the math monkeys have to adjust their equation values left or right

        var monkeysOriginal = monkeys.ToDictionary(m => m.Name);
        monkeysOriginal["root"] = new MathMonkey("root", ((MathMonkey)monkeysOriginal["root"]).Left, '=', ((MathMonkey)monkeysOriginal["root"]).Right);

        var monkeysWithChangedHumanNmbr = monkeys.ToDictionary(m => m.Name);
        monkeysWithChangedHumanNmbr["root"] = new MathMonkey("root", ((MathMonkey)monkeysOriginal["root"]).Left, '=', ((MathMonkey)monkeysOriginal["root"]).Right);
        monkeysWithChangedHumanNmbr["humn"] = new SimpleMonkey("humn", -((SimpleMonkey)monkeysOriginal["humn"]).Number);

        // precalc all values for faster eval calls

        monkeysOriginal["root"].Eval(monkeysOriginal);
        monkeysWithChangedHumanNmbr["root"].Eval(monkeysWithChangedHumanNmbr);

        // work from the root down the branch where humn is

        var currentNode = (MathMonkey)monkeysOriginal["root"]!;
        var targetValue = 0L;

        do
        {
            IMonkey nextNode;

            if(monkeysOriginal[currentNode.Left].Eval(monkeysOriginal) ==
               monkeysWithChangedHumanNmbr[currentNode.Left].Eval(monkeysWithChangedHumanNmbr))
            {
                targetValue = currentNode.CalcRight(monkeysOriginal, targetValue);
                nextNode    = monkeysOriginal[currentNode.Right];
            }
            else
            {
                targetValue = currentNode.CalcLeft(monkeysOriginal, targetValue);
                nextNode    = monkeysOriginal[currentNode.Left];
            }

            if(nextNode is SimpleMonkey)
            {
                return targetValue;
            }
            else
            {
                currentNode = (MathMonkey)nextNode;
            }
        }
        while(true);
    }
}
