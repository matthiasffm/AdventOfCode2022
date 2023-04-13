import java.nio.file.{Files, Paths}
import scala.io.Source
import org.scalatest._
import flatspec._
import matchers._

class Day04Solver {

    case class Assignment(min: Int, max: Int) {
        def contains(other: Assignment) : Boolean = min <= other.min && max >= other.max
        def overlaps(other: Assignment) : Boolean = (min >= other.min && min <= other.max) ||
                                                    (other.min >= min && other.min <= max)
    }

    def parseData(pairs: Seq[String]) : Seq[(Assignment, Assignment)] =
        pairs.map {
            case s"$min1-$max1,$min2-$max2" => (Assignment(min1.toInt, max1.toInt), Assignment(min2.toInt, max2.toInt))
        }

    // Elves have been assigned the job of cleaning up sections of the camp. Every section has a unique ID number, and
    // each Elf is assigned a range of section IDs. However, as some of the Elves compare their section assignments with
    // each other, they've noticed that many of the assignments overlap.
    //
    // Puzzle == In how many assignment pairs does one range fully contain the other?
    def puzzle1(pairs: Seq[(Assignment, Assignment)]) : Int = pairs.count(p => p._1.contains(p._2) || p._2.contains(p._1))

    // Puzzle == In how many assignment pairs do the ranges overlap?
    def puzzle2(pairs: Seq[(Assignment, Assignment)]) : Int = pairs.count(p => p._1.overlaps(p._2))
}

class Day04 extends AnyFlatSpec with should.Matchers {

    val sampleData = Array(
        "2-4,6-8",
        "2-3,4-5",
        "5-7,7-9",
        "2-8,3-7",
        "6-6,4-6",
        "2-6,4-8",
    ).toSeq

    "Puzzle 1" should "count how many assignment pairs does one range fully contain the other in the sample data" in {
        val day4 = new Day04Solver
        val data = day4.parseData(sampleData)
        day4.puzzle1(data) should be (2)
    }

    "Puzzle 2" should "count in how many assignment pairs the ranges overlap in the sample data" in {
        val day4 = new Day04Solver
        val data = day4.parseData(sampleData)
        day4.puzzle2(data) should be (4)
    }

    val realData = Source.fromFile(new java.io.File(new java.io.File(".").getCanonicalPath).getParent() + "/.input/day04.data")
                         .getLines
                         .toSeq

    "Puzzle 1" should "count how many assignment pairs does one range fully contain the other in the AoC data" in {
        val day4 = new Day04Solver
        val data = day4.parseData(realData)
        day4.puzzle1(data) should be (651)
    }

    "Puzzle 2" should "count in how many assignment pairs the ranges overlap in the AoC data" in {
        val day4 = new Day04Solver
        val data = day4.parseData(realData)
        day4.puzzle2(data) should be (956)
    }
}
