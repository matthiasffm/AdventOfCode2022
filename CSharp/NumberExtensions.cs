namespace AdventOfCode2022;

public static class NumberExtensions
{
    public static int Mod(this int nmbr, int mod)
    {
        var res = nmbr % mod;
        return (res < 0 && mod > 0) || (res > 0 && mod < 0) ? res + mod : res;
    }

    public static long Mod(this long nmbr, long mod)
    {
        var res = nmbr % mod;
        return (res < 0L && mod > 0L) || (res > 0L && mod < 0L) ? res + mod : res;
    }
}
