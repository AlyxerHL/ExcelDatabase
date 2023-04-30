using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExcelDatabase.Editor.Parser;
using ExcelDatabase.Editor.Library;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ExcelDatabase.Editor.GUI
{
    public class Manager : EditorWindow
    {
        private static IEnumerable<ParseResult> _selection;
        private static SortedSet<ParseResult> _resultSet;

        private static SortedSet<ParseResult> ResultSet
        {
            get
            {
                _resultSet ??= File.Exists(ResultPath)
                    ? JsonConvert.DeserializeObject<SortedSet<ParseResult>>(
                        File.ReadAllText(ResultPath)
                    )
                    : new();
                return _resultSet;
            }
        }

        private static string ResultPath => $"{Config.DistPath}/ParseResult.json";

        [MenuItem("Tools/Excel Database/Show Manager")]
        public static void ShowManager()
        {
            var window = GetWindow<Manager>();
            window.titleContent = new("Excel Database | Manager");
        }

        [MenuItem("Tools/Excel Database/Parse Convert Tables")]
        public static void ParseConvertTables()
        {
            ParseTables(Selection.objects, TableType.Convert);
        }

        [MenuItem("Tools/Excel Database/Parse Enum Tables")]
        public static void ParseEnumTables()
        {
            ParseTables(Selection.objects, TableType.Enum);
        }

        [MenuItem("Tools/Excel Database/Parse Variable Tables")]
        public static void ParseVariableTables()
        {
            ParseTables(Selection.objects, TableType.Variable);
        }

        public void CreateGUI()
        {
            ApplyUI();
            RegisterButtons();
            ListTables();
        }

        private void ApplyUI()
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/Plugins/ExcelDatabase/Editor/GUI/Manager.uxml"
            );
            rootVisualElement.Add(visualTree.Instantiate());

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/Plugins/ExcelDatabase/Editor/GUI/Style.uss"
            );
            rootVisualElement.styleSheets.Add(styleSheet);
        }

        private void RegisterButtons()
        {
            var editButton = rootVisualElement.Q<Button>("edit-button");
            editButton.RegisterCallback<ClickEvent>(HandleEdit);
            editButton.SetEnabled(false);

            var parseButton = rootVisualElement.Q<Button>("parse-button");
            parseButton.RegisterCallback<ClickEvent>(HandleParse);
            parseButton.SetEnabled(false);

            var removeButton = rootVisualElement.Q<Button>("remove-button");
            removeButton.RegisterCallback<ClickEvent>(HandleRemove);
            removeButton.SetEnabled(false);

            void HandleEdit(ClickEvent _)
            {
                JsonEditor.Open(_selection.First().DistPaths[1]);
            }

            void HandleParse(ClickEvent _)
            {
                foreach (var parseResults in _selection.GroupBy(table => table.Type))
                {
                    var files = parseResults.Select(
                        (table) => AssetDatabase.LoadAssetAtPath<Object>(table.ExcelPath)
                    );
                    ParseTables(files, parseResults.Key);
                }
            }

            void HandleRemove(ClickEvent _)
            {
                RemoveTables(_selection);
            }
        }

        private void ListTables()
        {
            var listView = rootVisualElement.Q<ListView>();
            listView.bindItem = null;

            listView.itemsSource = ResultSet.ToList();
            listView.makeItem = MakeItem;
            listView.bindItem = BindItem;
            listView.onSelectionChange += OnSelectionChange;

            VisualElement MakeItem()
            {
                var label = new Label();
                label.AddToClassList("list-label");
                return label;
            }

            void BindItem(VisualElement element, int i)
            {
                if (element is Label label)
                {
                    label.text = ResultSet.ElementAt(i).ToString();
                }
            }

            void OnSelectionChange(IEnumerable<object> selection)
            {
                _selection = selection.Cast<ParseResult>();
                var editButton = rootVisualElement.Q<Button>("edit-button");
                editButton.SetEnabled(
                    _selection?.Count() == 1 && _selection?.First().Type == TableType.Convert
                );

                var parseButton = rootVisualElement.Q<Button>("parse-button");
                parseButton.SetEnabled(_selection?.Count() > 0);
                var removeButton = rootVisualElement.Q<Button>("remove-button");
                removeButton.SetEnabled(_selection?.Count() > 0);
            }
        }

        private static void ParseTables(IEnumerable<Object> files, TableType type)
        {
            foreach (var file in files.Where(IsExcelFile))
            {
                IParser parser = type switch
                {
                    TableType.Convert => new ConvertParser(file),
                    TableType.Enum => new EnumParser(file),
                    TableType.Variable => new VariableParser(file),
                    _ => throw new ArgumentOutOfRangeException()
                };

                try
                {
                    var result = parser.Parse();
                    if (ResultSet.Add(result))
                    {
                        SyncResultSet();
                    }
                }
                catch (ParserException e)
                {
                    Debug.LogError($"{e.TableName}: {e.Message}");
                }
            }

            AssetDatabase.Refresh();
            Debug.Log("Excel Database: Parsing has been completed");

            static bool IsExcelFile(Object file)
            {
                var path = AssetDatabase.GetAssetPath(file);
                return Path.GetExtension(path) == ".xlsx"
                    && !Path.GetFileName(path).StartsWith(Config.ExcludePrefix);
            }
        }

        private static void RemoveTables(IEnumerable<ParseResult> tables)
        {
            foreach (var table in tables)
            {
                ResultSet.Remove(table);
                foreach (var distPath in table.DistPaths)
                {
                    AssetDatabase.DeleteAsset(distPath);
                }
            }

            SyncResultSet();
            Debug.Log("Excel Database: Removing has been completed");
        }

        private static void SyncResultSet()
        {
            var json = JsonConvert.SerializeObject(ResultSet, Formatting.Indented);
            File.WriteAllText(ResultPath, json);
        }
    }
}
