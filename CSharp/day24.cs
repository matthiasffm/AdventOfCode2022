namespace AdventOfCode2022;

using NUnit.Framework;
using FluentAssertions;
using System;

// TODO: 800ms is still too slow
//       all collections could be replaced with bitfields for blizzards, visited and neighbors
//       => all operations would be bitwise shift, and/or then

[TestFixture]
public partial class Day24
{
    private record Blizzard(int X, int Y, int Direction)
    {
        // see MoveBlizzard function
        public Blizzard(int X, int Y, char c) : this(X, Y, c switch {
            '>' => 0,
            'v' => 1,
            '<' => 2,
            _   => 3,
        }) { }
    }

    private static IEnumerable<Blizzard> ParseData(string[] map) =>
        map.Select((l, y) => (l, y))
           .SelectMany(line => line.l.Select((c, x) => (c, x))
                                     .Where(pos => pos.c != '#' && pos.c != '.')
                                     .Select(pos => new Blizzard(pos.x, line.y, pos.c)))
           .ToArray();

    [Test]
    public void TestSamples()
    {
        var data = new [] {
            "#.######",
            "#>>.<^<#",
            "#.<..<<#",
            "#>v.><>#",
            "#<^v^^>#",
            "######.#",
        };
        var valley = ParseData(data);

        Puzzle1(valley, data[0].Length, data.Length).Should().Be(18);
        Puzzle2(valley, data[0].Length, data.Length).Should().Be(18 + 23 + 13);
    }

    [Test]
    public void TestAocInput() {
        var data   = FileUtils.ReadAllLines(this);
        var valley = ParseData(data);

        Puzzle1(valley, data[0].Length, data.Length).Should().Be(332);
        Puzzle2(valley, data[0].Length, data.Length).Should().Be(942);
    }

    // As the expedition reaches a valley that must be traversed to reach the extraction site, you find that strong, turbulent winds are pushing small
    // blizzards of snow and sharp ice around the valley. To make it across safely, you'll need to find a way to avoid them.
    // Due to conservation of blizzard energy, as a blizzard reaches the wall of the valley, a new blizzard forms on the opposite side of the valley
    // moving in the same direction. After another minute, the bottom downward-moving blizzard has been replaced with a new downward-moving blizzard
    // at the top of the valley instead.
    // Because blizzards are made of tiny snowflakes, they pass right through each other.
    // Your expedition begins in the only non-wall position in the top row and needs to reach the only non-wall position in the bottom row. On each
    // minute, you can move up, down, left, or right, or you can wait in place. You and the blizzards act simultaneously, and you cannot share a
    // position with a blizzard.
    //
    // Puzzle == What is the fewest number of minutes required to avoid the blizzards and reach the goal?
    private static int Puzzle1(IEnumerable<Blizzard> blizzards, int width, int height)
    {
        return Trip(blizzards, width, height, (1, 0), (width - 2, height - 1), 1);
    }

    // One of the Elves looks especially dismayed: He forgot his snacks at the entrance to the valley!
    // Since you're so good at dodging blizzards, the Elves humbly request that you go back for his snacks. From the same initial conditions, how
    // quickly can you make it from the start to the goal, then back to the start, then back to the goal?
    //
    // Puzzle ==  What is the fewest number of minutes required to reach the goal, go back to the start, then reach the goal again?
    private static int Puzzle2(IEnumerable<Blizzard> blizzards, int width, int height)
    {
        var trip1 = Trip(blizzards, width, height, (1, 0), (width - 2, height - 1), 1);
        var trip2 = Trip(blizzards, width, height, (width - 2, height - 1), (1, 0), trip1);
        var trip3 = Trip(blizzards, width, height, (1, 0), (width - 2, height - 1), trip2);

        return trip3;
    }

    // for every minute compute the free cells which are in reach from the cells visited in the previous minute
    // if one is the end pos => finished
    private static int Trip(IEnumerable<Blizzard> blizzards, int width, int height, (int x, int y) start, (int x, int y) end, int minuteStart)
    {
        IEnumerable<(int x, int y)> cellsVisited = new HashSet<(int x, int y)>
        {
            start,
        };

        for(int minute = minuteStart; ; minute++)
        {
            cellsVisited = cellsVisited.SelectMany(c => AllNeighbors(c, width, height))
                                       .Intersect(CalcFreeSpaceForMinute(blizzards, width, height, minute))
                                       .ToHashSet();

            if(cellsVisited.Contains(end))
            {
                return minute;
            }
        }
    }

    private static IEnumerable<(int x, int y)> AllNeighbors((int x, int y) pos, int width, int height)
    {
        yield return (pos.x, pos.y); // wait

        if(pos.x > 1)
        {
            yield return (pos.x - 1, pos.y); // left
        }
        if(pos.x < width - 2)
        {
            yield return (pos.x + 1, pos.y); // right
        }

        // always consider up and down because the end state for every trip must be reachable
        yield return (pos.x, pos.y - 1);
        yield return (pos.x, pos.y + 1);
    }

    // computes all cells where you can travel safely to (i.e. there is no blizzard) for a specific minute
    private static IEnumerable<(int X, int Y)> CalcFreeSpaceForMinute(IEnumerable<Blizzard> blizzards, int width, int height, int minute)
        => Enumerable.Range(1, width - 2)
                     .SelectMany(x => Enumerable.Range(1, height - 2)
                                                .Select(y => (x, y)))
                     .Except(blizzards.Select(b => MoveBlizzard(b, width, height, minute))
                                      .Select(b => (b.X, b.Y)))
                     .Append((1, 0)) // start
                     .Append((width - 2, height - 1)); // exit

    private static Blizzard MoveBlizzard(Blizzard b, int width, int height, int minute)
        => b.Direction switch {
            0 => b with { X = (b.X + minute - 1).Mod(width - 2) + 1 },  // move right
            1 => b with { Y = (b.Y + minute - 1).Mod(height - 2) + 1 }, // move down
            2 => b with { X = (b.X - minute - 1).Mod(width - 2) + 1 },  // move left
            _ => b with { Y = (b.Y - minute - 1).Mod(height - 2) + 1 }, // move up
        };
}
