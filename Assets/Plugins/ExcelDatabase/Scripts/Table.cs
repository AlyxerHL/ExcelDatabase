#nullable enable

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace ExcelDatabase.Scripts
{
    public class Table<T> : IEnumerable<T>
        where T : ITableType
    {
        private readonly Dictionary<string, T> collection;
        public T this[string id] => collection[id];

        public Table(string name)
        {
            var json = Resources.Load<TextAsset>($"ExcelDatabase/{name}");
            collection = JsonConvert
                .DeserializeObject<T[]>(json.text)
                .ToDictionary((row) => row.ID);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return collection.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
