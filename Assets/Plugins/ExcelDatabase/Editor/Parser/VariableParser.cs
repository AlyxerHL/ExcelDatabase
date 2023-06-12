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
    public class VariableParser
    {
        private const int NameCol = 0;
        private const int TypeCol = 1;
        private const int ValueCol = 2;
        private const int NameRow = 0;

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
            File.WriteAllText(Config.DistPath(tableName, TableType.Variable), BuildScript(rows));
            return new ParseResult(TableType.Variable, tableName, excelPath);
        }

        private IEnumerable<Row> ValidateRows()
        {
            var nameRow = sheet.GetRow(NameRow);
            if (
                nameRow?.GetCellValue(NameCol) != "VariableName"
                || nameRow.GetCellValue(TypeCol) != "DataType"
                || nameRow.GetCellValue(ValueCol) != "Value"
            )
            {
                throw new ParserException(tableName, "Invalid column name");
            }

            var diffChecker = new HashSet<string>();
            for (var i = NameRow + 1; i <= sheet.LastRowNum; i++)
            {
                var poiRow = sheet.GetRow(i);
                if (poiRow is null)
                {
                    break;
                }

                var row = new Row(
                    poiRow.GetCellValue(NameCol),
                    poiRow.GetCellValue(TypeCol),
                    poiRow.GetCellValue(ValueCol)
                );

                if (row.name?.Length == 0)
                {
                    break;
                }

                if (char.IsDigit(row.name, 0))
                {
                    throw new ParserException(
                        tableName,
                        $"Variable name '{row.name}' starts with a number"
                    );
                }

                if (!diffChecker.Add(row.name))
                {
                    throw new ParserException(tableName, $"Duplicate variable name '{row.name}'");
                }

                if (!ParseUtility.typeValidators.ContainsKey(row.type))
                {
                    throw new ParserException(tableName, $"Invalid variable type '{row.type}'");
                }

                if (!ParseUtility.typeValidators[row.type](row.value))
                {
                    throw new ParserException(
                        tableName,
                        $"Variable value '{row.value}' is not of variable type '{row.type}'"
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
                    .Replace(TypeVariable, row.type)
                    .Replace(NameVariable, row.name)
                    .Replace(
                        ValueVariable,
                        row.type switch
                        {
                            "float" => row.value + 'f',
                            "bool" => row.value.ToLower(),
                            _ => row.value
                        }
                    );
            }

            builder.Replace(RowTemplate, string.Empty);
            return builder.ToString();
        }

        private readonly struct Row
        {
            public string name { get; }
            public string type { get; }
            public string value { get; }

            public Row(string name, string type, string value)
            {
                this.name = ParseUtility.Format(name);
                this.type = type;
                this.value = value;
            }
        }
    }
}
