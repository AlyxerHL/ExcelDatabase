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
        private static readonly string TableDataPath = $"{Config.DistPath}/TableData.json";
        private static SortedSet<TableData> _tableDataSet;
        private static IEnumerable<Object> _selection;

        private static SortedSet<TableData> TableDataSet
        {
            get
            {
                if (_tableDataSet != null)
                {
                    return _tableDataSet;
                }

                if (File.Exists(TableDataPath))
                {
                    var json = File.ReadAllText(TableDataPath);
                    _tableDataSet = JsonConvert.DeserializeObject<SortedSet<TableData>>(json);
                }
                else
                {
                    _tableDataSet = new SortedSet<TableData>();
                }

                return _tableDataSet;
            }
        }

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
                try
                {
                    var tableData = new EnumParser(file).Parse();
                    if (TableDataSet.Add(tableData))
                    {
                        SyncTableData();
                    }
                }
                catch (InvalidTableException e)
                {
                    Debug.LogError($"{e.TableName}: {e.Message}");
                }
            }

            AssetDatabase.Refresh();
        }

        private static bool IsExcelFile(Object file)
        {
            var path = AssetDatabase.GetAssetPath(file);
            return Path.GetExtension(path) == ".xlsx";
        }

        private static void SyncTableData()
        {
            var json = JsonConvert.SerializeObject(TableDataSet);
            File.WriteAllText(TableDataPath, json);
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
            rootVisualElement.Q<Button>("edit-button")
                .RegisterCallback<ClickEvent>(_ => Debug.Log("Edit Button"));
            rootVisualElement.Q<Button>("parse-button")
                .RegisterCallback<ClickEvent>(_ => Debug.Log("Parse Button"));
            rootVisualElement.Q<Button>("remove-button")
                .RegisterCallback<ClickEvent>(_ => Debug.Log("Remove Button"));
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
                    label.text = TableDataSet.ElementAt(i).ToString();
                }
            }

            var listView = rootVisualElement.Q<ListView>();
            listView.makeItem = MakeItem;
            listView.bindItem = BindItem;
            listView.itemsSource = TableDataSet.ToList();

            listView.onSelectionChange += tables =>
                _selection = tables.Select(table =>
                    AssetDatabase.LoadAssetAtPath<Object>(((TableData)table).ExcelPath));
        }
    }
}