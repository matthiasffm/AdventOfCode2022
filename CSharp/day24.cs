namespace AdventOfCode2022;

using NUnit.Framework;
using FluentAssertions;
using System;
using System.Collections;

[TestFixture]
public partial class Day24
{
    // map as a bit representation
    // set bits either represent visited cells and/or cells free of blizzards
    private class Bitmap
    {
        private readonly int        _width;
        private readonly int        _height;
        private readonly BitArray[] _bits;

        public Bitmap(int width, int height)
        {
            _bits = new BitArray[height];
            for(var i = 0; i < height; i++)
            {
                _bits[i] = new BitArray(width);
            }

            _width  = width;
            _height = height;
        }

        public Bitmap(Bitmap right)
        {
            _width  = right._width;
            _height = right._height;

            _bits = new BitArray[_height];
            for(int row = 0; row < _height; row++)
            {
                _bits[row] = new BitArray(right._bits[row]);
            }
        }

        public int Width  { get { return _width; } }
        public int Height { get { return _height; } }

        public void Set(int x, int y, bool val = true)
        {
            _bits[y].Set(x, val);
        }

        public bool Get(int x, int y) => _bits[y].Get(x);

        public Bitmap RollRight()
        {
            for(int row = 0; row < _bits.Length; row++)
            {
                var carry = _bits[row].Get(_width - 1);
                _bits[row].LeftShift(1);
                _bits[row].Set(0, carry);
            }

            return this;
        }

        public Bitmap RollLeft()
        {
            for(int row = 0; row < _bits.Length; row++)
            {
                var carry = _bits[row].Get(0);
                _bits[row].RightShift(1);
                _bits[row].Set(_width - 1, carry);
            }

            return this;
        }

        public Bitmap RollUp()
        {
            var top = _bits[0];
            for(int row = 0; row < _bits.Length - 1; row++)
            {
                _bits[row] = _bits[row + 1];
            }
            _bits[_height - 1] = top;

            return this;
        }

        public Bitmap RollDown()
        {
            var bottom = _bits[_height - 1];
            for(int row = _bits.Length - 1; row > 0; row--)
            {
                _bits[row] = _bits[row - 1];
            }
            _bits[0] = bottom;

            return this;
        }

        public Bitmap And(Bitmap right)
        {
            for(int row = 0; row < _bits.Length; row++)
            {
                _bits[row].And(right._bits[row]);
            }

            return this;
        }

        // adds 4 neighboring bits
        // neighbor(bitmap) = bitmap | leftshift(bitmap) | rightshift(bitmap) | upshift(bitmap) | downshift(bitmap)
        public Bitmap AddNeighbors()
        {
            var copy = new Bitmap(this);

            for(var row = 0; row < _height; row++)
            {
                _bits[row].Or(((BitArray)copy._bits[row].Clone()).RightShift(1));
                _bits[row].Or(((BitArray)copy._bits[row].Clone()).LeftShift(1));
                if(row > 0)
                {
                    _bits[row].Or(copy._bits[row - 1]);
                }
                if(row < _height - 1)
                {
                    _bits[row].Or(copy._bits[row + 1]);
                }
            }

            return this;
        }
    }

    // parses data into bitmap representation of cells free of blizzards for all 4 directions
    private static (Bitmap right, Bitmap down, Bitmap left, Bitmap up) ParseData(string[] map)
    {
        var right = new Bitmap(map[0].Length - 2, map.Length - 2);
        var down  = new Bitmap(map[0].Length - 2, map.Length - 2);
        var left  = new Bitmap(map[0].Length - 2, map.Length - 2);
        var up    = new Bitmap(map[0].Length - 2, map.Length - 2);

        for(int y = 1; y < map.Length - 1; y++)
        {
            for(int x = 1; x < map[0].Length - 1; x++)
            {
                right.Set(x - 1, y - 1, map[y][x] != '>');
                down.Set(x - 1, y - 1, map[y][x] != 'v');
                left.Set(x - 1, y - 1, map[y][x] != '<');
                up.Set(x - 1, y - 1, map[y][x] != '^');
            }
        }

        return (right, down, left, up);
    }

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

        var blizzardFreeMinute0  = ParseData(data);
        var precomputedFreeCells = PrecomputeBlizzardFreeCells(blizzardFreeMinute0);

        Puzzle1(precomputedFreeCells).Should().Be(18);
        Puzzle2(precomputedFreeCells).Should().Be(18 + 23 + 13);
    }

    [Test]
    public void TestAocInput()
    {
        var data = FileUtils.ReadAllLines(this);

        var blizzardFreeMinute0  = ParseData(data);
        var precomputedFreeCells = PrecomputeBlizzardFreeCells(blizzardFreeMinute0);

        Puzzle1(precomputedFreeCells).Should().Be(332);
        Puzzle2(precomputedFreeCells).Should().Be(942);
    }

    // precompute free space in horizontal and vertical direction so that
    // for every minute free[minute] = horizontal[minute % width] & vertical[minute % height]
    private static (Bitmap horizontal, Bitmap vertical)[] PrecomputeBlizzardFreeCells((Bitmap right, Bitmap down, Bitmap left, Bitmap up) freeMinute0)
    {
        var maxMinute = Math.Max(freeMinute0.right.Width, freeMinute0.right.Height);

        var blizzardFreeCells = new (Bitmap horizontal, Bitmap vertical)[maxMinute];

        blizzardFreeCells[0].horizontal = new Bitmap(freeMinute0.right).And(freeMinute0.left);
        blizzardFreeCells[0].vertical   = new Bitmap(freeMinute0.up).And(freeMinute0.down);

        for(var minute = 1; minute < maxMinute; minute++)
        {
            if(minute < freeMinute0.right.Width)
            {
                freeMinute0.right.RollRight();
                freeMinute0.left.RollLeft();

                blizzardFreeCells[minute].horizontal = new Bitmap(freeMinute0.right).And(freeMinute0.left);
            }

            if(minute < freeMinute0.right.Height)
            {
                freeMinute0.up.RollUp();
                freeMinute0.down.RollDown();

                blizzardFreeCells[minute].vertical = new Bitmap(freeMinute0.up).And(freeMinute0.down);
            }
        }

        return blizzardFreeCells;
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
    private static int Puzzle1((Bitmap horizontal, Bitmap vertical)[] blizzardFreeCells)
    {
        var width  = blizzardFreeCells[0].horizontal.Width;
        var height = blizzardFreeCells[0].vertical.Height;

        return Trip(blizzardFreeCells, (0, 0), (width - 1, height - 1), 1);
    }

    // One of the Elves looks especially dismayed: He forgot his snacks at the entrance to the valley!
    // Since you're so good at dodging blizzards, the Elves humbly request that you go back for his snacks. From the same initial conditions, how
    // quickly can you make it from the start to the goal, then back to the start, then back to the goal?
    //
    // Puzzle ==  What is the fewest number of minutes required to reach the goal, go back to the start, then reach the goal again?
    private static int Puzzle2((Bitmap horizontal, Bitmap vertical)[] blizzardFreeCells)
    {
        var width  = blizzardFreeCells[0].horizontal.Width;
        var height = blizzardFreeCells[0].vertical.Height;

        var trip1 = Trip(blizzardFreeCells, (0, 0), (width - 1, height - 1), 1);
        var trip2 = Trip(blizzardFreeCells, (width - 1, height - 1), (0, 0), trip1 + 1);
        var trip3 = Trip(blizzardFreeCells, (0, 0), (width - 1, height - 1), trip2 + 1);

        return trip3;
    }

    // for every minute compute the free cells which are in reach from the cells visited in the previous minute with:
    // visited(minute) = neighbors(visited(minute-1)) & precomputedfree(minute)
    // if the end pos is a visited cell => finished
    private static int Trip((Bitmap horizontal, Bitmap vertical)[] blizzardFreeCells, (int x, int y) start, (int x, int y) end, int minuteStart)
    {
        var width  = blizzardFreeCells[0].horizontal.Width;
        var height = blizzardFreeCells[0].vertical.Height;

        var visited = new Bitmap(width, height);

        for(int minute = minuteStart; ; minute++)
        {
            visited.AddNeighbors();
            visited.Set(start.x, start.y); // trick to completely ignore walls and start/end location specified in puzzle description

            var freeCellsForMinute = new Bitmap(blizzardFreeCells[minute % width].horizontal)
                                         .And(blizzardFreeCells[minute % height].vertical);

            visited.And(freeCellsForMinute);

            if(visited.Get(end.x, end.y))
            {
                return minute + 1; // walls are ignored and outside the bitmaps so 1 has to be added to reach end pos (which is outside the bitmaps)
            }
        }
    }
}
