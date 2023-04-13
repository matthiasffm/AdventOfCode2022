import java.nio.file.{Files, Paths}
import scala.io.Source
import org.scalatest._
import flatspec._
import matchers._

class Day10Solver {

    // The communication device's video system seems to be some kind of cathode-ray tube screen and simple CPU that are both driven by a precise clock circuit. The clock circuit
    // ticks at a constant rate; each tick is called a cycle. The CPU has a single register, X, which starts with the value 1. It supports only two instructions:
    // 'addx V' takes two cycles to complete. After two cycles, the X register is increased by the value V. (V can be negative.)
    // 'noop' takes one cycle to complete. It has no other effect.
    //
    // Puzzle == Consider the signal strength (the cycle number multiplied by the value of the X register) _during_ the 20th cycle and every 40 cycles after that.
    //           What is the sum of these six signal strengths?
    def puzzle1(instructions: List[String]) : Int = sumSignalStrengths(0, 1, instructions)

    def sumSignalStrengths(clock: Int, x: Int, instructions: List[String]) : Int = instructions match {
        case "noop" :: tail => signalStrength(clock + 1, x) +
                               sumSignalStrengths(clock + 1, x, tail)

        case head :: tail   => signalStrength(clock + 1, x) +
                               signalStrength(clock + 2, x) +
                               sumSignalStrengths(clock + 2, addX(x, head), tail)

        case Nil            => 0
    }

    def signalStrength(clock: Int, x: Int) : Int = if((clock + 20) % 40 == 0) then x * clock else 0

    // It seems like the X register controls the horizontal position of a sprite on the CRT. Specifically, the sprite is 3 pixels wide, and the X register sets the
    // horizontal position of the middle of that sprite. The CRT is 40 wide and 6 high. This CRT screen draws the top row of pixels left-to-right, then the row below
    // that, and so on. Like the CPU, the CRT is tied closely to the clock circuit: the CRT draws a single pixel during each cycle.
    // So, by carefully timing the CPU instructions and the CRT drawing operations, you should be able to determine whether the sprite is visible the instant each
    // pixel is drawn. If the sprite is positioned such that one of its three pixels is the pixel currently being drawn, the screen produces a lit pixel (#);
    // otherwise, the screen leaves the pixel dark (.).
    //
    // Puzzle == Render the image given by your program. What eight capital letters appear on your CRT?
    def puzzle2(instructions: List[String]) : String = raceTheBeam(0, 1, instructions).mkString

    def raceTheBeam(clock: Int, spritePos: Int, instructions: List[String]) : List[Char] = instructions match {
        case "noop" :: tail => moveBeam(clock + 1, spritePos) ::: raceTheBeam(clock + 1, spritePos, tail)

        case head :: tail   => moveBeam(clock + 1, spritePos) :::
                               moveBeam(clock + 2, spritePos) :::
                               raceTheBeam(clock + 2, addX(spritePos, head), tail)

        case Nil            => Nil
    }

    // moves the beam to the next location and adds a horizontal blank at the end of the line
    def moveBeam(clock: Int, spritePos: Int) : List[Char] = nextPixel((clock - 1) % 40, spritePos) +: hBlanc(clock)

    // draws a # if the beam at its current screen pos hits the sprite
    def nextPixel(screenX: Int, spritePos: Int) : Char = if(screenX >= (spritePos - 1) && screenX <= (spritePos + 1)) '#' else '.'

    def hBlanc(clock: Int) : List[Char] = if(clock % 40 == 0 && clock < 240) List('\n') else Nil;

    def addX(x: Int, addxInstruction: String) = x + addxInstruction.drop(5).toInt
}

class Day10 extends AnyFlatSpec with should.Matchers {

    var sampleData = List("addx 15", "addx -11", "addx 6", "addx -3", "addx 5", "addx -1", "addx -8", "addx 13", "addx 4",
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
                          "noop", "noop", "noop")

    "Puzzle 1" should " in the sample data" in {
        val day10 = new Day10Solver
        day10.puzzle1(sampleData) should be (13140)
    }

    "Puzzle 2" should " in the sample data" in {
        val day10 = new Day10Solver
        day10.puzzle2(sampleData) should be (
            "##..##..##..##..##..##..##..##..##..##..\n" +
            "###...###...###...###...###...###...###.\n" +
            "####....####....####....####....####....\n" +
            "#####.....#####.....#####.....#####.....\n" +
            "######......######......######......####\n" +
            "#######.......#######.......#######.....")
    }

    val realData = Source.fromFile(new java.io.File(new java.io.File(".").getCanonicalPath).getParent() + "/.input/day10.data")
                         .getLines
                         .toList

    "Puzzle 1" should " in the AoC data" in {
        val day10 = new Day10Solver
        day10.puzzle1(realData) should be (14860)
    }

    "Puzzle 2" should " in the AoC data" in {
        val day10 = new Day10Solver
        day10.puzzle2(realData) should be (
            "###...##..####.####.#..#.#..#.###..#..#.\n" +
            "#..#.#..#....#.#....#..#.#..#.#..#.#.#..\n" +
            "#..#.#......#..###..####.#..#.#..#.##...\n" +
            "###..#.##..#...#....#..#.#..#.###..#.#..\n" +
            "#.#..#..#.#....#....#..#.#..#.#.#..#.#..\n" +
            "#..#..###.####.####.#..#..##..#..#.#..#.") // RGZEHURK
    }
}
