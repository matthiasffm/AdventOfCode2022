namespace AdventOfCode2022;

using NUnit.Framework;
using FluentAssertions;

using matthiasffm.Common;

[TestFixture]
public class Day06
{
    [Test]
    public void TestSamples()
    {
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
        var signal = FileUtils.ReadAllLines(this)[0];

        Puzzle1(signal).Should().Be(1640);
        Puzzle2(signal, 14).Should().Be(3613);
    }

    // To fix the Elves communication system, you need to add a subroutine to the device that detects a start-of-packet marker in the datastream. In
    // the protocol being used by the Elves, the start of a packet is indicated by a sequence of four characters that are all different.
    // Puzzle == How many characters need to be processed before the first start-of-packet marker is detected?
    private static int Puzzle1(string signal) =>
        3.To(signal.Length)
         .First(i => Test4Diff(signal.AsSpan().Slice(i - 3, 4)))
         + 1;

    // tests if all 4 letters in toTest are different
    private static bool Test4Diff(ReadOnlySpan<char> toTest) =>
        toTest[0] != toTest[1] && toTest[0] != toTest[2] && toTest[0] != toTest[3] &&
        toTest[1] != toTest[2] && toTest[1] != toTest[3] &&  
        toTest[2] != toTest[3]; 

    // A start-of-message marker is just like a start-of-packet marker, except it consists of 14 distinct characters rather than 4.
    // Puzzle == How many characters need to be processed before the first start-of-message marker is detected?
    private static int Puzzle2(string signal, int markerLength)
    {
        // uses a character lookup array for all 26 letters in the alphabet
        // maps: [letter of alphabet in sliding window] -> uneven (true) or even (false) number of occurences in sliding window
        // for the next position of the sliding window the algorithm only has to update the first letter leaving
        // the sliding window and the first letter entering the sliding window to update in the lookuk array.

        bool[] slidingWindow = new bool[26]; // could also be just a single 32 bit int

        for(int i = 0; i < markerLength; i++)
        {
            slidingWindow[signal[i] - 'a'] ^= true;
        }

        // we know we have 14 different characters when 14 values are true in the lookup map (nmbrOfUniques tracks this)

        int nmbrOfUniques = slidingWindow.Count(a => a == true);

        for(int i = markerLength; i < signal.Length; i++)
        {
            if(nmbrOfUniques == markerLength)
            {
                return i;
            }

            nmbrOfUniques += UpdateSlidingWindow(slidingWindow, signal[i - markerLength] - 'a');
            nmbrOfUniques += UpdateSlidingWindow(slidingWindow, signal[i] - 'a');
        }

        System.Diagnostics.Debug.Fail("no start-of-message marker found");
        return -1;
    }

    // updates the values entering or leavong the sliding window
    // if the character leaving the sliding window has even occurences we write true and add 1 to the unique count
    //                                             has uneven occurences we write false and subtract 1 from the unique count
    // if the character entering the sliding window has even occurences we write true and add 1 to the unique count
    //                                              has uneven occurences we write false and subtract 1 from the unique count
    private static int UpdateSlidingWindow(bool[] alphabet, int diff)
    {
        if(alphabet[diff] == false)
        {
            alphabet[diff] = true;
            return 1;
        }
        else
        {
            alphabet[diff] = false;
            return -1;
        }
    }
}
