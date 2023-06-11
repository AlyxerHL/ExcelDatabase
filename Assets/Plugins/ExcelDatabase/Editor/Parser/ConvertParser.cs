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
                if (col.Name.StartsWith(Config.excludePrefix))
                {
                    continue;
                }

                if (col.Name?.Length == 0)
                {
                    break;
                }

                if (char.IsDigit(col.Name, 0))
                {
                    throw new ParserException(
                        tableName,
                        $"Column name '{col.Name}' starts with a number"
                    );
                }

                if (!diffChecker.Add(col.Name))
                {
                    throw new ParserException(tableName, $"Duplicate column name '{col.Name}'");
                }

                bool TypeExists(string type)
                {
                    var systemType = Type.GetType(
                        $"ExcelDatabase.{type.Replace('.', '+')}, Assembly-CSharp-firstpass"
                    );
                    return systemType != null || type == $"Tb.{tableName}Type";
                }

                switch (col.TypeSpec)
                {
                    case Col.TypeSpecification.None:
                    case Col.TypeSpecification.Primitive
                        when !ParseUtility.typeValidators.ContainsKey(col.Type):
                    case Col.TypeSpecification.Convert when !TypeExists(col.Type + "Type"):
                    case Col.TypeSpecification.Enum when !TypeExists(col.Type):
                        throw new ParserException(
                            tableName,
                            $"Type '{col.Type}' of column '{col.Name}' is invalid"
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
                if (row.ID?.Length == 0)
                {
                    break;
                }

                if (!diffChecker.Add(row.ID))
                {
                    throw new ParserException(tableName, $"Duplicate ID '{row.ID}'");
                }

                foreach (var col in cols)
                {
                    var cell = poiRow.GetCellValue(col.Index);
                    if (cell.StartsWith(Config.excludePrefix))
                    {
                        continue;
                    }

                    if (cell?.Length == 0)
                    {
                        throw new ParserException(
                            tableName,
                            $"An empty cell exists in column '{col.Name}' of '{row.ID}'"
                        );
                    }

                    var cellValues = cell.Split(ArraySeparator);
                    if (!col.IsArray && cellValues.Length > 1)
                    {
                        throw new ParserException(
                            tableName,
                            $"The cell in column '{col.Name}' of '{row.ID}' is array, "
                                + "but its type is not an array"
                        );
                    }

                    if (
                        col.TypeSpec == Col.TypeSpecification.Primitive
                        && cellValues.Any(
                            (cellValue) => !ParseUtility.typeValidators[col.Type](cellValue)
                        )
                    )
                    {
                        throw new ParserException(
                            tableName,
                            $"The cell in column '{col.Name}' of '{row.ID}' type mismatch"
                        );
                    }

                    if (col.TypeSpec == Col.TypeSpecification.Enum)
                    {
                        var type = Type.GetType(
                            $"ExcelDatabase.{col.Type.Replace('.', '+')}, Assembly-CSharp-firstpass"
                        );

                        if (
                            type == null
                            || cellValues.Any((cellValue) => !Enum.IsDefined(type, cellValue))
                        )
                        {
                            throw new ParserException(
                                tableName,
                                $"The cell in column '{col.Name}' of '{row.ID}' type mismatch"
                            );
                        }
                    }

                    row.Cells[col.BackingName] = col.IsArray ? cellValues : cell;
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
                var isNullable = !rows.All((row) => row.Cells.ContainsKey(col.BackingName));

                if (col.TypeSpec == Col.TypeSpecification.Convert)
                {
                    builder.Replace(
                        ColTemplate,
                        (
                            col.IsArray
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
                            col.IsArray
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
                        col.TypeSpec == Col.TypeSpecification.Variable ? "string" : col.Type
                    )
                    .Replace(NameVariable, col.Name);
            }

            builder.Replace(ColTemplate, string.Empty);
            return builder.ToString();
        }

        private string WriteJson(IEnumerable<Row> rows)
        {
            var cells = rows.Select((row) => row.Cells);
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
            public readonly int Index;
            public readonly string Name;
            public readonly string BackingName;
            public readonly string Type;
            public readonly bool IsArray;
            public readonly TypeSpecification TypeSpec;

            public Col(int index, string name, string type)
            {
                Index = index;
                Name = ParseUtility.Format(name);
                Type = ParseUtility.Format(type);
                IsArray = type.EndsWith("[]");

                TypeSpec = ParseUtility.typeValidators.ContainsKey(Type) switch
                {
                    true => TypeSpecification.Primitive,
                    false when Type.StartsWith("Tb") => TypeSpecification.Convert,
                    false when Type.StartsWith("Em") => TypeSpecification.Enum,
                    false when Type.StartsWith("DesignVariable") => TypeSpecification.Variable,
                    _ => TypeSpecification.None
                };

                BackingName = TypeSpec == TypeSpecification.Convert ? '_' + Name : Name;
            }

            public enum TypeSpecification
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
            public readonly string ID;
            public readonly Dictionary<string, object> Cells;

            public Row(string id)
            {
                ID = id;
                Cells = new Dictionary<string, object> { { "ID", id } };
            }
        }
    }
}
