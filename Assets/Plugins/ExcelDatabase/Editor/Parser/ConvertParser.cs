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

namespace ExcelDatabase.Editor.Parser
{
    public class ConvertParser : IParser
    {
        private const int NameRow = 0;
        private const int TypeRow = 1;
        private const int IDCol = 0;

        private const string ArraySeparator = "\n";
        private const string ColTemplate = "#COL#";
        private const string TableVariable = "$TABLE$";
        private const string TypeVariable = "$TYPE$";
        private const string NameVariable = "$NAME$";

        private static readonly string tablePath = $"{Config.templatePath}/Convert/Table.txt";
        private static readonly string generalColPath =
            $"{Config.templatePath}/Convert/GeneralCol.txt";
        private static readonly string generalNullableColPath =
            $"{Config.templatePath}/Convert/GeneralNullableCol.txt";
        private static readonly string convertColPath =
            $"{Config.templatePath}/Convert/ConvertCol.txt";
        private static readonly string convertNullableColPath =
            $"{Config.templatePath}/Convert/ConvertNullableCol.txt";
        private static readonly string generalArrayColPath =
            $"{Config.templatePath}/Convert/GeneralArrayCol.txt";
        private static readonly string generalNullableArrayColPath =
            $"{Config.templatePath}/Convert/GeneralNullableArrayCol.txt";
        private static readonly string convertArrayColPath =
            $"{Config.templatePath}/Convert/ConvertArrayCol.txt";
        private static readonly string convertNullableArrayColPath =
            $"{Config.templatePath}/Convert/ConvertNullableArrayCol.txt";

        private readonly ISheet sheet;
        private readonly string tableName;
        private readonly string excelPath;

        public ConvertParser(Object file)
        {
            var path = AssetDatabase.GetAssetPath(file);
            using var stream = File.Open(path, FileMode.Open, FileAccess.Read);
            sheet = new XSSFWorkbook(stream).GetSheetAt(0);
            tableName = ParseUtility.Format(file.name);
            excelPath = AssetDatabase.GetAssetPath(file);
        }

        public ParseResult Parse()
        {
            var cols = ValidateCols().ToArray();
            var rows = ValidateRows(cols);
            var script = BuildScript(cols, rows);
            var jsonPath = WriteJson(rows);
            var distPath = ParseUtility.WriteScript(TableType.Convert, tableName, script);

            return new ParseResult(
                TableType.Convert,
                tableName,
                excelPath,
                new[] { distPath, jsonPath }
            );
        }

        private IEnumerable<Col> ValidateCols()
        {
            var nameRow = sheet.GetRow(NameRow);
            var typeRow = sheet.GetRow(TypeRow);
            if (nameRow?.GetCellValue(IDCol) != "ID" || typeRow?.GetCellValue(IDCol) != "string")
            {
                throw new ParserException(tableName, "Invalid ID column");
            }

            var diffChecker = new HashSet<string>();
            for (var i = 0; i <= nameRow.LastCellNum; i++)
            {
                var col = new Col(i, nameRow.GetCellValue(i), typeRow.GetCellValue(i));
                if (col.name.StartsWith(Config.excludePrefix))
                {
                    continue;
                }

                if (col.name?.Length == 0)
                {
                    break;
                }

                if (char.IsDigit(col.name, 0))
                {
                    throw new ParserException(
                        tableName,
                        $"Column name '{col.name}' starts with a number"
                    );
                }

                if (!diffChecker.Add(col.name))
                {
                    throw new ParserException(tableName, $"Duplicate column name '{col.name}'");
                }

                bool TypeExists(string type)
                {
                    var systemType = Type.GetType(
                        $"ExcelDatabase.{type.Replace('.', '+')}, Assembly-CSharp-firstpass"
                    );
                    return systemType != null || type == $"Tb.{tableName}Type";
                }

                switch (col.typeSpec)
                {
                    case Col.TypeSpec.None:
                    case Col.TypeSpec.Primitive
                        when !ParseUtility.typeValidators.ContainsKey(col.type):
                    case Col.TypeSpec.Convert when !TypeExists(col.type + "Type"):
                    case Col.TypeSpec.Enum when !TypeExists(col.type):
                        throw new ParserException(
                            tableName,
                            $"Type '{col.type}' of column '{col.name}' is invalid"
                        );
                }

                yield return col;
            }
        }

        private IEnumerable<Row> ValidateRows(Col[] cols)
        {
            var diffChecker = new HashSet<string>();
            for (var i = 2; i <= sheet.LastRowNum; i++)
            {
                var poiRow = sheet.GetRow(i);
                if (poiRow == null)
                {
                    break;
                }

                var row = new Row(poiRow.GetCellValue(IDCol));
                if (row.id?.Length == 0)
                {
                    break;
                }

                if (!diffChecker.Add(row.id))
                {
                    throw new ParserException(tableName, $"Duplicate ID '{row.id}'");
                }

                foreach (var col in cols)
                {
                    var cell = poiRow.GetCellValue(col.index);
                    if (cell.StartsWith(Config.excludePrefix))
                    {
                        continue;
                    }

                    if (cell?.Length == 0)
                    {
                        throw new ParserException(
                            tableName,
                            $"An empty cell exists in column '{col.name}' of '{row.id}'"
                        );
                    }

                    var cellValues = cell.Split(ArraySeparator);
                    if (!col.isArray && cellValues.Length > 1)
                    {
                        throw new ParserException(
                            tableName,
                            $"The cell in column '{col.name}' of '{row.id}' is array, "
                                + "but its type is not an array"
                        );
                    }

                    if (
                        col.typeSpec == Col.TypeSpec.Primitive
                        && cellValues.Any(
                            (cellValue) => !ParseUtility.typeValidators[col.type](cellValue)
                        )
                    )
                    {
                        throw new ParserException(
                            tableName,
                            $"The cell in column '{col.name}' of '{row.id}' type mismatch"
                        );
                    }

                    if (col.typeSpec == Col.TypeSpec.Enum)
                    {
                        var type = Type.GetType(
                            $"ExcelDatabase.{col.type.Replace('.', '+')}, Assembly-CSharp-firstpass"
                        );

                        if (
                            type == null
                            || cellValues.Any((cellValue) => !Enum.IsDefined(type, cellValue))
                        )
                        {
                            throw new ParserException(
                                tableName,
                                $"The cell in column '{col.name}' of '{row.id}' type mismatch"
                            );
                        }
                    }

                    row.cells[col.backingName] = col.isArray ? cellValues : cell;
                }

                yield return row;
            }
        }

        private string BuildScript(IEnumerable<Col> cols, IEnumerable<Row> rows)
        {
            var tableTemplate = File.ReadAllText(tablePath);
            var generalColTemplate = File.ReadAllText(generalColPath);
            var generalNullableColTemplate = File.ReadAllText(generalNullableColPath);
            var convertColTemplate = File.ReadAllText(convertColPath);
            var convertNullableColTemplate = File.ReadAllText(convertNullableColPath);
            var generalArrayColTemplate = File.ReadAllText(generalArrayColPath);
            var generalNullableArrayColTemplate = File.ReadAllText(generalNullableArrayColPath);
            var convertArrayColTemplate = File.ReadAllText(convertArrayColPath);
            var convertNullableArrayColTemplate = File.ReadAllText(convertNullableArrayColPath);
            var builder = new StringBuilder(tableTemplate).Replace(TableVariable, tableName);

            foreach (var col in cols)
            {
                var isNullable = !rows.All((row) => row.cells.ContainsKey(col.backingName));

                if (col.typeSpec == Col.TypeSpec.Convert)
                {
                    builder.Replace(
                        ColTemplate,
                        (
                            col.isArray
                                ? (
                                    isNullable
                                        ? convertNullableArrayColTemplate
                                        : convertArrayColTemplate
                                )
                                : (isNullable ? convertNullableColTemplate : convertColTemplate)
                        ) + ColTemplate
                    );
                }
                else
                {
                    builder.Replace(
                        ColTemplate,
                        (
                            col.isArray
                                ? (
                                    isNullable
                                        ? generalNullableArrayColTemplate
                                        : generalArrayColTemplate
                                )
                                : (isNullable ? generalNullableColTemplate : generalColTemplate)
                        ) + ColTemplate
                    );
                }

                builder
                    .Replace(
                        TypeVariable,
                        col.typeSpec == Col.TypeSpec.Variable ? "string" : col.type
                    )
                    .Replace(NameVariable, col.name);
            }

            builder.Replace(ColTemplate, string.Empty);
            return builder.ToString();
        }

        private string WriteJson(IEnumerable<Row> rows)
        {
            var cells = rows.Select((row) => row.cells);
            var json = JsonConvert.SerializeObject(cells, Formatting.Indented);
            const string DistDirectory = "Assets/Resources/ExcelDatabase";

            if (!Directory.Exists(DistDirectory))
            {
                Directory.CreateDirectory(DistDirectory);
            }

            var distPath = $"{DistDirectory}/{tableName}.json";
            File.WriteAllText(distPath, json);
            return distPath;
        }

        private readonly struct Col
        {
            public int index { get; }
            public string name { get; }
            public string backingName { get; }
            public string type { get; }
            public bool isArray { get; }
            public TypeSpec typeSpec { get; }

            public Col(int index, string name, string type)
            {
                this.index = index;
                this.name = ParseUtility.Format(name);
                this.type = ParseUtility.Format(type);

                typeSpec = ParseUtility.typeValidators.ContainsKey(this.type) switch
                {
                    true => TypeSpec.Primitive,
                    false when this.type.StartsWith("Tb") => TypeSpec.Convert,
                    false when this.type.StartsWith("Em") => TypeSpec.Enum,
                    false when this.type.StartsWith("DesignVariable") => TypeSpec.Variable,
                    _ => TypeSpec.None
                };

                isArray = type.EndsWith("[]");
                backingName = typeSpec == TypeSpec.Convert ? '_' + this.name : this.name;
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

        private readonly struct Row
        {
            public string id { get; }
            public Dictionary<string, object> cells { get; }

            public Row(string id)
            {
                this.id = id;
                cells = new Dictionary<string, object> { { "ID", id } };
            }
        }
    }
}
