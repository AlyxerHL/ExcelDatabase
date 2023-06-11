using System.Collections.Generic;
using System.IO;
using System.Text;
using ExcelDatabase.Editor.Library;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using UnityEditor;
using UnityEngine;

namespace ExcelDatabase.Editor.Parser
{
    public class EnumParser : IParser
    {
        private const int GroupCol = 0;
        private const int EnumCol = 1;

        private const string GroupTemplate = "#GROUP#";
        private const string RowTemplate = "#ROW#";
        private const string TableVariable = "$TABLE$";
        private const string GroupVariable = "$GROUP$";
        private const string RowVariable = "$ROW$";

        private static readonly string tablePath = $"{Config.templatePath}/Enum/Table.txt";
        private static readonly string groupPath = $"{Config.templatePath}/Enum/Group.txt";
        private static readonly string rowPath = $"{Config.templatePath}/Enum/Row.txt";

        private readonly ISheet sheet;
        private readonly string tableName;
        private readonly string excelPath;

        public EnumParser(Object file)
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
            var distPath = ParseUtility.WriteScript(TableType.Enum, tableName, script);
            return new ParseResult(TableType.Enum, tableName, excelPath, new[] { distPath });
        }

        private IEnumerable<Row> ValidateRows()
        {
            var firstRow = sheet.GetRow(0);
            if (
                firstRow?.GetCellValue(GroupCol) != "EnumGroup"
                || firstRow.GetCellValue(EnumCol) != "Enum"
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

                var row = new Row(poiRow.GetCellValue(GroupCol), poiRow.GetCellValue(EnumCol));

                if (row.Group?.Length == 0)
                {
                    break;
                }

                if (char.IsDigit(row.Group, 0))
                {
                    throw new ParserException(
                        tableName,
                        $"Enum group '{row.Group}' starts with a number"
                    );
                }

                if (row.Enum?.Length == 0)
                {
                    throw new ParserException(
                        tableName,
                        $"Enum value in group '{row.Group}' is empty"
                    );
                }

                if (char.IsDigit(row.Enum, 0))
                {
                    throw new ParserException(
                        tableName,
                        $"Enum value '{row.Enum}' in group '{row.Group}' starts with a number"
                    );
                }

                if (!diffChecker.Add(row.Group + row.Enum))
                {
                    throw new ParserException(
                        tableName,
                        $"Duplicate enum value '{row.Enum}' in group '{row.Group}'"
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
            string prevGroupValue = null;

            foreach (var row in rows)
            {
                if (prevGroupValue != row.Group)
                {
                    prevGroupValue = row.Group;
                    builder.Replace(RowTemplate, string.Empty);
                    var groupTemplate = File.ReadAllText(groupPath);
                    builder
                        .Replace(GroupTemplate, groupTemplate + GroupTemplate)
                        .Replace(GroupVariable, row.Group);
                }

                builder
                    .Replace(RowTemplate, rowTemplate + RowTemplate)
                    .Replace(RowVariable, row.Enum);
            }

            builder.Replace(RowTemplate, string.Empty);
            builder.Replace(GroupTemplate, string.Empty);
            return builder.ToString();
        }

        private readonly struct Row
        {
            public readonly string Group;
            public readonly string Enum;

            public Row(string group, string @enum)
            {
                Group = ParseUtility.Format(group);
                Enum = ParseUtility.Format(@enum);
            }
        }
    }
}
