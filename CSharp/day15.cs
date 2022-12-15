namespace AdventOfCode2022;

using NUnit.Framework;
using FluentAssertions;

using static System.Math;
using matthiasffm.Common.Math;

[TestFixture]
public class Day15
{
    private record Sensor(Vec2<int> Pos, Vec2<int> Beacon)
    {
        // returns the range (x1, x2) of signal positions x1 to x2 this sensor covers in this row
        public (int, int) CoverageAtRow(int row)
        {
            var distanceConveredInRow = Pos.ManhattanDistance(Beacon) - Abs(Pos.Y - row);
            return (Pos.X - distanceConveredInRow, Pos.X + distanceConveredInRow);
        }
    }

    private static Sensor[] ParseData(string[] data) =>
        data.Select(s => s.Split(new [] { "Sensor at x=", ", y=", ": closest beacon is at x=" }, StringSplitOptions.RemoveEmptyEntries)
                          .Select(i => int.Parse(i))
                          .ToArray())
            .Select(s => new Sensor(new Vec2<int>(s[0], s[1]), new Vec2<int>(s[2], s[3])))
            .ToArray();

    [Test]
    public void TestSamples()
    {
        var data = new[] {
            "Sensor at x=2, y=18: closest beacon is at x=-2, y=15",
            "Sensor at x=9, y=16: closest beacon is at x=10, y=16",
            "Sensor at x=13, y=2: closest beacon is at x=15, y=3",
            "Sensor at x=12, y=14: closest beacon is at x=10, y=16",
            "Sensor at x=10, y=20: closest beacon is at x=10, y=16",
            "Sensor at x=14, y=17: closest beacon is at x=10, y=16",
            "Sensor at x=8, y=7: closest beacon is at x=2, y=10",
            "Sensor at x=2, y=0: closest beacon is at x=2, y=10",
            "Sensor at x=0, y=11: closest beacon is at x=2, y=10",
            "Sensor at x=20, y=14: closest beacon is at x=25, y=17",
            "Sensor at x=17, y=20: closest beacon is at x=21, y=22",
            "Sensor at x=16, y=7: closest beacon is at x=15, y=3",
            "Sensor at x=14, y=3: closest beacon is at x=15, y=3",
            "Sensor at x=20, y=1: closest beacon is at x=15, y=3",
        };

        var sensors = ParseData(data);

        sensors[6].CoverageAtRow(10).Should().Be((2, 14));
        sensors[6].CoverageAtRow(15).Should().Be((7, 9));

        Puzzle1(sensors, 10).Should().Be(26);
        Puzzle2(sensors, 0, 20).Should().Be(14 * 4000000L + 11);
    }

    [Test]
    public void TestAocInput()
    {
        var data    = FileUtils.ReadAllLines(this);
        var sensors = ParseData(data);

        Puzzle1(sensors, 2000000).Should().Be(4861076L);
        Puzzle2(sensors, 0, 4000000).Should().Be(10649103160102L);
    }

    // You pull the emergency sensor system out of your pack, hit the big button on top, and the sensors zoom off down the tunnels. Once a sensor finds a spot
    // it thinks will give it a good reading, it attaches itself to a hard surface and begins monitoring for the nearest signal source beacon. Each sensor knows
    // its own position and can determine the position of a beacon precisely; however, sensors can only lock on to the one beacon closest to the sensor as measured
    // by the Manhattan distance. (There is never a tie where two beacons are the same distance to a sensor.)
    // None of the detected beacons seem to be producing the distress signal, so you'll need to work out where the distress beacon is by working out where it isn't.
    // For now, keep things simple by counting the positions where a beacon cannot possibly be along just a single row.
    //
    // Puzzle == Consult the report from the sensors you just deployed. In the specified row, how many positions cannot contain a beacon?
    private static long Puzzle1(IEnumerable<Sensor> sensors, int row)
    {
        // result for the specified row is all sensor ranges in this row - all beacon positions in these ranges

        var ranges = sensors.Select(sensor => sensor.CoverageAtRow(row))
                            .Where(range => IsValidRange(range))
                            .Aggregate(new LinkedList<(int, int)>(),
                                       (mergedRanges, range) => MergeRange(mergedRanges, range));

        var beacons = sensors.Where(s => s.Beacon.Y == row)
                             .Select(s => s.Beacon)
                             .ToHashSet();

        return ranges.Sum(range => range.Item2 - range.Item1 + 1 -
                                   beacons.Where(beacon => beacon.X.Between(range.Item1, range.Item2))
                                          .Count());
    }

    // Your handheld device indicates that the distress signal is coming from a beacon nearby. The distress beacon is not detected by any sensor, but the
    // distress beacon must have x and y coordinates betweend startCoord and endCoord.
    // To isolate the distress beacon's signal, you need to determine its tuning frequency, which can be found by multiplying its x coordinate by 4000000
    // and then adding its y coordinate.
    //
    // Puzzle == Find the only possible position for the distress beacon. What is its tuning frequency?
    private static long Puzzle2(IEnumerable<Sensor> sensors, int startCoord, int endCoord)
    {
        // search every row where ranges have a single gap

        for(int row = endCoord; row >= startCoord; row--) // faster to search backwards, both solutions are at the end ;)
        {
            var ranges = sensors.Select(sensor => sensor.CoverageAtRow(row))
                                .Where(range => IsValidRange(range))
                                .Aggregate(new LinkedList<(int, int)>(),
                                           (mergedRanges, range) => MergeRange(mergedRanges, range));
            System.Diagnostics.Debug.Assert(ranges.Count >= 1);

            if(ranges.Count > 1)
            {
                // no gap directly at the border occurs in this data 
                // in this data there should only be a single gap

                System.Diagnostics.Debug.Assert(ranges.Count == 2);

                var colWithGap = ranges.First!.Value.Item2 + 1;
                System.Diagnostics.Debug.Assert(ranges.First!.Next!.Value.Item1 == colWithGap + 1);

                return colWithGap * 4000000L + row;
            }
        }

        System.Diagnostics.Debug.Fail("Houston we have a problem! No gap in sensor coverage found.");
        return -1L;
    }

    private static bool IsValidRange((int, int) range) => range.Item2 >= range.Item1;

    private static LinkedList<(int, int)> MergeRange(LinkedList<(int, int)> ranges, (int, int) toMerge)
    {
        ranges.AddFirst(toMerge);

        var currentRange = ranges.First!;
        var nextRange    = currentRange!.Next;

        while(nextRange != null)
        {
            if(currentRange.Value.Item2 + 1 < nextRange.Value.Item1)
            {
                return ranges;
            }
            else if(currentRange.Value.Item1 > nextRange.Value.Item2 + 1)
            {
                var tmp = currentRange.Value;
                currentRange.Value = nextRange.Value;
                nextRange.Value = tmp;
            }
            else
            {
                nextRange.Value = (Min(currentRange.Value.Item1, nextRange.Value.Item1),
                                   Max(currentRange.Value.Item2, nextRange.Value.Item2));
                ranges.Remove(currentRange);
            }

            currentRange = nextRange;
            nextRange    = currentRange.Next;
        }

        return ranges;
    }
}
