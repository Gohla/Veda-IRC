using System;
using Veda.Interface;

namespace Veda.Storage
{
    internal class NestedStorage : IStorage
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

        ~NestedStorage()
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

        public void Set(String id, object obj)
        {
            Storage.Set(id, obj);
        }

        public bool Remove(String id)
        {
            if(Storage.Remove(id))
                return true;
            if(Parent != null && Parent.Remove(id))
                return true;
            return false;
        }

        public bool Persist()
        {
            bool persisted = Storage.Persist();
            if(Parent != null)
                persisted &= Parent.Persist();
            return persisted;
        }
    }
}
