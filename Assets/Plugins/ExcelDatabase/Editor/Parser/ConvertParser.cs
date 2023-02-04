using System.Collections.Generic;
using System.IO;
using ExcelDatabase.Editor.Tools;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using UnityEditor;
using UnityEngine;

namespace ExcelDatabase.Editor.Parser
{
    public class ConvertParser : IParsable
    {
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
            var rows = ValidateColumns();
            var script = BuildScript(rows);
            var distPaths = WriteScript(script);
            return new ParseResult(TableType.Convert, _tableName, _excelPath, distPaths);
        }

        private IEnumerable<Column> ValidateColumns()
        {
            return null;
        }

        private string BuildScript(IEnumerable<Column> columns)
        {
            return null;
        }

        private string[] WriteScript(string script)
        {
            return null;
        }

        private readonly struct Column
        {
            public readonly string Name;
            public readonly string Type;

            public Column(string name, string type)
            {
                Name = name;
                Type = type;
            }
        }
    }
}