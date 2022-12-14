namespace AdventOfCode2022;

using System.Collections;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

public class Program
{
    public static void Main()
    {
        // var summary = BenchmarkRunner.Run<BenchmarkDay3>(ManualConfig.CreateMinimumViable()
        //                                                              .WithOptions(ConfigOptions.DisableOptimizationsValidator));
    }
}

// BenchmarkDotNet=v0.13.2, OS=Windows 11 (10.0.22621.819)
// .NET SDK=7.0.100
//   [Host]   : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT AVX2
//   .NET 7.0 : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT AVX2
// Job=.NET 7.0  Runtime=.NET 7.0
//
// |   Method |     Mean |   Error |  StdDev | Ratio |
// |--------- |---------:|--------:|--------:|------:|
// |  HashSet | 308.5 us | 1.31 us | 1.22 us |  1.00 |
// | BitArray | 126.4 us | 0.39 us | 0.36 us |  0.41 |
// |    Ulong | 104.8 us | 0.56 us | 0.52 us |  0.34 |

[SimpleJob(RuntimeMoniker.Net70)]
[RPlotExporter]
public class BenchmarkDay3
{
    private string[] _rucksacks = Array.Empty<string>();

    private static int Priority(char letter) => char.IsLower(letter) ? letter - 'a' + 1 : letter - 'A' + 27;

    [GlobalSetup]
    public void Setup()
    {
        _rucksacks = File.ReadAllLines(@"day3.data");
    }

    #region HashSet

    [Benchmark(Baseline = true)]
    public int HashSet()
    {
        var sumPriorities = _rucksacks.Select(r => IncorrectLetter(r))
                                      .Sum(l => Priority(l));

        var sumPriorities2 = _rucksacks.Where((r, i) => i % 3 == 0)
                                       .Zip(_rucksacks.Where((r, i) => i % 3 == 1), (r1, r2) => (r1, r2))
                                       .Zip(_rucksacks.Where((r, i) => i % 3 == 2), (r, r3) => (r.r1, r.r2, r3))
                                       .Select(r => Badge(r.r1, r.r2, r.r3))
                                       .Sum(l => Priority(l));

        return sumPriorities + sumPriorities2;
    }

    private static char IncorrectLetter(string rucksack)
    {
        var lettersMisplaced = rucksack.ToCharArray(0, rucksack.Length / 2)
                                       .Intersect(rucksack.ToCharArray(rucksack.Length / 2, rucksack.Length / 2));
        return lettersMisplaced.First();
    }

    private static char Badge(string rucksack1, string rucksack2, string rucksack3)
    {
        var badge = rucksack1.ToCharArray()
                             .Intersect(rucksack2.ToCharArray())
                             .Intersect(rucksack3.ToCharArray());
        return badge.First();
    }

    #endregion

    #region BitArray

    [Benchmark]
    public int BitArray()
    {
        var sumPriorities = _rucksacks.Sum(r => FirstSetBit(ToBitArray(r.AsSpan()[..(r.Length / 2)])
                                                            .And(
                                                            ToBitArray(r.AsSpan()[(r.Length / 2)..]))));

        var sumPriorities2 = _rucksacks.Where((r, i) => i % 3 == 0)
                                       .Zip(_rucksacks.Where((r, i) => i % 3 == 1), (r1, r2) => (r1, r2))
                                       .Zip(_rucksacks.Where((r, i) => i % 3 == 2), (r, r3) => (r.r1, r.r2, r3))
                                       .Sum(r => FirstSetBit(ToBitArray(r.r1)
                                                            .And(ToBitArray(r.r2))
                                                            .And(ToBitArray(r.r3))));

        return sumPriorities + sumPriorities2;
    }

    private static BitArray ToBitArray(ReadOnlySpan<char> letters)
    {
        var arr = new BitArray(52 + 1);

        foreach(var l in letters)
        {
            arr.Set(Priority(l), true);
        }

        return arr;
    }

    // finds the index of the first set bit in bits (should be only one here in this puzzle)
    private static int FirstSetBit(BitArray bits)
    {
        for(int i = 0; i < bits.Length; i++)
        {
            if(bits[i])
            {
                return i;
            }
        }
        return -1;
    }

    #endregion

    #region unsigned long

    [Benchmark]
    public int Ulong()
    {
        var sumPriorities = _rucksacks.Sum(r => FirstSetBit(ToUlong(r.AsSpan()[..(r.Length / 2)])
                                                            &
                                                            ToUlong(r.AsSpan().Slice(r.Length / 2, r.Length / 2 ))));

        var sumPriorities2 = _rucksacks.Where((r, i) => i % 3 == 0)
                                       .Zip(_rucksacks.Where((r, i) => i % 3 == 1), (r1, r2) => (r1, r2))
                                       .Zip(_rucksacks.Where((r, i) => i % 3 == 2), (r, r3) => (r.r1, r.r2, r3))
                                       .Sum(r => FirstSetBit(ToUlong(r.r1)
                                                             & ToUlong(r.r2)
                                                             & ToUlong(r.r3)));

        return sumPriorities + sumPriorities2;
    }

    private static ulong ToUlong(ReadOnlySpan<char> letters)
    {
        ulong bits = 0L;

        foreach(var l in letters)
        {
            bits |= (ulong)(1L << Priority(l));
        }

        return bits;
    }

    private static int FirstSetBit(ulong bits)
    {
        for(int i = 0; i < 60; i++)
        {
            if((bits & (ulong)(1L << i)) != 0)
            {
                return i;
            }
        }

        return -1;
    }

    #endregion
}