using System;
using Veda.Interface;

namespace Veda.Storage
{
    public class NestedStorage : IStorage
    {
        public IStorage Storage { get; private set; }
        public IStorage Parent { get; private set; }

        public NestedStorage(IStorage storage)
        {
            Storage = storage;
        }

        public NestedStorage(IStorage storage, IStorage parent)
            : this(storage)
        {
            Parent = parent;
        }

        public void Dispose()
        {
            if(Storage == null)
                return;

            Storage.Dispose();
            Storage = null;
            if(Parent != null)
            {
                Parent.Dispose();
                Parent = null;
            }
        }

        public T Get<T>(String id)
        {
            if(Storage.Exists(id))
                return Storage.Get<T>(id);
            if(Parent != null)
                return Parent.Get<T>(id);
            return default(T);
        }

        public T GetOrCreate<T>(String id) 
            where T : new()
        {
            if(Storage.Exists(id))
                return Storage.GetOrCreate<T>(id);
            if(Parent != null)
                return Parent.GetOrCreate<T>(id);
            return new T();
        }

        public bool Exists(String id)
        {
            if(Storage.Exists(id))
                return true;
            if(Parent != null && Parent.Exists(id))
                return true;
            return false;
        }

        public void Set<T>(String id, T obj)
        {
            Storage.Set<T>(id, obj);
        }

        public bool Remove(String id)
        {
            if(Storage.Remove(id))
                return true;
            if(Parent != null && Parent.Remove(id))
                return true;
            return false;
        }
    }
}
