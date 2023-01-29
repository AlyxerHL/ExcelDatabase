using System.Collections.Generic;
using System.IO;
using System.Text;
using ExcelDatabase.Editor.Tools;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using UnityEditor;
using UnityEngine;

namespace ExcelDatabase.Editor.Parser
{
    public class EnumParser
    {
        private const int GroupColumn = 0;
        private const int EnumColumn = 1;

        private const string GroupTemplate = "#GROUP#";
        private const string RowTemplate = "#ROW#";
        private const string TableVariable = "$TABLE$";
        private const string GroupVariable = "$GROUP$";
        private const string RowVariable = "$ROW$";

        private static readonly string TablePath = $"{Config.TemplatePath}/Enum/Table.txt";
        private static readonly string GroupPath = $"{Config.TemplatePath}/Enum/Group.txt";
        private static readonly string RowPath = $"{Config.TemplatePath}/Enum/Row.txt";

        private readonly ISheet _sheet;
        private readonly string _tableName;
        private readonly string _excelPath;

        public EnumParser(Object file)
        {
            var path = AssetDatabase.GetAssetPath(file);
            using var stream = File.Open(path, FileMode.Open, FileAccess.Read);
            _sheet = new XSSFWorkbook(stream).GetSheetAt(0);
            _tableName = file.name;
            _excelPath = AssetDatabase.GetAssetPath(file);
        }

        public ParseResult Parse()
        {
            var rows = ValidateRows();
            var script = BuildScript(rows);
            var distPaths = WriteScript(script);
            return new ParseResult(TableType.Enum, _tableName, _excelPath, distPaths);
        }

        private IEnumerable<Row> ValidateRows()
        {
            var diffChecker = new HashSet<string>();
            for (var i = 1; i <= _sheet.LastRowNum; i++)
            {
                var row = _sheet.GetRow(i);
                var groupValue = row.GetCell(GroupColumn).GetValue();
                var enumValue = row.GetCell(EnumColumn).GetValue();

                if (groupValue == string.Empty)
                {
                    break;
                }

                if (char.IsDigit(groupValue, 0))
                {
                    throw new InvalidTableException(_tableName, $"Enum group '{groupValue}' starts with a number");
                }

                if (enumValue == string.Empty)
                {
                    throw new InvalidTableException(_tableName, $"Enum value in group '{groupValue}' is empty");
                }

                if (char.IsDigit(enumValue, 0))
                {
                    throw new InvalidTableException(_tableName,
                        $"Enum value '{enumValue}' in group '{groupValue}' starts with a number");
                }

                if (!diffChecker.Add(groupValue + enumValue))
                {
                    throw new InvalidTableException(_tableName,
                        $"Duplicate enum value '{enumValue}' in group '{groupValue}'");
                }

                yield return new Row(groupValue, enumValue);
            }
        }

        private string BuildScript(IEnumerable<Row> rows)
        {
            var tableTemplate = File.ReadAllText(TablePath);
            var builder = new StringBuilder(tableTemplate).Replace(TableVariable, _tableName);
            string prevGroupValue = null;

            foreach (var row in rows)
            {
                if (prevGroupValue != row.Group)
                {
                    prevGroupValue = row.Group;
                    builder.Replace(RowTemplate, string.Empty);
                    var groupTemplate = File.ReadAllText(GroupPath);
                    builder.Replace(GroupTemplate, groupTemplate + GroupTemplate).Replace(GroupVariable, row.Group);
                }

                var rowTemplate = File.ReadAllText(RowPath);
                builder.Replace(RowTemplate, rowTemplate + RowTemplate).Replace(RowVariable, row.Enum);
            }

            builder.Replace(RowTemplate, string.Empty);
            builder.Replace(GroupTemplate, string.Empty);
            return builder.ToString();
        }

        private string[] WriteScript(string script)
        {
            var distDirectory = $"{Config.DistPath}/Enum";
            if (!Directory.Exists(distDirectory))
            {
                Directory.CreateDirectory(distDirectory);
            }

            var distPath = $"{distDirectory}/{_tableName}.cs";
            File.WriteAllText(distPath, script);
            return new[] { distPath };
        }

        private readonly struct Row
        {
            public readonly string Group;
            public readonly string Enum;

            public Row(string groupValue, string enumValue)
            {
                Group = groupValue;
                Enum = enumValue;
            }
        }
    }
}