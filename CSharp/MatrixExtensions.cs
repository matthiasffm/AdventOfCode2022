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

    /// <summary>
    /// Zählt alle Elemente der Matrix die eine Bedingung erfüllen.
    /// </summary>
    /// <typeparam name="TSource">Elementtyp der Matrix</typeparam>
    /// <param name="matrix">Matrix deren Elemente spalten- und dann zeilenweise iteriert werden.</param>
    /// <param name="predicate">die Entscheidungsfunktion wird für jedes Element der Matrix aufgerufen</param>
    /// <returns>Anzahl an Elementen in der Matrix, für die <paramref name="predicate"/> <i>true</i> liefert.</returns>
    public static int Count<TSource>(this TSource[,] matrix, Func<TSource, int, int, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(matrix);
        ArgumentNullException.ThrowIfNull(predicate);

        var count = 0;

        for(int i = 0; i < matrix.GetLength(0); i++)
        {
            for(int j = 0; j < matrix.GetLength(1); j++)
            {
                if(predicate(matrix[i, j], i, j))
                {
                    count++;
                }
            }
        }

        return count;
    }
}
