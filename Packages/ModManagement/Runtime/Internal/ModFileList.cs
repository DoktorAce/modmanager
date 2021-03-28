using System;
using System.Collections.Generic;

namespace ModManagement.Internal
{
    public class ModFileList<TKey, TValue> : IDisposable
    {
        private List<string> _names = new List<string>();
        private List<TKey> _keys = new List<TKey>();
        private List<bool> _isCached = new List<bool>();
        private List<TValue> _cache = new List<TValue>();

        Func<TKey, byte[]> _byteLoader;
        Func<byte[], TValue> _deserailizer;

        public ModFileList(Func<TKey, byte[]> byteLoader, Func<byte[], TValue> deserailizer)
        {
            _byteLoader = byteLoader;
            _deserailizer = deserailizer;
        }

        public void Add(string name, TKey key)
        {
            _names.Add(name);
            _keys.Add(key);
            _isCached.Add(false);
            _cache.Add(default);
        }

        public bool Has(string file) => _names.Contains(file);

        public TValue Get(string file)
        {
            var index = _names.IndexOf(file);
            if(index < 0) throw new KeyNotFoundException($"This file list does not contain an item with the name {file}");

            if(_isCached[index]) return _cache[index];

            var bytes = _byteLoader.Invoke(_keys[index]);
            var obj = _deserailizer.Invoke(bytes);

            _cache[index] = obj;
            _isCached[index] = true;

            return obj;
        }

        public int count => _names.Count;
        public string[] List() => _names.ToArray();

        public void Clear()
        {
            _names.Clear();
            _keys.Clear();
            _isCached.Clear();
            _cache.Clear();
        }

        public void Dispose()
        {
            Clear();
            _byteLoader = null;
            _deserailizer = null;
        }

        public override string ToString()
        {
            var list = new List<string>();
            for(var i = 0; i < _names.Count; i ++)
            {
                list.Add($"({_keys[i].ToString()}) {_names[i]}");
            }
            return string.Join(", ", list);
        }
    }
}