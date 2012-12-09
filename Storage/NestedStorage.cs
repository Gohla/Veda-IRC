using System;
using Gohla.Shared;
using Veda.Interface;

namespace Veda.Storage
{
    public class NestedStorage : IDisposable, IKeyedObject<String>
    {
        public String Key { get; private set; }
        public IStorage Storage { get; private set; }
        public KeyedCollection<String, NestedStorage> Nested { get; private set; }

        public NestedStorage(String key, IStorage storage)
        {
            Key = key;
            Storage = storage;
            Nested = new KeyedCollection<String, NestedStorage>();
        }

        public void Dispose()
        {
            if(Storage == null)
                return;

            Storage.Dispose();
            Storage = null;
            Nested.Do(x => x.Dispose());
            Nested.Clear();
            Nested.Dispose();
            Nested = null;
        }
    }
}
