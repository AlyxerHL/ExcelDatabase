namespace ExcelDatabase.Editor.Config
{
    public static class TemplateConfig
    {
        public static class Convert
        {
            public static string TablePath => $"{Path}/Table.txt";
            public static string ConvertColPath => $"{Path}/ConvertCol.txt";
            public static string PrimitiveColPath => $"{Path}/PrimitiveCol.txt";

            private static string Path => $"{Config.Root}/Editor/Templates/Convert";
        }

        public static class Enum
        {
            public const string GroupTemplate = "#GROUP#";
            public const string RowTemplate = "#ROW#";
            public const string TableVariable = "$TABLE$";
            public const string GroupVariable = "$GROUP$";
            public const string RowVariable = "$ROW$";

            public static string TablePath => $"{Path}/Table.txt";
            public static string EnumPath => $"{Path}/Enum.txt";
            public static string RowPath => $"{Path}/Row.txt";

            private static string Path => $"{Config.Root}/Editor/Templates/Enum";
        }
    }
}