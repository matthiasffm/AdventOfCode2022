namespace AdventOfCode2022;

using NUnit.Framework;
using FluentAssertions;

using static Console;
using matthiasffm.Common.Math;

[TestFixture]
public class Day04
{
    [Test]
    public void TestSamples()
    {
        var data = new [] { 
            "2-4,6-8",
            "2-3,4-5",
            "5-7,7-9",
            "2-8,3-7",
            "6-6,4-6",
            "2-6,4-8",
        };
        var pairs = ParseData(data);

        WriteLine("Day 04 -- Samples --");

        Puzzle1(pairs).Should().Be(2);
        Puzzle2(pairs).Should().Be(4);
    }

    [Test]
    public void TestAocInput()
    {
        var data = File.ReadAllLines(@"day4.data");
        var pairs = ParseData(data);

        WriteLine("Day 04 -- AoC input --");

        Puzzle1(pairs).Should().Be(651);
        Puzzle2(pairs).Should().Be(956);
    }

    private static IEnumerable<(Pair left, Pair right)> ParseData(IEnumerable<string> data)
        => data.Select(d => d.Split(','))
               .Select(s => (new Pair(s[0]), new Pair(s[1])))
               .ToArray();

    private record Pair(int min, int max)
    {
        public Pair(string s) : this(int.Parse(s.Split('-')[0]), int.Parse(s.Split('-')[1])) {}
        public bool FullyContains(Pair other) => min <= other.min && max >= other.max;
        public bool Overlaps(Pair other) => min.Between(other.min, other.max) || other.min.Between(min, max);
    }

    // Elves have been assigned the job of cleaning up sections of the camp. Every section has a unique ID number, and
    // each Elf is assigned a range of section IDs. However, as some of the Elves compare their section assignments with
    // each other, they've noticed that many of the assignments overlap.
    // Puzzle == In how many assignment pairs does one range fully contain the other?
    private int Puzzle1(IEnumerable<(Pair left, Pair right)> pairs)
    {
        var fullyContains = pairs.Count(p => p.left.FullyContains(p.right) || p.right.FullyContains(p.left));

        WriteLine($"  Puzzle 1: {fullyContains} assignment pairs fully contain each other.");
        return fullyContains;
    }

    // Puzzle == In how many assignment pairs does one range fully contain the other?
    private int Puzzle2(IEnumerable<(Pair left, Pair right)> pairs)
    {
        var overlaps = pairs.Count(p => p.left.Overlaps(p.right));

        WriteLine($"  Puzzle 2: {overlaps} assignment pairs overlap each other.");
        return overlaps;
    }
}
