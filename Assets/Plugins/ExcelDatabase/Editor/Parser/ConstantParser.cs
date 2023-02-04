using System.Collections.Generic;
using System.IO;
using ExcelDatabase.Editor.Tools;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using UnityEditor;
using UnityEngine;

namespace ExcelDatabase.Editor.Parser
{
    public class ConstantParser
    {
        private const int NameColumn = 0;
        private const int TypeColumn = 1;
        private const int ValueColumn = 2;

        private const string RowTemplate = "#ROW#";
        private const string TypeVariable = "$TYPE$";
        private const string NameVariable = "$NAME$";
        private const string ValueVariable = "$VALUE$";

        private static readonly string TablePath = $"{Config.TemplatePath}/Constant/Table.txt";
        private static readonly string RowPath = $"{Config.TemplatePath}/Constant/Row.txt";

        private readonly ISheet _sheet;
        private readonly string _tableName;
        private readonly string _excelPath;

        public ConstantParser(Object file)
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
            return new ParseResult(TableType.Constant, _tableName, _excelPath, distPaths);
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
                    throw new InvalidTableException(_tableName, $"Constant name '{nameValue}' starts with a number");
                }

                if (!diffChecker.Add(nameValue))
                {
                    throw new InvalidTableException(_tableName, $"Duplicate constant name '{nameValue}'");
                }

                // - [x] 이름이 숫자로 시작
                // - [x] 이름 중복
                // - [ ] 타입이 primitive type이 아님
                // - [ ] value가 타입에 맞지 않음
            }

            return null;
        }

        private string BuildScript(IEnumerable<Row> rows)
        {
            return null;
        }

        private string[] WriteScript(string script)
        {
            return null;
        }

        private readonly struct Row
        {
            public readonly string Type;
            public readonly string Name;
            public readonly string Value;

            public Row(string type, string name, string value)
            {
                Type = type;
                Name = name;
                Value = value;
            }
        }
    }
}