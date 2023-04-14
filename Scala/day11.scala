import java.nio.file.{Files, Paths}
import scala.io.Source
import org.scalatest._
import flatspec._
import matchers._

class Day11Solver {

    // this trait does the recalculation of the worry thing of the monkeys
    trait WorryOp:
        def calc(old: Long): Long

    case class AddWorryOp(toAdd: Long) extends WorryOp:
        def calc(oldWorryLevel: Long) = toAdd + oldWorryLevel

    case class MultiplyWorryOp(factor: Long) extends WorryOp:
        def calc(oldWorryLevel: Long) = factor * oldWorryLevel

    class SquareWorryOp extends WorryOp:
        def calc(oldWorryLevel: Long) = oldWorryLevel * oldWorryLevel

    case class Test(divisibleBy: Long, throwToMonkeyIfTrue: Int, throwToMonkeyIfFalse: Int):
        def throwToNext(worryLevel: Long) : Int = if(worryLevel % divisibleBy == 0) throwToMonkeyIfTrue else throwToMonkeyIfFalse

    class Monkey(val id: Int, var items: List[Long], val op: WorryOp, val test: Test)

    def parseData(data: String) : Seq[Monkey] = {
        var monkeys = Seq[Monkey]()

        for(monkeySpec <- data.split("\n\n"))
        {
            val monkeyLines = monkeySpec.split('\n')

            val id    = monkeyLines(0).drop(7).take(1).toInt // only single digit ids
            val items = monkeyLines(1).drop(18).trim.split(", ").map(_.toLong).toList

            val op = monkeyLines(2).drop(23).trim match {
                case "* old"      => SquareWorryOp()
                case s"* $factor" => MultiplyWorryOp(factor.toInt)
                case _            => AddWorryOp(monkeyLines(2).drop(25).trim.toInt)
            }

            val test = Test(monkeyLines(3).drop(21).trim.toInt,
                            monkeyLines(4).drop(29).trim.toInt,
                            monkeyLines(5).drop(30).trim.toInt)

            monkeys = monkeys :+ Monkey(id, items, op, test)
        }

        return monkeys
    }

    // Monkeys are playing Keep Away with your missing things! In this play the monkeys operate based on how worried you are about each item.
    // Each monkey has several attributes:
    // - 'Starting items' lists your worry level for each item the monkey is currently holding in the order they will be inspected.
    // - 'Operation' shows how your worry level changes as that monkey inspects an item.
    // - 'Test' shows how the monkey uses your worry level to decide where to throw an item next.
    // After each monkey inspects an item but before it tests your worry level, your relief that the monkey's inspection didn't damage the item causes
    // your worry level to be divided by three and rounded down to the nearest integer.
    // The monkeys take turns inspecting and throwing items. On a single monkey's turn, it inspects and throws all of the items it is holding one at a
    // time and in the order listed. Monkey 0 goes first, then monkey 1, and so on until each monkey has had one turn. The process of each monkey taking
    // a single turn is called a round.
    // Chasing all of the monkeys at once is impossible; focus on the two most active monkeys if you want any hope of getting your stuff back. Multiplying
    // their level is called the monkey business in this situation.
    //
    // Puzzle == What is the level of monkey business after 20 rounds of stuff-slinging simian shenanigans?
    def puzzle1(monkeys: Seq[Monkey]) : Long = monkeyBusiness(monkeys, 3, 20)

    // You're worried you might not ever get your items back. So worried, in fact, that your relief that a monkey's inspection didn't damage an item no
    // longer causes your worry level to be divided by three.
    //
    // Puzzle == What is the level of monkey business after 10000 rounds?
    def puzzle2(monkeys: Seq[Monkey]) : Long = monkeyBusiness(monkeys, 1, 10000)

    def monkeyBusiness(monkeysOriginal: Seq[Monkey], div: Long, rounds: Int) : Long = {
        val monkeys   = monkeysOriginal.toArray
        val inspected = Array.fill(monkeys.length)(0)

        // numbers in monkey calculations can get very big, limit the numbers to the product of all divisors (fits in a long for our data)
        val worryRing = monkeys.foldLeft(1L)((p, m) => p * m.test.divisibleBy)

        for(round <- 1 to rounds) {
            for(m <- 0 to monkeys.length - 1) {
                val monkey = monkeys(m)

                while(monkey.items.length > 0) {
                    inspected(m) = inspected(m) + 1

                    val item = monkey.items(0)
                    monkey.items = monkey.items.drop(1)

                    val worryLevel = (monkey.op.calc(item) / div) % worryRing

                    var throwTo = monkey.test.throwToNext(worryLevel)
                    monkeys(throwTo).items = monkeys(throwTo).items :+ worryLevel
                }
            }
        }

        val mostActiveMonkeys = inspected.sorted.reverse.take(2)
        return mostActiveMonkeys(0).toLong * mostActiveMonkeys(1).toLong
    }
}

class Day11 extends AnyFlatSpec with should.Matchers {

    var sampleData =
         """Monkey 7:
           |  Starting items: 79, 98
           |  Operation: new = old * 19
           |  Test: divisible by 23
           |    If true: throw to monkey 2
           |    If false: throw to monkey 3""".stripMargin + "\n\n" + 
         """Monkey 1:
           |  Starting items: 54, 65, 75, 74
           |  Operation: new = old + 6
           |  Test: divisible by 19
           |    If true: throw to monkey 2
           |    If false: throw to monkey 0""".stripMargin + "\n\n" + 
         """Monkey 2:
           |  Starting items: 79, 60, 97
           |  Operation: new = old * old
           |  Test: divisible by 13
           |    If true: throw to monkey 1
           |    If false: throw to monkey 3""".stripMargin + "\n\n" + 
         """Monkey 3:
           |  Starting items: 74
           |  Operation: new = old + 3
           |  Test: divisible by 17
           |    If true: throw to monkey 0
           |    If false: throw to monkey 1""".stripMargin

    "Puzzle 1" should " in the sample data" in {
        val day11 = new Day11Solver
        val monkeys = day11.parseData(sampleData)
        day11.puzzle1(monkeys) should be (10605)
    }

    "Puzzle 2" should " in the sample data" in {
        val day11 = new Day11Solver
        val monkeys = day11.parseData(sampleData)
        day11.puzzle2(monkeys) should be (2713310158L)
    }

    val realData = Source.fromFile(new java.io.File(new java.io.File(".").getCanonicalPath).getParent() + "/.input/day11.data")
                         .getLines()
                         .mkString("\n")

    "Puzzle 1" should " in the AoC data" in {
        val day11 = new Day11Solver
        val monkeys = day11.parseData(realData)
        day11.puzzle1(monkeys) should be (99840)
    }

    "Puzzle 2" should " in the AoC data" in {
        val day11 = new Day11Solver
        val monkeys = day11.parseData(realData)
        day11.puzzle2(monkeys) should be (20683044837L)
    }
}
