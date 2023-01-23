namespace AdventOfCode2022;

using NUnit.Framework;
using FluentAssertions;

using matthiasffm.Common.Math;

[TestFixture]
public class Day18
{
    private Vec3<int>[] ParseData(string[] data)
        => data.Select(l => l.Split(',').Select(s => int.Parse(s)).ToArray())
               .Select(nmbrs => new Vec3<int>(nmbrs[0], nmbrs[1], nmbrs[2]))
               .ToArray();

    [Test]
    public void TestSamples()
    {
        var data = new[] {
            "2,2,2",
            "1,2,2",
            "3,2,2",
            "2,1,2",
            "2,3,2",
            "2,2,1",
            "2,2,3",
            "2,2,4",
            "2,2,6",
            "1,2,5",
            "3,2,5",
            "2,1,5",
            "2,3,5", };

        var lavaDroplets = ParseData(data);

        Puzzle1(lavaDroplets).Should().Be(64);
        Puzzle2(lavaDroplets).Should().Be(58);
    }

    [Test]
    public void TestAocInput()
    {
        var data         = FileUtils.ReadAllLines(this);
        var lavaDroplets = ParseData(data);

        Puzzle1(lavaDroplets).Should().Be(4504);
        Puzzle2(lavaDroplets).Should().Be(2556);
    }

    // You've emerged near the base of a large volcano that seems to be actively erupting! Bits of lava are still being ejected toward you, so you're sheltering
    // in the cavern exit a little longer. Outside the cave, you can see lava droplets landing in a pond and hear it loudly hissing as it solidifies. You take a
    // quick scan of a droplet as it flies past you (your puzzle input). Because of how quickly the lava is moving, the scan isn't very good; its resolution is
    // quite low and, as a result, it approximates the shape of the lava droplet with 1x1x1 cubes on a 3D grid, each given as its x,y,z position.
    // To approximate the surface area, count the number of sides of each cube that are not immediately connected to another cube.
    //
    // Puzzle == What is the surface area of your scanned lava droplets?
    private static int Puzzle1(IEnumerable<Vec3<int>> lavaDroplets)
    {
        var solidCubes = new HashSet<Vec3<int>>(lavaDroplets);

        return solidCubes.SelectMany(c => DirectNeighbors.Select(n => c + n))
                         .Count(side => !solidCubes.Contains(side));
    }

    // Something seems off about your calculation. The cooling rate depends on exterior surface area, but your calculation also included the surface area of air
    // pockets trapped in the lava droplet. Instead, consider only cube sides that could be reached by the water and steam as the lava droplet tumbles into the
    // pond. The steam will expand to reach as much as possible, completely displacing any air on the outside of the lava droplet but never expanding diagonally.
    //
    // Puzzle == What is the exterior surface area of your scanned lava droplet?
    private static int Puzzle2(IEnumerable<Vec3<int>> lavaDroplets)
    {
        var solidCubes = new HashSet<Vec3<int>>(lavaDroplets);

        // iterate inner min-max 3D area, inner cube is _potentially_ part of air pocket if
        // - is not a solid cube
        // - has other cubes on all sides, i.e. has linear direct line of sight to outside air

        var minX = solidCubes.Min(c => c.X);
        var maxX = solidCubes.Max(c => c.X);
        var minY = solidCubes.Min(c => c.Y);
        var maxY = solidCubes.Max(c => c.Y);
        var minZ = solidCubes.Min(c => c.Z);
        var maxZ = solidCubes.Max(c => c.Z);

        var maybeBubble        = new HashSet<Vec3<int>>();
        var connectedToOutside = new HashSet<Vec3<int>>();

        for(int x = minX + 1; x < maxX; x++)
        {
            for(int y = minY + 1; y < maxY; y++)
            {
                for(int z = minZ + 1; z < maxZ; z++)
                {
                    if(!solidCubes.Contains(new Vec3<int>(x, y, z)))
                    {
                        if(Enumerable.Range(minX, x - minX).Any(tX => solidCubes.Contains(new Vec3<int>(tX, y, z))) &&
                           Enumerable.Range(x + 1, maxX - x).Any(tX => solidCubes.Contains(new Vec3<int>(tX, y, z))) &&
                           Enumerable.Range(minY, y - minY).Any(tY => solidCubes.Contains(new Vec3<int>(x, tY, z))) &&
                           Enumerable.Range(y + 1, maxY - y).Any(tY => solidCubes.Contains(new Vec3<int>(x, tY, z))) &&
                           Enumerable.Range(minZ, z - minZ).Any(tZ => solidCubes.Contains(new Vec3<int>(x, y, tZ))) &&
                           Enumerable.Range(z + 1, maxZ - z).Any(tZ => solidCubes.Contains(new Vec3<int>(x, y, tZ))))
                        {
                            maybeBubble.Add(new Vec3<int>(x, y, z));
                        }
                        else
                        {
                            connectedToOutside.Add(new Vec3<int>(x, y, z));
                        }
                    }
                }
            }
        }

        int airPocketSize;

        // iterate over outside border and add air pocket cubes to it if connected
        // before: every cube in connectedToOutside is outside, every cube in maybeBubble is _potentially_ in an air pocket
        // aftet:  every cube in connectedToOutside is outside, every cube in maybeBubble is in an air pocket
        // spoiler: shockingly (and a bit disappointing) this only takes one iteration

        do
        {
            airPocketSize = maybeBubble.Count;

            var connectedToOutsideBorder = connectedToOutside.SelectMany(air => DirectNeighbors.Select(n => air + n))
                                                             .Distinct()
                                                             .Where(n => !solidCubes.Contains(n) && maybeBubble.Contains(n))
                                                             .ToArray();

            foreach(var connected in connectedToOutsideBorder)
            {
                maybeBubble.Remove(connected);
                connectedToOutside.Add(connected);
            }
        }
        while(airPocketSize != maybeBubble.Count);

        return lavaDroplets.SelectMany(c => DirectNeighbors.Select(n => c + n))
                           .Count(n => !solidCubes.Contains(n) && !maybeBubble.Contains(n));
    }

    private static readonly Vec3<int>[] DirectNeighbors = new Vec3<int>[] {
        new Vec3<int>(1, 0, 0),
        new Vec3<int>(-1, 0, 0),
        new Vec3<int>(0, 1, 0),
        new Vec3<int>(0, -1, 0),
        new Vec3<int>(0, 0, 1),
        new Vec3<int>(0, 0, -1),
    };
}
