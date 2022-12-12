namespace AdventOfCode2022;

using NUnit.Framework;
using FluentAssertions;

using matthiasffm.Common.Math;
using matthiasffm.Common.Algorithms;

[TestFixture]
public class Day12
{
    private static int Elevation(char c) => c - 'a';

    private static (int[,], (int, int), (int, int)) ParseData(string[] data)
    {
        int[,]      heightmap = new int[data.Length, data[0].Length];
        (int, int)  startPos  = (0, 0);
        (int, int)  goal      = (0, 0);

        for(int row = 0; row < data.Length; row++)
        {
            for(int col = 0; col < data[0].Length; col++)
            {
                if(data[row][col] == 'S')
                {
                    startPos = (row, col);
                    heightmap[row, col] = Elevation('a');
                }
                else if(data[row][col] == 'E')
                {
                    goal = (row, col);
                    heightmap[row, col] = Elevation('z');;
                }
                else
                {
                    heightmap[row, col] = Elevation(data[row][col]);;
                }
            }
        }

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

    // The heightmap shows the local area from above broken into a grid with values from zero to 26.
    // You'd like to reach the end goal location with the best signal, but to save energy, you should do it in as few
    // steps as possible. During each step, you can move exactly one square up, down, left, or right. To avoid needing
    // to get out your climbing gear, the elevation of the destination square can be at most one higher than the elevation
    // of your current square.
    // Puzzle == What is the fewest steps required to move from the start to the location that should get the best signal?
    private static int Puzzle1(int[,] heightmap, (int, int) startPos, (int, int) goal)
    {
        var bestPath = Search.AStar(heightmap.Select((e, r, c) => (r, c)),
                                    (startPos.Item1, startPos.Item2),
                                    (goal.Item1, goal.Item2),
                                    pos => Neighbors(heightmap, pos.Item1, pos.Item2),
                                    (pos, n) => 1,
                                    pos => Math.Abs(pos.Item1 - goal.Item1) + Math.Abs(pos.Item2 - goal.Item2),
                                    int.MaxValue);

        return bestPath.Count() - 1; // steps == nodes in path - 1
    }

    // As you walk up the hill, you suspect that the Elves will want to turn this into a hiking trail. The beginning isn't very
    // scenic, though; perhaps you can find a better starting point. To maximize exercise while hiking, the trail should start as
    // low as possible: elevation zero. The goal is still the same. However, the trail should still be direct, taking the fewest
    // steps to reach its goal. So, you'll need to find the shortest path from any square at elevation zero.
    // Puzzle == What is the fewest steps required to move starting from any square with elevation zero to the location that
    //           should get the best signal?
    private static int Puzzle2(int[,] heightmap, (int, int) goal)
    {
        return heightmap.Where((h, r, c) => h == 0)
                        .Select(tuple => Puzzle1(heightmap, (tuple.Item2, tuple.Item3), goal))
                        .Where(pathlength => pathlength > 0) // some starting locations could lead to a dead end
                        .Min();
    }

    private static IEnumerable<(int, int)> Neighbors(int[,] heightmap, int row, int col)
    {
        if(row > 0 && heightmap[row - 1, col] <= heightmap[row, col] + 1)
        {
            yield return (row - 1, col);
        }

        if(col > 0 && heightmap[row, col - 1] <= heightmap[row, col] + 1)
        {
            yield return (row, col - 1);
        }

        if(row < heightmap.GetLength(0) - 1 && heightmap[row + 1, col] <= heightmap[row, col] + 1)
        {
            yield return (row + 1, col);
        }

        if(col < heightmap.GetLength(1) - 1 && heightmap[row, col + 1] <= heightmap[row, col] + 1)
        {
            yield return (row, col + 1);
        }
    }
}
