using System.IO;
using UnityEditor;

namespace ExcelDatabase.Editor.Config
{
    public static partial class Config
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

                var assets = AssetDatabase.FindAssets("ExcelDatabase.root");
                var rootFilePath = AssetDatabase.GUIDToAssetPath(assets[0]);
                _root = Path.GetDirectoryName(rootFilePath);
                return _root;
            }
        }
    }
}