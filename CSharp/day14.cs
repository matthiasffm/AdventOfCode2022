namespace AdventOfCode2022;

using NUnit.Framework;
using FluentAssertions;

using static System.Math;

[TestFixture]
public class Day14
{
    private static (int, int)[][] ParseData(string[] lines) =>
        lines.Select(line => line.Split(" -> ")
                                 .Select(pair => (int.Parse(pair.Split(',').ElementAt(0)),
                                                  int.Parse(pair.Split(',').ElementAt(1))))
                                 .ToArray())
             .ToArray();

    [Test]
    public void TestSamples()
    {
        var data = new[] {
            "498,4 -> 498,6 -> 496,6",
            "503,4 -> 502,4 -> 502,9 -> 494,9",
        };

        var rockPaths = ParseData(data);

        Puzzle1(rockPaths, 500).Should().Be(24);
        Puzzle2(rockPaths, 500).Should().Be(93);
    }

    [Test]
    public void TestAocInput()
    {
        var data      = FileUtils.ReadAllLines(this);
        var rockPaths = ParseData(data);

        Puzzle1(rockPaths, 500).Should().Be(961);
        Puzzle2(rockPaths, 500).Should().Be(26375);
    }

    private const byte AIR = 0;
    private const byte ROCK = 1;
    private const byte SAND = 2;

    // The distress signal leads you behind a giant waterfall! There seems to be a large cave system here, and the signal definitely leads further inside.
    // Sand begins pouring into the cave! You scan a two-dimensional vertical slice of the cave above you (the puzzle input) and discover that it is
    // mostly air with structures made of rock. Sand is produced one unit at a time, and the next unit of sand is not produced until the previous unit of
    // sand comes to rest. A unit of sand is large enough to fill one tile of air in your scan. Sand flows straight down, diagonally to the left or then
    // to the right. If it can't move anymore, it comes to rest and the simulation continues with the next unit of sand.
    //
    // Puzzle == Using your scan, simulate the falling sand. How many units of sand come to rest before sand starts flowing into the abyss below?
    private static int Puzzle1((int, int)[][] rockPaths, int sandSouceCol)
    {
        var allCols = rockPaths.SelectMany(p => p.Select(r => r.Item1)).ToArray();
        var minCol  = allCols.Min();
        var maxCol  = allCols.Max();
        var maxRow  = rockPaths.SelectMany(p => p.Select(r => r.Item2)).Max();

        var cave = new byte[maxRow + 1, maxCol - minCol + 1];
        DrawCave(cave, minCol, rockPaths);

        var sandUnits = 0;
        var sandStart = (0, sandSouceCol - minCol);

        while(SimulateSand(cave, sandStart) != null)
        {
            sandUnits++;
        }

        return sandUnits;
    }

    // You realize you misread the scan. There isn't an endless void at the bottom of the scan - there's floor, and you're standing on it!
    // You don't have time to scan the floor, so assume the floor is an infinite horizontal line with a y coordinate equal to two plus the
    // highest y coordinate of any point in your scan.
    // To find somewhere safe to stand, you'll need to simulate falling sand until a unit of sand comes to rest at the source location of
    // the sand, blocking the source entirely and stopping the flow of sand into the cave.
    //
    // Puzzle == Using your scan, simulate the falling sand until the source of the sand becomes blocked. How many units of sand come to rest?
    private static int Puzzle2((int, int)[][] rockPaths, int sandSouceCol)
    {
        // Cave is the same like Puzzle1 but there has to be a 'infinite' plane at bottom + 2. Sand only
        // flows diagonally so to reach the top we need bottom + 2 space to the left and to the right in
        // the cave.

        var allCols = rockPaths.SelectMany(p => p.Select(r => r.Item1));
        var minCol  = allCols.Min();
        var maxCol  = allCols.Max();
        var maxRow  = rockPaths.SelectMany(p => p.Select(r => r.Item2)).Max() + 2; // bottom + 2

        var cave = new byte[maxRow + 1, (maxCol - minCol + 1) + 2 * maxRow];
        DrawCave(cave, minCol - maxRow, rockPaths);
        DrawInfiniteBottom(cave, maxRow);

        var sandUnits = 0;
        var sandStart = (0, sandSouceCol - minCol + maxRow);

        do
        {
            sandUnits++;
        }
        while(SimulateSand(cave, sandStart) != sandStart);

        return sandUnits;
    }

    // A unit of sand always falls down one step if possible. If the tile immediately below is blocked (by rock or sand), the unit of sand attempts to instead
    // move diagonally one step down and to the left. If that tile is blocked, the unit of sand attempts to instead move diagonally one step down and to the
    // right. Sand keeps moving as long as it is able to do so, at each step trying to move down, then down-left, then down-right. If all three possible destinations
    // are blocked, the unit of sand comes to rest and no longer moves.
    private static (int, int)? SimulateSand(byte[,] cave, (int, int) sand)
    {
        do
        {
            if(sand.Item1 + 1 >= cave.GetLength(0))
            {
                return null; // sand flows over to bottom
            }
            else if(cave[sand.Item1 + 1, sand.Item2] == AIR)
            {
                sand = (sand.Item1 + 1, sand.Item2);
            }
            else if(sand.Item2 <= 0)
            {
                return null; // sand flows over to left
            }
            else if(cave[sand.Item1 + 1, sand.Item2 - 1] == AIR)
            {
                sand = (sand.Item1 + 1, sand.Item2 - 1);
            }
            else if(sand.Item2 + 1 >= cave.GetLength(1))
            {
                return null; // sand flows over to left
            }
            else if(cave[sand.Item1 + 1, sand.Item2 + 1] == AIR)
            {
                sand = (sand.Item1 + 1, sand.Item2 + 1);
            }
            else
            {
                cave[sand.Item1, sand.Item2] = SAND;
                return sand; // sand comes to rest
            }
        }
        while(true);
    }

    // draws all input rock segments for the cave formation
    private static byte[,] DrawCave(byte[,] cave, int minCol, (int, int)[][] rockPaths)
    {
        foreach (var rockPath in rockPaths)
        {
            var pos = rockPath[0];
            foreach (var segment in rockPath.Skip(1))
            {
                DrawRockSegment(cave, pos with { Item1 = pos.Item1 - minCol }, segment with { Item1 = segment.Item1 - minCol });
                pos = segment;
            }
        }

        return cave;
    }

    // draws a single rock formation (line) for the cave formation
    private static void DrawRockSegment(byte[,] cave, (int, int) start, (int, int) end)
    {
        var diff   = (end.Item1 - start.Item1, end.Item2 - start.Item2);
        var length = Max(Abs(diff.Item1), Abs(diff.Item2));

        diff = (diff.Item1 / length, diff.Item2 / length);

        for(int i = 0; i <= length; i++)
        {
            cave[start.Item2, start.Item1] = ROCK;
            start = (start.Item1 + diff.Item1, start.Item2 + diff.Item2);
        }
    }

    private static void DrawInfiniteBottom(byte[,] cave, int maxRow) =>
        DrawRockSegment(cave, (0, maxRow), (cave.GetLength(1) - 1, maxRow));
}
