import java.nio.file.{Files, Paths}
import scala.io.Source
import org.scalatest._
import flatspec._
import matchers._

class Day06Solver {

    // To fix the Elves communication system, you need to add a subroutine to the device that detects a start-of-packet marker in the datastream. In
    // the protocol being used by the Elves, the start of a packet is indicated by a sequence of four characters that are all different.
    //
    // Puzzle1 == How many characters need to be processed before the first start-of-packet marker is detected?
    //
    // A start-of-message marker is just like a start-of-packet marker, except it consists of 14 distinct characters rather than 4.
    //
    // Puzzle2 == How many characters need to be processed before the first start-of-message marker is detected?
    def puzzle(msg: String, size: Int) : Int = 1 + (size - 1 to msg.length).find(i => msg.slice(i - size + 1, i + 1).toSet.size == size)
                                                                           .getOrElse(0)
}

class Day06 extends AnyFlatSpec with should.Matchers {

    "Puzzle 1" should "find how many characters need to be processed before the first start-of-packet marker is detected in the sample data" in {
        val day6 = new Day06Solver
        day6.puzzle("mjqjpqmgbljsphdztnvjfqwrcgsmlb", 4) should be (7)
        day6.puzzle("bvwbjplbgvbhsrlpgdmjqwftvncz", 4) should be (5)
        day6.puzzle("nppdvjthqldpwncqszvftbrmjlhg", 4) should be (6)
        day6.puzzle("nznrnfrfntjfmvfwmzdfjlvtqnbhcprsg", 4) should be (10)
        day6.puzzle("zcfzfwzzqfrljwzlrfnpqdbhtmscgvjw", 4) should be (11)
    }

    "Puzzle 2" should "find how many characters need to be processed before the first start-of-message marker is detected in the sample data" in {
        val day6 = new Day06Solver
        day6.puzzle("mjqjpqmgbljsphdztnvjfqwrcgsmlb", 14) should be (19)
        day6.puzzle("bvwbjplbgvbhsrlpgdmjqwftvncz", 14) should be (23)
        day6.puzzle("nppdvjthqldpwncqszvftbrmjlhg", 14) should be (23)
        day6.puzzle("nznrnfrfntjfmvfwmzdfjlvtqnbhcprsg", 14) should be (29)
        day6.puzzle("zcfzfwzzqfrljwzlrfnpqdbhtmscgvjw", 14) should be (26)
    }

    val realData = Source.fromFile(new java.io.File(new java.io.File(".").getCanonicalPath).getParent() + "/.input/day06.data")
                         .getLines
                         .mkString

    "Puzzle 1" should "find how many characters need to be processed before the first start-of-packet marker is detected in the AoC data" in {
        val day6 = new Day06Solver
        day6.puzzle(realData, 4) should be (1640)
    }

    "Puzzle 2" should "find how many characters need to be processed before the first start-of-message marker is detected in the AoC data" in {
        val day6 = new Day06Solver
        day6.puzzle(realData, 14) should be (3613)
    }
}
