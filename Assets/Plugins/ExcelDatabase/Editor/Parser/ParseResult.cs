using System;
using ExcelDatabase.Editor.Library;
using Newtonsoft.Json;

namespace ExcelDatabase.Editor.Parser
{
    [JsonObject(MemberSerialization.Fields)]
    public readonly struct ParseResult : IComparable<ParseResult>, IComparable
    {
        public readonly TableType Type;
        public readonly string Name;
        public readonly string ExcelPath;
        public readonly string[] DistPaths;

        public ParseResult(TableType type, string name, string excelPath, string[] distPaths)
        {
            Type = type;
            Name = name;
            ExcelPath = excelPath;
            DistPaths = distPaths;
        }

        public override string ToString()
        {
            return $"{Type} - {Name}";
        }

        public int CompareTo(ParseResult other)
        {
            var typeComparison = Type.CompareTo(other.Type);
            return typeComparison != 0 ? typeComparison : string.Compare(Name, other.Name);
        }

        int IComparable.CompareTo(object obj)
        {
            return CompareTo((ParseResult)obj);
        }
    }
}
