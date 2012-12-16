using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veda.Interface;

namespace Veda.Storage
{
    internal class CachedStorage : IOpenableStorage
    {
        private Dictionary<String, object> _cache = new Dictionary<String, object>();

        public IOpenableStorage Storage { get; private set; }

        public CachedStorage(IOpenableStorage storage)
        {
            Storage = storage;
        }

        ~CachedStorage()
        {
            Dispose();
        }

        public void Dispose()
        {
            if(Storage == null)
                return;

            GC.SuppressFinalize(this);
            Persist();

            Storage.Dispose();
        }

        public void Open(String file)
        {
            Storage.Open(file);
        }

        public T Get<T>(String id)
        {
            if(_cache.ContainsKey(id))
            {
                return (T)_cache[id];
            }
            else
            {
                T obj = Storage.Get<T>(id);
                if(obj != null)
                    _cache[id] = obj;
                return obj;
            }
        }

        public object Get(String id, Type type)
        {
            if(_cache.ContainsKey(id))
            {
                return _cache[id];
            }
            else
            {
                object obj = Storage.Get(id, type);
                if(obj != null)
                    _cache[id] = obj;
                return obj;
            }
        }

        public T GetOrCreate<T>(String id) 
            where T : new()
        {
            if(_cache.ContainsKey(id))
            {
                return (T)_cache[id];
            }
            else
            {
                T t = Storage.GetOrCreate<T>(id);
                _cache[id] = t;
                return t;
            }
        }

        public bool Exists(String id)
        {
            if(_cache.ContainsKey(id))
            {
                return true;
            }
            else
            {
                return Storage.Exists(id);
            }
        }

        public void Set(String id, object obj)
        {
            _cache[id] = obj;
            Storage.Set(id, obj);
        }

        public bool Remove(String id)
        {
            _cache.Remove(id);
            return Storage.Remove(id);
        }

        public bool Persist()
        {
            foreach(KeyValuePair<String, object> pair in _cache)
            {
                Storage.Set(pair.Key, pair.Value);
            }
            return Storage.Persist();
        }
    }
}
