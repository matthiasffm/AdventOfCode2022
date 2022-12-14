namespace AdventOfCode2022;

using NUnit.Framework;
using FluentAssertions;

[TestFixture]
public class Day01
{
    // parsed the input data and returns all calories every Elf carries
    private static IEnumerable<IEnumerable<int>> ParseData(string[] data) =>
        FileUtils.ParseMultilineTuples(data, cal => int.Parse(cal));

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

        Puzzle1(elves).Should().Be(7000 + 8000 + 9000);
        Puzzle2(elves).Should().Be(10000 + 7000 + 8000 + 9000 + 5000 + 6000);
    }

    [Test]
    public void TestAocInput()
    {
        var data  = FileUtils.ReadAllLines(this);
        var elves = ParseData(data);

        Puzzle1(elves).Should().Be(64929);
        Puzzle2(elves).Should().Be(193697);
    }

    // One important consideration is food - in particular, the number of Calories each Elf is carrying (your puzzle input).
    // In case the Elves get hungry and need extra snacks, they need to know which Elf to ask: they'd like to know how many
    // Calories are being carried by the Elf carrying the most Calories.
    //
    // Puzzle == find the Elf carrying the most Calories. How many total Calories is that Elf carrying?
    private static int Puzzle1(IEnumerable<IEnumerable<int>> elves) =>
        elves.Max(e => e.Sum());

    // By the time you calculate the answer to the Elves' question, they've already realized that the Elf carrying the most Calories
    // of food might eventually run out of snacks. To avoid this unacceptable situation, the Elves would instead like to know the
    // total Calories carried by the top three Elves carrying the most Calories.
    //
    // Puzzle == find the top three Elves carrying the most Calories. How many Calories are those Elves carrying in total?
    private static int Puzzle2(IEnumerable<IEnumerable<int>> elves) =>
        elves.Select(e => e.Sum())
             .OrderDescending()
             .Take(3)
             .Sum();
}
