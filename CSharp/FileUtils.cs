namespace AdventOfCode2022;

public static class FileUtils
{
    public static string[] ReadAllLines<T>(T day)
    {
        return File.ReadAllLines(typeof(T).Name + ".data");
    }

    public static TResult[] ParseByLine<T, TResult>(T day, Func<string, int, TResult> converter)
    {
        return ReadAllLines(day).Select((l, i) => converter(l, i))
                                .ToArray();
    }
}
