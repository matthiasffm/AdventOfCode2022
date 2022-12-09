namespace AdventOfCode2022;

using static System.Math;

using NUnit.Framework;
using FluentAssertions;

using matthiasffm.Common.Math;

[TestFixture]
public class Day09
{
    private record Move(Vec2<int> Direction, int Repeat);

    private Move ParseMove(string l)
    {
        int dist = int.Parse(l.AsSpan().Slice(2));

        return l[0] switch {
            'L' => new Move(new Vec2<int>(-1, 0), dist),
            'R' => new Move(new Vec2<int>(1, 0),  dist),
            'U' => new Move(new Vec2<int>(0, -1), dist),
            _   => new Move(new Vec2<int>(0, 1),  dist),
        };
    }

    [Test]
    public void TestSamples()
    {
        var sample1 = new [] {
            "R 4",
            "U 4",
            "L 3",
            "D 1",
            "R 4",
            "D 1",
            "L 5",
            "R 2",
        };
        var moves1 = sample1.Select(l => ParseMove(l));

        Puzzle(moves1, 2).Should().Be(13);
        Puzzle(moves1, 10).Should().Be(1);

        var sample2 = new [] {
            "R 5",
            "U 8",
            "L 8",
            "D 3",
            "R 17",
            "D 10",
            "L 25",
            "U 20",
        };
        var moves2 = sample2.Select(l => ParseMove(l));

        Puzzle(moves2, 10).Should().Be(36);
    }

    [Test]
    public void TestAocInput()
    {
        var moves = File.ReadAllLines(@"day9.data").Select(l => ParseMove(l));

        Puzzle(moves, 2).Should().Be(6745);
        Puzzle(moves, 10).Should().Be(2793);
    }

    // moves a knot if it is not close enough (d > 1)
    private Vec2<int> MoveKnot(Vec2<int> knot, Vec2<int> diff) =>
        Abs(diff.X) > 1 || Abs(diff.Y) > 1 ?
            new Vec2<int>(knot.X + Min(1, Abs(diff.X)) * Sign(diff.X),
                          knot.Y + Min(1, Abs(diff.Y)) * Sign(diff.Y)) :
            knot;

    // add move to head and then reposition all other knots in the rope
    private void MoveRope(Vec2<int>[] rope, Vec2<int> move)
    {
        rope[0] = rope.First() + move;

        for(int k = 1; k < rope.Length; k++)
        {
            rope[k] = MoveKnot(rope[k], rope[k - 1] - rope[k]);
        }
    }

    // Consider a rope with a knot at each end; these knots mark the head and the tail of the rope. If the head moves far enough away from the
    // tail, the tail is pulled toward the head. All elements of the rope from head to tail must always be touching at every single move (diagonally adjacent
    // and even overlapping both count as touching). After simulating the ropes moves, you can count up all of the positions the tail visited at least once.
    // Puzzle == Simulate the series of motions. How many positions does the tail of the rope visit at least once?
    private int Puzzle(IEnumerable<Move> moves, int ropeLength)
    {
        var rope        = Enumerable.Repeat(new Vec2<int>(0, 0), ropeLength).ToArray();
        var tailVisited = new [] { rope.Last() }.ToHashSet();

        foreach(var move in moves)
        {
            for(int m = 0; m < move.Repeat; m++)
            {
                MoveRope(rope, move.Direction);
                tailVisited.Add(rope.Last());
            }
        }

        return tailVisited.Count;
    }
}
