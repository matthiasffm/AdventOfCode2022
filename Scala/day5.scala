import java.nio.file.{Files, Paths}
import scala.io.Source
import org.scalatest._
import flatspec._
import matchers._

class Day5Solver {

    case class Move(count: Int, from: Int, to: Int)

    def parseData(input: Seq[String]) : (Seq[List[Char]], List[Move]) = {
        val moves = input.filter(s => s.contains("move"))
                         .map {
                              case s"move $count from $from to $to" => Move(count.toInt, from.toInt - 1, to.toInt - 1)
                          }
                         .toList

        val nmbrStacks = input.map(s => s.count(c => c == '[')).max
        val stacks = (0 to nmbrStacks - 1).map(i =>
            input.filter(s => s.contains('[') && s(i * 4 + 1) != ' ')
                               .map(s => s(i * 4 + 1))
                               .toList)

        return (stacks.toSeq, moves)
    }

    // The ship has a giant cargo crane (CrateMover 9000) capable of moving crates between stacks. The Elves have a drawing
    // of the starting stacks of and crates and the moving procedure as input. In each step of the movement procedure, a
    // quantity of crates is moved from one stack to a different stack _one_ crate at a time.
    // Puzzle == The Elves just need to know which crate will end up on top of each stack at the end. So you should combine these
    //           crate letters together and give the Elves the resulting message.
    def puzzle1(stacks: Seq[List[Char]], moves: List[Move]) : String = moves match {
            case Move(1, _, _) :: tail => puzzle1(moveCrate(stacks, Move(1, moves.head.from, moves.head.to)), tail)
            case head :: tail          => puzzle1(moveCrate(stacks, head), Move(head.count - 1, head.from, head.to) :: tail)
            case Nil                   => stacks.map(s => s.head).mkString
        }

    // moves the top crate from one stack to another and returns the new stack configuration
    def moveCrate(stacks: Seq[List[Char]], move: Move) : Seq[List[Char]] = 
        stacks.zipWithIndex.map(s => s._2 match
            case move.to   => stacks(move.from).head :: stacks(move.to)
            case move.from => stacks(move.from).tail
            case _         => s._1)

    // The CrateMover 9001 is notable for many new and exciting features, especially the ability to pick up and move multiple
    // crates at once. The action of moving multiple crates from one stack to another means that those moved crates stay _in the same order_.
    // Puzzle == After the rearrangement procedure completes, what crate ends up on top of each stack? Give the Elves the resulting message.
    def puzzle2(stacks: Seq[List[Char]], moves: List[Move]) : String = moves match {
            case head :: tail          => puzzle2(moveCrates(stacks, head), tail)
            case Nil                   => stacks.map(s => s.head).mkString
        }

    // moves move.count crates from one stack to another and returns the new stack configuration
    def moveCrates(stacks: Seq[List[Char]], move: Move) : Seq[List[Char]] = 
        stacks.zipWithIndex.map(s => s._2 match
            case move.to   => stacks(move.from).take(move.count) ::: stacks(move.to)
            case move.from => stacks(move.from).drop(move.count)
            case _         => s._1)
}

class Day5 extends AnyFlatSpec with should.Matchers {

    val sampleData = Array(
        "    [D]    ",
        "[N] [C]    ",
        "[Z] [M] [P]",
        " 1   2   3 ",
        "",
        "move 1 from 2 to 1",
        "move 3 from 1 to 3",
        "move 2 from 2 to 1",
        "move 1 from 1 to 2",
    ).toSeq

    "Puzzle 1" should "contain the message from the letters on the top crates in the sample data" in {
        val day5 = new Day5Solver
        val data = day5.parseData(sampleData)
        day5.puzzle1(data._1, data._2) should be ("CMZ")
    }

    "Puzzle 2" should "contain the message from the letters on the top crates in the sample data" in {
        val day5 = new Day5Solver
        val data = day5.parseData(sampleData)
        day5.puzzle2(data._1, data._2) should be ("MCD")
    }

    val realData = Source.fromFile(new java.io.File(new java.io.File(".").getCanonicalPath).getParent() + "/.input/day5.data")
                         .getLines
                         .toSeq

    "Puzzle 1" should "contain the message from the letters on the top crates in the AoC data" in {
        val day5 = new Day5Solver
        val data = day5.parseData(realData)
        day5.puzzle1(data._1, data._2) should be ("PSNRGBTFT")
    }

    "Puzzle 2" should "count in how many assignment pairs the ranges overlap in the AoC data" in {
        val day5 = new Day5Solver
        val data = day5.parseData(realData)
        day5.puzzle2(data._1, data._2) should be ("BNTZFPMMW")
    }
}
