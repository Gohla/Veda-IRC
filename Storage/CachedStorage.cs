using System;
using System.Collections.Generic;
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

        public T Get<T>(params String[] id)
        {
            if(_cache.ContainsKey(FromIdentifier(id)))
            {
                return (T)_cache[FromIdentifier(id)];
            }
            else
            {
                T obj = Storage.Get<T>(id);
                if(obj != null)
                    _cache[FromIdentifier(id)] = obj;
                return obj;
            }
        }

        public object Get(Type type, params String[] id)
        {
            if(_cache.ContainsKey(FromIdentifier(id)))
            {
                return _cache[FromIdentifier(id)];
            }
            else
            {
                object obj = Storage.Get(type, id);
                if(obj != null)
                    _cache[FromIdentifier(id)] = obj;
                return obj;
            }
        }

        public T GetOrCreate<T>(params String[] id) 
            where T : new()
        {
            if(_cache.ContainsKey(FromIdentifier(id)))
            {
                return (T)_cache[FromIdentifier(id)];
            }
            else
            {
                T t = Storage.GetOrCreate<T>(id);
                _cache[FromIdentifier(id)] = t;
                return t;
            }
        }

        public bool Exists(params String[] id)
        {
            if(_cache.ContainsKey(FromIdentifier(id)))
            {
                return true;
            }
            else
            {
                return Storage.Exists(id);
            }
        }

        public void Set(object obj, params String[] id)
        {
            _cache[FromIdentifier(id)] = obj;
            Storage.Set(obj, id);
        }

        public bool Remove(params String[] id)
        {
            _cache.Remove(FromIdentifier(id));
            return Storage.Remove(id);
        }

        public bool Persist()
        {
            foreach(KeyValuePair<String, object> pair in _cache)
            {
                Storage.Set(pair.Value, ToIdentifier(pair.Key));
            }
            return Storage.Persist();
        }

        private String FromIdentifier(params String[] id)
        {
            return id.ToString("_");
        }

        private String[] ToIdentifier(String id)
        {
            return id.Split('_');
        }
    }
}
