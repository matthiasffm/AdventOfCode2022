using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using static matthiasffm.Common.Algorithms.Basics;

namespace AdventOfCode2022;

/// <summary>
/// Implements a mininum heap in the form of a minimum binary tree where every
/// child element has a greater or equal value than its parent (heap property).
/// </summary>
public class BinaryHeap<TElement> : IPriorityQueue<TElement, int>
{
    private readonly IComparer<TElement> _comparer;

    private TElement[] _elems;
    private int        _last = -1;

    public BinaryHeap(int capacity = 10)
    {
        _elems = new TElement[capacity];

        _comparer = Comparer<TElement>.Default;
    }

    public BinaryHeap(int capacity, IComparer<TElement> comparer)
    {
        _elems = new TElement[capacity];

        _comparer = comparer;
    }

    public BinaryHeap(IEnumerable<TElement> items)
    {
        _elems = items.ToArray();

        _last     = _elems.Length - 1;
        _comparer = Comparer<TElement>.Default;

        BuildHeap();
    }

    public BinaryHeap(IEnumerable<TElement> items, IComparer<TElement> comparer)
    {
        _elems = items.ToArray();

        _last     = _elems.Length - 1;
        _comparer = comparer;

        BuildHeap();
    }

    public int Count => _last + 1;

    /// <remarks>runs in O(n)</remarks>
    public bool Contains(TElement toFind, out int pos)
    {
        for(pos = 0; pos <= _last; pos++)
        {
            if(object.Equals(_elems[pos], toFind))
            {
                return true;
            }
        }

        return false;
    }

    public TElement Min
    {
        get
        {
            if(_last >= 0)
            {
                return _elems[0];
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
        }
    }

    /// <summary>
    /// removes the minimum element from the heap
    /// does this by swapping the first and last element end restoring the minimum heap property. 
    /// </summary>
    /// <remarks>runs in O(log n)</remarks>
    public TElement ExtractMin()
    {
        if(_last >= 0)
        {
            var min = _elems[0];

            Swap(ref _elems[0], ref _elems[_last]);
            _last--;

            if(_last > 0)
            {
                Heapify(0);
                TestInvariant();
            }

            return min;
        }
        else
        {
            throw new IndexOutOfRangeException();
        }
    }

    public bool TryExtractMin([MaybeNullWhen(false)] out TElement element)
    {
        if(_last >= 0)
        {
            element = ExtractMin();
            return true;
        }
        else
        {
            element = default;
            return false;
        }
    }

    public int DecreaseElement(int pos, TElement newElem)
    {
        if(_last >= 0 || pos >= _last)
        {
            if (_comparer.Compare(newElem, _elems[pos]) > 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            return DecreaseInternal(pos, newElem);
        }
        else
        {
            throw new IndexOutOfRangeException();
        }
    }

    private int DecreaseInternal(int pos, TElement newElem)
    {
        _elems[pos] = newElem;

        while(pos > 0 && Parent(pos) is var parent && _comparer.Compare(_elems[parent], _elems[pos]) > 0)
        {
            Swap(ref _elems[parent], ref _elems[pos]);
            pos = parent;
        }

        TestInvariant();

        return pos;
    }

    public int Insert(TElement element)
    {
        ResizeInternalHeap(_last + 1);

        _last++;
        return DecreaseInternal(_last, element);
    }

    public void Clear()
    {
        _last = -1;
    }

    public static IEnumerable<TElement> Sort(IEnumerable<TElement> items)
    {
        var heap = new BinaryHeap<TElement>(items);
        while(heap.Count > 0)
        {
            yield return heap.ExtractMin();
        }
    }

    private int Parent(int idx) => (idx + 1) / 2 - 1;
    private int Left(int idx)   => idx * 2 + 1;
    private int Right(int idx)  => idx * 2 + 2;

    /// <summary>
    /// maintains the minimum heap property after node i was changed
    /// starts at node i and swaps child nodes so long til the min heap property is restored
    /// </summary>
    /// <remarks>runs in O(log n)</remarks>
    private void Heapify(int i)
    {
        int smallest;

        var left = Left(i);
        if((left <= _last) && (_comparer.Compare(_elems[left], _elems[i]) < 0))
        {
            smallest = left;
        }
        else
        {
            smallest = i;
        }

        var right = Right(i);
        if((right <= _last) && (_comparer.Compare(_elems[right], _elems[smallest]) < 0))
        {
            smallest = right;
        }

        if(smallest != i)
        {
            Swap(ref _elems[i], ref _elems[smallest]);
            Heapify(smallest);
        }

        TestInvariant(i);
    }

    /// <summar>
    /// Reorders all items in _elems so that the resulting array is a minimum heap.
    /// To do this just ensure the heap property from the bottom up to the top.
    /// </summary>
    /// <remarks>runs in O(n)</remarks>
    private void BuildHeap()
    {
        for(int i = _last / 2; i >= 0; i--)
        {
            Heapify(i);
        }

        TestInvariant();
    }

    private void ResizeInternalHeap(int newCapacity)
    {
        if(newCapacity >= _elems.Length)
        {
            var newArray = new TElement[Math.Max(newCapacity, _elems.Length * 2)];
            _elems.CopyTo(newArray, 0);
            _elems = newArray;

            TestInvariant();
        }
    }

    /// <summary>
    /// Checks the heap property for all elements of the binary tree starting at index idx.
    /// </summary>
    /// <remarks>
    /// Heap property: parent element <= left child element and parent element <= right child element
    /// </remarks>
    /// <param name="idx">if idx = 0 the whole heap will be checked</param>
    [Conditional("DEBUG")]
    private void TestInvariant(int idx = 0)
    {
        if(idx <= _last)
        {
            var parent = _elems[idx];

            if(Left(idx) <= _last)
            {
                Debug.Assert(_comparer.Compare(parent, _elems[Left(idx)]) <= 0);
                TestInvariant(Left(idx));
            }

            if(Right(idx) <= _last)
            {
                Debug.Assert(_comparer.Compare(parent, _elems[Right(idx)]) <= 0);
                TestInvariant(Right(idx));
            }
        }
    }
}
