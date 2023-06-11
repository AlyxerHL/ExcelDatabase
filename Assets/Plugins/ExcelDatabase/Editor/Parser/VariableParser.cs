using System.Collections.Generic;
using System.IO;
using System.Text;
using ExcelDatabase.Editor.Library;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using UnityEditor;
using Object = UnityEngine.Object;

namespace ExcelDatabase.Editor.Parser
{
    public class VariableParser : IParser
    {
        private const int NameCol = 0;
        private const int TypeCol = 1;
        private const int ValueCol = 2;

        private const string RowTemplate = "#ROW#";
        private const string TableVariable = "$TABLE$";
        private const string TypeVariable = "$TYPE$";
        private const string NameVariable = "$NAME$";
        private const string ValueVariable = "$VALUE$";

        private static readonly string tablePath = $"{Config.templatePath}/Variable/Table.txt";
        private static readonly string rowPath = $"{Config.templatePath}/Variable/Row.txt";

        private readonly ISheet sheet;
        private readonly string tableName;
        private readonly string excelPath;

        public VariableParser(Object file)
        {
            var path = AssetDatabase.GetAssetPath(file);
            using var stream = File.Open(path, FileMode.Open, FileAccess.Read);
            sheet = new XSSFWorkbook(stream).GetSheetAt(0);
            tableName = ParseUtility.Format(file.name);
            excelPath = AssetDatabase.GetAssetPath(file);
        }

        public ParseResult Parse()
        {
            var rows = ValidateRows();
            var script = BuildScript(rows);
            var distPath = ParseUtility.WriteScript(TableType.Variable, tableName, script);
            return new ParseResult(TableType.Variable, tableName, excelPath, new[] { distPath });
        }

        private IEnumerable<Row> ValidateRows()
        {
            var firstRow = sheet.GetRow(0);
            if (
                firstRow?.GetCellValue(NameCol) != "VariableName"
                || firstRow.GetCellValue(TypeCol) != "DataType"
                || firstRow.GetCellValue(ValueCol) != "Value"
            )
            {
                throw new ParserException(tableName, "Invalid column name");
            }

            var diffChecker = new HashSet<string>();
            for (var i = 1; i <= sheet.LastRowNum; i++)
            {
                var poiRow = sheet.GetRow(i);
                if (poiRow == null)
                {
                    break;
                }

                var row = new Row(
                    poiRow.GetCellValue(NameCol),
                    poiRow.GetCellValue(TypeCol),
                    poiRow.GetCellValue(ValueCol)
                );

                if (row.Name?.Length == 0)
                {
                    break;
                }

                if (char.IsDigit(row.Name, 0))
                {
                    throw new ParserException(
                        tableName,
                        $"Variable name '{row.Name}' starts with a number"
                    );
                }

                if (!diffChecker.Add(row.Name))
                {
                    throw new ParserException(tableName, $"Duplicate variable name '{row.Name}'");
                }

                if (!ParseUtility.typeValidators.ContainsKey(row.Type))
                {
                    throw new ParserException(tableName, $"Invalid variable type '{row.Type}'");
                }

                if (!ParseUtility.typeValidators[row.Type](row.Value))
                {
                    throw new ParserException(
                        tableName,
                        $"Variable value '{row.Value}' is not of variable type '{row.Type}'"
                    );
                }

                yield return row;
            }
        }

        private string BuildScript(IEnumerable<Row> rows)
        {
            var tableTemplate = File.ReadAllText(tablePath);
            var rowTemplate = File.ReadAllText(rowPath);
            var builder = new StringBuilder(tableTemplate).Replace(TableVariable, tableName);

            foreach (var row in rows)
            {
                builder
                    .Replace(RowTemplate, rowTemplate + RowTemplate)
                    .Replace(TypeVariable, row.Type)
                    .Replace(NameVariable, row.Name)
                    .Replace(
                        ValueVariable,
                        row.Type switch
                        {
                            "float" => row.Value + 'f',
                            "bool" => row.Value.ToLower(),
                            _ => row.Value
                        }
                    );
            }

            builder.Replace(RowTemplate, string.Empty);
            return builder.ToString();
        }

        private readonly struct Row
        {
            public readonly string Name;
            public readonly string Type;
            public readonly string Value;

            public Row(string name, string type, string value)
            {
                Name = ParseUtility.Format(name);
                Type = type;
                Value = value;
            }
        }
    }
}
