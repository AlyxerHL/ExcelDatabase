using System;
using ExcelDatabase.Editor.Tools;
using Newtonsoft.Json;

namespace ExcelDatabase.Editor.Parser
{
    [JsonObject(MemberSerialization.Fields)]
    public readonly struct ParseResult : IComparable<ParseResult>, IComparable
    {
        private readonly TableType _type;
        private readonly string _name;
        public readonly string ExcelPath;
        public readonly string[] DistPaths;

        public ParseResult(TableType type, string name, string excelPath, string[] distPaths)
        {
            _type = type;
            _name = name;
            ExcelPath = excelPath;
            DistPaths = distPaths;
        }

        public override string ToString()
        {
            return $"{_type} - {_name}";
        }

        public int CompareTo(ParseResult other)
        {
            var typeComparison = _type.CompareTo(other._type);
            return typeComparison != 0
                ? typeComparison
                : string.Compare(_name, other._name, StringComparison.Ordinal);
        }

        int IComparable.CompareTo(object obj)
        {
            return CompareTo((ParseResult)obj);
        }
    }
}