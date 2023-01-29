using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExcelDatabase.Editor.Parser;
using ExcelDatabase.Editor.Tools;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

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
                if (_resultSet != null)
                {
                    return _resultSet;
                }

                if (File.Exists(ResultPath))
                {
                    var json = File.ReadAllText(ResultPath);
                    _resultSet = JsonConvert.DeserializeObject<SortedSet<ParseResult>>(json);
                }
                else
                {
                    _resultSet = new SortedSet<ParseResult>();
                }

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

        [MenuItem("Tools/Excel Database/Parse Selection")]
        private static void ParseSelection()
        {
            ParseTables(Selection.objects);
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
            void HandleEdit(ClickEvent _)
            {
                Debug.Log("Edit Button");
            }

            void HandleParse(ClickEvent _)
            {
                var obj = _selection.Select(table => AssetDatabase.LoadAssetAtPath<Object>(table.ExcelPath));
                ParseTables(obj);
            }

            void HandleRemove(ClickEvent _)
            {
                RemoveTables(_selection);
            }

            rootVisualElement.Q<Button>("edit-button").RegisterCallback<ClickEvent>(HandleEdit);
            rootVisualElement.Q<Button>("parse-button").RegisterCallback<ClickEvent>(HandleParse);
            rootVisualElement.Q<Button>("remove-button").RegisterCallback<ClickEvent>(HandleRemove);
        }

        private void ListTables()
        {
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

            var listView = rootVisualElement.Q<ListView>();
            listView.itemsSource = ResultSet.ToList();
            listView.makeItem = MakeItem;
            listView.bindItem = BindItem;
            listView.onSelectionChange += HandleSelectionChange;
        }

        private static void ParseTables(IEnumerable<Object> files)
        {
            bool IsExcelFile(Object file)
            {
                var path = AssetDatabase.GetAssetPath(file);
                return Path.GetExtension(path) == ".xlsx";
            }

            foreach (var file in files.Where(IsExcelFile))
            {
                try
                {
                    var tableData = new EnumParser(file).Parse();
                    if (ResultSet.Add(tableData))
                    {
                        SyncResultSet();
                    }
                }
                catch (InvalidTableException e)
                {
                    Debug.LogError($"{e.TableName}: {e.Message}");
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
            var json = JsonConvert.SerializeObject(ResultSet);
            File.WriteAllText(ResultPath, json);
        }
    }
}