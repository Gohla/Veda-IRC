using System;
using System.IO;
using Gohla.Shared.Composition;
using Veda.Interface;

namespace Veda.Storage
{
    public class StorageManager : IStorageManager
    {
        private NestedStorage _storage;
        private String _storagePath;

        public StorageManager()
        {

        }

        public void Dispose()
        {
            if(_storage == null)
                return;

            _storage.Dispose();
            _storage = null;
        }

        public void Open(String storagePath, String globalStorageFile)
        {
            _storagePath = storagePath;

            IStorage globalStorage = CompositionManager.Get<IStorage>();
            globalStorage.Open(Path.Combine(storagePath, globalStorageFile));
            _storage = new NestedStorage("Global", globalStorage);
        }

        private void EnsureOpened()
        {
            if(_storage == null)
                throw new InvalidOperationException("Storage has not been opened, open storage with the Open method.");
        }

        public T Get<T>(String identifier)
        {
            EnsureOpened();
            return _storage.Storage.Get<T>(identifier);
        }

        public bool Exists(String identifier)
        {
            EnsureOpened();
            return _storage.Storage.Exists(identifier);
        }

        public void Set<T>(String identifier, T value)
        {
            EnsureOpened();
            _storage.Storage.Set<T>(identifier, value);
        }
    }
}
