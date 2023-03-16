namespace AdventOfCode2022;

using NUnit.Framework;
using FluentAssertions;

using matthiasffm.Common.Math;
using NUnit.Framework.Internal;

// TODO: 2s too slow
//       - most of the time is spent in HasNeighbor and CanMoveDir where some positions
//         are looked up twice in the hashset. this could be avoided if HasNeighbor returns
//         output compatible with CanMoveDir (like a bitfield for all 8 neighbors)
//       - the data is dense, so a 200x200 bitmap may be small enough to be way faster than the
//         hashset lookups

[TestFixture]
public partial class Day23
{
    private static Vec2<int>[] ParseData(string[] map) =>
        map.Select((l, y) => (l, y))
           .SelectMany(line => line.l.Select((c, x) => (c, x))
                                     .Where(pos => pos.c == '#')
                                     .Select(pos => new Vec2<int>(pos.x, line.y)))
           .ToArray();

    [Test]
    public void TestSamples()
    {
        var smallMap = new [] {
            ".....",
            "..##.",
            "..#..",
            ".....",
            "..##.",
            ".....",
        };
        var elvesSmallMap = ParseData(smallMap);

        Puzzle1(elvesSmallMap, 3).Should().Be(5 * 6 - 5);
        Puzzle1(elvesSmallMap, 10).Should().Be(5 * 6 - 5);
        Puzzle2(elvesSmallMap).Should().Be(4);

        var biggerMap = new [] {
            "....#..",
            "..###.#",
            "#...#.#",
            ".#...##",
            "#.###..",
            "##.#.##",
            ".#..#..",
        };
        var elvesBiggerMap = ParseData(biggerMap);

        Puzzle1(elvesBiggerMap, 10).Should().Be(12 * 11 - 22);
        Puzzle2(elvesBiggerMap).Should().Be(20);
    }

    [Test]
    public void TestAocInput() {
        var data     = FileUtils.ReadAllLines(this);
        var elvesMap = ParseData(data);

        Puzzle1(elvesMap, 10).Should().Be(3920);
        Puzzle2(elvesMap).Should().Be(889);
    }

    // The scan of the grove (your puzzle input) shows Elves # and empty ground .; outside your scan, more empty ground extends a long way in every direction.
    // The scan is oriented so that north is up. The Elves follow a time-consuming process spread over multiple rounds to figure out where they should each go.
    // During these rounds the Elves alternate between considering where to move and actually moving. During the first half of each round, each Elf considers
    // the eight positions adjacent to themself. If no other Elves are in one of those eight positions, the Elf does not do anything during this round. Otherwise,
    // the Elf looks in each of four directions in the following order and proposes moving one step in the first valid direction:
    // - If there is no Elf in the N, NE, or NW adjacent positions, the Elf proposes moving north one step.
    // - If there is no Elf in the S, SE, or SW adjacent positions, the Elf proposes moving south one step.
    // - If there is no Elf in the W, NW, or SW adjacent positions, the Elf proposes moving west one step.
    // - If there is no Elf in the E, NE, or SE adjacent positions, the Elf proposes moving east one step.
    // After each Elf has had a chance to propose a move, the second half of the round can begin. Simultaneously, each Elf moves to their proposed destination
    // tile if they were the only Elf to propose moving to that position. If two or more Elves propose moving to the same position, none of those Elves move.
    // Finally, at the end of the round, the first direction the Elves considered is moved to the end of the list of directions. For example, during the second
    // round, the Elves would try proposing a move to the south first, then west, then east, then north.
    //
    // Puzzle == Consider the ground tiles contained by the smallest rectangle that contains every Elf. How many empty ground tiles does that rectangle contain?
    private static int Puzzle1(IEnumerable<Vec2<int>> elves, int rounds)
    {
        var startingDir = 0; // start N

        for(int round = 0; round < rounds; round++)
        {
            elves = SimulateRound(elves, startingDir);

            // rotate starting direction for elve movement for the next round
            startingDir = (startingDir + 1) % 4;
        }

        return EmptyGroundTiles(elves);
    }

    // It seems you're on the right track. Finish simulating the process and figure out where the Elves need to go. How many rounds did you save them?
    //
    // Puzzle = What is the number of the first round where no Elf moves?
    private static int Puzzle2(IEnumerable<Vec2<int>> elves)
    {
        var startingDir = 0; // start N

        int round = 1;

        while(true)
        {
            var newElvePos = SimulateRound(elves, startingDir);
            if(elves.SequenceEqual(newElvePos))
            {
                return round;
            }

            elves = newElvePos;
            round++;

            // rotate starting direction for elve movement for the next round
            startingDir = (startingDir + 1) % 4;
        }
    }

    private static IEnumerable<Vec2<int>> SimulateRound(IEnumerable<Vec2<int>> elves, int startingDir)
    {
        var elvePos        = new HashSet<Vec2<int>>(elves);
        int elveDebugCount = elvePos.Count;

        var stationaryElves = new List<Vec2<int>>();
        var movedElves      = new List<(Vec2<int>, Vec2<int>)>();

        // move elves

        foreach(var elve in elves)
        {
            if(HasNeighbor(elve, elvePos))
            {
                bool moved = false;
                for(int dir = startingDir; dir < startingDir + 4; dir++)
                {
                    if(CanMoveDir(elve, dir % 4, elvePos))
                    {
                        movedElves.Add((elve, elve + DirOffset[dir % 4]));
                        moved = true;
                        break;
                    }
                }

                if(!moved)
                {
                    stationaryElves.Add(elve);
                }
            }
            else
            {
                stationaryElves.Add(elve);
            }
        }

        // resolve collisions and consolidate elve positions for next round

        elvePos.Clear();

        foreach(var g in movedElves.GroupBy(e => e.Item2))
        {
            if(g.Count() == 1)
            {
                elvePos.Add(g.First().Item2);
            }
            else
            {
                foreach(var elve in g)
                {
                    elvePos.Add(elve.Item1);
                }
            }
        }

        foreach(var stationary in stationaryElves)
        {
            elvePos.Add(stationary);
        }

        return elvePos;
    }

    private static readonly Vec2<int>[] DirOffset = new[] {
        new Vec2<int>( 0, -1), // N
        new Vec2<int>( 0,  1), // S
        new Vec2<int>(-1,  0), // W
        new Vec2<int>( 1,  0), // E
    };

    private static readonly Vec2<int>[] Neighbors = new[] {
        new Vec2<int>(-1, -1), new Vec2<int>(0, -1), new Vec2<int>(1, -1),
        new Vec2<int>(-1,  0),                       new Vec2<int>(1,  0),
        new Vec2<int>(-1,  1), new Vec2<int>(0,  1), new Vec2<int>(1,  1),
    };

    private static bool HasNeighbor(Vec2<int> elve, HashSet<Vec2<int>> elves) =>
        Neighbors.Select(n => elve + n)
                 .Any(n => elves.Contains(n));

    private static readonly Vec2<int>[][] NeighborsForDir = new[] {
        new[] { new Vec2<int>(-1, -1), new Vec2<int>( 0, -1), new Vec2<int>( 1, -1),}, // N
        new[] { new Vec2<int>(-1,  1), new Vec2<int>( 0,  1), new Vec2<int>( 1,  1),}, // S
        new[] { new Vec2<int>(-1, -1), new Vec2<int>(-1,  0), new Vec2<int>(-1,  1),}, // W
        new[] { new Vec2<int>( 1, -1), new Vec2<int>( 1,  0), new Vec2<int>( 1,  1),}, // E
    };

    private static bool CanMoveDir(Vec2<int> elve, int dir, IEnumerable<Vec2<int>> elves) =>
        !NeighborsForDir[dir].Any(d => elves.Contains(elve + d));

    // counts the number of empty tiles in the rectangle spanned out by the elve positions
    private static int EmptyGroundTiles(IEnumerable<Vec2<int>> elves) =>
        (elves.Max(e => e.X) - elves.Min(e => e.X) + 1) *
        (elves.Max(e => e.Y) - elves.Min(e => e.Y) + 1) -
        elves.Count();
}
