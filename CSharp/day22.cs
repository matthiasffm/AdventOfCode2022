namespace AdventOfCode2022;

using NUnit.Framework;
using FluentAssertions;

using System.Numerics;
using System.Text.RegularExpressions;

using matthiasffm.Common.Math;

[TestFixture]
public partial class Day22
{
    private record Move(int Steps, char Turn);

    [GeneratedRegex("(?<=[L_R])")]
    private static partial Regex SplitMovementRegex();

    private static (char[,], IEnumerable<Move>) ParseData(string[] lines)
    {
        var tmp = string.Join('|', lines).Split("||");

        var rows     = tmp[0].Split('|', StringSplitOptions.RemoveEmptyEntries);
        var nmbrCols = rows.Max(r => r.Length);
        var board    = rows.Select(r => r.Concat(Enumerable.Repeat(' ', nmbrCols - r.Length)).ToArray())
                           .ToArray()
                           .ConvertToMatrix();

        var moves = SplitMovementRegex().Split(tmp[1] + "_")
                                        .Where(s => !string.IsNullOrWhiteSpace(s))
                                        .Select(s => new Move(int.Parse(s[..(s.Length-1)]), s.Last()));

        return (board, moves);
    }

    // simplified utility class for matrix multiplication with vec2 and vec3
    private record class Mat3<T>(T[,] Values) where T: INumber<T>
    {
        public T this[int row, int col]
        {
            get { return Values[row, col]; }
        }

        public static Vec3<T> operator *(Vec3<T> left, Mat3<T> right)
        {
            return new Vec3<T>(left.X * right[0, 0] + left.Y * right[1, 0] + left.Z * right[2, 0],
                               left.X * right[0, 1] + left.Y * right[1, 1] + left.Z * right[2, 1],
                               left.X * right[0, 2] + left.Y * right[1, 2] + left.Z * right[2, 2]);
        }

        // expands (x, y) vector to a (x, y, 1) vector and multiplies it with a matrix
        public static Vec2<T> operator *(Vec2<T> left, Mat3<T> right)
        {
            var res3 = new Vec3<T>(left.X, left.Y, T.One) * right;
            return new Vec2<T>(res3.X / res3.Z, res3.Y / res3.Z);
        }
    }

    // a neighbor is a side with a transformation matrix to reposition and a new direction
    private record Neighbor(Side Other, Mat3<int> Transform, int NewDir);

    // side of a thing/cube with 4 neighboring sides
    // neighbors are positioned clockwise. index 0 is to the right, index 1 is to the bottom, 2 is to the left and 3 is to the top
    private record Side(int Left, int Top, int Right, int Bottom)
    {
        private Neighbor[] _neighbors = Array.Empty<Neighbor>();

        public void SetNeighbors(Neighbor[] neighbors)
        {
            _neighbors = neighbors;
        }

        public bool IsOutside(Vec2<int> pos)
            => pos.X < Left || pos.X > Right || pos.Y < Top || pos.Y > Bottom;

        // select 1 of the 4 neighbors according to which direction the overflow happens
        public Neighbor FindNeighboringSide(Vec2<int> outsidePos) => outsidePos switch {
            { X: var x } when x > Right  => _neighbors[0],
            { Y: var y } when y > Bottom => _neighbors[1],
            { X: var x } when x < Left   => _neighbors[2],
            _                            => _neighbors[3],
        };
    }

    [Test]
    public void TestSamples()
    {
        var data = new [] { 
            "        ...#",
            "        .#..",
            "        #...",
            "        ....",
            "...#.......#",
            "........#...",
            "..#....#....",
            "..........#.",
            "        ...#....",
            "        .....#..",
            "        .#......",
            "        ......#.",
            "",
            "10R5L5R10L4R5L5",
        };
        var (board, moves) = ParseData(data);

        // side length: 4
        // side layout:     1
        //              2 3 4
        //                  5 6

        var side1 = new Side( 8, 0, 11,  3);
        var side2 = new Side( 0, 4,  3,  7);
        var side3 = new Side( 4, 4,  7,  7);
        var side4 = new Side( 8, 4, 11,  7);
        var side5 = new Side( 8, 8, 11, 11);
        var side6 = new Side(12, 8, 15, 11);

        // prepare sides for a flat 2D board for part 1 of the puzzle

        side1.SetNeighbors(new[] {
            new Neighbor(side1, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {-4,  0, 1} }), 0),
            new Neighbor(side4, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, { 0,  0, 1} }), 1),
            new Neighbor(side1, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, { 4,  0, 1} }), 2),
            new Neighbor(side5, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, { 0, 12, 1} }), 3),
        });
        side2.SetNeighbors(new[] {
            new Neighbor(side3, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, { 0,  0, 1} }), 0),
            new Neighbor(side2, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, { 0, -4, 1} }), 1),
            new Neighbor(side4, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {12,  0, 1} }), 2),
            new Neighbor(side2, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, { 0,  4, 1} }), 3),
        });
        side3.SetNeighbors(new[] {
            new Neighbor(side4, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {0,  0, 1} }), 0),
            new Neighbor(side3, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {0, -4, 1} }), 1),
            new Neighbor(side2, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {0,  0, 1} }), 2),
            new Neighbor(side3, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {0,  4, 1} }), 3),
        });
        side4.SetNeighbors(new[] {
            new Neighbor(side2, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {-12, 0, 1} }), 0),
            new Neighbor(side5, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {  0, 0, 1} }), 1),
            new Neighbor(side3, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {  0, 0, 1} }), 2),
            new Neighbor(side1, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {  0, 0, 1} }), 3),
        });
        side5.SetNeighbors(new[] {
            new Neighbor(side6, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {0,   0, 1} }), 0),
            new Neighbor(side1, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {0, -12, 1} }), 1),
            new Neighbor(side6, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {8,   0, 1} }), 2),
            new Neighbor(side4, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {0,   0, 1} }), 3),
        });
        side6.SetNeighbors(new[] {
            new Neighbor(side5, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {-8,  0, 1} }), 0),
            new Neighbor(side6, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, { 0, -4, 1} }), 1),
            new Neighbor(side5, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, { 0,  0, 1} }), 2),
            new Neighbor(side6, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, { 0,  4, 1} }), 3),
        });

        Puzzle(board, new Vec2<int>(8, 0), side1, 0, moves).Should().Be(1000 * 6 + 4 * 8 + 0);

        // prepare sides for a cube flattened on a 2D board for part 2 of the puzzle

        side1.SetNeighbors(new[] {
            new Neighbor(side6, new Mat3<int>(new int[,] { { 1, 0, 0}, {0, -1, 0}, { 3, 11, 1} }), 2),
            new Neighbor(side4, new Mat3<int>(new int[,] { { 1, 0, 0}, {0,  1, 0}, { 0,  0, 1} }), 1),
            new Neighbor(side3, new Mat3<int>(new int[,] { { 0, 1, 0}, {1,  0, 0}, { 4, -3, 1} }), 1),
            new Neighbor(side2, new Mat3<int>(new int[,] { {-1, 0, 0}, {0,  1, 0}, {11,  5, 1} }), 1),
        });
        side2.SetNeighbors(new[] {
            new Neighbor(side3, new Mat3<int>(new int[,] { { 1, 0, 0}, { 0, 1, 0}, { 0,  0, 1} }), 0),
            new Neighbor(side5, new Mat3<int>(new int[,] { {-1, 0, 0}, { 0, 1, 0}, {11,  3, 1} }), 3),
            new Neighbor(side6, new Mat3<int>(new int[,] { { 0, 1, 0}, {-1, 0, 0}, {19, 12, 1} }), 3),
            new Neighbor(side1, new Mat3<int>(new int[,] { {-1, 0, 0}, { 0, 1, 0}, {11, -3, 1} }), 1),
        });
        side3.SetNeighbors(new[] {
            new Neighbor(side4, new Mat3<int>(new int[,] { {1,  0, 0}, {0, 1, 0}, {0,  0, 1} }), 0),
            new Neighbor(side5, new Mat3<int>(new int[,] { {0, -1, 0}, {1, 0, 0}, {0, 15, 1} }), 0),
            new Neighbor(side2, new Mat3<int>(new int[,] { {1,  0, 0}, {0, 1, 0}, {0,  0, 1} }), 2),
            new Neighbor(side1, new Mat3<int>(new int[,] { {0,  1, 0}, {1, 0, 0}, {5, -4, 1} }), 0),
        });
        side4.SetNeighbors(new[] {
            new Neighbor(side6, new Mat3<int>(new int[,] { {0, 1, 0}, {-1, 0, 0}, {19, -4, 1} }), 1),
            new Neighbor(side5, new Mat3<int>(new int[,] { {1, 0, 0}, { 0, 1, 0}, { 0,  0, 1} }), 1),
            new Neighbor(side3, new Mat3<int>(new int[,] { {1, 0, 0}, { 0, 1, 0}, { 0,  0, 1} }), 2),
            new Neighbor(side1, new Mat3<int>(new int[,] { {1, 0, 0}, { 0, 1, 0}, { 0,  0, 1} }), 3),
        });
        side5.SetNeighbors(new[] {
            new Neighbor(side6, new Mat3<int>(new int[,] { { 1, 0, 0}, { 0, 1, 0}, { 0,  0, 1} }), 0),
            new Neighbor(side2, new Mat3<int>(new int[,] { {-1, 0, 0}, { 0, 1, 0}, {11, -5, 1} }), 3),
            new Neighbor(side3, new Mat3<int>(new int[,] { { 0, 1, 0}, {-1, 0, 0}, {15,  0, 1} }), 3),
            new Neighbor(side4, new Mat3<int>(new int[,] { { 1, 0, 0}, {0, 1, 0}, {0, 0, 1} }), 3),
        });
        side6.SetNeighbors(new[] {
            new Neighbor(side1, new Mat3<int>(new int[,] { {1,  0, 0}, {0, -1, 0}, { -5, 11, 1} }), 2),
            new Neighbor(side2, new Mat3<int>(new int[,] { {0, -1, 0}, {1,  0, 0}, {-12, 19, 1} }), 0),
            new Neighbor(side5, new Mat3<int>(new int[,] { {1,  0, 0}, {0,  1, 0}, {  0,  0, 1} }), 2),
            new Neighbor(side4, new Mat3<int>(new int[,] { {0, -1, 0}, {1,  0, 0}, {  3, 19, 1} }), 2),
        });

        Puzzle(board, new Vec2<int>(8, 0), side1, 0, moves).Should().Be(1000 * 5 + 4 * 7 + 3);
    }

    [Test]
    public void TestAocInput()
    {
        var data           = FileUtils.ReadAllLines(this);
        var (board, moves) = ParseData(data);

        // side length: 50
        // side layout:   1 2
        //                3
        //              4 5
        //              6

        var side1 = new Side( 50,   0,  99,  49);
        var side2 = new Side(100,   0, 149,  49);
        var side3 = new Side( 50,  50,  99,  99);
        var side4 = new Side(  0, 100,  49, 149);
        var side5 = new Side( 50, 100,  99, 149);
        var side6 = new Side(  0, 150,  49, 199);

        // prepare sides for a flat 2D board for part 1 of the puzzle

        side1.SetNeighbors(new[] {
            new Neighbor(side2, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {  0,   0, 1} }), 0),
            new Neighbor(side3, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {  0,   0, 1} }), 1),
            new Neighbor(side2, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {100,   0, 1} }), 2),
            new Neighbor(side5, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {  0, 150, 1} }), 3),
        });
        side2.SetNeighbors(new[] {
            new Neighbor(side1, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {-100,   0, 1} }), 0),
            new Neighbor(side2, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {   0, -50, 1} }), 1),
            new Neighbor(side1, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {   0,   0, 1} }), 2),
            new Neighbor(side2, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {   0,  50, 1} }), 3),
        });
        side3.SetNeighbors(new[] {
            new Neighbor(side3, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {-50, 0, 1} }), 0),
            new Neighbor(side5, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {  0, 0, 1} }), 1),
            new Neighbor(side3, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, { 50, 0, 1} }), 2),
            new Neighbor(side1, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {  0, 0, 1} }), 3),
        });
        side4.SetNeighbors(new[] {
            new Neighbor(side5, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {  0,   0, 1} }), 0),
            new Neighbor(side6, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {  0,   0, 1} }), 1),
            new Neighbor(side5, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {100,   0, 1} }), 2),
            new Neighbor(side6, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {  0, 100, 1} }), 3),
        });
        side5.SetNeighbors(new[] {
            new Neighbor(side4, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {-100,    0, 1} }), 0),
            new Neighbor(side1, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {   0, -150, 1} }), 1),
            new Neighbor(side4, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {   0,    0, 1} }), 2),
            new Neighbor(side3, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {   0,    0, 1} }), 3),
        });
        side6.SetNeighbors(new[] {
            new Neighbor(side6, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {-50,    0, 1} }), 0),
            new Neighbor(side4, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {  0, -100, 1} }), 1),
            new Neighbor(side6, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, { 50,    0, 1} }), 2),
            new Neighbor(side4, new Mat3<int>(new int[,] { {1, 0, 0}, {0, 1, 0}, {  0,    0, 1} }), 3),
        });

        Puzzle(board, new Vec2<int>(50, 0), side1, 0, moves).Should().Be(164014);

        // prepare sides for a cube flattened on a 2D board for part 2 of the puzzle

        side1.SetNeighbors(new[] {
            new Neighbor(side2, new Mat3<int>(new int[,] { {1, 0, 0}, {0,  1, 0}, {   0,   0, 1} }), 0),
            new Neighbor(side3, new Mat3<int>(new int[,] { {1, 0, 0}, {0,  1, 0}, {   0,   0, 1} }), 1),
            new Neighbor(side4, new Mat3<int>(new int[,] { {1, 0, 0}, {0, -1, 0}, { -49, 149, 1} }), 0),
            new Neighbor(side6, new Mat3<int>(new int[,] { {0, 1, 0}, {1,  0, 0}, {   1, 100, 1} }), 0),
        });
        side2.SetNeighbors(new[] {
            new Neighbor(side5, new Mat3<int>(new int[,] { {1, 0, 0}, {0, -1, 0}, { -51, 149, 1} }), 2),
            new Neighbor(side3, new Mat3<int>(new int[,] { {0, 1, 0}, {1,  0, 0}, {  49, -50, 1} }), 2),
            new Neighbor(side1, new Mat3<int>(new int[,] { {1, 0, 0}, {0,  1, 0}, {   0,   0, 1} }), 2),
            new Neighbor(side6, new Mat3<int>(new int[,] { {1, 0, 0}, {0,  1, 0}, {-100, 200, 1} }), 3),
        });
        side3.SetNeighbors(new[] {
            new Neighbor(side2, new Mat3<int>(new int[,] { {0, 1, 0}, {1,  0, 0}, {  50, -51, 1} }), 3),
            new Neighbor(side5, new Mat3<int>(new int[,] { {1, 0, 0}, {0,  1, 0}, {   0,   0, 1} }), 1),
            new Neighbor(side4, new Mat3<int>(new int[,] { {0, 1, 0}, {1,  0, 0}, { -50,  51, 1} }), 1),
            new Neighbor(side1, new Mat3<int>(new int[,] { {1, 0, 0}, {0,  1, 0}, {   0,   0, 1} }), 3),
        });
        side4.SetNeighbors(new[] {
            new Neighbor(side5, new Mat3<int>(new int[,] { {1, 0, 0}, {0,  1, 0}, {   0,   0, 1} }), 0),
            new Neighbor(side6, new Mat3<int>(new int[,] { {1, 0, 0}, {0,  1, 0}, {   0,   0, 1} }), 1),
            new Neighbor(side1, new Mat3<int>(new int[,] { {1, 0, 0}, {0, -1, 0}, {  51, 149, 1} }), 0),
            new Neighbor(side3, new Mat3<int>(new int[,] { {0, 1, 0}, {1,  0, 0}, { -49,  50, 1} }), 0),
        });
        side5.SetNeighbors(new[] {
            new Neighbor(side2, new Mat3<int>(new int[,] { {1, 0, 0}, {0, -1, 0}, {  49, 149, 1} }), 2),
            new Neighbor(side6, new Mat3<int>(new int[,] { {0, 1, 0}, {1,  0, 0}, {-101, 100, 1} }), 2),
            new Neighbor(side4, new Mat3<int>(new int[,] { {1, 0, 0}, {0,  1, 0}, {   0,   0, 1} }), 2),
            new Neighbor(side3, new Mat3<int>(new int[,] { {1, 0, 0}, {0,  1, 0}, {   0,   0, 1} }), 3),
        });
        side6.SetNeighbors(new[] {
            new Neighbor(side5, new Mat3<int>(new int[,] { {0, 1, 0}, {1,  0, 0}, {-100,  99, 1} }), 3),
            new Neighbor(side2, new Mat3<int>(new int[,] { {1, 0, 0}, {0,  1, 0}, { 100,-200, 1} }), 1),
            new Neighbor(side1, new Mat3<int>(new int[,] { {0, 1, 0}, {1,  0, 0}, {-100,   1, 1} }), 1),
            new Neighbor(side4, new Mat3<int>(new int[,] { {1, 0, 0}, {0,  1, 0}, {   0,   0, 1} }), 3),
        });

        Puzzle(board, new Vec2<int>(50, 0), side1, 0, moves).Should().Be(47525);
    }

    // vector offsets for all 4 movement directions: to the right, down, to the left and up
    private static readonly Vec2<int>[] Directions = new Vec2<int>[] {
        new(1, 0),
        new(0, 1),
        new(-1, 0),
        new(0, -1),
    };

    // As you walk, the monkeys explain that the grove is protected by a force field. To pass through the force field, you have to enter a password; doing so involves
    // tracing a specific path on a strangely-shaped board. The monkeys give you notes that they took when they last saw the password entered (your puzzle input).
    // The first half of the monkeys' notes is a map of the board. It is comprised of a set of open tiles (drawn .) and solid walls (drawn #).
    // The second half is a description of the path you must follow. It consists of alternating numbers and letters:
    // - A number indicates the number of tiles to move in the direction you are facing. If you run into a wall, you stop moving forward and continue with the next instruction.
    // - A letter indicates whether to turn 90 degrees clockwise (R) or counterclockwise (L). Turning happens in-place; it does not change your current tile.
    // If a movement instruction would take you off of the map, you wrap around to the other side of the board. Be careful how the map is layed our and/or folded!
    // You begin the path in the leftmost open tile of the top row of tiles = input parameter pos. Initially, you are facing to the right (from the perspective of
    // how the map is drawn), this is input parameter dir.
    // To finish providing the password to this strange input device, you need to determine numbers for your final row, column, and facing as your final position appears
    // from the perspective of the original map.
    //
    // Puzzle == The final password is the sum of 1000 times the row, 4 times the column, and the facing. What is the final password?
    private static int Puzzle(char[,] board, Vec2<int> pos, Side side, int dir, IEnumerable<Move> moves)
    {
        foreach(var move in moves)
        {
            for(int step = 0; step < move.Steps; step++)
            {
                // calc next position
                var nextDir  = dir;
                var nextSide = side;
                var nextPos  = pos + Directions[dir];

                // change sides and position if border reached
                if(side.IsOutside(nextPos))
                {
                    var neighboringSide = side.FindNeighboringSide(nextPos);

                    nextDir  = neighboringSide.NewDir;
                    nextSide = neighboringSide.Other;
                    nextPos *= neighboringSide.Transform;
                }

                // hit wall => end move, so _dont_ use new calculated position
                if(board[nextPos.Y, nextPos.X] == '#')
                {
                    break;
                }

                dir  = nextDir;
                side = nextSide;
                pos  = nextPos;
            }

            // turn so that dir indexes the next vector clock- or counterclockwise in the Directions array
            dir = move.Turn switch {
                'L' => Mod(dir - 1, Directions.Length),
                'R' => Mod(dir + 1, Directions.Length),
                _   => dir,
            };
        }

        return 1000 * (pos.Y + 1) + 4 * (pos.X + 1) + dir;
    }
    private static int Mod(int nmbr, int mod)
    {
        var res = nmbr % mod;
        return (res < 0 && mod > 0) || (res > 0 && mod < 0) ? res + mod : res;
    }

}
