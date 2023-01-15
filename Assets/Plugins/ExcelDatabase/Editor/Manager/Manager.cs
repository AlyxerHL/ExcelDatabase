using System.IO;
using System.Linq;
using ExcelDatabase.Editor.Parser;
using ExcelDatabase.Editor.Tools;
using NPOI.XSSF.UserModel;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ExcelDatabase.Editor.Manager
{
    public class Manager : EditorWindow
    {
        [MenuItem("Tools/Excel Database/Show Manager")]
        private static void ShowManager()
        {
            var window = GetWindow<Manager>();
            window.titleContent = new GUIContent("Excel Database Manager");
        }

        [MenuItem("Tools/Excel Database/Parse Enum")]
        private static void ParseEnum()
        {
            foreach (var file in Selection.objects.Where(IsExcelFile))
            {
                var path = AssetDatabase.GetAssetPath(file);
                using var stream = File.Open(path, FileMode.Open, FileAccess.Read);
                var workbook = new XSSFWorkbook(stream);

                try
                {
                    var enumParser = new EnumParser(workbook, file.name);
                    enumParser.Parse();
                }
                catch (InvalidTableException e)
                {
                    Debug.LogError(e.Message);
                }
            }

            AssetDatabase.Refresh();
        }

        private static bool IsExcelFile(Object file)
        {
            var path = AssetDatabase.GetAssetPath(file);
            return Path.GetExtension(path) == ".xlsx";
        }

        private void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            var root = rootVisualElement;

            // VisualElements objects can contain other VisualElement following a tree hierarchy.
            VisualElement label = new Label("Hello World! From C#");
            root.Add(label);

            // Import UXML
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Assets/Plugins/ExcelDatabase/Editor/Manager/Manager.uxml");
            VisualElement labelFromUxml = visualTree.Instantiate();
            root.Add(labelFromUxml);

            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            var styleSheet =
                AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Plugins/ExcelDatabase/Editor/Manager/Manager.uss");
            VisualElement labelWithStyle = new Label("Hello World! With Style");
            labelWithStyle.styleSheets.Add(styleSheet);
            root.Add(labelWithStyle);
        }
    }
}