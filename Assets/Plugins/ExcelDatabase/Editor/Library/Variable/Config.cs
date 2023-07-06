using System;
using System.IO;

namespace ExcelDatabase.Editor.Library.Variable
{
    public static class Config
    {
        public static int NameCol { get; } = 0;
        public static int TypeCol { get; } = 1;
        public static int ValueCol { get; } = 2;
        public static int NameRow { get; } = 0;

        public static string RowTemplate { get; } = "#ROW#";
        public static string TableVariable { get; } = "$TABLE$";
        public static string TypeVariable { get; } = "$TYPE$";
        public static string NameVariable { get; } = "$NAME$";
        public static string ValueVariable { get; } = "$VALUE$";

        public static string table => lazyTable.Value;
        public static string row => lazyRow.Value;

        private static readonly Lazy<string> lazyTable = new(CreateTemplate("Table.txt"));
        private static readonly Lazy<string> lazyRow = new(CreateTemplate("Row.txt"));

        private static Func<string> CreateTemplate(string fileName)
        {
            return () =>
                File.ReadAllText($"{TableParser.root}/Editor/Library/Variable/Template/{fileName}");
        }
    }
}
