using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using ExcelDatabase.Editor.Library;
using NPOI.SS.UserModel;

namespace ExcelDatabase.Editor.Parser
{
    public static class ParseUtility
    {
        public static readonly Dictionary<string, Func<string, bool>> TypeValidators =
            new()
            {
                { "string", (_) => true },
                { "int", (value) => int.TryParse(value, out _) },
                { "float", (value) => float.TryParse(value, out _) },
                { "bool", (value) => bool.TryParse(value, out _) }
            };

        public static string GetCellValue(this IRow row, int index)
        {
            var cell = row.GetCell(index);
            return cell?.CellType switch
            {
                CellType.String => cell.StringCellValue,
                CellType.Formula => cell.StringCellValue,
                CellType.Numeric => cell.NumericCellValue.ToString(),
                CellType.Boolean => cell.BooleanCellValue.ToString(),
                _ => string.Empty,
            };
        }

        public static string Format(string value)
        {
            return Regex.Replace(value, "[^a-zA-Z0-9.#]+", string.Empty);
        }

        public static string WriteScript(TableType type, string tableName, string script)
        {
            var distDirectory = $"{Config.DistPath}/{type}";
            if (!Directory.Exists(distDirectory))
            {
                Directory.CreateDirectory(distDirectory);
            }

            var distPath = $"{distDirectory}/{tableName}.cs";
            File.WriteAllText(distPath, script);
            return distPath;
        }
    }
}
