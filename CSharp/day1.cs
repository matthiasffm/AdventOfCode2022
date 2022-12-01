namespace AdventOfCode2022;

using NUnit.Framework;
using FluentAssertions;

using static Console;

[TestFixture]
public class Day01
{
    [Test]
    public void TestSamples()
    {
        var data = new [] { 
            "1000",
            "2000",
            "3000",
            "",
            "4000",
            "",
            "5000",
            "6000",
            "",
            "7000",
            "8000",
            "9000",
            "",
            "10000",
         };
        var elves = ParseData(data);

        WriteLine("Day 01 -- Beispiele --");

        Puzzle1(elves).Should().Be(7000 + 8000 + 9000);
        Puzzle2(elves).Should().Be(10000 + 7000 + 8000 + 9000 + 5000 + 6000);
    }

    [Test]
    public void TestAocInput()
    {
        var data  = File.ReadAllLines(@"day1.data");
        var elves = ParseData(data);

        WriteLine("Day 01 -- richtige Eingaben --");

        Puzzle1(elves).Should().Be(64929);
        Puzzle2(elves).Should().Be(193697);
    }

    // parsed the input data and returns all calories every elv carries
    private IEnumerable<IEnumerable<int>> ParseData(IEnumerable<string> data) =>
        string.Join(',', data.Select(d => d == string.Empty ? "#" : d))
              .Split("#", StringSplitOptions.RemoveEmptyEntries)
              .Select(s => s.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(cal => int.Parse(cal)));

    // One important consideration is food - in particular, the number of Calories each Elf is carrying (your puzzle input).
    // In case the Elves get hungry and need extra snacks, they need to know which Elf to ask: they'd like to know how many
    // Calories are being carried by the Elf carrying the most Calories.
    // Puzzle == find the Elf carrying the most Calories. How many total Calories is that Elf carrying?
    private int Puzzle1(IEnumerable<IEnumerable<int>> elves)
    {
        var maxCal = elves.Max(e => e.Sum());

        WriteLine($"  Antwort 1: Der Elf mit den meisten Kalorien trägt {maxCal} kcal.");
        return maxCal;
    }

    // By the time you calculate the answer to the Elves' question, they've already realized that the Elf carrying the most Calories
    // of food might eventually run out of snacks. To avoid this unacceptable situation, the Elves would instead like to know the
    // total Calories carried by the top three Elves carrying the most Calories.
    // Puzzle == find the top three Elves carrying the most Calories. How many Calories are those Elves carrying in total?
    private static int Puzzle2(IEnumerable<IEnumerable<int>> elves)
    {
        var top3Cal = elves.Select(e => e.Sum())
                           .OrderDescending()
                           .Take(3)
                           .Sum();

        WriteLine($"  Antwort 2: Die 3 Elfen mit den meisten Kalorien tragen zusammen {top3Cal} kcal.");
        return top3Cal;
    }
}
