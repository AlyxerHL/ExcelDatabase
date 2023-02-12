using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace ExcelDatabase.Scripts
{
    public class Table<T> : IEnumerable<T> where T : TableType
    {
        private readonly Dictionary<string, T> _collection;

        public Table(string name)
        {
            var json = Resources.Load<TextAsset>($"ExcelDatabase/{name}");
            _collection = JsonConvert
                .DeserializeObject<T[]>(json.text)?
                .ToDictionary(row => row.ID);
        }

        public T this[string id] => _collection[id];

        public IEnumerator<T> GetEnumerator()
        {
            return _collection.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}