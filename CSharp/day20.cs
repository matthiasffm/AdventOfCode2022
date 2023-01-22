namespace AdventOfCode2022;

using NUnit.Framework;
using FluentAssertions;

// TODO: 200ms still a bit too slow
//       list could be replaced with fixed sized array with each entry have indexes for
//       Next and Previous instead of pointers; same O, but maybe better constant factor

[TestFixture]
public class Day20
{
    private static IEnumerable<long> ParseData(string[] data)
        => data.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => long.Parse(s));

    [Test]
    public void TestSamples()
    {
        var data = new [] { 
            "1",
            "2",
            "-3",
            "3",
            "-2",
            "0",
            "4",
         };
        var numbers = ParseData(data);

        Puzzle(numbers, 1, 1).Should().Be(4 + -3 + 2);
        Puzzle(numbers, 811589153L, 10).Should().Be(811589153L + 2434767459L  + -1623178306L);
    }

    [Test]
    public void TestAocInput()
    {
        var data    = FileUtils.ReadAllLines(this);
        var numbers = ParseData(data);

        Puzzle(numbers, 1, 1).Should().Be(6712);
        Puzzle(numbers, 811589153L, 10).Should().Be(1595584274798L);
    }

    // node in a circular linked list
    private class CircularLinkedListNode<T>
    {
        public T Value { get; private set; }

        public CircularLinkedListNode<T> Previous { get; set; }
        public CircularLinkedListNode<T> Next     { get; set; }

        public CircularLinkedListNode(T val)
        {
            this.Value = val;
            this.Next = this.Previous = this;
        }

        public CircularLinkedListNode(T val, CircularLinkedListNode<T> previous, CircularLinkedListNode<T> next)
        {
            this.Value    = val;
            this.Next     = next;
            this.Previous = previous;

            previous.Next = this;
            next.Previous = this;
        }
    }

    // circular linked list where you can traverse around in both directions without end
    private class CircularLinkedList<T>
    {
        public CircularLinkedListNode<T> Head { get; private set; } // one node of the circle is the 'head';

        public CircularLinkedList(T headValue)
        {
            Head = new CircularLinkedListNode<T>(headValue);
        }

        public void SwapAfter(CircularLinkedListNode<T> toSwap, CircularLinkedListNode<T> after)
        {
            if(toSwap != after)
            {
                // remove this from links

                toSwap.Next.Previous = toSwap.Previous;
                toSwap.Previous.Next = toSwap.Next;

                // link this after

                toSwap.Previous     = after;
                toSwap.Next         = after.Next;
                after.Next.Previous = toSwap;
                after.Next          = toSwap;
            }
        }

        public void SwapBefore(CircularLinkedListNode<T> toSwap, CircularLinkedListNode<T> before)
        {
            if(toSwap != before)
            {
                // remove this from links

                toSwap.Next.Previous = toSwap.Previous;
                toSwap.Previous.Next = toSwap.Next;

                // link this before

                toSwap.Next          = before;
                toSwap.Previous      = before.Previous;
                before.Previous.Next = toSwap;
                before.Previous      = toSwap;
            }
        }
        public CircularLinkedListNode<T>? Find(T toFind)
        {
            var current = this.Head;
            do
            {
                if(object.Equals(current.Value, toFind))
                {
                    return current;
                }
                current = current.Next;
            }
            while(current != this.Head);

            return null;
        }

        public CircularLinkedListNode<T> Skip(CircularLinkedListNode<T> node, int skip)
        {
            for(int i = 0; i < skip; i++)
            {
                node = node.Next;
            }

            return node;
        }
    }

    // Your handheld device has a file (your puzzle input) that contains the grove's coordinates! Unfortunately, the file is encrypted.
    // The main operation involved in decrypting is called mixing. The encrypted file is a list of numbers. Multiply each number by the
    // decryption key before you begin; this will produce the actual list of numbers to mix. To mix the file, move each number forward
    // or backward a number of positions equal to the value of the number being moved. The list is circular, so moving a number off one
    // end of the list wraps back around to the other end as if the ends were connected.
    // The numbers should be moved in the order they originally appear in the encrypted file. Numbers moving around during the mixing
    // process do not change the order in which the numbers are moved.
    // To complete the decryption this process of mixing the complete list has to be repeated n times.
    // Then, the grove coordinates can be found by looking at the 1000th, 2000th, and 3000th numbers after the value 0, wrapping around
    // the list as necessary.
    //
    // Puzzle == What is the sum of the three numbers that form the grove coordinates?
    private static long Puzzle(IEnumerable<long> numbers, long decryptionKey, int repeat)
    {
        numbers = numbers.Select(n => n * decryptionKey).ToArray();

        // build circular list and remember all node references in nodes list
        // these _references_ are important for step 2 where we iterate over
        // them in order to move them (as these are refs their movement itself
        // in the circular list doesnt change their order in the nodes list!)

        var list  = new CircularLinkedList<long>(numbers.First());
        var nodes = new List<CircularLinkedListNode<long>>
        {
            list.Head,
        };

        var currentNode = list.Head;
        foreach(var number in numbers.Skip(1))
        {
            currentNode = new CircularLinkedListNode<long>(number, currentNode, list.Head);
            nodes.Add(currentNode);
        }

        // move all nodes n steps around in the list according to their value
        // repeat x times

        for(int r = 0; r < repeat; r++)
        {
            foreach(var node in nodes)
            {
                var steps = node.Value >= 0 ? node.Value % (nodes.Count - 1) : -((-node.Value) % (nodes.Count - 1));

                // if n steps is more than halfway in one direction its faster to move length-n steps in the other direction
                // this almost halfs the runtime
                if(Math.Abs(steps) > nodes.Count / 2)
                {
                    steps = steps > 0 ? steps - nodes.Count + 1 : steps + nodes.Count - 1;
                }

                var iter = node;
                for(int step = 0; step < Math.Abs(steps); step++)
                {
                    iter = steps > 0 ? iter.Next : iter.Previous;
                }

                if(steps > 0)
                {
                    list.SwapAfter(node, iter);
                }
                else if(steps < 0)
                {
                    list.SwapBefore(node, iter);
                }
            }
        }

        // find 0 node and 1000th, 2000th and 3000th node after it

        var node0    = list.Find(0L)!;
        var node1000 = list.Skip(node0, 1000);
        var node2000 = list.Skip(node1000, 1000);
        var node3000 = list.Skip(node2000, 1000);

        return node1000.Value + node2000.Value + node3000.Value;
    }
}
