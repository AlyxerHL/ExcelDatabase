using System.IO;
using UnityEditor;

namespace ExcelDatabase.Editor.Library
{
    public static class Config
    {
        private static string root_;
        public static string root
        {
            get
            {
                if (root_ is not null)
                {
                    return root_;
                }

                var assets = AssetDatabase.FindAssets("ExcelDatabaseRoot");
                var rootFilePath = AssetDatabase.GUIDToAssetPath(assets[0]);
                root_ = Path.GetDirectoryName(rootFilePath);
                return root_;
            }
        }

        public static string templatePath { get; } = $"{root}/Editor/Templates";
        public static string excludePrefix { get; } = "#";

        public static string DistPath(string tableName, TableType tableType) =>
            $"{root}/Dist/{tableType}/{tableName}.cs";

        public static string JsonPath(string tableName) =>
            $"Assets/Resources/ExcelDatabase/{tableName}.json";
    }
}
