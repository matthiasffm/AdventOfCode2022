namespace AdventOfCode2022;

public static class FileUtils
{
    public static string[] ReadAllLines(object day)
    {
        return File.ReadAllLines(day.GetType().Name.ToLower() + ".data");
    }

    public static string ReadAllText(object day)
    {
        return File.ReadAllText(day.GetType().Name.ToLower() + ".data");
    }

    public static TResult[] ParseByLine<TResult>(object day, Func<string, int, TResult> converter)
    {
        return ReadAllLines(day).Select((l, i) => converter(l, i))
                                .ToArray();
    }

    public static byte[,] ParseToMatrix(string[] lines, Func<char, int, int, byte> Converter)
    {
        byte[,] matrix = new byte[lines.Length, lines[0].Length];

        for(int row = 0; row < lines.Length; row++)
        {
            for(int col = 0; col < lines[0].Length; col++)
            {
                matrix[row, col] = Converter(lines[row][col], row, col);
            }
        }

        return matrix;
    }

    public static IEnumerable<TResult> ParseMultilinePairs<TResult>(string[] data, Func<(string, string), TResult> PairConverter)
    {
        var pairs = new List<TResult>();

        foreach(var pair in string.Join('#', data).Split("##").Select(p => p.Split('#')))
        {
            pairs.Add(PairConverter((pair[0], pair[1])));
        }

        return pairs;
    }

    public static IEnumerable<IEnumerable<TResult>> ParseMultilineTuples<TResult>(string[] data, Func<string, TResult> ElementConverter)
    {
        return string.Join('#', data)
                     .Split("##", StringSplitOptions.RemoveEmptyEntries)
                     .Select(s => s.Split('#', StringSplitOptions.RemoveEmptyEntries)
                                   .Select(elem => ElementConverter(elem)));
    }
}
