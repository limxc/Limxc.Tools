using System.Linq;

namespace Limxc.Tools.Extensions
{
    public static class MatrixExtension
    {
        public static T[] GetColumn<T>(this T[,] matrix, int columnNumber)
        {
            return Enumerable
                .Range(0, matrix.GetLength(0))
                .Select(x => matrix[x, columnNumber])
                .ToArray();
        }

        public static T[] GetRow<T>(this T[,] matrix, int rowNumber)
        {
            return Enumerable
                .Range(0, matrix.GetLength(1))
                .Select(x => matrix[rowNumber, x])
                .ToArray();
        }
    }
}
