namespace AdventOfCode2022;

using NUnit.Framework;
using FluentAssertions;

using static Console;

[TestFixture]
public class Day03
{
    [Test]
    public void TestSamples()
    {
        var rucksacks = new [] { 
            "vJrwpWtwJgWrhcsFMMfFFhFp",
            "jqHRNqRjqzjGDLGLrsFMfFZSrLrFZsSL",
            "PmmdzqPrVvPwwTWBwg",
            "wMqvLMZHhHMvwLHjbvcjnnSBnvTQFn",
            "ttgJtRGJQctTZtZT",
            "CrZsJsPPZsGzwwsLwLmpwMDw",
        };

        WriteLine("Day 03 -- Samples --");

        Puzzle1(rucksacks).Should().Be(16 + 38 +42 + 22 + 20 + 19);
        Puzzle2(rucksacks).Should().Be(18 + 52);
    }

    [Test]
    public void TestAocInput()
    {
        var rucksacks = File.ReadAllLines(@"day3.data");

        WriteLine("Day 03 -- AoC input --");

        Puzzle1(rucksacks).Should().Be(8394);
        Puzzle2(rucksacks).Should().Be(2413);
    }

    // Each rucksack has two large compartments of the same size. All items of a given type are meant to go into
    // exactly one of the two compartments. The Elf that did the packing failed to follow this rule for exactly
    // one item type per rucksack.
    // To help prioritize item rearrangement, every item type can be converted to a priority (= number of letter). 
    // Puzzle == Find the item type that appears in both compartments of each rucksack. What is the sum of the
    //           priorities of those item types?
    private int Puzzle1(IEnumerable<string> rucksacks)
    {
        var sumPriorities = rucksacks.Select(r => IncorrectLetter(r))
                                     .Sum(l => Priority(l));

        WriteLine($"  Puzzle 1: The sum of all priorities for the misplaced items is {sumPriorities}.");
        return sumPriorities;
    }

    private static char IncorrectLetter(string rucksack)
    {
        var lettersMisplaced = rucksack.ToCharArray(0, rucksack.Length / 2)
                                       .Intersect(rucksack.ToCharArray(rucksack.Length / 2, rucksack.Length / 2));
        System.Diagnostics.Debug.Assert(lettersMisplaced.Count() == 1);

        return lettersMisplaced.First();
    }

    // The Elves are divided into groups of three. Every Elf carries a badge that identifies their group. For
    // efficiency, within each group of three Elves, the badge is the only item type carried by all three Elves.
    // That is, if a group's badge is item type B, then all three Elves will have item type B somewhere in their
    // rucksack, and at most two of the Elves will be carrying any other item type.
    // Additionally, nobody wrote down which item type corresponds to each group's badges. The only way to tell
    // which item type is the right one is by finding the one that is common between all three Elves in each group.
    // Puzzle == Find the item type that corresponds to the badges of each three-Elf group. What is the sum of
    //           the priorities of those item types?
    private int Puzzle2(IEnumerable<string> rucksacks)
    {
        System.Diagnostics.Debug.Assert(rucksacks.Count() % 3 == 0);

        var sumPriorities = rucksacks.Where((r, i) => i % 3 == 0)
                                     .Zip(rucksacks.Where((r, i) => i % 3 == 1), (r1, r2) => (r1, r2))
                                     .Zip(rucksacks.Where((r, i) => i % 3 == 2), (r, r3) => (r.r1, r.r2, r3))
                                     .Select(r => Badge(r.r1, r.r2, r.r3))
                                     .Sum(l => Priority(l));

        WriteLine($"  Puzzle 2: The sum of all priorities for the badges is {sumPriorities}.");
        return sumPriorities;
    }

    private static char Badge(string rucksack1, string rucksack2, string rucksack3)
    {
        var badge = rucksack1.ToCharArray()
                             .Intersect(rucksack2.ToCharArray())
                             .Intersect(rucksack3.ToCharArray());
        System.Diagnostics.Debug.Assert(badge.Count() == 1);

        return badge.First();
    }

    private static int Priority(char letter) => char.IsLower(letter) ? letter - 'a' + 1 : letter - 'A' + 27;
}
