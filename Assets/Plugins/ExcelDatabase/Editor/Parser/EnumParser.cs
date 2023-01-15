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

        private readonly StringBuilder _builder;
        private readonly ISheet _sheet;
        private readonly string _tableName;

        public EnumParser(IWorkbook workbook, string tableName)
        {
            var tableTemplate = File.ReadAllText(TablePath);
            _builder = new StringBuilder(tableTemplate).Replace(TableVariable, tableName);
            _sheet = workbook.GetSheetAt(0);
            _tableName = tableName;
        }

        public void Parse()
        {
            ParseRows(ValidateRows());
        }

        private IEnumerable<(string, string)> ValidateRows()
        {
            var diffChecker = new HashSet<string>();
            for (var i = 1; i <= _sheet.LastRowNum; i++)
            {
                var row = _sheet.GetRow(i);
                var groupValue = row.GetCell(GroupColumn).GetValue();
                var enumValue = row.GetCell(EnumColumn).GetValue();

                if (groupValue == string.Empty)
                {
                    throw new InvalidTableException(_tableName, "Enum group is empty");
                }

                if (char.IsDigit(groupValue, 0))
                {
                    throw new InvalidTableException(_tableName, $"Enum group '{groupValue}' starts with a number");
                }

                if (enumValue == string.Empty)
                {
                    throw new InvalidTableException(_tableName, $"Enum value in group {groupValue} is empty");
                }

                if (char.IsDigit(enumValue, 0))
                {
                    throw new InvalidTableException(_tableName,
                        $"Enum value '{enumValue}' in group {groupValue} starts with a number");
                }

                if (!diffChecker.Add(groupValue + enumValue))
                {
                    throw new InvalidTableException(_tableName,
                        $"Duplicate enum value '{enumValue}' in group {groupValue}");
                }

                yield return (groupValue, enumValue);
            }
        }

        private void ParseRows(IEnumerable<(string, string)> rows)
        {
            string prevGroupValue = null;
            foreach (var (groupValue, enumValue) in rows)
            {
                if (prevGroupValue != groupValue)
                {
                    prevGroupValue = groupValue;
                    _builder.Replace(RowTemplate, string.Empty);
                    var groupTemplate = File.ReadAllText(GroupPath);
                    _builder.Replace(GroupTemplate, groupTemplate + GroupTemplate).Replace(GroupVariable, groupValue);
                }

                var rowTemplate = File.ReadAllText(RowPath);
                _builder.Replace(RowTemplate, rowTemplate + RowTemplate).Replace(RowVariable, enumValue);
            }

            _builder.Replace(RowTemplate, string.Empty);
            _builder.Replace(GroupTemplate, string.Empty);
            File.WriteAllText($"{Config.Root}/Dist/Em.{_tableName}.cs", _builder.ToString());
        }
    }
}