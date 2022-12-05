namespace AdventOfCode2022;

public static class EnumerableExtension
{
    public static void For<T>(this IEnumerable<T> coll, Action<T> action)
    {
        foreach(var elem in coll)
        {
            action(elem);
        }
    }
}
