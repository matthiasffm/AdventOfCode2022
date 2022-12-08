namespace AdventOfCode2022;

public static class MatrixExtensions
{
    public static IEnumerable<(int, T)> Row<T>(this T[,] matrix, int row)
    {
        for(int col = 0; col < matrix.GetLength(1); col++)
        {
            yield return (col, matrix[row, col]);
        }
    }

    public static IEnumerable<(int, T)> Col<T>(this T[,] matrix, int col)
    {
        for(int row = 0; row < matrix.GetLength(0); row++)
        {
            yield return (row, matrix[row, col]);
        }
    }
}
