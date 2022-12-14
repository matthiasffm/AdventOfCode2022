namespace AdventOfCode2022;

public static class StringExtensions
{
    public static IEnumerable<string> Tokenize(this string text,
                                               string[] separators,
                                               string[] tokens,
                                               StringSplitOptions splitOptions = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
    {
        var result = new List<string>();

        foreach(var s in text.Split(separators, splitOptions))
        {
            for(int i = 0; i < s.Length;)
            {
                var (tokenFound, idx) = tokens.Select(t => (token: t, idx: s.AsSpan()[i..].IndexOf(t)))
                                              .Select(t => t.idx >= 0 ? t : (t.token, idx: int.MaxValue))
                                              .MinBy(tuple => tuple.idx);

                if(idx == int.MaxValue)
                {
                    result.Add(s[i..]);
                    i = s.Length;
                }
                else if(idx == 0)
                {
                    result.Add(tokenFound);
                    i += tokenFound.Length;
                }
                else
                {
                    result.Add(s.AsSpan()[i..(i + idx)].ToString());
                    result.Add(tokenFound);
                    i += idx + tokenFound.Length;
                }
            }
        }

        return result;
    }
}
