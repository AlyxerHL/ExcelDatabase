using System.Collections.Generic;
using System.IO;
using System.Text;
using ExcelDatabase.Editor.Tools;
using NPOI.SS.UserModel;

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

        private static readonly string TemplatePath = $"{Config.Root}/Editor/Templates/Enum";
        private static readonly string TablePath = $"{TemplatePath}/Table.txt";
        private static readonly string GroupPath = $"{TemplatePath}/Group.txt";
        private static readonly string RowPath = $"{TemplatePath}/Row.txt";

        private readonly string _tableName;
        private readonly ISheet _sheet;
        private readonly StringBuilder _builder = new();

        public EnumParser(IWorkbook workbook, string tableName)
        {
            _sheet = workbook.GetSheetAt(0);
            _tableName = tableName;
        }

        public void Parse()
        {
            ParseRows(ValidateRows());
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

        private void ParseRows(IEnumerable<Row> rows)
        {
            var tableTemplate = File.ReadAllText(TablePath);
            _builder.Append(tableTemplate).Replace(TableVariable, _tableName);
            string prevGroupValue = null;

            foreach (var row in rows)
            {
                if (prevGroupValue != row.Group)
                {
                    prevGroupValue = row.Group;
                    _builder.Replace(RowTemplate, string.Empty);
                    var groupTemplate = File.ReadAllText(GroupPath);
                    _builder.Replace(GroupTemplate, groupTemplate + GroupTemplate).Replace(GroupVariable, row.Group);
                }

                var rowTemplate = File.ReadAllText(RowPath);
                _builder.Replace(RowTemplate, rowTemplate + RowTemplate).Replace(RowVariable, row.Enum);
            }

            _builder.Replace(RowTemplate, string.Empty);
            _builder.Replace(GroupTemplate, string.Empty);
            File.WriteAllText(Config.EnumDistPath(_tableName), _builder.ToString());
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