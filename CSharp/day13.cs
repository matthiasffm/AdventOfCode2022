namespace AdventOfCode2022;

using NUnit.Framework;
using FluentAssertions;

[TestFixture]
public class Day13
{
    private static string[] separators = new[] { "," };
    private static string[] tokens     = new[] { "[", "]" };

    // converts a tokenized list like "[", "[", "11", "2", "]", "33", "]" (from puzzle input "[[11,2],33]") to a
    // packet, which is a list of lists or integers, like for this example = List<object>{ List<object> { 11, 2}, 33 }
    private static List<object> ConvertToPackets(List<string> tokens)
    {
        var list = new List<object>();

        while(tokens.Any())
        {
            var token = tokens.First();
            tokens.RemoveAt(0);

            if(token == "[")
            {
                list.Add(ConvertToPackets(tokens));
            }
            else if(token == "]")
            {
                return list;
            }
            else
            {
                list.Add(int.Parse(token));
            }
        }

        return list;
    }

    // parses data for puzzle 1 into pairs of packets
    private static IEnumerable<(List<object>, List<object>)> ParseData(string[] data) =>
        FileUtils.ParseMultilinePairs(data, t => (ConvertToPackets(t.Item1.Tokenize(separators, tokens).ToList()),
                                                  ConvertToPackets(t.Item2.Tokenize(separators, tokens).ToList())));

    [Test]
    public void TestSamples()
    {
        var data = new[] {
            "[1,1,3,1,1]",
            "[1,1,5,1,1]",
            "",
            "[[1],[2,3,4]]",
            "[[1],4]",
            "",
            "[9]",
            "[[8,7,6]]",
            "",
            "[[4,4],4,4]",
            "[[4,4],4,4,4]",
            "",
            "[7,7,7,7]",
            "[7,7,7]",
            "",
            "[]",
            "[3]",
            "",
            "[[[]]]",
            "[[]]",
            "",
            "[1,[2,[3,[4,[5,6,7]]]],8,9]",
            "[1,[2,[3,[4,[5,6,0]]]],8,9]",
        };

        var packetPairs = ParseData(data).ToArray();

        ComparePackets(packetPairs[0].Item1, packetPairs[0].Item2).Should().Be(-1);
        ComparePackets(packetPairs[1].Item1, packetPairs[1].Item2).Should().Be(-1);
        ComparePackets(packetPairs[2].Item1, packetPairs[2].Item2).Should().Be( 1);
        ComparePackets(packetPairs[3].Item1, packetPairs[3].Item2).Should().Be(-1);
        ComparePackets(packetPairs[4].Item1, packetPairs[4].Item2).Should().Be( 1);
        ComparePackets(packetPairs[5].Item1, packetPairs[5].Item2).Should().Be(-1);
        ComparePackets(packetPairs[6].Item1, packetPairs[6].Item2).Should().Be( 1);
        ComparePackets(packetPairs[7].Item1, packetPairs[7].Item2).Should().Be( 1);

        Puzzle1(packetPairs).Should().Be(1 + 2 + 4 + 6);

        var packets = data.Concat(new[] { "[[2]]", "[[6]]" })
                          .Where(l => l != string.Empty)
                          .Select(l => ConvertToPackets(l.Tokenize(separators, tokens).ToList()))
                          .ToArray();

        Puzzle2(packets).Should().Be(10 * 14);
    }

    [Test]
    public void TestAocInput()
    {
        var data        = FileUtils.ReadAllLines(this);
        var packetPairs = ParseData(data).ToArray();

        Puzzle1(packetPairs).Should().Be(6235);

        var packets = data.Concat(new[] { "[[2]]", "[[6]]" })
                          .Where(l => l != string.Empty)
                          .Select(l => ConvertToPackets(l.Tokenize(separators, tokens).ToList()))
                          .ToArray();

        Puzzle2(packets).Should().Be(22866);
    }

    // You receive a distress signal, but the packets from the signal got decoded out of order. Your list consists of pairs of
    // packets. You need to identify how many pairs of packets are in the right order.
    // Packet data consists of lists and integers. You need to compare them and determine which pairs of packets are already in the
    // right order.
    // Puzzle ==  What is the sum of the indices of those pairs?
    private static int Puzzle1(IEnumerable<(List<object>, List<object>)> packetPairs)
    {
        return packetPairs.Select((pair, idx) => (pair, idx))
                          .Where(t => ComparePackets(t.pair.Item1, t.pair.Item2) == -1)
                          .Sum(t => t.idx + 1); // puzzle result index is 1 based
    }

    // Now, you just need to put all of the packets in the right order.
    // The distress signal protocol also requires that you include two additional divider packets: [[2]] and [[6]]
    // Using the same rules as before, organize all packets - the ones in your list of received packets as well as the two divider
    // packets - into the correct order.
    // Afterward, locate the divider packets. To find the decoder key for this distress signal, you need to determine the indices
    // of the two divider packets and multiply them together.
    // Puzzle == Organize all of the packets into the correct order. What is the decoder key for the distress signal?
    private static int Puzzle2(IEnumerable<List<object>> packets)
    {
        var sortedPackets = packets.Order(new PacketComparer());

        var two = new List<object>{ new List<object>{ 2 } };
        var six = new List<object>{ new List<object>{ 6 } };
        var twoAndSix = sortedPackets.Select((packet, idx) => (packet, idx))
                                     .Where(t => ComparePackets(t.packet, two) == 0 || ComparePackets(t.packet, six) == 0)
                                     .ToArray();

        System.Diagnostics.Debug.Assert(twoAndSix.Length == 2);
        return (twoAndSix[0].idx + 1) * (twoAndSix[1].idx + 1);
    }

    // comparer class for sorting packages
    private class PacketComparer : IComparer<List<object>>
    {
        public int Compare(List<object>? left, List<object>? right)
        {
            return ComparePackets(left!, right!);
        }
    }

    // compares two packets called left and right
    // - If both values are integers, the lower integer should come first.
    //   If the left integer is lower, the inputs are in the right order. If the left integer is higher, the inputs are not in
    //   the right order. Otherwise, continue checking the next part of the input.
    // - If exactly one value is an integer, convert the integer to a list which contains that integer as its only value, then
    //   retry the comparison.
    // - If both values are lists, compare the first value of each list, then the second value, and so on. If the left list runs
    //   out of items first, the inputs are in the right order. If the right list runs out of items first, the inputs are not
    //   in the right order. If the lists are the same length and no comparison makes a decision about the order, continue
    //   checking the next part of the input.
    // 
    // left and right may change in the process when an integer packet is converted to a list
    private static int ComparePackets(List<object> left, List<object> right)
    {
        var posLeft  = 0;
        var posRight = 0;

        while(posLeft < left.Count && posRight < right.Count)
        {
            if(left[posLeft] is int && right[posRight] is int)
            {
                var intCompare = ((int)left[posLeft]).CompareTo((int)right[posRight]);
                if(intCompare != 0)
                {
                    return intCompare;
                }
                else
                {
                    posLeft++;
                    posRight++;
                }
            }
            else if(left[posLeft] is List<object> && right[posRight] is int)
            {
                right[posRight] = new List<object>{ right[posRight] };
            }
            else if(left[posLeft] is int && right[posRight] is List<object>)
            {
                left[posLeft] = new List<object>{ left[posLeft] };
            }
            else
            {
                // TODO: has this to be even recursive?
                var orderRes = ComparePackets((List<object>)left[posLeft], (List<object>)right[posRight]);

                if(orderRes != 0)
                {
                    return orderRes;
                }
                else
                {
                    posLeft++;
                    posRight++;
                }
            }
        }

        System.Diagnostics.Debug.Assert(posLeft == posRight);
        return left.Count.CompareTo(right.Count);
    }
}
