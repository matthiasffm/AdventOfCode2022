namespace AdventOfCode2022;

public static class EnumerableExtensions
{
    public static void For<T>(this IEnumerable<T> coll, Action<T> action)
    {
        foreach(var elem in coll)
        {
            action(elem);
        }
    }
    public static IEnumerable<(T, T)> Variations<T>(this IEnumerable<T> collLeft, IEnumerable<T> collRight)
    {
        return collLeft.SelectMany(leftItem => collRight.Select(rightItem => (leftItem, rightItem)));
    }
}
