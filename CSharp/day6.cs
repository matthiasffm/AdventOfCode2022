namespace AdventOfCode2022;

using NUnit.Framework;
using FluentAssertions;

using static Console;
using matthiasffm.Common;

[TestFixture]
public class Day06
{
    [Test]
    public void TestSamples()
    {
        WriteLine("Day 06 -- Samples --");

        Puzzle1("mjqjpqmgbljsphdztnvjfqwrcgsmlb").Should().Be(7);
        Puzzle1("bvwbjplbgvbhsrlpgdmjqwftvncz").Should().Be(5);
        Puzzle1("nppdvjthqldpwncqszvftbrmjlhg").Should().Be(6);
        Puzzle1("nznrnfrfntjfmvfwmzdfjlvtqnbhcprsg").Should().Be(10);
        Puzzle1("zcfzfwzzqfrljwzlrfnpqdbhtmscgvjw").Should().Be(11);

        Puzzle2("mjqjpqmgbljsphdztnvjfqwrcgsmlb", 14).Should().Be(19);
        Puzzle2("bvwbjplbgvbhsrlpgdmjqwftvncz", 14).Should().Be(23);
        Puzzle2("nppdvjthqldpwncqszvftbrmjlhg", 14).Should().Be(23);
        Puzzle2("nznrnfrfntjfmvfwmzdfjlvtqnbhcprsg", 14).Should().Be(29);
        Puzzle2("zcfzfwzzqfrljwzlrfnpqdbhtmscgvjw", 14).Should().Be(26);
    }

    [Test]
    public void TestAocInput()
    {
        var signal = File.ReadAllText(@"day6.data");

        WriteLine("Day 06 -- AoC input --");

        Puzzle1(signal).Should().Be(1640);
        Puzzle2(signal, 14).Should().Be(3613);
    }

    // To fix the Elves communication system, you need to add a subroutine to the device that detects a start-of-packet marker in the datastream. In
    // the protocol being used by the Elves, the start of a packet is indicated by a sequence of four characters that are all different.
    // Puzzle == How many characters need to be processed before the first start-of-packet marker is detected?
    private int Puzzle1(string signal)
    {
        var sopMarkerPos = 3.To(signal.Length)
                            .First(i => Test4Diff(signal.AsSpan().Slice(i - 3, 4)))
                           + 1;

        WriteLine($"  Puzzle 1: the first start-of-packet marker of 4 different letters is at position {sopMarkerPos}.");
        return sopMarkerPos;
    }

    // A start-of-message marker is just like a start-of-packet marker, except it consists of 14 distinct characters rather than 4.
    // Puzzle == How many characters need to be processed before the first start-of-message marker is detected?
    private int Puzzle2(string signal, int markerLength)
    {
        var msgMarkerPos = (markerLength - 1).To(signal.Length)
                                             .First(i => TestNDiff(signal.AsSpan().Slice(i - markerLength + 1, markerLength)))
                           + 1;

        WriteLine($"  Puzzle 2: the first start-of-message marker of {markerLength} different letters is at position {msgMarkerPos}.");
        return msgMarkerPos;
    }

    // tests if all 4 letters in toTest are different
    private static bool Test4Diff(ReadOnlySpan<char> toTest) =>
        toTest[0] != toTest[1] && toTest[0] != toTest[2] && toTest[0] != toTest[3] &&
        toTest[1] != toTest[2] && toTest[1] != toTest[3] &&  
        toTest[2] != toTest[3]; 

    private static readonly byte[] _charLookup = new byte[26];

    // tests if all letters in toTest are different
    // uses a character lookup array for all 26 different letters in the alphabet
    private static bool TestNDiff(ReadOnlySpan<char> toTest)
    {
        for(int i = 0; i < toTest.Length; i++)
        {
            _charLookup[toTest[i] - 'a']++;
        }

        bool alldiff = true;
        for(int i = 0; i < 26; i++)
        {
            if(_charLookup[i] > 1)
            {
                alldiff = false;
            }
            _charLookup[i] = 0;
        }

        return alldiff;
    }
}
