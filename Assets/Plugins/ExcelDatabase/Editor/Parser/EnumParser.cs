using System.Collections.Generic;
using System.IO;
using System.Text;
using ExcelDatabase.Editor.Config;
using ExcelDatabase.Editor.Tools;
using NPOI.SS.UserModel;

namespace ExcelDatabase.Editor.Parser
{
    public class EnumParser
    {
        private const int GroupColumn = 0;
        private const int EnumColumn = 1;

        private readonly StringBuilder _builder;
        private readonly ISheet _sheet;
        private readonly string _tableName;

        public EnumParser(IWorkbook workbook, string tableName)
        {
            var tableTemplate = File.ReadAllText(TemplateConfig.Enum.TablePath);
            _builder = new StringBuilder(tableTemplate).Replace(TemplateConfig.Enum.TableVariable, tableName);
            _sheet = workbook.GetSheetAt(0);
            _tableName = tableName;
        }

        public void Parse()
        {
            var rows = ValidateRows();
        }

        private IEnumerable<IRow> ValidateRows()
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

                yield return row;
            }
        }
    }
}