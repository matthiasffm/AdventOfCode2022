namespace AdventOfCode2022;

using NUnit.Framework;
using FluentAssertions;

using System.Collections.Generic;
using System.Diagnostics;
using static System.Math;

using matthiasffm.Common;
using matthiasffm.Common.Math;

[TestFixture]
public class Day15
{
    private record Sensor(Vec2<int> Pos, Vec2<int> Beacon)
    {
        public int Distance => Pos.ManhattanDistance(Beacon);

        // returns the range (x1, x2) of signal positions x1 to x2 this sensor covers in this row
        public (int, int) CoverageAtRow(int row)
        {
            var distanceConveredInRow = Distance - Abs(Pos.Y - row);
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

        var ranges = FindSensorRangesInRow(sensors, row);

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
        // we could investigate _every_ row from startCoord to endCoord (takes about a second) or we
        // could only investigage the rows, where a gap could be even near.
        // the gap has to be where borders of 4 or more signals are very close to each other or intersect:
        // all border are diagonals with y = (+/-)x + n
        // so we collect and sort the params of these diagonals on every axis and select only those of them which are _very_ close
        // to each other (within 2 units)
        // then we intersect all these remaining diagonals with each other and only investigate the rows near these intersections
        // 
        // so we get from O(rows*m) to O(close_intersections*m); and close intersections are a small subset of the
        // intersections of all 4 diagonals of all signals combined (from 2500 down to around like a 100)

        var diagParams1 = sensors.Select(sensor => sensor.Pos.X - sensor.Pos.Y - sensor.Distance)
                                 .Concat(sensors.Select(sensor => sensor.Pos.X - sensor.Pos.Y + sensor.Distance))
                                 .Order()
                                 .Pairs()
                                 .Where(pair => Abs(pair.Item1 - pair.Item2) <= 2)
                                 .SelectMany(pair => new[] { pair.Item1, pair.Item2 });
        var diagParams2 = sensors.Select(sensor => sensor.Pos.X + sensor.Pos.Y - sensor.Distance)
                                 .Concat(sensors.Select(sensor => sensor.Pos.X + sensor.Pos.Y + sensor.Distance))
                                 .Order()
                                 .Pairs()
                                 .Where(pair => Abs(pair.Item1 - pair.Item2) <= 2)
                                 .SelectMany(pair => new[] { pair.Item1, pair.Item2 });

        var rowsNearCloseIntersections = diagParams1.Variations(diagParams2)
                                                    .Select(ns => (ns.Item2 - ns.Item1) / 2) // diagonales intersect at y = (n2 - n1) / 2;
                                                    .Where(row => row.Between(startCoord, endCoord))
                                                    .Distinct();

        // search every row near a crossing of diagonals if there are ranges have a single gap in this row

        foreach(var row in rowsNearCloseIntersections)
        {
            var ranges = FindSensorRangesInRow(sensors, row);

            if(ranges.Count > 1)
            {
                var colWithGap = ranges.First!.Value.Item2 + 1;
                return colWithGap * 4000000L + row;
            }
        }

        Debug.Fail("Houston we have a problem! No gap in sensor coverage found.");
        return -1L;
    }

    // finds all ranges in a row covered by signals, returns a list of (start, end) tuple sorted from left to right
    private static LinkedList<(int, int)> FindSensorRangesInRow(IEnumerable<Sensor> sensors, int row) =>
        sensors.Select(sensor => sensor.CoverageAtRow(row))
               .Where(range => IsValidRange(range))
               .Aggregate(new LinkedList<(int, int)>(),
                          (mergedRanges, range) => MergeRange(mergedRanges, range));

    private static bool IsValidRange((int, int) range) => range.Item2 >= range.Item1;

    // merges a range to a (sorted) list of ranges
    // if the new range overlaps an existing range in the list the 2 ranges are merged
    // after the merge operation only ranges are in the list with a minimum of 1 unit apart
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
                (nextRange.Value, currentRange.Value) = (currentRange.Value, nextRange.Value);
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
