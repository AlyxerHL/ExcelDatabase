using System.Globalization;
using NPOI.SS.UserModel;

namespace ExcelDatabase.Editor.Tools
{
    public static class TableUtility
    {
        public static string GetValue(this ICell cell)
        {
            var cellType = cell switch
            {
                null => CellType.Blank,
                _ when cell.CellType == CellType.Formula => cell.CachedFormulaResultType,
                _ => cell.CellType
            };

            return cellType switch
            {
                CellType.String => cell!.StringCellValue,
                CellType.Numeric => cell!.NumericCellValue.ToString(CultureInfo.InvariantCulture),
                CellType.Boolean => cell!.BooleanCellValue.ToString(),
                _ => string.Empty
            };
        }
    }
}