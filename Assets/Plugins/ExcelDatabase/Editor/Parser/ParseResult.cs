using System;
using ExcelDatabase.Editor.Library;

namespace ExcelDatabase.Editor.Parser
{
    public readonly struct ParseResult : IComparable<ParseResult>, IComparable
    {
        public TableType type { get; }
        public string name { get; }
        public string excelPath { get; }
        public string[] distPaths { get; }

        public ParseResult(TableType type, string name, string excelPath, string[] distPaths)
        {
            this.type = type;
            this.name = name;
            this.excelPath = excelPath;
            this.distPaths = distPaths;
        }

        public override string ToString()
        {
            return $"{type} - {name}";
        }

        public int CompareTo(ParseResult other)
        {
            var typeComparison = type.CompareTo(other.type);
            return typeComparison != 0 ? typeComparison : string.Compare(name, other.name);
        }

        int IComparable.CompareTo(object obj)
        {
            return CompareTo((ParseResult)obj);
        }
    }
}
