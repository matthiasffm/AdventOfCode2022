import java.nio.file.{Files, Paths}
import scala.io.Source
import org.scalatest._
import flatspec._
import matchers._

class Day09Solver {

    case class Vec2(x: Int, y: Int) {
        def plus(d: Vec2)  : Vec2 = new Vec2(this.x + d.x, this.y + d.y)
        def minus(d: Vec2) : Vec2 = new Vec2(this.x - d.x, this.y - d.y)
    }

    case class Move(direction: Vec2, repeat: Int)

    def parseMove(line: String): Move = {
        val dist = line.substring(2).toInt

        return line(0) match {
            case 'L' => Move(Vec2(-1,  0), dist)
            case 'R' => Move(Vec2( 1,  0), dist)
            case 'U' => Move(Vec2( 0, -1), dist)
            case 'D' => Move(Vec2( 0,  1), dist)
        }
    }

    def parseData(input: List[String]) : Seq[Move] = input.map(parseMove(_))

    // Consider a rope with a knot at each end; these knots mark the head and the tail of the rope. If the head moves far enough away from the
    // tail, the tail is pulled toward the head. All elements of the rope from head to tail must always be touching at every single move (diagonally adjacent
    // and even overlapping both count as touching). After simulating the ropes moves, you can count up all of the positions the tail visited at least once.
    //
    // Puzzle == Simulate the series of motions. How many positions does the tail of the rope visit at least once?
    def puzzle(moves: Seq[Move], ropeLength: Int) : Int = {
        var rope = Array.fill[Vec2](ropeLength)(Vec2(0, 0)).toList

        var tailVisited = Set[Vec2]()
        tailVisited += rope.last

        for(move <- moves) {
            for(m <- 0 to move.repeat - 1) {
                rope = moveRope(rope, move.direction)
                tailVisited += rope.last
            }
        }

        return tailVisited.size
    }

    // add diff to head and then reposition all other knots in the rope recursively
    def moveRope(rope: List[Vec2], diff: Vec2) : List[Vec2] = rope match
        case Nil          => Nil
        case head :: Nil  => head.plus(diff) :: Nil
        case head :: tail => head.plus(diff) :: moveRope(tail, diffForNextKnot(head.plus(diff), tail.head))

    // calculates the move for the next knot if it is not close enough (d > 1)
    def diffForNextKnot(prevKnot: Vec2, nextKnot: Vec2) : Vec2 = {
        val diff = prevKnot.minus(nextKnot)

        return if(diff.x.abs > 1 || diff.y.abs > 1) {
            Vec2(diff.x.abs.min(1) * diff.x.sign, diff.y.abs.min(1) * diff.y.sign)
        }
        else {
            Vec2(0, 0)
        }
    }
}

class Day09 extends AnyFlatSpec with should.Matchers {

    var sampleData1 = List("R 4", "U 4", "L 3", "D 1", "R 4", "D 1", "L 5", "R 2")
    var sampleData2 = List("R 5", "U 8", "L 8", "D 3", "R 17", "D 10", "L 25", "U 20")

    "Puzzle 1" should "count how many positions the tail of a 2-element rope visits in the sample data" in {
        val day9 = new Day09Solver
        var moves1 = day9.parseData(sampleData1)
        day9.puzzle(moves1, 2) should be (13)
    }

    "Puzzle 2" should "count how many positions the tail of a 10-element rope visits in the sample data" in {
        val day9 = new Day09Solver
        var moves1 = day9.parseData(sampleData1)
        day9.puzzle(moves1, 10) should be (1)
        var moves2 = day9.parseData(sampleData2)
        day9.puzzle(moves2, 10) should be (36)
    }

    val realData = Source.fromFile(new java.io.File(new java.io.File(".").getCanonicalPath).getParent() + "/.input/day09.data")
                         .getLines
                         .toList

    "Puzzle 1" should "count how many positions the tail of a 2-element rope visits in the AoC data" in {
        val day9 = new Day09Solver
        var moves = day9.parseData(realData)
        day9.puzzle(moves, 2) should be (6745)
    }

    "Puzzle 2" should "count how many positions the tail of a 10-element rope visits in the AoC data" in {
        val day9 = new Day09Solver
        var moves = day9.parseData(realData)
        day9.puzzle(moves, 10) should be (2793)
    }
}
