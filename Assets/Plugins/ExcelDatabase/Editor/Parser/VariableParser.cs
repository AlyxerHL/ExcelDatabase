using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ExcelDatabase.Editor.Tools;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using UnityEditor;
using Object = UnityEngine.Object;

namespace ExcelDatabase.Editor.Parser
{
    public class VariableParser : IParsable
    {
        private const int NameColumn = 0;
        private const int TypeColumn = 1;
        private const int ValueColumn = 2;

        private const string RowTemplate = "#ROW#";
        private const string TableVariable = "$TABLE$";
        private const string TypeVariable = "$TYPE$";
        private const string NameVariable = "$NAME$";
        private const string ValueVariable = "$VALUE$";

        private static readonly string TablePath = $"{Config.TemplatePath}/Variable/Table.txt";
        private static readonly string RowPath = $"{Config.TemplatePath}/Variable/Row.txt";

        private static readonly Dictionary<string, Func<string, bool>> TypeValidators = new()
        {
            { "string", _ => true },
            { "int", value => int.TryParse(value, out _) },
            { "float", value => float.TryParse(value, out _) },
            { "bool", value => bool.TryParse(value, out _) }
        };

        private readonly ISheet _sheet;
        private readonly string _tableName;
        private readonly string _excelPath;

        public VariableParser(Object file)
        {
            var path = AssetDatabase.GetAssetPath(file);
            using var stream = File.Open(path, FileMode.Open, FileAccess.Read);
            _sheet = new XSSFWorkbook(stream).GetSheetAt(0);
            _tableName = file.name;
            _excelPath = AssetDatabase.GetAssetPath(file);
        }

        public ParseResult Parse()
        {
            var rows = ValidateRows();
            var script = BuildScript(rows);
            var distPaths = WriteScript(script);
            return new ParseResult(TableType.Variable, _tableName, _excelPath, distPaths);
        }

        private IEnumerable<Row> ValidateRows()
        {
            var diffChecker = new HashSet<string>();
            for (var i = 1; i <= _sheet.LastRowNum; i++)
            {
                var row = _sheet.GetRow(i);
                var nameValue = row.GetCell(NameColumn).GetValue();
                var typeValue = row.GetCell(TypeColumn).GetValue();
                var valueValue = row.GetCell(ValueColumn).GetValue();

                if (nameValue == string.Empty)
                {
                    break;
                }

                if (char.IsDigit(nameValue, 0))
                {
                    throw new InvalidTableException(_tableName, $"Variable name '{nameValue}' starts with a number");
                }

                if (!diffChecker.Add(nameValue))
                {
                    throw new InvalidTableException(_tableName, $"Duplicate variable name '{nameValue}'");
                }

                if (!TypeValidators.ContainsKey(typeValue))
                {
                    throw new InvalidTableException(_tableName, $"Invalid variable type '{typeValue}'");
                }

                if (!TypeValidators[typeValue](valueValue))
                {
                    throw new InvalidTableException(_tableName,
                        $"Variable value '{valueValue}' is not of variable type '{typeValue}'");
                }

                yield return new Row(nameValue, typeValue, valueValue);
            }
        }

        private string BuildScript(IEnumerable<Row> rows)
        {
            var tableTemplate = File.ReadAllText(TablePath);
            var rowTemplate = File.ReadAllText(RowPath);
            var builder = new StringBuilder(tableTemplate).Replace(TableVariable, _tableName);

            foreach (var row in rows)
            {
                builder
                    .Replace(RowTemplate, rowTemplate + RowTemplate)
                    .Replace(TypeVariable, row.Type)
                    .Replace(NameVariable, row.Name)
                    .Replace(ValueVariable, row.Type switch
                    {
                        "float" => row.Value + 'f',
                        "bool" => row.Value.ToLower(),
                        _ => row.Value
                    });
            }

            builder.Replace(RowTemplate, string.Empty);
            return builder.ToString();
        }

        private string[] WriteScript(string script)
        {
            var distDirectory = $"{Config.DistPath}/Variable";
            if (!Directory.Exists(distDirectory))
            {
                Directory.CreateDirectory(distDirectory);
            }

            var distPath = $"{distDirectory}/{_tableName}.cs";
            File.WriteAllText(distPath, script);
            return new[] { distPath };
        }

        private readonly struct Row
        {
            public readonly string Name;
            public readonly string Type;
            public readonly string Value;

            public Row(string name, string type, string value)
            {
                Name = name;
                Type = type;
                Value = value;
            }
        }
    }
}