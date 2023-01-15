using System.Globalization;
using NPOI.SS.UserModel;

namespace ExcelDatabase.Editor.Tools
{
    public class TableHandler
    {
        private readonly string[,] _table;

        public TableHandler(IWorkbook workbook)
        {
            var sheet = workbook.GetSheetAt(0);
            _table = new string[sheet.LastRowNum + 1, sheet.GetRow(0).LastCellNum + 1];

            for (var i = 0; i < _table.GetLength(0); i++)
            {
                var row = sheet.GetRow(i);
                for (var j = 0; j < _table.GetLength(1); j++)
                {
                    var cell = row.GetCell(j);
                    _table[i, j] = GetCellValue(cell);
                }
            }
        }

        private static string GetCellValue(ICell cell)
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