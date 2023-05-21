using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ExcelDatabase.Editor.GUI
{
    public class JsonEditor : EditorWindow
    {
        public static void Open(string jsonPath)
        {
            var window = GetWindow<JsonEditor>();
            var json = AssetDatabase.LoadAssetAtPath<TextAsset>(jsonPath);
            var table = JsonConvert.DeserializeObject<Dictionary<string, object>[]>(json.text);

            window.titleContent = new("Excel Database | Json Editor");
            window.RegisterButton(table, jsonPath);
            window.ListIDs(table);
        }

        public void CreateGUI()
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/Plugins/ExcelDatabase/Editor/GUI/JsonEditor.uxml"
            );
            rootVisualElement.Add(visualTree.Instantiate());

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/Plugins/ExcelDatabase/Editor/GUI/Style.uss"
            );
            rootVisualElement.styleSheets.Add(styleSheet);

            var splitView = new TwoPaneSplitView(0, 300f, TwoPaneSplitViewOrientation.Horizontal);
            splitView.Add(new ListView { name = "id-list" });
            splitView.Add(new ListView { name = "column-list" });
            rootVisualElement.Add(splitView);
        }

        private void RegisterButton(IDictionary<string, object>[] table, string jsonPath)
        {
            var button = rootVisualElement.Q<Button>("save-button");
            button.RegisterCallback<ClickEvent>(HandleSave);

            void HandleSave(ClickEvent _)
            {
                var json = JsonConvert.SerializeObject(table, Formatting.Indented);
                File.WriteAllText(jsonPath, json);
                AssetDatabase.Refresh();
                Debug.Log("Excel Database: Saving has been completed");
            }
        }

        private void ListIDs(IDictionary<string, object>[] table)
        {
            var idList = rootVisualElement.Q<ListView>("id-list");
            idList.bindItem = null;

            idList.itemsSource = table;
            idList.makeItem = MakeItem;
            idList.bindItem = BindItem;
            idList.onSelectionChange += OnSelectionChange;

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
                    label.text = table[i]["ID"] as string;
                }
            }

            void OnSelectionChange(IEnumerable<object> selection)
            {
                var columns = selection.First() as IDictionary<string, object>;
                ListColumns(columns);
            }
        }

        private void ListColumns(IDictionary<string, object> columns)
        {
            var columnsWithoutID = columns.Skip(1);
            var columnList = rootVisualElement.Q<ListView>("column-list");
            columnList.bindItem = null;

            columnList.itemsSource = columnsWithoutID.ToList();
            columnList.makeItem = MakeItem;
            columnList.bindItem = BindItem;

            VisualElement MakeItem()
            {
                var field = new TextField();
                field.AddToClassList("list-field");
                field.RegisterValueChangedCallback(OnValueChanged);
                return field;

                void OnValueChanged(ChangeEvent<string> e)
                {
                    if (e.target is TextField field)
                    {
                        columns[field.label] = field.multiline
                            ? e.newValue.Split("\n")
                            : e.newValue;
                    }
                }
            }

            void BindItem(VisualElement element, int i)
            {
                if (element is TextField field)
                {
                    var column = columnsWithoutID.ElementAt(i);
                    field.label = column.Key;

                    if (column.Value is string value)
                    {
                        field.multiline = false;
                        field.value = value;
                    }
                    else if (column.Value is JArray arrayValue)
                    {
                        field.multiline = true;
                        field.value = string.Join("\n", arrayValue);
                    }
                }
            }
        }
    }
}
