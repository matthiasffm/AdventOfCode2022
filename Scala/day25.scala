import java.nio.file.{Files, Paths}
import scala.io.Source
import org.scalatest._
import flatspec._
import matchers._

class Day25Solver {
    // To heat the fuel of the hot air balloon, Bob needs to know the total amount of fuel that will be processed ahead of time so it can correctly calibrate
    // heat output and flow rate. This amount is simply the sum of the fuel requirements of all of the hot air balloons, and those fuel requirements are even
    // listed clearly on the side of each hot air balloon's burner in Special Numeral-Analogue Fuel Units - SNAFU for short.
    // Instead of using digits zero through four, the SNAFU system uses digits 2, 1, 0, minus (written -), and double-minus (written =). Minus is
    // worth -1, and double-minus is worth -2.
    // 
    // Puzzle == The Elves are starting to get cold. What SNAFU number do you supply to Bob's console for the sum of the fuel requirements of all balloons?
    def puzzle(quinaryNumbers: List[String]) : String =
        toQuinaryNumber(quinaryNumbers.map(q => toDecimalNumber(q.toList)).sum).mkString

    def toDecimalNumber(quinaryNumber: List[Char]) : Long = quinaryNumber match {
        case head :: tail => toDecimalDigit(head) * pow(5, tail.length) + toDecimalNumber(tail)
        case Nil          => 0
    }

    def toDecimalDigit(quinaryDigit: Char) : Long = quinaryDigit match {
        case '=' => -2
        case '-' => -1
        case _   => quinaryDigit - '0'
    }

    def pow(m: Long, e: Int) : Long = e match {
        case 0            => 1
        case 1            => m
        case _            => (if(e % 2 == 0) 1 else m) * pow(m * m, e / 2)
    }

    val quinaryDigits = List('=', '-', '0', '1', '2')

    def toQuinaryNumber(decimalNumber: Long) : List[Char] = decimalNumber match {
        case 0 => Nil
        case _ => toQuinaryNumber((decimalNumber + 2) / 5) :+ quinaryDigits(((decimalNumber + 2) % 5).toInt)
    }
}

class Day25 extends AnyFlatSpec with should.Matchers {

    var sampleData = List("1=-0-2", "12111", "2=0=", "21", "2=01", "111", "20012", "112", "1=-1=", "1-12", "12", "1=", "122")

    "Puzzle" should "sum the SNAFU pressure values in the sample data" in {
        val day25 = new Day25Solver
        day25.puzzle(sampleData) should be ("2=-1=0")
    }

    val realData = Source.fromFile(new java.io.File(new java.io.File(".").getCanonicalPath).getParent() + "/.input/day25.data")
                         .getLines
                         .toList

    "Puzzle " should "sum the SNAFU pressure values in the AoC data" in {
        val day25 = new Day25Solver
        day25.puzzle(realData) should be ("2--1=0=-210-1=00=-=1")
    }
}
