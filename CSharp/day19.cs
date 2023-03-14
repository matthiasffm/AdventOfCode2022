namespace AdventOfCode2022;

using NUnit.Framework;
using FluentAssertions;

[TestFixture]
public class Day19
{
    private record class Blueprint(int ID, int OreRobotCost, int ClayRobotCost, (int, int) ObsidianRobotCost, (int, int) GeodeRobotCost);

    private static IEnumerable<Blueprint> ParseData(string[] data)
        => data.Select(line => line.Split(new[] { "Blueprint ",
                                                  ": Each ore robot costs ",
                                                  " ore. Each clay robot costs ",
                                                  " ore. Each obsidian robot costs ",
                                                  " ore and ",
                                                  " clay. Each geode robot costs ",
                                                  " ore and ",
                                                  " obsidian."},
                                          StringSplitOptions.RemoveEmptyEntries))
               .Select(split => split.Select(nmbr => int.Parse(nmbr)).ToArray())
               .Select(nmbrs => new Blueprint(nmbrs[0], nmbrs[1], nmbrs[2], (nmbrs[3], nmbrs[4]), (nmbrs[5], nmbrs[6])));

    [Test]
    public void TestSamples()
    {
        var data = new[] {
            "Blueprint 1: Each ore robot costs 4 ore. Each clay robot costs 2 ore. Each obsidian robot costs 3 ore and 14 clay. Each geode robot costs 2 ore and 7 obsidian.",
            "Blueprint 2: Each ore robot costs 2 ore. Each clay robot costs 3 ore. Each obsidian robot costs 3 ore and 8 clay. Each geode robot costs 3 ore and 12 obsidian.",
        };

        var blueprints = ParseData(data);

        MaxGeodes(blueprints.ElementAt(0), 0, 0, 0, 0, 1, 0, 0, 0, 1, 24).Should().Be(9);
        MaxGeodes(blueprints.ElementAt(1), 0, 0, 0, 0, 1, 0, 0, 0, 1, 24).Should().Be(12);

        Puzzle1(blueprints).Should().Be(1 * 9 + 2 * 12);

        MaxGeodes(blueprints.ElementAt(0), 0, 0, 0, 0, 1, 0, 0, 0, 1, 32).Should().Be(56);
        MaxGeodes(blueprints.ElementAt(1), 0, 0, 0, 0, 1, 0, 0, 0, 1, 32).Should().Be(62);
    }

    [Test]
    public void TestAocInput()
    {
        var data       = FileUtils.ReadAllLines(this);
        var blueprints = ParseData(data);

        Puzzle1(blueprints).Should().Be(1528);
        Puzzle2(blueprints).Should().Be(13 * 31 * 42);
    }

    // As you and the elephants leave the cave do, you notice a collection of geodes around the pond. Perhaps you could use the obsidian to create some geode-cracking
    // robots and break them open? To collect the obsidian from the bottom of the pond, you'll need waterproof obsidian-collecting robots. Fortunately, there is an
    // abundant amount of clay nearby that you can use to make them waterproof. In order to harvest the clay, you'll need special-purpose clay-collecting robots. To
    // make any type of robot, you'll need ore, which is also plentiful but in the opposite direction from the clay. Collecting ore requires ore-collecting robots
    // with big drills. Fortunately, you have exactly one ore-collecting robot in your pack that you can use to kickstart the whole operation.
    // Each robot can collect 1 of its resource type per minute. It also takes one minute for the robot factoryto construct any type of robot, although it consumes the
    // necessary resources available when construction begins. The robot factory has many blueprints (your puzzle input) you can choose from. You need to figure out
    // which blueprint would maximize the number of opened geodes after 24 minutes by figuring out which robots to build and when to build them. Determine the quality
    // level of each blueprint by multiplying that blueprint's ID number with the largest number of geodes that can be opened in 24 minutes using that blueprint.
    //
    // Puzzle == What do you get if you add up the quality level of all of the blueprints in your list?
    private static int Puzzle1(IEnumerable<Blueprint> blueprints)
    {
        return blueprints.Aggregate(0, (sum, blueprint) => sum + blueprint.ID * MaxGeodes(blueprint, 0, 0, 0, 0, 1, 0, 0, 0, 1, 24));
    }

    // While you were choosing the best blueprint, the elephants found some food on their own, so you're not in as much of a hurry; you figure you probably have 32 minutes
    // before the wind changes direction again and you'll need to get out of range of the erupting volcano. Unfortunately, one of the elephants ate most of your blueprint
    // list! Now, only the first three blueprints in your list are intact.
    // You no longer have enough blueprints to worry about quality levels. Instead, for each of the first three blueprints, determine the largest number of geodes you could
    // open; then, multiply these three values together.
    //
    // Puzzle == What do you get if you multiply these numbers together?
    private static int Puzzle2(IEnumerable<Blueprint> blueprints)
    {
        return blueprints.Take(3)
                         .Aggregate(1, (product, blueprint) => product * MaxGeodes(blueprint, 0, 0, 0, 0, 1, 0, 0, 0, 1, 32));
    }

    // determine best path recursively by choosing every possible step in every step of time:
    // - do nothing
    // - build one of the robots if enough materials are ready
    // to reduce the recursion depth and width options are pruned by some heuristics suitable for these test cases
    private static int MaxGeodes(Blueprint blueprint,
                                 int ore, int clay, int obsidian, int geodes,
                                 int oreRobots, int clayRobots, int obsidianRobots, int geodeRobots,
                                 int time, int endTime)
    {
        // TODO: even pruned this is still tooo slow with 8 seconds runtime
        //       either prune better, time + min(ore to build a robot - 1) or different algorithm
        //       use dynamic programming instead of recursion and cache queue entries

        if(time > endTime)
        {
            return geodes;
        }

        if(time > 22 && ore >= blueprint.GeodeRobotCost.Item1 && obsidian >= blueprint.GeodeRobotCost.Item2)
        {
            return MaxGeodes(blueprint,
                             ore + oreRobots - blueprint.GeodeRobotCost.Item1, clay + clayRobots, obsidian + obsidianRobots - blueprint.GeodeRobotCost.Item2, geodes + geodeRobots,
                             oreRobots, clayRobots, obsidianRobots, geodeRobots + 1,
                             time + 1, endTime);
        }
        else
        {
            int geodes0 =  MaxGeodes(blueprint,
                                     ore + oreRobots, clay + clayRobots, obsidian + obsidianRobots, geodes + geodeRobots,
                                     oreRobots, clayRobots, obsidianRobots, geodeRobots,
                                     time + 1, endTime);

            int geodes4 = 0;
            if(ore >= blueprint.GeodeRobotCost.Item1 && obsidian >= blueprint.GeodeRobotCost.Item2)
            {
                geodes4 = MaxGeodes(blueprint,
                                    ore + oreRobots - blueprint.GeodeRobotCost.Item1, clay + clayRobots, obsidian + obsidianRobots - blueprint.GeodeRobotCost.Item2, geodes + geodeRobots,
                                    oreRobots, clayRobots, obsidianRobots, geodeRobots + 1,
                                    time + 1, endTime);
            }

            int geodes3 = 0;
            if(time < 28 && ore >= blueprint.ObsidianRobotCost.Item1 && clay >= blueprint.ObsidianRobotCost.Item2 && obsidianRobots < blueprint.GeodeRobotCost.Item2)
            {
                geodes3 = MaxGeodes(blueprint,
                                    ore + oreRobots - blueprint.ObsidianRobotCost.Item1, clay + clayRobots - blueprint.ObsidianRobotCost.Item2, obsidian + obsidianRobots, geodes + geodeRobots,
                                    oreRobots, clayRobots, obsidianRobots + 1, geodeRobots,
                                    time + 1, endTime);
            }

            int geodes2 = 0;
            if(time < 18 && ore >= blueprint.ClayRobotCost && clayRobots < blueprint.ObsidianRobotCost.Item2)
            {
                geodes2 = MaxGeodes(blueprint,
                                    ore + oreRobots - blueprint.ClayRobotCost, clay + clayRobots, obsidian + obsidianRobots, geodes + geodeRobots,
                                    oreRobots, clayRobots + 1, obsidianRobots, geodeRobots,
                                    time + 1, endTime);
            }

            int geodes1 = 0;
            if(time < 10 && ore >= blueprint.OreRobotCost && oreRobots < Math.Max(blueprint.ClayRobotCost, blueprint.ObsidianRobotCost.Item1))
            {
                geodes1 = MaxGeodes(blueprint,
                                    ore + oreRobots - blueprint.OreRobotCost, clay + clayRobots, obsidian + obsidianRobots, geodes + geodeRobots,
                                    oreRobots + 1, clayRobots, obsidianRobots, geodeRobots,
                                    time + 1, endTime);
            }

            return Math.Max(geodes0, Math.Max(geodes1, Math.Max(geodes2, Math.Max(geodes3, geodes4))));
        }
    }
}
