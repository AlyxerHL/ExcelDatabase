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

                var assets = AssetDatabase.FindAssets("ExcelDatabase.root");
                var rootFilePath = AssetDatabase.GUIDToAssetPath(assets[0]);
                _root = Path.GetDirectoryName(rootFilePath);
                return _root;
            }
        }
    }
}