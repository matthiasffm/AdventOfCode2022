namespace AdventOfCode2022;

using NUnit.Framework;
using FluentAssertions;

[TestFixture]
public class Day07
{
    private class Dir
    {
        public string               Name   { get; }
        public Dir?                 Parent { get; }
        public List<Dir>            Dirs   { get; }
        public List<(string, long)> Files  { get; }

        public Dir(string name, Dir? parent = null)
        {
            this.Name   = name;
            this.Parent = parent ?? this;
            this.Dirs   = new List<Dir>();
            this.Files  = new List<(string, long)>();
        }

        public Dir AddSubdir(string subdirName)
        {
            Dirs.Add(new Dir(subdirName, this));
            return this;
        }

        public Dir AddFile(string[] fileSpecs)
        {
            Files.Add((fileSpecs[1], long.Parse(fileSpecs[0])));
            return this;
        }

        public long Size { get => Dirs.Sum(d => d.Size) + Files.Sum(f => f.Item2); }

        public IEnumerable<long> DirectorySizes { get => Dirs.SelectMany(d => d.DirectorySizes).Concat(new[] { Size }); }
    }

    private Dir ParseData(IEnumerable<string> lines)
    {
        var root       = new Dir("root");
        var currentDir = root;

        // The input is a terminal output. Within the terminal output, lines that begin with $ are commands you executed, very much
        // like some modern computers: "$ ls" for listing a directory structure and "$ cd xyz" is for changing the current directory. The
        // rest of the lines are files (in the form "size, name") and directory listings (in the form "dir xyz")

        foreach(var line in lines.Skip(1))
        {
            currentDir = line.Split(' ') switch
            {
                // commands
                ["$", "ls"]             => currentDir,
                ["$", "cd", ".."]       => currentDir.Parent!,
                ["$", "cd", var dir]    => currentDir.Dirs.First(d => d.Name == dir),

                // directory listings
                ["dir", var dir]        => currentDir.AddSubdir(dir),
                var fileSpecs           => currentDir.AddFile(fileSpecs!),
            };
        }

        return root;
    }

    [Test]
    public void TestSamples()
    {
        var data = new [] {
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
        };
        var rootDir = ParseData(data);

        Puzzle1(rootDir).Should().Be(95437L);
        Puzzle2(rootDir).Should().Be(24933642L);
    }

    [Test]
    public void TestAocInput()
    {
        var data = FileUtils.ReadAllLines(this);
        var rootDir = ParseData(data);

        Puzzle1(rootDir).Should().Be(1792222L);
        Puzzle2(rootDir).Should().Be(1112963L);
    }

    // Since the disk is full, your first step should be to find directories that are good candidates for deletion. To do this, you need to determine the total
    // size of each directory. The total size of a directory is the sum of the sizes of the files it contains, directly or indirectly. To begin, find all of the
    // directories with a total size of at most 100000.
    // Puzzle == What is the sum of the total sizes of those directories?
    private long Puzzle1(Dir root)
    {
        return root.DirectorySizes
                   .Where(size => size <= 100000L)
                   .Sum();
    }

    // The total disk space available to the filesystem is 70000000. To run the update, you need unused space of at least 30000000. You need to find a
    // directory you can delete that will free up enough space to run the update.
    // Puzzle == Find the smallest directory that, if deleted, would free up enough space on the filesystem to run the update. What is the total
    //           size of that directory?
    private long Puzzle2(Dir root)
    {
        var unusedSpace     = 70000000L - root.Size;
        var neededForUpdate = 30000000L - unusedSpace;

        return root.DirectorySizes
                   .Where(size => size >= neededForUpdate)
                   .Min();
    }
}
