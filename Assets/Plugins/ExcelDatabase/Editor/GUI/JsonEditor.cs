using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
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
            var table = JsonConvert.DeserializeObject<Dictionary<string, string>[]>(json.text);

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
        }

        private void RegisterButton(IDictionary<string, string>[] table, string jsonPath)
        {
            var button = rootVisualElement.Q<Button>("save-button");
            button.RegisterCallback<ClickEvent>(HandleSave);

            void HandleSave(ClickEvent _)
            {
                var json = JsonConvert.SerializeObject(table, Formatting.Indented);
                File.WriteAllText(jsonPath, json);
                AssetDatabase.Refresh();
            }
        }

        private void ListIDs(IDictionary<string, string>[] table)
        {
            var idList = rootVisualElement.Q<ListView>("id-list");
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
                    label.text = table[i]["ID"];
                }
            }

            void OnSelectionChange(IEnumerable<object> selection)
            {
                var columns = selection.First() as IDictionary<string, string>;
                ListColumns(columns);
            }
        }

        private void ListColumns(IDictionary<string, string> columns)
        {
            var columnList = rootVisualElement.Q<ListView>("column-list");
            columnList.itemsSource = columns.ToList();
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
                        columns[field.label] = e.newValue;
                    }
                }
            }

            void BindItem(VisualElement element, int i)
            {
                if (element is TextField field)
                {
                    var column = columns.ElementAt(i);
                    field.label = column.Key;
                    field.value = column.Value;

                    if (i == 0)
                    {
                        field.SetEnabled(false);
                    }
                }
            }
        }
    }
}
