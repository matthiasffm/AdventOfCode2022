namespace AdventOfCode2022;

using NUnit.Framework;
using FluentAssertions;

using matthiasffm.Common.Collections;

[TestFixture]
public class Day05
{
    [Test]
    public void TestSamples()
    {
        var data = new [] {
            "    [D]    ",
            "[N] [C]    ",
            "[Z] [M] [P]",
            " 1   2   3 ",
            "",
            "move 1 from 2 to 1",
            "move 3 from 1 to 3",
            "move 2 from 2 to 1",
            "move 1 from 1 to 2",
        };

        var (stacks, moves) = ParseData(data);
        Puzzle1(stacks, moves).Should().Be("CMZ");

        var (stacks2, moves2) = ParseData(data);
        Puzzle2(stacks2, moves2).Should().Be("MCD");
    }

    [Test]
    public void TestAocInput()
    {
        var data = FileUtils.ReadAllLines(this);

        var (stacks, moves) = ParseData(data);
        Puzzle1(stacks, moves).Should().Be("PSNRGBTFT");
        var (stacks2, moves2) = ParseData(data);
        Puzzle2(stacks2, moves2).Should().Be("BNTZFPMMW");
    }

    private record Move(int Count, int From, int To);

    private static (Stack<char>[], IEnumerable<Move>) ParseData(string[] data)
    {
        int stackHeight = data.Count(s => s.Contains('['));
        int nmbrStacks  = data[stackHeight - 1].Count(c => c == '[');

        var stacks = 0.To(nmbrStacks - 1)
                      .Select(i => data.Where(l => l.Contains('[') && l[i * 4 + 1] != ' ')
                                       .Select(l => l[i * 4 + 1])
                                       .Reverse())
                      .Select(s => new Stack<char>(s))
                      .ToArray();

        var moves = data.Skip(stackHeight + 2)
                        .Select(line => line.Split(new string[] { "move", "from", "to" }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                                            .Select(i => int.Parse(i))
                                            .ToArray())
                        .Select(m => new Move(m[0], m[1], m[2]))
                        .ToArray();

        return (stacks, moves);
    }

    // The ship has a giant cargo crane (CrateMover 9000) capable of moving crates between stacks. The Elves have a drawing
    // of the starting stacks of and crates and the moving procedure as input. In each step of the movement procedure, a
    // quantity of crates is moved from one stack to a different stack _one_ crate at a time.
    //
    // Puzzle == The Elves just need to know which crate will end up on top of each stack at the end. So you should combine these
    //           crate letters together and give the Elves the resulting message.
    private static string Puzzle1(Stack<char>[] stacks, IEnumerable<Move> moves)
    {
        moves.Do(move => 1.To(move.Count)
                          .Do(m => stacks[move.To - 1].Push(stacks[move.From - 1].Pop())));

        return TopCrates(stacks);
    }

    // The CrateMover 9001 is notable for many new and exciting features, especially the ability to pick up and move multiple
    // crates at once. The action of moving multiple crates from one stack to another means that those moved crates stay _in the same order_.
    //
    // Puzzle == After the rearrangement procedure completes, what crate ends up on top of each stack? Give the Elves the resulting message.
    private static string Puzzle2(Stack<char>[] stacks, IEnumerable<Move> moves)
    {
        moves.Do(move => 1.To(move.Count)
                          .Select(i => stacks[move.From - 1].Pop())
                          .Reverse()
                          .Do(c => stacks[move.To - 1].Push(c)));

        return TopCrates(stacks);
    }

    private static string TopCrates(Stack<char>[] stacks) => string.Concat(stacks.Select(s => s.Peek()));
}
