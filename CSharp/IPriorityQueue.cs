using System.Diagnostics.CodeAnalysis;

namespace AdventOfCode2022;

/// <summary>
/// Interface of all minimum queues where enqueue and dequeue are ordered by a priority value and the first element is the one
/// with the lowest priority value.
/// </summary>
public interface IPriorityQueue<TElement, TPos>
{
    int Count { get; }
    bool Contains(TElement toFind, out TPos pos);

    TElement Min { get; }
    TElement ExtractMin();
    bool TryExtractMin([MaybeNullWhen(false)] out TElement element);

    TPos DecreaseElement(TPos pos, TElement newElement);

    TPos Insert(TElement element);
    void Clear();
}
