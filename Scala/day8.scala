import java.nio.file.{Files, Paths}
import scala.io.Source
import org.scalatest._
import flatspec._
import matchers._

class Day8Solver {

    def parseData(input: Array[String]) : Array[Array[Byte]] = Array.tabulate(input.length, input.length)((row, col) => (input(row)(col) - '0').toByte)

    def getRow(trees: Array[Array[Byte]], row: Int) : Seq[Byte] = 0 to trees.length - 1 map(col => trees(row)(col))
    def getCol(trees: Array[Array[Byte]], col: Int) : Seq[Byte] = 0 to trees.length - 1 map(row => trees(row)(col))

    // First, determine whether there is enough tree cover here to keep a tree house hidden. To do this, you need to count the
    // number of trees that are visible from outside the grid when looking directly along a row or column.
    // A tree is visible if all of the other trees between it and an edge of the grid are shorter than it. Only consider trees in
    // the same row or column; that is, only look up, down, left, or right from any given tree.
    // Puzzle == Consider your map; how many trees are visible from outside the grid?
    def puzzle1(trees: Array[Array[Byte]]) : Int = (1 to trees.length - 2).flatMap(row => (1 to trees.length - 2).map(col => (row, col)))
                                                                          .filter((row, col) => visible(trees, trees(row)(col), row, col))
                                                                          .length +
                                                    trees.length * 2 * 2 - 4

    def visible(trees: Array[Array[Byte]], tree: Byte, row: Int, col: Int) : Boolean =
        getRow(trees, row).take(col).forall(c => c < tree) ||
        getRow(trees, row).drop(col + 1).forall(c => c < tree) ||
        getCol(trees, col).take(row).forall(r => r < tree) ||
        getCol(trees, col).drop(row + 1).forall(r => r < tree)

    // The Elves just need to know the best spot to build their tree house: they would like to be able to see a lot of trees.
    // To measure the viewing distance from a given tree, look up, down, left, and right from that tree; stop if you reach an
    // edge or at the first tree that is the same height or taller than the tree under consideration. (so a tree on the edge == 0)
    // A tree's scenic score is found by multiplying together its viewing distance in each of the four directions.
    // Puzzle == What is the highest scenic score possible for any tree?
    def puzzle2(trees: Array[Array[Byte]]) : Int = (0 to trees.length - 1).flatMap(row => (0 to trees.length - 1).map(col => (row, col)))
                                                                          .map((row, col) => scenicScore(trees, trees(row)(col), row, col))
                                                                          .max

    def scenicScore(trees: Array[Array[Byte]], tree: Byte, row: Int, col: Int) : Int =
        viewingDistance(tree, getRow(trees, row).take(col).reverse) *
        viewingDistance(tree, getRow(trees, row).drop(col + 1))     *
        viewingDistance(tree, getCol(trees, col).take(row).reverse) *
        viewingDistance(tree, getCol(trees, col).drop(row + 1))

    def viewingDistance(tree: Byte, lineOfSight: Seq[Byte]) : Int = {
        val los = lineOfSight.takeWhile(t => t < tree).length
        if(los < lineOfSight.length) {
            return los + 1;
        }
        else {
            return los;
        }
    }
}

class Day8 extends AnyFlatSpec with should.Matchers {

    var sampleData = Array(
        "30373",
        "25512",
        "65332",
        "33549",
        "35390",
    )

    "Puzzle 1" should "count all trees visible from outside the forest in the sample data" in {
        val day8 = new Day8Solver
        var trees = day8.parseData(sampleData);
        day8.puzzle1(trees) should be (21)
    }

    "Puzzle 2" should "find the tree with the most scenic score in the sample data" in {
        val day8 = new Day8Solver
        var trees = day8.parseData(sampleData);
        day8.puzzle2(trees) should be (8)
    }

    val realData = Source.fromFile(new java.io.File(new java.io.File(".").getCanonicalPath).getParent() + "/.input/day8.data")
                         .getLines
                         .toArray

    "Puzzle 1" should "count all trees visible from outside the forest in the AoC data" in {
        val day8 = new Day8Solver
        var trees = day8.parseData(realData);
        day8.puzzle1(trees) should be (1717)
    }

    "Puzzle 2" should "find the tree with the most scenic score in the AoC data" in {
        val day8 = new Day8Solver
        var trees = day8.parseData(realData);
        day8.puzzle2(trees) should be (321975)
    }
}
