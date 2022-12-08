namespace AdventOfCode2022;

using NUnit.Framework;
using FluentAssertions;

using matthiasffm.Common.Math;

[TestFixture]
public class Day08
{
    private byte[,] ParseData(string[] lines)
    {
        var trees = new byte[lines.Length, lines[0].Length];
        trees.Populate((r, c) => (byte)(lines[r][c] - '0'));
        return trees;
    }

    [Test]
    public void TestSamples()
    {
        var data = new [] {
            "30373",
            "25512",
            "65332",
            "33549",
            "35390",
        };
        var trees = ParseData(data);

        Puzzle1(trees).Should().Be(21);
        Puzzle2(trees).Should().Be(2 * 2 * 1 * 2);
    }

    [Test]
    public void TestAocInput()
    {
        var data = File.ReadAllLines(@"day8.data");
        var trees = ParseData(data);

        Puzzle1(trees).Should().Be(1717);
        Puzzle2(trees).Should().Be(321975);
    }

    // First, determine whether there is enough tree cover here to keep a tree house hidden. To do this, you need to count the
    // number of trees that are visible from outside the grid when looking directly along a row or column.
    // A tree is visible if all of the other trees between it and an edge of the grid are shorter than it. Only consider trees in
    // the same row or column; that is, only look up, down, left, or right from any given tree.
    // Puzzle == Consider your map; how many trees are visible from outside the grid?
    private int Puzzle1(byte[,] trees)
    {
        // TODO: too slow O(n*n*n)
        //       precalc 4 lookop matrices which contain the rolling max for every direction on every position in O(n*n)
        return trees.Select((t, r, c) => IsVisible(trees, t, r, c))
                    .Sum();
    }

    private int IsVisible(byte[,] trees, byte tree, int row, int col) =>
        row == 0 || col == 0 || row == trees.GetLength(0) - 1 || col == trees.GetLength(1) - 1 ||
        trees.Row(row).Take(col).All(c => c.Item2 < tree) ||
        trees.Row(row).Skip(col + 1).All(c => c.Item2 < tree) ||
        trees.Col(col).Take(row).All(r => r.Item2 < tree) ||
        trees.Col(col).Skip(row + 1).All(r => r.Item2 < tree)
        ?
        1 : 0;

    // The Elves just need to know the best spot to build their tree house: they would like to be able to see a lot of trees.
    // To measure the viewing distance from a given tree, look up, down, left, and right from that tree; stop if you reach an
    // edge or at the first tree that is the same height or taller than the tree under consideration. (so a tree on the edge == 0)
    // A tree's scenic score is found by multiplying together its viewing distance in each of the four directions.
    // Puzzle == What is the highest scenic score possible for any tree?
    private int Puzzle2(byte[,] trees)
    {
        // TODO: too slow O(n*n*n)
        //       precalc 4 lookop matrices which contain the rolling lenght of local downramps for every direction on every position in O(n*n)
        return trees.Select((t, r, c) => ScenicScore(trees, t, r, c))
                    .Max();
    }

    private int ScenicScore(byte[,] trees, byte tree, int row, int col) =>
        ViewingDistance(tree, trees.Row(row).Take(col).Reverse()) *
        ViewingDistance(tree, trees.Row(row).Skip(col + 1)) *
        ViewingDistance(tree, trees.Col(col).Take(row).Reverse()) *
        ViewingDistance(tree, trees.Col(col).Skip(row + 1));

    private int ViewingDistance(byte tree, IEnumerable<(int, byte)> lineOfSight)
    {
        var scenicScore = 0;

        foreach(var loS in lineOfSight)
        {
            scenicScore++;

            if(loS.Item2 >= tree)
            {
                break;
            }
        }

        return scenicScore;
    }
}
