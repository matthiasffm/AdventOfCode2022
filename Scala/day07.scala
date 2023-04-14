import java.nio.file.{Files, Paths}
import scala.io.Source
import org.scalatest._
import flatspec._
import matchers._

class Day07Solver {

    class Dir(var dirs: List[Dir], val parent: Dir, var fileSizes: Int) {
        def this(parent: Dir) = this(List[Dir](), parent, 0)

        def addDir() : Dir = {
            val newDir = new Dir(this)
            this.dirs = this.dirs :+ newDir
            return newDir
        }

        def addFile(fileSize: Int) : Dir = {
            this.fileSizes += fileSize
            return this
        }

        def size()  : Int       = this.fileSizes + this.dirs.map(_.size()).sum
        def sizes() : List[Int] = this.size() :: this.dirs.flatMap(_.sizes())
    }

    def parseData(input: Seq[String]) : Dir = {

        val root       = new Dir(null)
        var currentDir = root

        // compared to c# solution here we ignore all names and individual sizes, we only care about
        // directory sizes and the directory structure

        for(line <- input.tail) {
            currentDir = line match {
                case "$ ls"                 => currentDir
                case "$ cd .."              => currentDir.parent
                case s"$$ cd $dir"          => currentDir.addDir()

                case s"dir $dir"            => currentDir
                case s"$size $file"         => currentDir.addFile(size.toInt)
            }
        }

        return root
    }

    // Since the disk is full, your first step should be to find directories that are good candidates for deletion. To do this, you need to determine the total
    // size of each directory. The total size of a directory is the sum of the sizes of the files it contains, directly or indirectly. To begin, find all of the
    // directories with a total size of at most 100000.
    //
    // Puzzle == What is the sum of the total sizes of those directories?
    def puzzle1(root: Dir) : Int = root.sizes().filter(_ <= 100000).sum

    // The total disk space available to the filesystem is 70000000. To run the update, you need unused space of at least 30000000. You need to find a
    // directory you can delete that will free up enough space to run the update.
    //
    // Puzzle == Find the smallest directory that, if deleted, would free up enough space on the filesystem to run the update. What is the total
    //           size of that directory?
    def puzzle2(root: Dir) : Int = {
        val unused = 70000000 - root.size()
        val neededForUpdate = 30000000 - unused

        return root.sizes().filter(_ >= neededForUpdate).min
    }
}

class Day07 extends AnyFlatSpec with should.Matchers {

    var sampleData =Array(
        "$ cd /",
        "$ ls",
        "dir a",
        "14848514 b.txt",
        "8504156 c.dat",
        "dir d",
        "$ cd a",
        "$ ls",
        "dir e",
        "29116 f",
        "2557 g",
        "62596 h.lst",
        "$ cd e",
        "$ ls",
        "584 i",
        "$ cd ..",
        "$ cd ..",
        "$ cd d",
        "$ ls",
        "4060174 j",
        "8033020 d.log",
        "5626152 d.ext",
        "7214296 k",
    ).toSeq

    "Puzzle 1" should "sum of the total sizes of directories <= 100000 in the sample data" in {
        val day7 = new Day07Solver
        var rootDir = day7.parseData(sampleData)
        day7.puzzle1(rootDir) should be (95437)
    }

    "Puzzle 2" should "find the smallest directory in the sample data that, if deleted, would free up enough space on the filesystem to run the update" in {
        val day7 = new Day07Solver
        var rootDir = day7.parseData(sampleData)
        day7.puzzle2(rootDir) should be (24933642)
    }

    val realData = Source.fromFile(new java.io.File(new java.io.File(".").getCanonicalPath).getParent() + "/.input/day07.data")
                         .getLines
                         .toSeq

    "Puzzle 1" should "sum of the total sizes of directories <= 100000 in the AoC data" in {
        val day7 = new Day07Solver
        var rootDir = day7.parseData(realData)
        day7.puzzle1(rootDir) should be (1792222)
    }

    "Puzzle 2" should "find the smallest directory in the AoC data that, if deleted, would free up enough space on the filesystem to run the update" in {
        val day7 = new Day07Solver
        var rootDir = day7.parseData(realData)
        day7.puzzle2(rootDir) should be (1112963)
    }
}
