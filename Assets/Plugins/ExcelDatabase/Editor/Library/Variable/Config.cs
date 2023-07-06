using System;
using System.IO;

namespace ExcelDatabase.Editor.Library.Variable
{
    public static class Config
    {
        public static int nameCol { get; } = 0;
        public static int typeCol { get; } = 1;
        public static int valueCol { get; } = 2;
        public static int nameRow { get; } = 0;

        public static string rowTemplate { get; } = "#ROW#";
        public static string tableVariable { get; } = "$TABLE$";
        public static string typeVariable { get; } = "$TYPE$";
        public static string nameVariable { get; } = "$NAME$";
        public static string valueVariable { get; } = "$VALUE$";

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
