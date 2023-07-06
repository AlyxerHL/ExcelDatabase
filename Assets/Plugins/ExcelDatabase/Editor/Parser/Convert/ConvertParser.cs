using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ExcelDatabase.Editor.Library;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using UnityEditor;
using Object = UnityEngine.Object;

namespace ExcelDatabase.Editor.Parser.Convert
{
    public class ConvertParser
    {
        private const int NameRow = 0;
        private const int TypeRow = 1;
        private const int IDCol = 0;

        private const string ArraySeparator = "\n";
        private const string ColTemplate = "#COL#";
        private const string TableVariable = "$TABLE$";
        private const string TypeVariable = "$TYPE$";
        private const string NameVariable = "$NAME$";

        private static readonly Lazy<string> tableTemplate =
            new(() => File.ReadAllText($"{Config.templatePath}/Convert/Table.txt"));
        private static readonly Lazy<string> generalCol =
            new(() => File.ReadAllText($"{Config.templatePath}/Convert/GeneralCol.txt"));
        private static readonly Lazy<string> generalNullCol =
            new(() => File.ReadAllText($"{Config.templatePath}/Convert/GeneralNullCol.txt"));
        private static readonly Lazy<string> convertCol =
            new(() => File.ReadAllText($"{Config.templatePath}/Convert/ConvertCol.txt"));
        private static readonly Lazy<string> convertNullCol =
            new(() => File.ReadAllText($"{Config.templatePath}/Convert/ConvertNullCol.txt"));
        private static readonly Lazy<string> generalArrCol =
            new(() => File.ReadAllText($"{Config.templatePath}/Convert/GeneralArrCol.txt"));
        private static readonly Lazy<string> generalNullArrCol =
            new(() => File.ReadAllText($"{Config.templatePath}/Convert/GeneralNullArrCol.txt"));
        private static readonly Lazy<string> convertArrCol =
            new(() => File.ReadAllText($"{Config.templatePath}/Convert/ConvertArrCol.txt"));
        private static readonly Lazy<string> convertNullArrCol =
            new(() => File.ReadAllText($"{Config.templatePath}/Convert/ConvertNullArrCol.txt"));

        private readonly ISheet sheet;
        private readonly string tableName;
        private readonly string excelPath;

        public ConvertParser(Object file)
        {
            var path = AssetDatabase.GetAssetPath(file);
            using var stream = File.Open(path, FileMode.Open, FileAccess.Read);
            sheet = new XSSFWorkbook(stream).GetSheetAt(0);
            tableName = TableParser.Format(file.name);
            excelPath = AssetDatabase.GetAssetPath(file);
        }

        public Library.TableParser.Result Parse()
        {
            var ids = ValidateIDs();
            var cols = ValidateCols(ids);
            File.WriteAllText(Config.DistPath(tableName, TableType.Convert), BuildScript(cols));
            File.WriteAllText(Config.JsonPath(tableName), BuildJson(cols));

            return new Library.TableParser.Result(TableType.Convert, tableName, excelPath);
        }

        private IEnumerable<string> ValidateIDs()
        {
            var diffChecker = new HashSet<string>();
            for (var i = TypeRow + 1; i <= sheet.LastRowNum; i++)
            {
                var id = sheet.GetRow(i)?.GetCellValue(IDCol);
                if (id is null || id.Length == 0)
                {
                    break;
                }

                if (!diffChecker.Add(id))
                {
                    throw new Library.TableParser.Exception(tableName, $"Duplicate ID '{id}'");
                }

                yield return id;
            }
        }

        private IEnumerable<Col> ValidateCols(IEnumerable<string> ids)
        {
            var nameRow = sheet.GetRow(NameRow);
            var typeRow = sheet.GetRow(TypeRow);
            if (nameRow?.GetCellValue(IDCol) != "ID" || typeRow?.GetCellValue(IDCol) != "string")
            {
                throw new Library.TableParser.Exception(tableName, "Invalid ID column");
            }

            var diffChecker = new HashSet<string>();
            for (var colIndex = IDCol; colIndex <= nameRow.LastCellNum; colIndex++)
            {
                var col = new Col(nameRow.GetCellValue(colIndex), typeRow.GetCellValue(colIndex));
                if (col.name.StartsWith(Config.excludePrefix))
                {
                    continue;
                }

                if (col.name.Length == 0)
                {
                    break;
                }

                if (char.IsDigit(col.name, 0))
                {
                    throw new Library.TableParser.Exception(
                        tableName,
                        $"Column name '{col.name}' starts with a number"
                    );
                }

                if (!diffChecker.Add(col.name))
                {
                    throw new Library.TableParser.Exception(
                        tableName,
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
                        throw new Library.TableParser.Exception(
                            tableName,
                            $"Type '{col.type}' of column '{col.name}' is invalid"
                        );
                }

                var rowIndex = TypeRow;
                foreach (var id in ids)
                {
                    rowIndex++;
                    var cell = sheet.GetRow(rowIndex).GetCellValue(colIndex);

                    if (cell.StartsWith(Config.excludePrefix))
                    {
                        col.cells[id] = null;
                        continue;
                    }

                    if (cell.Length == 0)
                    {
                        throw new Library.TableParser.Exception(
                            tableName,
                            $"An empty cell exists in column '{col.name}' of '{id}'"
                        );
                    }

                    var cellValues = cell.Split(ArraySeparator);
                    if (!col.isArray && cellValues.Length > 1)
                    {
                        throw new Library.TableParser.Exception(
                            tableName,
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
                        throw new Library.TableParser.Exception(
                            tableName,
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
                            || cellValues.Any((cellValue) => !Enum.IsDefined(type, cellValue))
                        )
                        {
                            throw new Library.TableParser.Exception(
                                tableName,
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
                return systemType is not null || type == $"Tb.{tableName}Type";
            }
        }

        private string BuildScript(IEnumerable<Col> cols)
        {
            var builder = new StringBuilder(tableTemplate.Value).Replace(TableVariable, tableName);

            foreach (var col in cols)
            {
                var isNullable = col.cells.Any((cell) => cell.Value is null);

                builder
                    .Replace(
                        ColTemplate,
                        (
                            col.typeSpec switch
                            {
                                Col.TypeSpec.Convert when col.isArray
                                    => isNullable ? convertNullArrCol.Value : convertArrCol.Value,
                                Col.TypeSpec.Convert
                                    => isNullable ? convertNullCol.Value : convertCol.Value,
                                _ when col.isArray
                                    => isNullable ? generalNullArrCol.Value : generalArrCol.Value,
                                _ => isNullable ? generalNullCol.Value : generalCol.Value
                            }
                        ) + ColTemplate
                    )
                    .Replace(
                        TypeVariable,
                        col.typeSpec == Col.TypeSpec.Variable ? "string" : col.type
                    )
                    .Replace(NameVariable, col.name);
            }

            builder.Replace(ColTemplate, string.Empty);
            return builder.ToString();
        }

        private string BuildJson(IEnumerable<Col> cols)
        {
            var ids = cols.First().cells.Keys;
            var json = ids.Select(
                (id) =>
                    cols.Where((col) => col.cells[id] is not null)
                        .ToDictionary(
                            (col) =>
                                col.typeSpec == Col.TypeSpec.Convert ? '_' + col.name : col.name,
                            (col) => col.cells[id]
                        )
            );

            return JsonConvert.SerializeObject(json, Formatting.Indented);
        }

        private readonly struct Col
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
