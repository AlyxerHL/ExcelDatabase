using System;
using System.Collections.Generic;
using System.Linq;

namespace ExcelDatabase.Editor.Library.Convert
{
    public static class Validator
    {
        public static IEnumerable<string> ValidateIDs(TableParser.Table table)
        {
            var diffChecker = new HashSet<string>();
            for (var i = Config.typeRow + 1; i <= table.sheet.LastRowNum; i++)
            {
                var id = table.sheet.GetRow(i)?.GetCellValue(Config.idCol);
                if (id is null || id.Length == 0)
                {
                    break;
                }

                if (!diffChecker.Add(id))
                {
                    throw new TableParser.Exception(table.name, $"Duplicate ID '{id}'");
                }

                yield return id;
            }
        }

        public static IEnumerable<Col> ValidateCols(
            TableParser.Table table,
            IEnumerable<string> ids
        )
        {
            var nameRow = table.sheet.GetRow(Config.nameRow);
            var typeRow = table.sheet.GetRow(Config.typeRow);

            if (
                nameRow?.GetCellValue(Config.idCol) != "ID"
                || typeRow?.GetCellValue(Config.idCol) != "string"
            )
            {
                throw new TableParser.Exception(table.name, "Invalid ID column");
            }

            var diffChecker = new HashSet<string>();
            for (var colIndex = Config.idCol; colIndex <= nameRow.LastCellNum; colIndex++)
            {
                var col = new Col(nameRow.GetCellValue(colIndex), typeRow.GetCellValue(colIndex));
                if (col.name.StartsWith(TableParser.excludePrefix))
                {
                    continue;
                }

                if (col.name.Length == 0)
                {
                    break;
                }

                if (char.IsDigit(col.name, 0))
                {
                    throw new TableParser.Exception(
                        table.name,
                        $"Column name '{col.name}' starts with a number"
                    );
                }

                if (!diffChecker.Add(col.name))
                {
                    throw new TableParser.Exception(
                        table.name,
                        $"Duplicate column name '{col.name}'"
                    );
                }

                switch (col.typeSpec)
                {
                    case Col.TypeSpec.None:
                    case Col.TypeSpec.Primitive
                        when !TableParser.typeValidators.ContainsKey(col.type):
                    case Col.TypeSpec.Convert when !TypeExists(col.type + "Type"):
                    case Col.TypeSpec.Enum when !TypeExists(col.type):
                        throw new TableParser.Exception(
                            table.name,
                            $"Type '{col.type}' of column '{col.name}' is invalid"
                        );
                }

                var rowIndex = Config.typeRow;
                foreach (var id in ids)
                {
                    rowIndex++;
                    var cell = table.sheet.GetRow(rowIndex).GetCellValue(colIndex);

                    if (cell.StartsWith(TableParser.excludePrefix))
                    {
                        col.cells[id] = null;
                        continue;
                    }

                    if (cell.Length == 0)
                    {
                        throw new TableParser.Exception(
                            table.name,
                            $"An empty cell exists in column '{col.name}' of '{id}'"
                        );
                    }

                    var cellValues = cell.Split(Config.arraySeparator);
                    if (!col.isArray && cellValues.Length > 1)
                    {
                        throw new TableParser.Exception(
                            table.name,
                            $"The cell in column '{col.name}' of '{id}' is array, "
                                + "but its type is not an array"
                        );
                    }

                    if (
                        col.typeSpec == Col.TypeSpec.Primitive
                        && cellValues.Any(
                            (cellValue) => !TableParser.typeValidators[col.type](cellValue)
                        )
                    )
                    {
                        throw new TableParser.Exception(
                            table.name,
                            $"The cell in column '{col.name}' of '{id}' type mismatch"
                        );
                    }

                    if (col.typeSpec == Col.TypeSpec.Enum)
                    {
                        var type = Type.GetType(
                            $"ExcelDatabase.{col.type.Replace('.', '+')}, Assembly-CSharp-firstpass"
                        );

                        if (
                            type is null
                            || cellValues.Any(
                                (cellValue) => !System.Enum.IsDefined(type, cellValue)
                            )
                        )
                        {
                            throw new TableParser.Exception(
                                table.name,
                                $"The cell in column '{col.name}' of '{id}' type mismatch"
                            );
                        }
                    }

                    col.cells[id] = col.isArray ? cellValues : cell;
                }

                yield return col;
            }

            bool TypeExists(string type)
            {
                var systemType = Type.GetType(
                    $"ExcelDatabase.{type.Replace('.', '+')}, Assembly-CSharp-firstpass"
                );
                return systemType is not null || type == $"Tb.{table.name}Type";
            }
        }

        public readonly struct Col
        {
            public string name { get; }
            public string type { get; }
            public bool isArray { get; }
            public TypeSpec typeSpec { get; }
            public Dictionary<string, object> cells { get; }

            public Col(string name, string type)
            {
                this.name = TableParser.Format(name);
                this.type = TableParser.Format(type);
                isArray = type.EndsWith("[]");
                cells = new();

                typeSpec = TableParser.typeValidators.ContainsKey(this.type) switch
                {
                    true => TypeSpec.Primitive,
                    false when this.type.StartsWith("Tb") => TypeSpec.Convert,
                    false when this.type.StartsWith("Em") => TypeSpec.Enum,
                    false when this.type.StartsWith("DesignVariable") => TypeSpec.Variable,
                    _ => TypeSpec.None
                };
            }

            public enum TypeSpec
            {
                None,
                Primitive,
                Convert,
                Enum,
                Variable
            }
        }
    }
}
