namespace AdventOfCode2022;

using NUnit.Framework;
using FluentAssertions;

[TestFixture]
public partial class Day25
{
    [Test]
    public void TestSamples()
    {
        var data = new [] {
            "1=-0-2",
            "12111",
            "2=0=",
            "21",
            "2=01",
            "111",
            "20012",
            "112",
            "1=-1=",
            "1-12",
            "12",
            "1=",
            "122",
        };
        InitPowTable(data.Max(d => d.Length));

        ToDecimal("12111").Should().Be(906);
        ToDecimal("1=").Should().Be(3);
        ToDecimal("1=-1=").Should().Be(353);
        ToDecimal("1=-0-2").Should().Be(1747);
        ToDecimal("1001=").Should().Be(628);
        ToDecimal("1000-").Should().Be(624);

        ToQuinary(3).Should().Be("1=");
        ToQuinary(1076).Should().Be("2-=01");
        ToQuinary(353).Should().Be("1=-1=");
        ToQuinary(1747).Should().Be("1=-0-2");
        ToQuinary(4890).Should().Be("2=-1=0");
        ToQuinary(906).Should().Be("12111");
        ToQuinary(628).Should().Be("1001=");
        ToQuinary(624).Should().Be("1000-");

        Puzzle(data).Should().Be("2=-1=0");
    }

    [Test]
    public void TestAocInput()
    {
        var data = FileUtils.ReadAllLines(this);
        InitPowTable(data.Max(d => d.Length));

        Puzzle(data).Should().Be("2--1=0=-210-1=00=-=1");
    }

    // To heat the fuel of the hot air balloon, Bob needs to know the total amount of fuel that will be processed ahead of time so it can correctly calibrate
    // heat output and flow rate. This amount is simply the sum of the fuel requirements of all of the hot air balloons, and those fuel requirements are even
    // listed clearly on the side of each hot air balloon's burner in Special Numeral-Analogue Fuel Units - SNAFU for short.
    // Instead of using digits zero through four, the SNAFU system uses digits 2, 1, 0, minus (written -), and double-minus (written =). Minus is
    // worth -1, and double-minus is worth -2.
    // 
    // Puzzle == The Elves are starting to get cold. What SNAFU number do you supply to Bob's console for the sum of the fuel requirements of all balloons?
    private static string Puzzle(IEnumerable<string> quinaryNumbers)
    {
        return ToQuinary(quinaryNumbers.Select(s => ToDecimal(s))
                                       .Sum());
    }

    // there is no Math.Pow(long, long) in C#
    static long[] Pow5Table = Array.Empty<long>();

    private static void InitPowTable(int max)
    {
        Pow5Table = new long[max];

        Pow5Table[0] = 1;

        for(int i = 1; i < max; i++)
        {
            Pow5Table[i] = 5L * Pow5Table[i - 1];
        }
    }

    private static long ToDecimal(string quinaryNumber)
        => Enumerable.Range(0, quinaryNumber.Length)
                     .Select(i => ToDecimal(quinaryNumber[i], quinaryNumber.Length - i - 1))
                     .Sum();

    private static long ToDecimal(char quinaryDigit, int exp)
        => Pow5Table[exp] * (quinaryDigit switch {
            '-' => -1,
            '=' => -2,
            _   => quinaryDigit - '0',
        });

    private static string ToQuinary(long decimalNumber)
    {
        var quinaryDigits = new[] { '=', '-', '0', '1', '2' };
        var quinaryNumber = new System.Text.StringBuilder();

        while(decimalNumber > 0)
        {
            decimalNumber += 2;
            var rest = decimalNumber % 5;

            quinaryNumber.Insert(0, quinaryDigits[rest]);

            decimalNumber = (decimalNumber - rest) / 5;
        }

        return quinaryNumber.ToString();
    }
}
