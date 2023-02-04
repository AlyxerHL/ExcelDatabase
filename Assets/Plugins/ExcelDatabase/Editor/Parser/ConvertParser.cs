using System.Collections.Generic;
using System.IO;
using ExcelDatabase.Editor.Library;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using UnityEditor;
using Object = UnityEngine.Object;

namespace ExcelDatabase.Editor.Parser
{
    public class ConvertParser : IParsable
    {
        private const int NameRow = 0;
        private const int TypeRow = 1;
        private const int IDCol = 0;

        private const string ColTemplate = "#COL#";
        private const string TableVariable = "$TABLE$";
        private const string TypeVariable = "$TYPE$";
        private const string NameVariable = "$NAME$";

        private static readonly string TablePath = $"{Config.TemplatePath}/Convert/Table.txt";
        private static readonly string ConvertColPath = $"{Config.TemplatePath}/Convert/ConvertCol.txt";
        private static readonly string PrimitiveColPath = $"{Config.TemplatePath}/Convert/PrimitiveCol.txt";

        private readonly ISheet _sheet;
        private readonly string _tableName;
        private readonly string _excelPath;

        public ConvertParser(Object file)
        {
            var path = AssetDatabase.GetAssetPath(file);
            using var stream = File.Open(path, FileMode.Open, FileAccess.Read);
            _sheet = new XSSFWorkbook(stream).GetSheetAt(0);
            _tableName = file.name;
            _excelPath = AssetDatabase.GetAssetPath(file);
        }

        public ParseResult Parse()
        {
            var rows = ValidateCols();
            var script = BuildScript(rows);
            var distPath = ParseUtility.WriteScript(TableType.Convert, _tableName, script);
            return new ParseResult(TableType.Convert, _tableName, _excelPath, new[] { distPath });
        }

        private IEnumerable<Col> ValidateCols()
        {
            var nameRow = _sheet.GetRow(NameRow);
            var typeRow = _sheet.GetRow(TypeRow);
            if (nameRow.GetCellValue(IDCol) != "ID" || typeRow.GetCellValue(0) != "string")
            {
                throw new ParseFailureException(_tableName, "Invalid ID column");
            }

            var diffChecker = new HashSet<string>();
            for (var i = 1; i <= nameRow.LastCellNum; i++)
            {
                var col = new Col(nameRow.GetCellValue(i), typeRow.GetCellValue(i));
                if (col.Name == string.Empty)
                {
                    break;
                }

                if (char.IsDigit(col.Name, 0))
                {
                    throw new ParseFailureException(_tableName,
                        $"Column name '{col.Name}' starts with a number");
                }

                if (!diffChecker.Add(col.Name))
                {
                    throw new ParseFailureException(_tableName, $"Duplicate column name '{col.Name}'");
                }

                if (!ParseUtility.TypeValidators.ContainsKey(col.Type))
                {
                    bool TypeExists(string type)
                    {
                        var systemType = System.Type.GetType(
                            $"ExcelDatabase.{type.Replace('.', '+')}, Assembly-CSharp-firstpass");
                        return systemType != null;
                    }

                    if (TypeExists(col.Type) || TypeExists(col.Type + "Type"))
                    {
                        throw new ParseFailureException(_tableName, $"Invalid column type '{col.Type}'");
                    }
                }

                yield return col;
            }
        }

        private string BuildScript(IEnumerable<Col> cols)
        {
            return null;
        }

        private readonly struct Col
        {
            public readonly string Name;
            public readonly string Type;
            public readonly bool IsArray;

            public Col(string name, string type)
            {
                Name = ParseUtility.Format(name);
                Type = ParseUtility.Format(type);
                IsArray = type.EndsWith("[]");
            }
        }
    }
}