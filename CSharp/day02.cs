namespace AdventOfCode2022;

using NUnit.Framework;
using FluentAssertions;

[TestFixture]
public class Day02
{
    [Test]
    public void TestSamples()
    {
        var data = new [] { 
            "A Y",
            "B X",
            "C Z",
         };
        var matches = ParseData(data);

        Puzzle1(matches).Should().Be(8 + 1 + 6);
        Puzzle2(matches).Should().Be(4 + 1 + 7);
    }

    [Test]
    public void TestAocInput()
    {
        var data    = FileUtils.ReadAllLines(this);
        var matches = ParseData(data);

        Puzzle1(matches).Should().Be(11386);
        Puzzle2(matches).Should().Be(13600);
    }

    // encode the input data so choices of player 1 and player 2 are in 0..2 (0 = rock, 1 = paper, 2 = scissors)
    private static IEnumerable<(byte, byte)> ParseData(IEnumerable<string> data) =>
        data.Select(d => ((byte)(d[0] -'A'), (byte)(d[2] - 'X')));

    // one Elf gives you an encrypted strategy guide: "The first column is what your opponent is going to play: A for Rock, B for Paper, and C for
    // Scissors." The second column, you reason, must be what you should play in response.
    // The winner of the whole tournament is the player with the highest score. Your total score is the sum of your scores for each round. The score
    // for a single round is the score for the shape you selected (1 for Rock, 2 for Paper, and 3 for Scissors) plus the score for the outcome of the
    // round (0 if you lost, 3 if the round was a draw, and 6 if you won).
    //
    // Puzzle == What would your total score be if everything goes exactly according to your strategy guide?
    private static int Puzzle1(IEnumerable<(byte, byte)> matches) =>
        matches.Sum(m => Scoring(m.Item2, Result[m.Item1, m.Item2]));

    // The Elf finishes helping with the tent and sneaks back over to you. "Anyway, the second column says how the round needs to end: X means you need
    // to lose, Y means you need to end the round in a draw, and Z means you need to win. Good luck!"
    // The total score is still calculated in the same way, but now you need to figure out what shape to choose so the round ends as indicated.
    //
    // Puzzle == Following the Elf's now complete instructions, what would your total score be if everything goes exactly according to your strategy guide?
    private static int Puzzle2(IEnumerable<(byte, byte)> matches) =>
        matches.Sum(m => Scoring(InverseResult[m.Item1, m.Item2], (byte)(m.Item2 * 3)));

    private static int Scoring(byte shape, byte outcome) => (shape + 1) + outcome;

    // precalc RPS winning matrix
    // row = choice player 1
    // col = choice player 2
    // result = outcome (0 = player 1 wins, 3 = draw, 6 = player 2 wins)
    private static readonly byte[,] Result = new byte[3,3] {
        { 3, 6, 0 }, // remarks: every row is shifted by one col to the right
        { 0, 3, 6 }, //          => ((col - row + 1) * 3) % 9
        { 6, 0, 3 },
    };

    // precalc RPS inverse winning matrix
    // row = choice player 1
    // col = outcome (0 = player 1 wins, 1 = draw, 2 = player 2 wins)
    // result = choice player 2
    private static readonly byte[,] InverseResult = new byte[3,3] {
        { 2, 0, 1 }, // remarks: every row is shifted by one col to the left
        { 0, 1, 2 }, //          => (col + row + 2) % 3
        { 1, 2, 0 },
    };
}
