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
        private const int NameRow = 0;
        private const int TypeRow = 1;

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
            var distPaths = WriteScript(script);
            return new ParseResult(TableType.Convert, _tableName, _excelPath, distPaths);
        }

        private IEnumerable<Col> ValidateCols()
        {
            return null;
        }

        private string BuildScript(IEnumerable<Col> cols)
        {
            return null;
        }

        private string[] WriteScript(string script)
        {
            return null;
        }

        private readonly struct Col
        {
            public readonly string Name;
            public readonly string Type;

            public Col(string name, string type)
            {
                Name = name;
                Type = type;
            }
        }
    }
}