import java.nio.file.{Files, Paths}
import scala.io.Source
import org.scalatest._
import flatspec._
import matchers._

class Day3Solver {

    // Each rucksack has two large compartments of the same size. All items of a given type are meant to go into
    // exactly one of the two compartments. The Elf that did the packing failed to follow this rule for exactly
    // one item type per rucksack.
    // To help prioritize item rearrangement, every item type can be converted to a priority (= number of letter). 
    // Puzzle == Find the item type that appears in both compartments of each rucksack. What is the sum of the
    //           priorities of those item types?
    def puzzle1(rucksacks: Seq[String]) : Int = rucksacks.map(r => priority(r.slice(0, r.length / 2).toSet
                                                                            .intersect(r.slice(r.length / 2, r.length).toSet)
                                                                            .head))
                                                         .sum

    // The Elves are divided into groups of three. Every Elf carries a badge that identifies their group. For
    // efficiency, within each group of three Elves, the badge is the only item type carried by all three Elves.
    // That is, if a group's badge is item type B, then all three Elves will have item type B somewhere in their
    // rucksack, and at most two of the Elves will be carrying any other item type.
    // Additionally, nobody wrote down which item type corresponds to each group's badges. The only way to tell
    // which item type is the right one is by finding the one that is common between all three Elves in each group.
    // Puzzle == Find the item type that corresponds to the badges of each three-Elf group. What is the sum of
    //           the priorities of those item types?
    def puzzle2(rucksacks: Seq[String]) : Int = rucksacks.grouped(3)
                                                         .map(rg => priority(rg(0).toSet
                                                                             .intersect(rg(1).toSet)
                                                                             .intersect(rg(2).toSet)
                                                                             .head))
                                                         .sum

    def priority(letter: Char) : Int = if(letter >= 'a') letter - 'a' + 1 else letter - 'A' + 27
}

class Day3 extends AnyFlatSpec with should.Matchers {

    val sampleData = Array(
        "vJrwpWtwJgWrhcsFMMfFFhFp",
        "jqHRNqRjqzjGDLGLrsFMfFZSrLrFZsSL",
        "PmmdzqPrVvPwwTWBwg",
        "wMqvLMZHhHMvwLHjbvcjnnSBnvTQFn",
        "ttgJtRGJQctTZtZT",
        "CrZsJsPPZsGzwwsLwLmpwMDw",
    ).toSeq

    "Puzzle 1" should "sum the priorities of all misplaced items in the sample data" in {
        val day3 = new Day3Solver
        day3.puzzle1(sampleData) should be (16 + 38 +42 + 22 + 20 + 19)
    }

    "Puzzle 2" should "sum the priorities of all badges in the sample data" in {
        val day3 = new Day3Solver
        day3.puzzle2(sampleData) should be (18 + 52)
    }

    val realData = Source.fromFile(new java.io.File(new java.io.File(".").getCanonicalPath).getParent() + "/.input/day03.data")
                         .getLines
                         .toSeq

    "Puzzle 1" should "sum the priorities of all misplaced items in the AoC data" in {
        val day3 = new Day3Solver
        day3.puzzle1(realData) should be (8394)
    }

    "Puzzle 2" should "sum the priorities of all badges in the AoC data" in {
        val day3 = new Day3Solver
        day3.puzzle2(realData) should be (2413)
    }
}
