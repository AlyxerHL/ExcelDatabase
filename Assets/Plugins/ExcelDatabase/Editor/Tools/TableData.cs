using System;
using Newtonsoft.Json;

namespace ExcelDatabase.Editor.Tools
{
    [JsonObject(MemberSerialization.Fields)]
    public readonly struct TableData : IComparable<TableData>, IComparable
    {
        private readonly TableType _type;
        private readonly string _name;
        public readonly string ExcelPath;
        private readonly string[] _distPaths;

        public TableData(TableType type, string name, string excelPath, string[] distPaths)
        {
            _type = type;
            _name = name;
            ExcelPath = excelPath;
            _distPaths = distPaths;
        }

        public override string ToString()
        {
            return $"{_type} - {_name}";
        }

        public int CompareTo(TableData other)
        {
            var typeComparison = _type.CompareTo(other._type);
            return typeComparison != 0
                ? typeComparison
                : string.Compare(_name, other._name, StringComparison.Ordinal);
        }

        int IComparable.CompareTo(object obj)
        {
            return CompareTo((TableData)obj);
        }
    }
}