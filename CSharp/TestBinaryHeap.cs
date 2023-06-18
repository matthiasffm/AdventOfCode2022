namespace AdventOfCode2022;

using NUnit.Framework;
using FluentAssertions;

[TestFixture]
public partial class TestBinaryHeap
{
    [Test]
    public void TestEmptyHeap()
    {
        // arrange
        var heap = new BinaryHeap<int>();

        // act
        var min = () => heap.Min;
        var extract = () => heap.ExtractMin();
        var count = heap.Count;
        var contains1 = heap.Contains(1, out _);

        // assert
        count.Should().Be(0);
        contains1.Should().BeFalse();
        min.Should().Throw<IndexOutOfRangeException>();
        extract.Should().Throw<IndexOutOfRangeException>();
    }

    [Test]
    public void TestCountSingleElementHeap()
    {
        // arrange
        var heap = new BinaryHeap<int>(new int[] { 1 });

        // act
        var count = heap.Count;

        // assert
        count.Should().Be(1);
    }

    [Test]
    public void TestCountMultipleElementHeap([Values(new int[] { 3, 5, 1, 4, 2 }, new int[] { 1, 2, 3, 2, 5, 7 })] int[] elements)
    {
        // arrange
        var heap = new BinaryHeap<int>(elements);

        // act
        var count = heap.Count;

        // assert
        count.Should().Be(elements.Length);
    }

    [Test]
    public void TestContainsSingleElementHeap()
    {
        // arrange
        var heap = new BinaryHeap<int>(new int[] { 1 });

        // act
        var contains1 = heap.Contains(1, out var dummy);
        var contains0 = heap.Contains(0, out _);

        // assert
        contains1.Should().BeTrue();
        dummy.Should().Be(0);
        contains0.Should().BeFalse();
    }

    [Test]
    public void TestContainsMultipleElementHeap()
    {
        // arrange
        var heap = new BinaryHeap<int>(new int[] { 3, 5, 1, 4, 2 });

        // act
        var contains1 = heap.Contains(1, out int dummy1);
        var contains2 = heap.Contains(2, out int dummy2);
        var contains3 = heap.Contains(3, out int dummy3);
        var contains4 = heap.Contains(4, out int dummy4);
        var contains5 = heap.Contains(5, out int dummy5);
        var contains6 = heap.Contains(6, out _);

        // assert
        contains1.Should().BeTrue();
        dummy1.Should().Be(0);
        contains2.Should().BeTrue();
        dummy2.Should().Be(1);
        contains3.Should().BeTrue();
        dummy3.Should().Be(2);
        contains4.Should().BeTrue();
        dummy4.Should().Be(3);
        contains5.Should().BeTrue();
        dummy5.Should().Be(4);
        contains6.Should().BeFalse();
    }

    [Test]
    public void TestMinSingleElementHeap()
    {
        // arrange
        var heap = new BinaryHeap<int>(new int[] { 1 });

        // act
        var min = heap.Min;

        // assert
        min.Should().Be(1);
    }

    [Test]
    public void TestMinMultipleElementHeap([Values(new int[] { 3, 5, 1, 4, 2 }, new int[] { 1, 2, 3, 2, 5, 7 })] int[] elements)
    {
        // arrange
        var heap = new BinaryHeap<int>(elements);

        // act
        var min = heap.Min;

        // assert
        min.Should().Be(elements.Min());
    }

    [Test]
    public void TestExtractMinSingleElementHeap()
    {
        // arrange
        var heap = new BinaryHeap<int>(new int[] { 4 });

        // act
        var initialCount = heap.Count;
        var min = heap.Min;
        var minExtracted = heap.ExtractMin();

        // assert
        min.Should().Be(minExtracted);
        min.Should().Be(4);
        initialCount.Should().Be(1);
        heap.Count.Should().Be(0);
    }

    [Test]
    public void TestExtractMinMultipleElementHeap()
    {
        // arrange
        var heap = new BinaryHeap<int>(new int[] { 3, 5, 1, 4, 2 });
        // heap tree:
        //             1
        //          2     3
        //         4 5      

        // act
        var initialCount = heap.Count;
        var min1 = heap.Min;
        var minExtracted1 = heap.ExtractMin();
        var min2 = heap.Min;
        var minExtracted2 = heap.ExtractMin();
        var min3 = heap.Min;
        var minExtracted3 = heap.ExtractMin();
        var min4 = heap.Min;
        var minExtracted4 = heap.ExtractMin();
        var min5 = heap.Min;
        var minExtracted5 = heap.ExtractMin();
        var count = heap.Count;

        // assert
        min1.Should().Be(minExtracted1);
        min1.Should().Be(1);
        min2.Should().Be(minExtracted2);
        min2.Should().Be(2);
        min3.Should().Be(minExtracted3);
        min3.Should().Be(3);
        min4.Should().Be(minExtracted4);
        min4.Should().Be(4);
        min5.Should().Be(minExtracted5);
        min5.Should().Be(5);
        initialCount.Should().Be(5);
        count.Should().Be(0);
    }

    [Test]
    public void TestTryExtractMin()
    {
        // arrange
        var heap = new BinaryHeap<int>(new int[] { 1, 3, 1 });

        // act
        var initialCount = heap.Count;
        var tryExtract1 = heap.TryExtractMin(out var min1);
        var tryExtract2 = heap.TryExtractMin(out var min2);
        var tryExtract3 = heap.TryExtractMin(out var min3);
        var tryExtract4 = heap.TryExtractMin(out var min4);
        var count = heap.Count;

        // assert
        initialCount.Should().Be(3);
        tryExtract1.Should().BeTrue();
        tryExtract2.Should().BeTrue();
        tryExtract3.Should().BeTrue();
        tryExtract4.Should().BeFalse();
        min1.Should().Be(1);
        min2.Should().Be(1);
        min3.Should().Be(3);
        min4.Should().Be(default);
        count.Should().Be(0);
    }

    [Test]
    public void TestDecreaseSingleElementHeap()
    {
        // arrange
        var heap = new BinaryHeap<int>(new int[] { 4 });

        // act, decrease single item 4 to 2
        var initialCount = heap.Count;
        var initialMin = heap.Min;
        heap.Contains(4, out var pos);
        var newPos = heap.DecreaseElement(pos, 2);
        var newMin = heap.Min;
        var newCount = heap.Count;

        // assert
        initialCount.Should().Be(1);
        initialMin.Should().Be(4);
        pos.Should().Be(0);
        newPos.Should().Be(0);
        newMin.Should().Be(2);
        newCount.Should().Be(1);
    }

    [Test]
    public void TestDecreaseMultiElementHeap()
    {
        // arrange
        var heap = new BinaryHeap<int>(new int[] { 4, 2, 3, 5, 7, 9, 1 });
        // heap tree:
        //             1
        //          2     3
        //         5 7   9 4

        // act
        var initialCount = heap.Count;
        var initialMin = heap.Min;
        heap.Contains(4, out var pos);
        var newPos = heap.DecreaseElement(pos, 2);
        var newMin = heap.Min;
        var newCount = heap.Count;

        // assert
        // heap tree:
        //             1
        //          2     2
        //         5 7   9 3
        initialCount.Should().Be(7);
        initialMin.Should().Be(1);
        pos.Should().Be(6);
        newPos.Should().Be(2);
        newMin.Should().Be(1);
        newCount.Should().Be(7);
    }

    [Test]
    public void TestDecreaseToNewMin()
    {
        // arrange
        var heap = new BinaryHeap<int>(new int[] { 4, 2, 3, 5, 7, 9, 2 });
        // heap tree:
        //             2
        //          4     2
        //         5 7   9 3

        // act
        var initialCount = heap.Count;
        var initialMin = heap.Min;
        heap.Contains(4, out var initialPos);
        var newPos = heap.DecreaseElement(initialPos, 1);
        var newMin = heap.Min;
        var newCount = heap.Count;

        // assert
        // heap tree:
        //             1
        //          2     2
        //         5 7   9 3
        initialCount.Should().Be(7);
        initialMin.Should().Be(2);
        initialPos.Should().Be(1);
        newPos.Should().Be(0);
        newMin.Should().Be(1);
        newCount.Should().Be(7);
    }

    [Test]
    public void TestDecreaseExistingMin()
    {
        // arrange
        var heap = new BinaryHeap<int>(new int[] { 6, 2, 3, 5, 4, 6 });
        // heap tree:
        //             2
        //          4     3
        //         5 6   6

        // act
        var initialCount = heap.Count;
        var initialMin = heap.Min;
        heap.Contains(2, out var initialPos);
        var newPos = heap.DecreaseElement(initialPos, 1);
        var newMin = heap.Min;
        var newCount = heap.Count;

        // assert
        // heap tree:
        //             1
        //          4      3
        //         5 6    6
        initialCount.Should().Be(6);
        initialMin.Should().Be(2);
        initialPos.Should().Be(0);
        newPos.Should().Be(0);
        newMin.Should().Be(1);
        newCount.Should().Be(6);
    }

    [Test]
    public void TestInsertOnEmptyHeap()
    {
        // arrange
        var heap = new BinaryHeap<int>(10);

        // act
        var initialCount = heap.Count;
        var insertPos3 = heap.Insert(3);
        var insertPos4 = heap.Insert(4);
        var insertPos1 = heap.Insert(1);
        var insertPos2 = heap.Insert(2);
        var min = heap.Min;
        var count = heap.Count;
        var contains1 = heap.Contains(1, out var endPos1);
        var contains2 = heap.Contains(2, out var endPos2);
        var contains3 = heap.Contains(3, out var endPos3);
        var contains4 = heap.Contains(4, out var endPos4);

        // assert
        // heap tree:
        //            1
        //          2   3
        //         4      
        initialCount.Should().Be(0);
        count.Should().Be(4);
        min.Should().Be(1);
        insertPos3.Should().Be(0);
        insertPos4.Should().Be(1);
        insertPos1.Should().Be(0);
        insertPos2.Should().Be(1);
        contains1.Should().BeTrue();
        contains2.Should().BeTrue();
        contains3.Should().BeTrue();
        contains4.Should().BeTrue();
        endPos1.Should().Be(0);
        endPos2.Should().Be(1);
        endPos3.Should().Be(2);
        endPos4.Should().Be(3);
    }

    [Test]
    public void TestInsertWithResize()
    {
        // arrange
        var heap = new BinaryHeap<int>(new[] { 6, 5, 4, 2 });
        // heap tree:
        //            2
        //          5   4
        //         6      

        // act
        var initialCount = heap.Count;
        var initialMin = heap.Min;
        var insertPos3 = heap.Insert(3);
        var insertPos1 = heap.Insert(1);
        var insertPos6 = heap.Insert(6);
        var insertPos8 = heap.Insert(8);
        var insertPos7 = heap.Insert(7);
        var count = heap.Count;
        var min = heap.Min;
        var contains1 = heap.Contains(1, out var endPos1);
        var contains2 = heap.Contains(2, out var endPos2);
        var contains3 = heap.Contains(3, out var endPos3);
        var contains4 = heap.Contains(4, out var endPos4);
        var contains5 = heap.Contains(5, out var endPos5);
        var contains6 = heap.Contains(6, out var endPos6);
        var contains7 = heap.Contains(7, out var endPos7);
        var contains8 = heap.Contains(8, out var endPos8);

        // assert
        // heap tree:
        //             1
        //          3      2
        //        6   5   4 6
        //       8 7         
        initialCount.Should().Be(4);
        initialMin.Should().Be(2);
        count.Should().Be(4 + 5);
        min.Should().Be(1);
        insertPos3.Should().Be(1);
        insertPos1.Should().Be(0);
        insertPos6.Should().Be(6);
        insertPos7.Should().Be(8);
        insertPos8.Should().Be(7);
        contains1.Should().BeTrue();
        contains2.Should().BeTrue();
        contains3.Should().BeTrue();
        contains4.Should().BeTrue();
        contains5.Should().BeTrue();
        contains6.Should().BeTrue();
        contains7.Should().BeTrue();
        contains8.Should().BeTrue();
        endPos1.Should().Be(0);
        endPos2.Should().Be(2);
        endPos3.Should().Be(1);
        endPos4.Should().Be(5);
        endPos5.Should().Be(4);
        endPos6.Should().Be(3);
        endPos7.Should().Be(8);
        endPos8.Should().Be(7);
    }

    [Test]
    public void TestSortEmptyHeap()
    {
        // arrange
        var unsorted = new int[0];

        // act
        var sorted = BinaryHeap<int>.Sort(unsorted).ToArray();

        // assert
        sorted.Length.Should().Be(0);
    }

    [Test]
    public void TestSortSingleElementHeap()
    {
        // arrange
        var unsorted = new[] { 1 };

        // act
        var sorted = BinaryHeap<int>.Sort(unsorted).ToArray();

        // assert
        sorted.Should()
              .HaveCount(1)
              .And.Contain(1);
    }

    [Test]
    public void TestSortMultipleElementHeap()
    {
        // arrange
        var unsorted = new[] { 7, 11, 532, 3, 9, 1, 44 };

        // act
        var sorted = BinaryHeap<int>.Sort(unsorted).ToArray();

        // assert
        sorted.Should()
              .HaveCount(7)
              .And.BeInAscendingOrder()
              .And.Contain(1)
              .And.Contain(3)
              .And.Contain(7)
              .And.Contain(9)
              .And.Contain(11)
              .And.Contain(44)
              .And.Contain(532);
    }

    [Test]
    public void TestSortBigHeap([Values(13, 101)] int size, [Random(1, 100, 5)] int seed)
    {
        var rand   = new Random(seed);
        var toSort = Enumerable.Range(0, size).Select(r => rand.Next()).ToArray();

        var sorted = BinaryHeap<int>.Sort(toSort).ToArray();

        sorted.Should()
              .NotBeEmpty()
              .And.HaveCount(toSort.Length)
              .And.BeInAscendingOrder()
              .And.BeEquivalentTo(toSort);
    }
}
