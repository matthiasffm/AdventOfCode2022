import java.nio.file.{Files, Paths}
import scala.io.Source
import org.scalatest._
import flatspec._
import matchers._

def Desc[T : Ordering] = implicitly[Ordering[T]].reverse

class Day1Solver {

    def Parse(data: Seq[String]) : Seq[Seq[Int]] = {
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

    def Puzzle1(elves : Seq[Seq[Int]]) : Int = elves.map(e => e.sum).max

    def Puzzle2(elves : Seq[Seq[Int]]) : Int = elves.map(e => e.sum).sorted(Desc).take(3).sum
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

    val realData = Source.fromFile(new java.io.File(new java.io.File(".").getCanonicalPath).getParent() + "/day1.data")
                         .getLines
                         .toSeq

    "Puzzle 1" should "find the elf carrying the most calories in the sample data" in {
        val day1 = new Day1Solver
        var elves = day1.Parse(sampleData);
        day1.Puzzle1(elves) should be (24000)
    }

    "Puzzle 2" should "find the top three elfs carrying the most calories in the sample data" in {
        val day1 = new Day1Solver
        var elves = day1.Parse(sampleData);
        day1.Puzzle2(elves) should be (45000)
    }

    "Puzzle 1" should "find the elf carrying the most calories in the AoC data" in {
        val day1 = new Day1Solver
        var elves = day1.Parse(realData);
        day1.Puzzle1(elves) should be (64929)
    }

    "Puzzle 2" should "find the top three elfs carrying the most calories in the AoC data" in {
        val day1 = new Day1Solver
        var elves = day1.Parse(realData);
        day1.Puzzle2(elves) should be (193697)
    }
}
