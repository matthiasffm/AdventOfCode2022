namespace AdventOfCode2022;

using NUnit.Framework;
using FluentAssertions;

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

        Puzzle1(rucksacks).Should().Be(16 + 38 +42 + 22 + 20 + 19);
        Puzzle2(rucksacks).Should().Be(18 + 52);
    }

    [Test]
    public void TestAocInput()
    {
        var rucksacks = FileUtils.ReadAllLines(this);

        Puzzle1(rucksacks).Should().Be(8394);
        Puzzle2(rucksacks).Should().Be(2413);
    }

    // Each rucksack has two large compartments of the same size. All items of a given type are meant to go into
    // exactly one of the two compartments. The Elf that did the packing failed to follow this rule for exactly
    // one item type per rucksack.
    // To help prioritize item rearrangement, every item type can be converted to a priority (= number of letter). 
    // Puzzle == Find the item type that appears in both compartments of each rucksack. What is the sum of the
    //           priorities of those item types?
    private int Puzzle1(IEnumerable<string> rucksacks) =>
        rucksacks.Sum(r => FirstSetBit(ToUlong(r.AsSpan().Slice(0, r.Length / 2 ))
                                       &
                                       ToUlong(r.AsSpan().Slice(r.Length / 2, r.Length / 2 ))));

    // The Elves are divided into groups of three. Every Elf carries a badge that identifies their group. For
    // efficiency, within each group of three Elves, the badge is the only item type carried by all three Elves.
    // That is, if a group's badge is item type B, then all three Elves will have item type B somewhere in their
    // rucksack, and at most two of the Elves will be carrying any other item type.
    // Additionally, nobody wrote down which item type corresponds to each group's badges. The only way to tell
    // which item type is the right one is by finding the one that is common between all three Elves in each group.
    // Puzzle == Find the item type that corresponds to the badges of each three-Elf group. What is the sum of
    //           the priorities of those item types?
    private int Puzzle2(IEnumerable<string> rucksacks) =>
        rucksacks.Where((r, i) => i % 3 == 0)
                 .Zip(rucksacks.Where((r, i) => i % 3 == 1), (r1, r2) => (r1, r2))
                 .Zip(rucksacks.Where((r, i) => i % 3 == 2), (r, r3) => (r.r1, r.r2, r3))
                 .Sum(r => FirstSetBit(ToUlong(r.r1)
                                       & ToUlong(r.r2)
                                       & ToUlong(r.r3)));

    // converts letters to bits [1-53] where every set bit means a letter with this priority is present in letters
    private static ulong ToUlong(ReadOnlySpan<char> letters)
    {
        ulong bits = 0L;

        foreach(var l in letters)
        {
            bits |= (ulong)(1L << Priority(l));
        }

        return bits;
    }

    private static int Priority(char letter) => char.IsLower(letter) ? letter - 'a' + 1 : letter - 'A' + 27;

    // finds the index of the first set bit (should be only one set bit here in this puzzle)
    private static int FirstSetBit(ulong bits)
    {
        for(int i = 0; i < 60; i++)
        {
            if((bits & (ulong)(1L << i)) != 0)
            {
                return i;
            }
        }

        return -1;
    }
}
