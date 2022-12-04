import java.nio.file.{Files, Paths}
import scala.io.Source
import org.scalatest._
import flatspec._
import matchers._

class Day4Solver {

    class Assignment(min: Int, max: Int) {
        val _min: Int = min
        val _max: Int = max

        def Contains(right: Assignment) : Boolean = _min <= right._min && _max >= right._max
        def Overlaps(right: Assignment) : Boolean = (_min >= right._min && _min <= right._max) ||
                                                    (right._min >= _min && right._min <= _max)
    }

    def ParseData(pairs: Seq[String]) : Seq[(Assignment, Assignment)] = pairs.map(p => p.split(","))
                                                                             .map(pp => (ParsePair(pp(0)), ParsePair(pp(1))))
    def ParsePair(s: String) : Assignment = new Assignment(s.split("-")(0).toInt, s.split("-")(1).toInt)

    // Elves have been assigned the job of cleaning up sections of the camp. Every section has a unique ID number, and
    // each Elf is assigned a range of section IDs. However, as some of the Elves compare their section assignments with
    // each other, they've noticed that many of the assignments overlap.
    // Puzzle == In how many assignment pairs does one range fully contain the other?
    def Puzzle1(pairs: Seq[(Assignment, Assignment)]) : Int = pairs.count(p => p._1.Contains(p._2) || p._2.Contains(p._1))

    // Puzzle == In how many assignment pairs do the ranges overlap?
    def Puzzle2(pairs: Seq[(Assignment, Assignment)]) : Int = pairs.count(p => p._1.Overlaps(p._2))
}

class Day4 extends AnyFlatSpec with should.Matchers {

    val sampleData = Array(
        "2-4,6-8",
        "2-3,4-5",
        "5-7,7-9",
        "2-8,3-7",
        "6-6,4-6",
        "2-6,4-8",
    ).toSeq

    val realData = Source.fromFile(new java.io.File(new java.io.File(".").getCanonicalPath).getParent() + "/day4.data")
                         .getLines
                         .toSeq

    "Puzzle 1" should "count how many assignment pairs does one range fully contain the other in the sample data" in {
        val day4 = new Day4Solver
        val data = day4.ParseData(sampleData)
        day4.Puzzle1(data) should be (2)
    }

    "Puzzle 2" should "count in how many assignment pairs the ranges overlap in the sample data" in {
        val day4 = new Day4Solver
        val data = day4.ParseData(sampleData)
        day4.Puzzle2(data) should be (4)
    }

    "Puzzle 1" should "count how many assignment pairs does one range fully contain the other in the AoC data" in {
        val day4 = new Day4Solver
        val data = day4.ParseData(realData)
        day4.Puzzle1(data) should be (651)
    }

    "Puzzle 2" should "count in how many assignment pairs the ranges overlap in the AoC data" in {
        val day4 = new Day4Solver
        val data = day4.ParseData(realData)
        day4.Puzzle2(data) should be (956)
    }
}
