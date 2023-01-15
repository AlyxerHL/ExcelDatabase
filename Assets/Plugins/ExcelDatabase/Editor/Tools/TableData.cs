using System;

namespace ExcelDatabase.Editor.Tools
{
    public readonly struct TableData : IComparable<TableData>, IComparable
    {
        public readonly TableType Type;
        public readonly string Name;
        public readonly string ExcelPath;
        public readonly string[] DistPaths;

        public TableData(TableType type, string name, string excelPath, string[] distPaths)
        {
            Type = type;
            Name = name;
            ExcelPath = excelPath;
            DistPaths = distPaths;
        }

        public int CompareTo(TableData other)
        {
            var typeComparison = Type.CompareTo(other.Type);
            return typeComparison != 0
                ? typeComparison
                : string.Compare(Name, other.Name, StringComparison.Ordinal);
        }

        int IComparable.CompareTo(object obj)
        {
            return CompareTo((TableData)obj);
        }
    }
}