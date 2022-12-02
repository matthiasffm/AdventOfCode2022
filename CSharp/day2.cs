namespace AdventOfCode2022;

using NUnit.Framework;
using FluentAssertions;

using static Console;

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

        WriteLine("Day 02 -- Beispiele --");

        Puzzle1(matches).Should().Be(8 + 1 + 6);
        Puzzle2(matches).Should().Be(4 + 1 + 7);
    }

    [Test]
    public void TestAocInput()
    {
        var data    = File.ReadAllLines(@"day2.data");
        var matches = ParseData(data);

        WriteLine("Day 02 -- richtige Eingaben --");

        Puzzle1(matches).Should().Be(11386);
        Puzzle2(matches).Should().Be(13600);
    }

    // encode the input data so choices of player 1 and player 2 are in 0..2 (0 = rock, 1 = paper, 2 = scissors)
    private IEnumerable<(int, int)> ParseData(IEnumerable<string> data) =>
        data.Select(d => (d[0] -'A', d[2] - 'X'));

    // one Elf gives you an encrypted strategy guide: "The first column is what your opponent is going to play: A for Rock, B for Paper, and C for
    // Scissors." The second column, you reason, must be what you should play in response.
    // The winner of the whole tournament is the player with the highest score. Your total score is the sum of your scores for each round. The score
    // for a single round is the score for the shape you selected (1 for Rock, 2 for Paper, and 3 for Scissors) plus the score for the outcome of the
    // round (0 if you lost, 3 if the round was a draw, and 6 if you won).
    // Puzzle == What would your total score be if everything goes exactly according to your strategy guide?
    private int Puzzle1(IEnumerable<(int, int)> matches)
    {
        var strategyScoringTotal = matches.Sum(m => Scoring(m.Item2, Result[m.Item1, m.Item2]));

        WriteLine($"  Antwort 1: Die Strategie liefert den Gesamtscore {strategyScoringTotal}.");
        return strategyScoringTotal;
    }

    // The Elf finishes helping with the tent and sneaks back over to you. "Anyway, the second column says how the round needs to end: X means you need
    // to lose, Y means you need to end the round in a draw, and Z means you need to win. Good luck!"
    // The total score is still calculated in the same way, but now you need to figure out what shape to choose so the round ends as indicated.
    // Puzzle == Following the Elf's now complete instructions, what would your total score be if everything goes exactly according to your strategy guide?
    private static int Puzzle2(IEnumerable<(int, int)> matches)
    {
        var strategyScoringTotal = matches.Sum(m => Scoring(InverseResult[m.Item1, m.Item2], m.Item2 * 3));

        WriteLine($"  Antwort 2: Die inverse Strategie liefert den Gesamtscore {strategyScoringTotal}.");
        return strategyScoringTotal;
    }

    private static int Scoring(int shape, int outcome) => (shape + 1) + outcome;

    // precalc RPS winning matrix
    // row = choice player 1
    // col = choice player 2
    // result = outcome (0 = player 1 wins, 3 = draw, 6 = player 2 wins)
    private static readonly int[,] Result = new int[3,3] {
        { 3, 6, 0 },
        { 0, 3, 6 },
        { 6, 0, 3 },
    };

    // precalc RPS inverse winning matrix
    // row = choice player 1
    // col = outcome (0 = player 1 wins, 1 = draw, 2 = player 2 wins)
    // result = choice player 2
    private static readonly int[,] InverseResult = new int[3,3] {
        { 2, 0, 1 },
        { 0, 1, 2 },
        { 1, 2, 0 },
    };
}
