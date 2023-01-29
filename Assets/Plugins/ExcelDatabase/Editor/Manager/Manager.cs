using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExcelDatabase.Editor.Parser;
using ExcelDatabase.Editor.Tools;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ExcelDatabase.Editor.Manager
{
    public class Manager : EditorWindow
    {
        private static readonly string TableDataPath = $"{Config.DistPath}/TableData.json";
        private static SortedSet<TableData> _tableDataSet;

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

        private void ListTables()
        {
            VisualElement MakeItem()
            {
                var tableGroup = new GroupBox();
                tableGroup.AddToClassList("table-data");
                tableGroup.Add(new Label());

                var tableButtonGroup = new VisualElement();
                tableButtonGroup.AddToClassList("table-data-buttons");
                tableButtonGroup.Add(new Button { text = "Edit", name = "edit-button" });
                tableButtonGroup.Add(new Button { text = "Parse", name = "parse-button" });
                tableButtonGroup.Add(new Button { text = "Remove", name = "remove-button" });

                tableGroup.Add(tableButtonGroup);
                return tableGroup;
            }

            void BindItem(VisualElement e, int i)
            {
                if (e is not GroupBox tableGroup)
                {
                    return;
                }

                var label = tableGroup.Q<Label>();
                label.text = TableDataSet.ElementAt(i).ToString();
                // TODO Make buttons work
            }

            var listView = rootVisualElement.Q<ListView>();
            listView.makeItem = MakeItem;
            listView.bindItem = BindItem;
            listView.itemsSource = TableDataSet.ToList();
            listView.selectionType = SelectionType.None;

            listView.onItemsChosen += Debug.Log;
            listView.onSelectionChange += Debug.Log;
        }
    }
}