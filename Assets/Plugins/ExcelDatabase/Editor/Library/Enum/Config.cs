using System;
using System.IO;

namespace ExcelDatabase.Editor.Library.Enum
{
    public static class Config
    {
        public static int groupCol { get; } = 0;
        public static int enumCol { get; } = 1;
        public static int nameRow { get; } = 0;

        public static string groupTemplate { get; } = "#GROUP#";
        public static string rowTemplate { get; } = "#ROW#";
        public static string tableVariable { get; } = "$TABLE$";
        public static string groupVariable { get; } = "$GROUP$";
        public static string rowVariable { get; } = "$ROW$";

        public static string table => lazyTable.Value;
        public static string group => lazyGroup.Value;
        public static string row => lazyRow.Value;

        private static readonly Lazy<string> lazyTable = new(CreateTemplate("Table.txt"));
        private static readonly Lazy<string> lazyGroup = new(CreateTemplate("Group.txt"));
        private static readonly Lazy<string> lazyRow = new(CreateTemplate("Row.txt"));

        private static Func<string> CreateTemplate(string fileName)
        {
            return () =>
                File.ReadAllText($"{TableParser.root}/Editor/Library/Enum/Template/{fileName}");
        }
    }
}
