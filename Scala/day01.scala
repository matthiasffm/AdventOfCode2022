import java.nio.file.{Files, Paths}
import scala.io.Source
import org.scalatest._
import flatspec._
import matchers._

class Day1Solver {

    def parse(data: Seq[String]) : Seq[Seq[Int]] = {
        return data.map(s => s match {
                                        case "" => "#"
                                        case _  => s
                                    })
                        .mkString(",")
                        .split("#").toSeq
                        .map(s => s.split(",").toSeq
                                   .filter(s => s != "")
                                   .map(nmbr => nmbr.toInt))
    }

    // One important consideration is food - in particular, the number of Calories each Elf is carrying (your puzzle input).
    // In case the Elves get hungry and need extra snacks, they need to know which Elf to ask: they'd like to know how many
    // Calories are being carried by the Elf carrying the most Calories.
    // Puzzle == find the Elf carrying the most Calories. How many total Calories is that Elf carrying?
    def puzzle1(elves : Seq[Seq[Int]]) : Int = elves.map(e => e.sum).max

    // By the time you calculate the answer to the Elves' question, they've already realized that the Elf carrying the most Calories
    // of food might eventually run out of snacks. To avoid this unacceptable situation, the Elves would instead like to know the
    // total Calories carried by the top three Elves carrying the most Calories.
    // Puzzle == find the top three Elves carrying the most Calories. How many Calories are those Elves carrying in total?
    def puzzle2(elves : Seq[Seq[Int]]) : Int = elves.map(e => e.sum).sorted.takeRight(3).sum
}

class Day1 extends AnyFlatSpec with should.Matchers {

    val sampleData = Array(
                       "1000", "2000", "3000",
                       "",
                       "4000",
                       "",
                       "5000", "6000",
                       "",
                       "7000", "8000", "9000",
                       "",
                       "10000",
                       ""
    ).toSeq

    "Puzzle 1" should "find the elf carrying the most calories in the sample data" in {
        val day1 = new Day1Solver
        val elves = day1.parse(sampleData);
        day1.puzzle1(elves) should be (24000)
    }

    "Puzzle 2" should "find the top three elfs carrying the most calories in the sample data" in {
        val day1 = new Day1Solver
        val elves = day1.parse(sampleData);
        day1.puzzle2(elves) should be (45000)
    }

    val realData = Source.fromFile(new java.io.File(new java.io.File(".").getCanonicalPath).getParent() + "/.input/day01.data")
                         .getLines
                         .toSeq

    "Puzzle 1" should "find the elf carrying the most calories in the AoC data" in {
        val day1 = new Day1Solver
        val elves = day1.parse(realData);
        day1.puzzle1(elves) should be (64929)
    }

    "Puzzle 2" should "find the top three elfs carrying the most calories in the AoC data" in {
        val day1 = new Day1Solver
        val elves = day1.parse(realData);
        day1.puzzle2(elves) should be (193697)
    }
}
