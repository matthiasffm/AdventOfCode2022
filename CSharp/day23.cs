namespace AdventOfCode2022;

using NUnit.Framework;
using FluentAssertions;
using System.Collections;

// TODO: 300ms is still too slow
//       bitmap is around 2x faster than hasmap with positions
//       parallelization doesn't bring any improvement
//       only option left seems to be replacing the bitarry with SIMD ops

[TestFixture]
public partial class Day23
{
    private static (int, int)[] ParseData(string[] map) =>
        map.Select((l, y) => (l, y))
           .SelectMany(line => line.l.Select((c, x) => (c, x))
                                     .Where(pos => pos.c == '#')
                                     .Select(pos => (pos.x, line.y)))
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
    private static int Puzzle1(IEnumerable<(int, int)> elves, int rounds)
    {
        var startingDir = 0; // start north

        var map = new ElvesMap(elves);

        for(int round = 0; round < rounds; round++)
        {
            SimulateRound(map, startingDir);

            // rotate starting direction for elve movement for the next round
            startingDir = (startingDir + 1) % 4;
        }

        return (map.MaxX - map.MinX + 1) *
               (map.MaxY - map.MinY + 1) - elves.Count();
    }

    // It seems you're on the right track. Finish simulating the process and figure out where the Elves need to go. How many rounds did you save them?
    //
    // Puzzle = What is the number of the first round where no Elf moves?
    private static int Puzzle2(IEnumerable<(int, int)> elves)
    {
        var startingDir = 0; // start N

        int round = 1;

        var map = new ElvesMap(elves);

        while(true)
        {
            if(!SimulateRound(map, startingDir))
            {
                return round;
            }

            round++;

            // rotate starting direction for elve movement for the next round
            startingDir = (startingDir+ 1) % 4;
        }
    }

    // simulates the elves moves according to the rules
    // the bitmap content of map is modified in place(!) in this process
    // if no move is made in a round this method returns false
    private static bool SimulateRound(ElvesMap map, int startingDir)
    {
        var proposedMoves  = new List<((int, int), (int, int))>(2500);

        int[] neighbors = new int[4];

        // record proposed move vectors of all elves

        for(int y = map.MinY; y <= map.MaxY; y++)
        {
            for(int x = map.MinX; x <= map.MaxX; x++)
            {
                if(map.Get(x, y) == 0)
                {
                    continue;
                }

                map.FillNeigborValues(x, y, neighbors);
                if(neighbors[0] > 0 || neighbors[1] > 0 || neighbors[2] > 0 || neighbors[3] > 0)
                {
                    for(int dir = startingDir; dir < startingDir + 4; dir++)
                    {
                        if(neighbors[dir % 4] == 0)
                        {
                            proposedMoves.Add(((x, y), (x + DirOffset[dir % 4].Item1, y + DirOffset[dir % 4].Item2)));
                            break;
                        }
                    }
                }
            }
        }

        // resolve collisions and move only these elves without collision to their new position

        bool modified = false;

        foreach(var move in proposedMoves.GroupBy(e => e.Item2))
        {
            if(move.Count() == 1)
            {
                var from = move.First().Item1;
                var to   = move.First().Item2;
                map.Set(from.Item1, from.Item2, false);
                map.Set(to.Item1, to.Item2);

                modified = true;
            }
        }

        return modified;
    }

    private static readonly (int, int)[] DirOffset = new[] {
        ( 0, -1), // N
        ( 0,  1), // S
        (-1,  0), // W
        ( 1,  0), // E
    };

    private class ElvesMap
    {
        private readonly BitArray   _bits;
        private readonly int        _width;
        private readonly int        _height;
        private          int        _minX;
        private          int        _maxX;
        private          int        _minY;
        private          int        _maxY;

        public int MinX { get { return _minX; } }
        public int MaxX { get { return _maxX; } }
        public int MinY { get { return _minY; } }
        public int MaxY { get { return _maxY; } }

        public ElvesMap(IEnumerable<(int, int)> elves)
        {
            var minX    = elves.Min(e => e.Item1);
            var maxX    = elves.Max(e => e.Item1);
            var offsetX = maxX - minX + 1;
            _width = 3 * (maxX - minX + 1);

            var minY    = elves.Min(e => e.Item2);
            var maxY    = elves.Max(e => e.Item2);
            var offsetY = maxY - minY + 1;
            _height = 3 * (maxY - minY + 1);

            _bits = new BitArray(_height * _width);

            _minX = _minY = int.MaxValue;
            _maxX = _maxY = -1;

            foreach(var elve in elves)
            {
                Set(elve.Item1 - minX + offsetX, elve.Item2 - minY + offsetY);
            }
        }

        public void Set(int x, int y, bool bit = true)
        {
            if(bit)
            {
                _minX = Math.Min(x, _minX);
                _maxX = Math.Max(x, _maxX);
                _minY = Math.Min(y, _minY);
                _maxY = Math.Max(y, _maxY);
            }

            _bits.Set(x + y * _width, bit);
        }

        public int Get(int x, int y)
        {
            if(x < _minX || x > _maxX || y < _minY || y > _maxY)
            {
                return 0;
            }
            else
            {
                return _bits.Get(x + y * _width) ? 1 : 0;
            }
        }

        public void FillNeigborValues(int x, int y, int[] neighbors)
        {
            var top    = x - 1 + (y - 1) * _width;
            var middle = top + _width;
            var bottom = middle + _width;

            neighbors[0] = (_bits[top]     || _bits[top + 1]    || _bits[top + 2])    ? 1 : 0; // N
            neighbors[1] = (_bits[bottom]  || _bits[bottom + 1] || _bits[bottom + 2]) ? 1 : 0; // S
            neighbors[2] = (_bits[top]     || _bits[middle]     || _bits[bottom])     ? 1 : 0; // W
            neighbors[3] = (_bits[top + 2] || _bits[middle + 2] || _bits[bottom + 2]) ? 1 : 0; // E
        }
    }
}
