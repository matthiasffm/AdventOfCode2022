namespace AdventOfCode2022;

using NUnit.Framework;
using FluentAssertions;

using System.Text;

[TestFixture]
public class Day10
{
    [Test]
    public void TestSamples()
    {
        var instructions = new [] {
            "addx 15", "addx -11", "addx 6", "addx -3", "addx 5", "addx -1", "addx -8", "addx 13", "addx 4",
            "noop", "addx -1", "addx 5", "addx -1", "addx 5", "addx -1", "addx 5", "addx -1", "addx 5", "addx -1",
            "addx -35", "addx 1", "addx 24", "addx -19", "addx 1", "addx 16", "addx -11", "noop", "noop",
            "addx 21", "addx -15", "noop", "noop", "addx -3", "addx 9", "addx 1", "addx -3", "addx 8", "addx 1",
            "addx 5", "noop", "noop", "noop", "noop", "noop", "addx -36", "noop", "addx 1", "addx 7", "noop",
            "noop", "noop", "addx 2", "addx 6", "noop", "noop", "noop", "noop", "noop", "addx 1", "noop", "noop",
            "addx 7", "addx 1", "noop", "addx -13", "addx 13", "addx 7", "noop", "addx 1", "addx -33", "noop",
            "noop", "noop", "addx 2", "noop", "noop", "noop", "addx 8", "noop", "addx -1", "addx 2", "addx 1",
            "noop", "addx 17", "addx -9", "addx 1", "addx 1", "addx -3", "addx 11", "noop", "noop", "addx 1",
            "noop", "addx 1", "noop", "noop", "addx -13", "addx -19", "addx 1", "addx 3", "addx 26", "addx -30",
            "addx 12", "addx -1", "addx 3", "addx 1", "noop", "noop", "noop", "addx -9", "addx 18", "addx 1",
            "addx 2", "noop", "noop", "addx 9", "noop", "noop", "noop", "addx -1", "addx 2", "addx -37", "addx 1",
            "addx 3", "noop", "addx 15", "addx -21", "addx 22", "addx -6", "addx 1", "noop", "addx 2", "addx 1",
            "noop", "addx -10", "noop", "noop", "addx 20", "addx 1", "addx 2", "addx 2", "addx -6", "addx -11",
            "noop", "noop", "noop",
        };

        Puzzle1(instructions).Should().Be(13140);
        Puzzle2(instructions).Should().Be("""
            ##..##..##..##..##..##..##..##..##..##..
            ###...###...###...###...###...###...###.
            ####....####....####....####....####....
            #####.....#####.....#####.....#####.....
            ######......######......######......####
            #######.......#######.......#######.....
            """);
    }

    [Test]
    public void TestAocInput()
    {
        var instructions = FileUtils.ReadAllLines(this);

        Puzzle1(instructions).Should().Be(14860);
        Puzzle2(instructions).Should().Be("""
            ###...##..####.####.#..#.#..#.###..#..#.
            #..#.#..#....#.#....#..#.#..#.#..#.#.#..
            #..#.#......#..###..####.#..#.#..#.##...
            ###..#.##..#...#....#..#.#..#.###..#.#..
            #.#..#..#.#....#....#..#.#..#.#.#..#.#..
            #..#..###.####.####.#..#..##..#..#.#..#.
            """); // RGZEHURK
    }

    // The communication device's video system seems to be some kind of cathode-ray tube screen and simple CPU that are both driven by a precise clock circuit. The clock circuit
    // ticks at a constant rate; each tick is called a cycle. The CPU has a single register, X, which starts with the value 1. It supports only two instructions:
    // 'addx V' takes two cycles to complete. After two cycles, the X register is increased by the value V. (V can be negative.)
    // 'noop' takes one cycle to complete. It has no other effect.
    // Puzzle == Consider the signal strength (the cycle number multiplied by the value of the X register) _during_ the 20th cycle and every 40 cycles after that.
    //           What is the sum of these six signal strengths?
    private static int Puzzle1(string[] instructions)
    {
        var x     = 1;
        var clock = 0;
        var sum   = 0;

        foreach(var instruction in instructions)
        {
            clock++;
            sum += SignalStrength(clock, x);

            if(instruction.StartsWith("addx"))
            {
                clock++;
                sum += SignalStrength(clock, x);

                x += int.Parse(instruction.AsSpan()[5..]);
            }
        }

        return sum;
    }

    private static int SignalStrength(int clock, int x) => (clock + 20) % 40 == 0 ? x * clock : 0;

    // It seems like the X register controls the horizontal position of a sprite on the CRT. Specifically, the sprite is 3 pixels wide, and the X register sets the
    // horizontal position of the middle of that sprite. The CRT is 40 wide and 6 high. This CRT screen draws the top row of pixels left-to-right, then the row below
    // that, and so on. Like the CPU, the CRT is tied closely to the clock circuit: the CRT draws a single pixel during each cycle.
    // So, by carefully timing the CPU instructions and the CRT drawing operations, you should be able to determine whether the sprite is visible the instant each
    // pixel is drawn. If the sprite is positioned such that one of its three pixels is the pixel currently being drawn, the screen produces a lit pixel (#);
    // otherwise, the screen leaves the pixel dark (.).
    // Puzzle == Render the image given by your program. What eight capital letters appear on your CRT?
    private static string Puzzle2(string[] instructions)
    {
        var spritePos = 1;
        var clock     = 0;

        var screen = new StringBuilder(40 * 6);

        foreach(var instruction in instructions)
        {
            clock++;
            screen = RaceTheBeam(clock, spritePos, screen);

            if(instruction.StartsWith("addx"))
            {
                clock++;
                screen = RaceTheBeam(clock, spritePos, screen);
                spritePos += int.Parse(instruction.AsSpan()[5..]);
            }
        }

        return screen.ToString();
    }

    private static StringBuilder RaceTheBeam(int pc, int spritePos, StringBuilder screen) =>
        screen.Append(NextPixel((pc - 1) % 40, spritePos))  // screen index is 1 based
              .Append(HBlanc(pc));

    private static char NextPixel(int screenX, int spritePos) =>
        screenX >= (spritePos - 1) && screenX <= (spritePos + 1) ? '#' : '.';

    private static string HBlanc(int pc) => (pc % 40 == 0 && pc < 240) ? Environment.NewLine : string.Empty;
}
