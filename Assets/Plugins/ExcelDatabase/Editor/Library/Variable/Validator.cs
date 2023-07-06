using System.Collections.Generic;

namespace ExcelDatabase.Editor.Library.Variable
{
    public static class Validator
    {
        public static IEnumerable<Row> ValidateRows(TableParser.Table table)
        {
            var nameRow = table.sheet.GetRow(Config.NameRow);
            if (
                nameRow?.GetCellValue(Config.NameCol) != "VariableName"
                || nameRow.GetCellValue(Config.TypeCol) != "DataType"
                || nameRow.GetCellValue(Config.ValueCol) != "Value"
            )
            {
                throw new TableParser.Exception(table.name, "Invalid column name");
            }

            var diffChecker = new HashSet<string>();
            for (var i = Config.NameRow + 1; i <= table.sheet.LastRowNum; i++)
            {
                var poiRow = table.sheet.GetRow(i);
                if (poiRow is null)
                {
                    break;
                }

                var row = new Row(
                    poiRow.GetCellValue(Config.NameCol),
                    poiRow.GetCellValue(Config.TypeCol),
                    poiRow.GetCellValue(Config.ValueCol)
                );

                if (row.name?.Length == 0)
                {
                    break;
                }

                if (char.IsDigit(row.name, 0))
                {
                    throw new TableParser.Exception(
                        table.name,
                        $"Variable name '{row.name}' starts with a number"
                    );
                }

                if (!diffChecker.Add(row.name))
                {
                    throw new TableParser.Exception(
                        table.name,
                        $"Duplicate variable name '{row.name}'"
                    );
                }

                if (!TableParser.typeValidators.ContainsKey(row.type))
                {
                    throw new TableParser.Exception(
                        table.name,
                        $"Invalid variable type '{row.type}'"
                    );
                }

                if (!TableParser.typeValidators[row.type](row.value))
                {
                    throw new TableParser.Exception(
                        table.name,
                        $"Variable value '{row.value}' is not of variable type '{row.type}'"
                    );
                }

                yield return row;
            }
        }

        public readonly struct Row
        {
            public string name { get; }
            public string type { get; }
            public string value { get; }

            public Row(string name, string type, string value)
            {
                this.name = TableParser.Format(name);
                this.type = type;
                this.value = value;
            }
        }
    }
}
