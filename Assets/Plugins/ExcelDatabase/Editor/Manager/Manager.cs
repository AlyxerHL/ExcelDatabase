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

namespace ExcelDatabase.Editor.Manager
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
                    ? JsonConvert.DeserializeObject<SortedSet<ParseResult>>(File.ReadAllText(ResultPath))
                    : new SortedSet<ParseResult>();
                return _resultSet;
            }
        }

        private static string ResultPath => $"{Config.DistPath}/ParseResult.json";

        [MenuItem("Tools/Excel Database/Show Manager")]
        private static void ShowManager()
        {
            var window = GetWindow<Manager>();
            window.titleContent = new GUIContent("Excel Database Manager");
        }

        [MenuItem("Tools/Excel Database/Parse Convert Tables")]
        private static void ParseConvertTables()
        {
            ParseTables(Selection.objects, TableType.Convert);
        }

        [MenuItem("Tools/Excel Database/Parse Enum Tables")]
        private static void ParseEnumTables()
        {
            ParseTables(Selection.objects, TableType.Enum);
        }

        [MenuItem("Tools/Excel Database/Parse Variable Tables")]
        private static void ParseVariableTables()
        {
            ParseTables(Selection.objects, TableType.Variable);
        }

        private void CreateGUI()
        {
            ApplyUI();
            RegisterButtons();
            ListTables();
        }

        private void ApplyUI()
        {
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Assets/Plugins/ExcelDatabase/Editor/Manager/Manager.uxml");
            rootVisualElement.Add(visualTree.Instantiate());

            var styleSheet =
                AssetDatabase.LoadAssetAtPath<StyleSheet>(
                    "Assets/Plugins/ExcelDatabase/Editor/Manager/Manager.uss");
            rootVisualElement.styleSheets.Add(styleSheet);
        }

        private void RegisterButtons()
        {
            rootVisualElement.Q<Button>("edit-button").RegisterCallback<ClickEvent>(HandleEdit);
            rootVisualElement.Q<Button>("parse-button").RegisterCallback<ClickEvent>(HandleParse);
            rootVisualElement.Q<Button>("remove-button").RegisterCallback<ClickEvent>(HandleRemove);

            void HandleEdit(ClickEvent _)
            {
                Debug.Log("Edit Button");
            }

            void HandleParse(ClickEvent _)
            {
                foreach (var parseResults
                         in _selection.GroupBy(table => table.Type))
                {
                    var files = parseResults.Select(table =>
                        AssetDatabase.LoadAssetAtPath<Object>(table.ExcelPath));
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
            listView.itemsSource = ResultSet.ToList();
            listView.makeItem = MakeItem;
            listView.bindItem = BindItem;
            listView.onSelectionChange += HandleSelectionChange;

            VisualElement MakeItem()
            {
                var label = new Label();
                label.AddToClassList("table-label");
                return label;
            }

            void BindItem(VisualElement e, int i)
            {
                if (e is Label label)
                {
                    label.text = ResultSet.ElementAt(i).ToString();
                }
            }

            void HandleSelectionChange(IEnumerable<object> selection)
            {
                _selection = selection.Cast<ParseResult>();
            }
        }

        private static void ParseTables(IEnumerable<Object> files, TableType type)
        {
            var loopCount = 0;
            var queue = new Queue<Object>(files.Where(file =>
            {
                var path = AssetDatabase.GetAssetPath(file);
                return Path.GetExtension(path) == ".xlsx";
            }));

            while (queue.TryDequeue(out var file))
            {
                loopCount++;
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
                    if (e.Yielding && loopCount < 100)
                    {
                        queue.Enqueue(file);
                    }
                    else
                    {
                        Debug.LogError($"{e.TableName}: {e.Message}");
                    }
                }
            }

            AssetDatabase.Refresh();
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
        }

        private static void SyncResultSet()
        {
            var json = JsonConvert.SerializeObject(ResultSet, Formatting.Indented);
            File.WriteAllText(ResultPath, json);
        }
    }
}