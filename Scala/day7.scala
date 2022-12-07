import java.nio.file.{Files, Paths}
import scala.io.Source
import org.scalatest._
import flatspec._
import matchers._

class Day7Solver {

    class Dir(val name: String, var dirs: List[Dir], val parent: Dir, var fileSizes: Int) {
        def this(name: String) = {
            this(name, List[Dir](), null, 0)
        }

        def this(name: String, parent: Dir) = {
            this(name, List[Dir](), parent, 0)
        }

        def test() : Dir = {
            return this
        }

        def addDir(name: String) : Dir = {
            this.dirs = this.dirs :+ new Dir(name, this)
            return this
        }

        def addFile(fileSize: Int) : Dir = {
            this.fileSizes += fileSize
            return this
        }

        def size()  : Int       = this.fileSizes + this.dirs.map(d => d.size()).sum
        def sizes() : List[Int] = this.size() :: this.dirs.flatMap(d => d.sizes())
    }

    def parseData(input: Seq[String]) : Dir = {

        val root       = new Dir("root")
        var currentDir = root

        // compared to c# solution here we ignore all file names and individual sizes, we only care about
        // directory sizes and the directory tree (this may be a bit short sighted for the coming days but ...)

        for(line <- input.tail) {
            currentDir = line match {
                case "$ ls"                 => currentDir
                case "$ cd .."              => currentDir.parent
                case s"$$ cd $dir"          => currentDir.dirs.filter(d => d.name == dir).head

                case s"dir $dir"            => currentDir.addDir(dir)
                case s"$size $file"         => currentDir.addFile(size.toInt)
            }
        }

        return root
    }

    // Since the disk is full, your first step should be to find directories that are good candidates for deletion. To do this, you need to determine the total
    // size of each directory. The total size of a directory is the sum of the sizes of the files it contains, directly or indirectly. To begin, find all of the
    // directories with a total size of at most 100000.
    // Puzzle == What is the sum of the total sizes of those directories?
    def puzzle1(root: Dir) : Int = root.sizes().filter(s => s <= 100000).sum

    // The total disk space available to the filesystem is 70000000. To run the update, you need unused space of at least 30000000. You need to find a
    // directory you can delete that will free up enough space to run the update.
    // Puzzle == Find the smallest directory that, if deleted, would free up enough space on the filesystem to run the update. What is the total
    //           size of that directory?
    def puzzle2(root: Dir) : Int = {
        val unused = 70000000 - root.size()
        val neededForUpdate = 30000000 - unused

        return root.sizes().filter(s => s >= neededForUpdate).min
    }
}

class Day7 extends AnyFlatSpec with should.Matchers {

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

    "Puzzle 1" should "find how many characters need to be processed before the first start-of-packet marker is detected in the sample data" in {
        val day7 = new Day7Solver
        var rootDir = day7.parseData(sampleData);
        day7.puzzle1(rootDir) should be (95437)
    }

    "Puzzle 2" should "find how many characters need to be processed before the first start-of-message marker is detected in the sample data" in {
        val day7 = new Day7Solver
        var rootDir = day7.parseData(sampleData);
        day7.puzzle2(rootDir) should be (24933642)
    }

    val realData = Source.fromFile(new java.io.File(new java.io.File(".").getCanonicalPath).getParent() + "/.input/day7.data")
                         .getLines
                         .toSeq

    "Puzzle 1" should "find how many characters need to be processed before the first start-of-packet marker is detected in the AoC data" in {
        val day7 = new Day7Solver
        var rootDir = day7.parseData(realData);
        day7.puzzle1(rootDir) should be (1792222)
    }

    "Puzzle 2" should "find how many characters need to be processed before the first start-of-message marker is detected in the AoC data" in {
        val day7 = new Day7Solver
        var rootDir = day7.parseData(realData);
        day7.puzzle2(rootDir) should be (1112963)
    }
}
