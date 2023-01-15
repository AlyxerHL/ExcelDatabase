using System.IO;
using UnityEditor;

namespace ExcelDatabase.Editor.Tools
{
    public static class Config
    {
        private static string _root;

        public static string Root
        {
            get
            {
                if (_root != null)
                {
                    return _root;
                }

                var assets = AssetDatabase.FindAssets("ExcelDatabaseRoot");
                var rootFilePath = AssetDatabase.GUIDToAssetPath(assets[0]);
                _root = Path.GetDirectoryName(rootFilePath);
                return _root;
            }
        }

        public static string EnumDistPath(string tableName)
        {
            return $"{Root}/Dist/Em.{tableName}.cs";
        }
    }
}