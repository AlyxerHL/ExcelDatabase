using System.IO;
using UnityEditor;

namespace ExcelDatabase.Editor.Library
{
    public static class Config
    {
        private static string _root;

        private static string Root
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

        public static readonly string DistPath = $"{Root}/Dist";
        public static readonly string TemplatePath = $"{Root}/Editor/Templates";
        public const string ExcludePrefix = "#";
    }
}