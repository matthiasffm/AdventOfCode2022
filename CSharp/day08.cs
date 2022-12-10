namespace AdventOfCode2022;

using NUnit.Framework;
using FluentAssertions;

using matthiasffm.Common.Math;

[TestFixture]
public class Day08
{
    private static byte[,] ParseData(string[] lines)
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
        var data  = FileUtils.ReadAllLines(this);
        var trees = ParseData(data);

        Puzzle1(trees).Should().Be(1717);
        Puzzle2(trees).Should().Be(321975);
    }

    // First, determine whether there is enough tree cover here to keep a tree house hidden. To do this, you need to count the
    // number of trees that are visible from outside the grid when looking directly along a row or column.
    // A tree is visible if all of the other trees between it and an edge of the grid are shorter than it. Only consider trees in
    // the same row or column; that is, only look up, down, left, or right from any given tree.
    // Puzzle == Consider your map; how many trees are visible from outside the grid?
    private static int Puzzle1(byte[,] trees)
    {
        // compute visibility of the inner trees without the border (border is always visible) separately on all 4 axis
        // by using a rolling maximum value from each border
        // O(4*n*n) time, O(n*n) space

        var visibility  = new bool[trees.GetLength(0), trees.GetLength(1)];
        visibility.Populate((r, c) => false);

        Parallel.For(1, trees.GetLength(0) - 1, row =>
        {
            byte maxFromLeft = trees[row, 0];
            for(int col = 1; col < trees.GetLength(1) - 1; col++)
            {
                visibility[row, col] |= maxFromLeft < trees[row, col];
                maxFromLeft = Math.Max(maxFromLeft, trees[row, col]);
            }

            byte maxFromRight = trees[row, trees.GetLength(1) - 1];
            for(int col = trees.GetLength(1) - 2; col > 0; col--)
            {
                visibility[row, col] |= maxFromRight < trees[row, col];
                maxFromRight = Math.Max(maxFromRight, trees[row, col]);
            }
        });

        Parallel.For(1, trees.GetLength(1) - 1, col =>
        {
            byte maxFromTop  = trees[0, col];
            for(int row = 1; row < trees.GetLength(0) - 1; row++)
            {
                visibility[row, col] |= maxFromTop < trees[row, col];
                maxFromTop = Math.Max(maxFromTop, trees[row, col]);
            }

            byte maxFromBottom = trees[trees.GetLength(1) - 1, col];
            for(int row = trees.GetLength(0) - 2; row > 0; row--)
            {
                visibility[row, col] |= maxFromBottom < trees[row, col];
                maxFromBottom = Math.Max(maxFromBottom, trees[row, col]);
            }
        });

        // count visible inner trees and add guaranteed visible border trees

        return trees.Count((t, r, c) => visibility[r, c]) +
                                        (trees.GetLength(0) + trees.GetLength(1)) * 2 - 4;
    }

    // The Elves just need to know the best spot to build their tree house: they would like to be able to see a lot of trees.
    // To measure the viewing distance from a given tree, look up, down, left, and right from that tree; stop if you reach an
    // edge or at the first tree that is the same height or taller than the tree under consideration. (so a tree on the edge == 0)
    // A tree's scenic score is found by multiplying together its viewing distance in each of the four directions.
    // Puzzle == What is the highest scenic score possible for any tree?
    private static int Puzzle2(byte[,] trees)
    {
        // compute viewing distances separately on all 4 axis by scanning the heights from each border
        // for this we use a lookup array with tree_height -> closest tree with this height
        // for every tree we have to just find the closest index > current tree height in this array (less than 10 comparisons)
        // scenicScore[border] == 0 so we dont loop over it, just init the scenicScore with 0 at the beginning
        // O(4*n*n*height) time, O(n*n + height) space with height = 0..9

        var scenicScore  = new int[trees.GetLength(0), trees.GetLength(1)];
        scenicScore.Populate((r, c) => 0);

        Parallel.For(1, trees.GetLength(0) - 1, row =>
        {
            var lastIdxForHeight = new int[10];

            Array.Fill(lastIdxForHeight, 0);
            for(int col = 1; col < trees.GetLength(1) - 1; col++)
            {
                scenicScore[row, col] = DistanceLeft(lastIdxForHeight, trees[row, col], col);
                lastIdxForHeight[trees[row, col]] = col;
            }

            Array.Fill(lastIdxForHeight, trees.GetLength(1) -1);
            for(int col = trees.GetLength(1) - 1; col > 0; col--)
            {
                scenicScore[row, col] *= DistanceRight(lastIdxForHeight, trees[row, col], col);
                lastIdxForHeight[trees[row, col]] = col;
            }
        });

        Parallel.For(1, trees.GetLength(1) - 1, col =>
        {
            var lastIdxForHeight = new int[10];

            Array.Fill(lastIdxForHeight, 0);
            for(int row = 1; row < trees.GetLength(0) - 1; row++)
            {
                scenicScore[row, col] *= DistanceLeft(lastIdxForHeight, trees[row, col], row);
                lastIdxForHeight[trees[row, col]] = row;
            }

            Array.Fill(lastIdxForHeight, trees.GetLength(0) -1);
            for(int row = trees.GetLength(0) - 1; row > 0; row--)
            {
                scenicScore[row, col] *= DistanceRight(lastIdxForHeight, trees[row, col], row);
                lastIdxForHeight[trees[row, col]] = row;
            }
        });

        return scenicScore.Select((t, r, c) => t)
                          .Max();
    }

    // find the closest treeheight to pos in the lookup array where the height >= tree
    // we look from the left (or top) so closest == max index
    private static int DistanceLeft(int[] heightIdx, byte tree, int pos)
    {
        int max = 0;
        for(int i = tree; i < 10; i++)
        {
            max = Math.Max(max, heightIdx[i]);
        }

        return pos - max;
    }

    // find the closest treeheight to pos in the lookup array where the height >= tree
    // we look from the right (or bottom) so closest == min index
    private static int DistanceRight(int[] heightIdx, byte tree, int pos)
    {
        int min = int.MaxValue;
        for(int i = tree; i < 10; i++)
        {
            min = Math.Min(min, heightIdx[i]);
        }

        return min - pos;
    }
}
