namespace AdventOfCode2022;

using NUnit.Framework;
using FluentAssertions;

using matthiasffm.Common.Math;
using matthiasffm.Common.Algorithms;

[TestFixture]
public class Day12
{
    private static (byte[,], (int, int), (int, int)) ParseData(string[] data)
    {
        (int, int)  startPos  = (0, 0);
        (int, int)  goal      = (0, 0);
        byte[,]     heightmap = FileUtils.ParseToMatrix(data, (c, row, col) => {
            if(c == 'S')
            {
                startPos = (row, col);
                return 0;
            }
            else if(c == 'E')
            {
                goal = (row, col);
                return 'z' - 'a';
            }
            else
            {
                return (byte)(c - 'a');
            }
        });

        return (heightmap, startPos, goal);
    }

    [Test]
    public void TestSamples()
    {
        var data = new[] {
            "Sabqponm",
            "abcryxxl",
            "accszExk",
            "acctuvwj",
            "abdefghi",
        };

        var (heightmap, startPos, goal) = ParseData(data);

        Puzzle1(heightmap, startPos, goal).Should().Be(31);
        Puzzle2(heightmap, goal).Should().Be(29);
    }

    [Test]
    public void TestAocInput()
    {
        var data = FileUtils.ReadAllLines(this);
        var (heightmap, startPos, goal) = ParseData(data);

        Puzzle1(heightmap, startPos, goal).Should().Be(408);
        Puzzle2(heightmap, goal).Should().Be(399);
    }

    // every step (left, up, down, right) in the heightmap has the same cost == 1
    private const int CostForOneStep = 1;

    // The heightmap shows the local area from above broken into a grid with values from zero to 26.
    // You'd like to reach the end goal location with the best signal, but to save energy, you should do it in as few
    // steps as possible. During each step, you can move exactly one square up, down, left, or right. To avoid needing
    // to get out your climbing gear, the elevation of the destination square can be at most one higher than the elevation
    // of your current square.
    //
    // Puzzle == What is the fewest steps required to move from the start to the location that should get the best signal?
    private static int Puzzle1(byte[,] heightmap, (int, int) startPos, (int, int) goal)
    {
        var bestPath = Search.AStar(heightmap.Select((e, r, c) => (r, c)),
                                    (startPos.Item1, startPos.Item2),
                                    (goal.Item1, goal.Item2),
                                    pos => Neighbors(heightmap, pos.Item1, pos.Item2),
                                    (pos1, pos2) => CostForOneStep,
                                    pos => EstimateToGoal(pos, goal),
                                    int.MaxValue);

        return bestPath.Count() - 1; // steps == nodes in path - 1
    }

    // neighbors are all 4 nodes (left, up, down, right) which are lower or only 1 step higher
    private static IEnumerable<(int, int)> Neighbors(byte[,] heightmap, int row, int col)
    {
        if(row > 0 && heightmap[row - 1, col] <= heightmap[row, col] + 1)
        {
            yield return (row - 1, col);
        }

        if(row < heightmap.GetLength(0) - 1 && heightmap[row + 1, col] <= heightmap[row, col] + 1)
        {
            yield return (row + 1, col);
        }

        if(col > 0 && heightmap[row, col - 1] <= heightmap[row, col] + 1)
        {
            yield return (row, col - 1);
        }

        if(col < heightmap.GetLength(1) - 1 && heightmap[row, col + 1] <= heightmap[row, col] + 1)
        {
            yield return (row, col + 1);
        }
    }

    // the goal location is known, so the best heuristic for the distance still to travel is the manhattan distance from pos
    private static int EstimateToGoal((int, int) pos, (int, int) goal) => Math.Abs(pos.Item1 - goal.Item1) + Math.Abs(pos.Item2 - goal.Item2);

    // As you walk up the hill, you suspect that the Elves will want to turn this into a hiking trail. The beginning isn't very
    // scenic, though; perhaps you can find a better starting point. To maximize exercise while hiking, the trail should start as
    // low as possible: elevation zero. The goal is still the same. However, the trail should still be direct, taking the fewest
    // steps to reach its goal. So, you'll need to find the shortest path from any square at elevation zero.
    //
    // Puzzle == What is the fewest steps required to move starting from any square with elevation zero to the location that
    //           should get the best signal?
    private static int Puzzle2(byte[,] heightmap, (int, int) goal)
    {
        // Instead of searching the best path for all zero starting locations we just invert the search
        // direction from Puzzle1 and start at the _goal_. Now if we reach a zero location, we found
        // the the (shortest) hike from goal to zero.

        var shortestHike = Search2.AStar(heightmap.Select((e, r, c) => (r, c)),
                                         (goal.Item1, goal.Item2),
                                         pos => heightmap[pos.Item1, pos.Item2] == 0,
                                         pos => NeighborsInverse(heightmap, pos.Item1, pos.Item2),
                                         (pos1, pos2) => CostForOneStep,
                                         pos => EstimateToAnA(heightmap, pos),
                                         int.MaxValue);

        return shortestHike.Count() - 1; // steps == nodes in path - 1
    }

    // neighbors are all 4 nodes (left, up, down, right) which are higher or only 1 step lower
    private static IEnumerable<(int, int)> NeighborsInverse(byte[,] heightmap, int row, int col)
    {
        if(row > 0 && heightmap[row - 1, col] >= heightmap[row, col] - 1)
        {
            yield return (row - 1, col);
        }

        if(row < heightmap.GetLength(0) - 1 && heightmap[row + 1, col] >= heightmap[row, col] - 1)
        {
            yield return (row + 1, col);
        }

        if(col > 0 && heightmap[row, col - 1] >= heightmap[row, col] - 1)
        {
            yield return (row, col - 1);
        }

        if(col < heightmap.GetLength(1) - 1 && heightmap[row, col + 1] >= heightmap[row, col] - 1)
        {
            yield return (row, col + 1);
        }
    }

    // the goal location is unknown, but we know we can only hike down max 1 step at a time and the goal is at
    // height 0, so the best heuristic for the distance still to travel is the current height value at pos
    private static int EstimateToAnA(byte[,] heightmap, (int, int) pos) => heightmap[pos.Item1, pos.Item2];
}
