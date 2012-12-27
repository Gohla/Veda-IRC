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

        public T Get<T>(params String[] id)
        {
            if(Storage.Exists(id))
                return Storage.Get<T>(id);
            if(Parent != null)
                return Parent.Get<T>(id);
            return default(T);
        }

        public object Get(Type type, params String[] id)
        {
            if(Storage.Exists(id))
                return Storage.Get(type, id);
            if(Parent != null)
                return Parent.Get(type, id);
            return null;
        }

        public T GetOrCreate<T>(params String[] id) 
            where T : new()
        {
            if(Storage.Exists(id))
                return Storage.GetOrCreate<T>(id);
            if(Parent != null)
                return Parent.GetOrCreate<T>(id);
            return new T();
        }

        public bool Exists(params String[] id)
        {
            if(Storage.Exists(id))
                return true;
            if(Parent != null && Parent.Exists(id))
                return true;
            return false;
        }

        public void Set(object obj, params String[] id)
        {
            Storage.Set(obj, id);
        }

        public bool Remove(params String[] id)
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
