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
    public class EnumParser
    {
        private const int GroupCol = 0;
        private const int EnumCol = 1;
        private const int NameRow = 0;

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
            tableName = TableParser.Format(file.name);
            excelPath = AssetDatabase.GetAssetPath(file);
        }

        public Library.TableParser.Result Parse()
        {
            var rows = ValidateRows();
            File.WriteAllText(Config.DistPath(tableName, TableType.Enum), BuildScript(rows));
            return new Library.TableParser.Result(TableType.Enum, tableName, excelPath);
        }

        private IEnumerable<Row> ValidateRows()
        {
            var nameRow = sheet.GetRow(NameRow);
            if (
                nameRow?.GetCellValue(GroupCol) != "EnumGroup"
                || nameRow.GetCellValue(EnumCol) != "Enum"
            )
            {
                throw new Library.TableParser.Exception(tableName, "Invalid column name");
            }

            var diffChecker = new HashSet<string>();
            for (var i = NameRow + 1; i <= sheet.LastRowNum; i++)
            {
                var poiRow = sheet.GetRow(i);
                if (poiRow is null)
                {
                    break;
                }

                var row = new Row(poiRow.GetCellValue(GroupCol), poiRow.GetCellValue(EnumCol));

                if (row.group?.Length == 0)
                {
                    break;
                }

                if (char.IsDigit(row.group, 0))
                {
                    throw new Library.TableParser.Exception(
                        tableName,
                        $"Enum group '{row.group}' starts with a number"
                    );
                }

                if (row.enumName?.Length == 0)
                {
                    throw new Library.TableParser.Exception(
                        tableName,
                        $"Enum value in group '{row.group}' is empty"
                    );
                }

                if (char.IsDigit(row.enumName, 0))
                {
                    throw new Library.TableParser.Exception(
                        tableName,
                        $"Enum value '{row.enumName}' in group '{row.group}' starts with a number"
                    );
                }

                if (!diffChecker.Add(row.group + row.enumName))
                {
                    throw new Library.TableParser.Exception(
                        tableName,
                        $"Duplicate enum value '{row.enumName}' in group '{row.group}'"
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
                if (prevGroupValue != row.group)
                {
                    prevGroupValue = row.group;
                    builder.Replace(RowTemplate, string.Empty);
                    var groupTemplate = File.ReadAllText(groupPath);
                    builder
                        .Replace(GroupTemplate, groupTemplate + GroupTemplate)
                        .Replace(GroupVariable, row.group);
                }

                builder
                    .Replace(RowTemplate, rowTemplate + RowTemplate)
                    .Replace(RowVariable, row.enumName);
            }

            builder.Replace(RowTemplate, string.Empty);
            builder.Replace(GroupTemplate, string.Empty);
            return builder.ToString();
        }

        private readonly struct Row
        {
            public string group { get; }
            public string enumName { get; }

            public Row(string group, string enumName)
            {
                this.group = TableParser.Format(group);
                this.enumName = TableParser.Format(enumName);
            }
        }
    }
}
