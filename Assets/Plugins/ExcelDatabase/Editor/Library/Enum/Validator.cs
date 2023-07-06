using System.Collections.Generic;

namespace ExcelDatabase.Editor.Library.Enum
{
    public static class Validator
    {
        public static IEnumerable<Row> ValidateRows(TableParser.Table table)
        {
            var nameRow = table.sheet.GetRow(Config.nameRow);
            if (
                nameRow?.GetCellValue(Config.groupCol) != "EnumGroup"
                || nameRow.GetCellValue(Config.enumCol) != "Enum"
            )
            {
                throw new TableParser.Exception(table.name, "Invalid column name");
            }

            var diffChecker = new HashSet<string>();
            for (var i = Config.nameRow + 1; i <= table.sheet.LastRowNum; i++)
            {
                var poiRow = table.sheet.GetRow(i);
                if (poiRow is null)
                {
                    break;
                }

                var row = new Row(
                    poiRow.GetCellValue(Config.groupCol),
                    poiRow.GetCellValue(Config.enumCol)
                );

                if (row.group?.Length == 0)
                {
                    break;
                }

                if (char.IsDigit(row.group, 0))
                {
                    throw new TableParser.Exception(
                        table.name,
                        $"Enum group '{row.group}' starts with a number"
                    );
                }

                if (row.enumName?.Length == 0)
                {
                    throw new TableParser.Exception(
                        table.name,
                        $"Enum value in group '{row.group}' is empty"
                    );
                }

                if (char.IsDigit(row.enumName, 0))
                {
                    throw new TableParser.Exception(
                        table.name,
                        $"Enum value '{row.enumName}' in group '{row.group}' starts with a number"
                    );
                }

                if (!diffChecker.Add(row.group + row.enumName))
                {
                    throw new TableParser.Exception(
                        table.name,
                        $"Duplicate enum value '{row.enumName}' in group '{row.group}'"
                    );
                }

                yield return row;
            }
        }

        public readonly struct Row
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
