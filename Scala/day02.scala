import java.nio.file.{Files, Paths}
import scala.io.Source
import org.scalatest._
import flatspec._
import matchers._

class Day02Solver {

    def parse(data: Seq[String]) : Seq[(Int, Int)] = data.map(s => (s.charAt(0) - 'A', s.charAt(2) - 'X'))

    // see day2.cs for explanation
    def intMod(x: Int, y: Int) : Int = x % y + (if (x < 0) y else 0)
    def result(player1: Int, player2: Int) : Int = intMod((player2 - player1 + 1) * 3, 9)

    // see day2.cs for explanation
    def inverseResult(player1: Int, res: Int) : Int = (player1 + res + 2) % 3

    def scoring(shape: Int, outcome: Int) : Int = (shape + 1) + outcome

    // one Elf gives you an encrypted strategy guide: "The first column is what your opponent is going to play: A for Rock, B for Paper, and C for
    // Scissors." The second column, you reason, must be what you should play in response.
    // The winner of the whole tournament is the player with the highest score. Your total score is the sum of your scores for each round. The score
    // for a single round is the score for the shape you selected (1 for Rock, 2 for Paper, and 3 for Scissors) plus the score for the outcome of the
    // round (0 if you lost, 3 if the round was a draw, and 6 if you won).
    //
    // Puzzle == What would your total score be if everything goes exactly according to your strategy guide?
    def puzzle1(matches: Seq[(Int, Int)]) : Int =  matches.map(m => scoring(m(1), result(m(0), m(1)))).sum

    // The Elf finishes helping with the tent and sneaks back over to you. "Anyway, the second column says how the round needs to end: X means you need
    // to lose, Y means you need to end the round in a draw, and Z means you need to win. Good luck!"
    // The total score is still calculated in the same way, but now you need to figure out what shape to choose so the round ends as indicated.
    //
    // Puzzle == Following the Elf's now complete instructions, what would your total score be if everything goes exactly according to your strategy guide?
    def puzzle2(matches: Seq[(Int, Int)]) : Int =  matches.map(m => scoring(inverseResult(m(0), m(1)), m(1) * 3)).sum
}

class Day02 extends AnyFlatSpec with should.Matchers {

    val sampleData = IndexedSeq("A Y", "B X", "C Z")

    "Puzzle 1" should "sum the scores of all matches in the sample data with the incomplete strategy" in {
        val day2 = new Day02Solver
        val matches = day2.parse(sampleData)
        day2.puzzle1(matches) should be (15)
    }

    "Puzzle 2" should "sum the scores of all matches in the sample data with the complete strategy" in {
        val day2 = new Day02Solver
        val matches = day2.parse(sampleData)
        day2.puzzle2(matches) should be (12)
    }

    val realData = Source.fromFile(new java.io.File(new java.io.File(".").getCanonicalPath).getParent() + "/.input/day02.data")
                         .getLines
                         .toSeq

    "Puzzle 1" should "sum the scores of all matches in the AoC data with the incomplete strategy" in {
        val day2 = new Day02Solver
        val matches = day2.parse(realData)
        day2.puzzle1(matches) should be (11386)
    }

    "Puzzle 2" should "sum the scores of all matches in the AoC data with the incomplete strategy" in {
        val day2 = new Day02Solver
        val matches = day2.parse(realData)
        day2.puzzle2(matches) should be (13600)
    }
}
