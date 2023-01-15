using System.IO;
using ExcelDatabase.Editor.Parser;
using NPOI.XSSF.UserModel;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ExcelDatabase.Editor.Manager
{
    public class Manager : EditorWindow
    {
        [MenuItem("Tools/Excel Database/Show Manager")]
        private static void ShowWindow()
        {
            var window = GetWindow<Manager>();
            window.titleContent = new GUIContent("Excel Database Manager");
        }

        [MenuItem("Tools/Excel Database/Parse Selection")]
        private static void ParseSelection()
        {
            var file = Selection.objects[0];
            using var stream = File.Open(AssetDatabase.GetAssetPath(file), FileMode.Open, FileAccess.Read);
            var workbook = new XSSFWorkbook(stream);
            var enumParser = new EnumParser(workbook, file.name);
            enumParser.Parse();
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