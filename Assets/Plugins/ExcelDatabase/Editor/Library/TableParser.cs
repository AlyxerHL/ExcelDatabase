using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NPOI.SS.UserModel;
using UnityEditor;

namespace ExcelDatabase.Editor.Library
{
    public static class TableParser
    {
        public static Dictionary<string, Func<string, bool>> typeValidators { get; } =
            new()
            {
                { "string", (value) => true },
                { "int", (value) => int.TryParse(value, out _) },
                { "float", (value) => float.TryParse(value, out _) },
                { "bool", (value) => bool.TryParse(value, out _) }
            };

        public static string root => lazyRoot.Value;
        public static string excludePrefix { get; } = "#";
        private static readonly Lazy<string> lazyRoot = new(CreateRoot);

        public static string DistPath(string tableName, TableType tableType)
        {
            return $"{root}/Dist/{tableType}/{tableName}.cs";
        }

        public static string JsonPath(string tableName)
        {
            return $"Assets/Resources/ExcelDatabase/{tableName}.json";
        }

        public static string GetCellValue(this IRow row, int index)
        {
            var cell = row.GetCell(index);
            return cell?.CellType switch
            {
                CellType.String => cell.StringCellValue,
                CellType.Formula => cell.StringCellValue,
                CellType.Numeric => cell.NumericCellValue.ToString(),
                CellType.Boolean => cell.BooleanCellValue.ToString(),
                _ => string.Empty,
            };
        }

        public static string Format(string value)
        {
            return Regex.Replace(value, "[^a-zA-Z0-9.#]+", string.Empty);
        }

        public static Result ParseConvert(UnityEngine.Object file)
        {
            return new Result();
        }

        public static Result ParseEnum(UnityEngine.Object file)
        {
            return new Result();
        }

        public static Result ParseVariable(UnityEngine.Object file)
        {
            return new Result();
        }

        private static string CreateRoot()
        {
            var assets = AssetDatabase.FindAssets("ExcelDatabaseRoot");
            var rootFilePath = AssetDatabase.GUIDToAssetPath(assets[0]);
            return Path.GetDirectoryName(rootFilePath);
        }

        public class Exception : System.Exception
        {
            public string tableName { get; }

            public Exception(string tableName, string message)
                : base(message)
            {
                this.tableName = tableName;
            }
        }

        public readonly struct Result : IComparable<Result>, IComparable
        {
            public TableType type { get; }
            public string name { get; }
            public string excelPath { get; }

            public Result(TableType type, string name, string excelPath)
            {
                this.type = type;
                this.name = name;
                this.excelPath = excelPath;
            }

            public override string ToString()
            {
                return $"{type} - {name}";
            }

            public int CompareTo(Result other)
            {
                var typeComparison = type.CompareTo(other.type);
                return typeComparison != 0 ? typeComparison : string.Compare(name, other.name);
            }

            int IComparable.CompareTo(object obj)
            {
                return CompareTo((Result)obj);
            }
        }
    }
}
