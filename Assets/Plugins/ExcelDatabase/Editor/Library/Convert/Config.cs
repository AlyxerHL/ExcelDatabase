using System;
using System.IO;

namespace ExcelDatabase.Editor.Library.Convert
{
    public static class Config
    {
        public static int nameRow { get; } = 0;
        public static int typeRow { get; } = 1;
        public static int idCol { get; } = 0;

        public static string arraySeparator { get; } = "\n";
        public static string colTemplate { get; } = "#COL#";
        public static string tableVariable { get; } = "$TABLE$";
        public static string typeVariable { get; } = "$TYPE$";
        public static string nameVariable { get; } = "$NAME$";

        public static string tableTemplate => lazyTableTemplate.Value;
        public static string generalCol => lazyGeneralCol.Value;
        public static string generalNullCol => lazyGeneralNullCol.Value;
        public static string convertCol => lazyConvertCol.Value;
        public static string convertNullCol => lazyConvertNullCol.Value;
        public static string generalArrCol => lazyGeneralArrCol.Value;
        public static string generalNullArrCol => lazyGeneralNullArrCol.Value;
        public static string convertArrCol => lazyConvertArrCol.Value;
        public static string convertNullArrCol => lazyConvertNullArrCol.Value;

        private static readonly Lazy<string> lazyTableTemplate = new(CreateTemplate("Table.txt"));
        private static readonly Lazy<string> lazyGeneralCol = new(CreateTemplate("GeneralCol.txt"));
        private static readonly Lazy<string> lazyGeneralNullCol =
            new(CreateTemplate("GeneralNullCol.txt"));
        private static readonly Lazy<string> lazyConvertCol = new(CreateTemplate("ConvertCol.txt"));
        private static readonly Lazy<string> lazyConvertNullCol =
            new(CreateTemplate("ConvertNullCol.txt"));
        private static readonly Lazy<string> lazyGeneralArrCol =
            new(CreateTemplate("GeneralArrCol.txt"));
        private static readonly Lazy<string> lazyGeneralNullArrCol =
            new(CreateTemplate("GeneralNullArrCol.txt"));
        private static readonly Lazy<string> lazyConvertArrCol =
            new(CreateTemplate("ConvertArrCol.txt"));
        private static readonly Lazy<string> lazyConvertNullArrCol =
            new(CreateTemplate("ConvertNullArrCol.txt"));

        private static Func<string> CreateTemplate(string fileName)
        {
            return () =>
                File.ReadAllText($"{TableParser.root}/Editor/Library/Convert/Template/{fileName}");
        }
    }
}
