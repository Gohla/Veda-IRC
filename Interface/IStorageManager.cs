using System;

namespace Veda.Interface
{
    public interface IStorageManager : IDisposable
    {
        void Open(String configPath, String configFile);

        T Get<T>(String identifier);
        bool Exists(String identifier);
        void Set<T>(String identifier, T value);
    }
}
