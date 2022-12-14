namespace AdventOfCode2022;

using NUnit.Framework;
using FluentAssertions;

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

        Puzzle1(pairs).Should().Be(2);
        Puzzle2(pairs).Should().Be(4);
    }

    [Test]
    public void TestAocInput()
    {
        var data  = FileUtils.ReadAllLines(this);
        var pairs = ParseData(data);

        Puzzle1(pairs).Should().Be(651);
        Puzzle2(pairs).Should().Be(956);
    }

    private static IEnumerable<(Assignment left, Assignment right)> ParseData(IEnumerable<string> data)
        => data.Select(d => d.Split(','))
               .Select(s => (new Assignment(s[0]), new Assignment(s[1])))
               .ToArray();

    private record Assignment(int Min, int Max)
    {
        public Assignment(string s) : this(int.Parse(s.Split('-')[0]), int.Parse(s.Split('-')[1])) {}
        public bool FullyContains(Assignment other) => Min <= other.Min && Max >= other.Max;
        public bool Overlaps(Assignment other) => Min.Between(other.Min, other.Max) || other.Min.Between(Min, Max);
    }

    // Elves have been assigned the job of cleaning up sections of the camp. Every section has a unique ID number, and
    // each Elf is assigned a range of section IDs. However, as some of the Elves compare their section assignments with
    // each other, they've noticed that many of the assignments overlap.
    //
    // Puzzle == In how many assignment pairs does one range fully contain the other?
    private static int Puzzle1(IEnumerable<(Assignment left, Assignment right)> pairs) =>
        pairs.Count(p => p.left.FullyContains(p.right) || p.right.FullyContains(p.left));

    // Puzzle == In how many assignment pairs do the ranges overlap?
    private static int Puzzle2(IEnumerable<(Assignment left, Assignment right)> pairs) =>
        pairs.Count(p => p.left.Overlaps(p.right));
}
