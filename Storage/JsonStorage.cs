using System;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Veda.Interface;

namespace Veda.Storage
{
    public class JsonStorage : IOpenableStorage
    {
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private JsonSerializerSettings _settings = new JsonSerializerSettings();
        private JObject _storage = new JObject();
        private String _file = null;
        private String _path = null;

        public JsonStorage()
        {
            _settings.TypeNameHandling = TypeNameHandling.Auto;
        }

        ~JsonStorage()
        {
            Dispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Persist();
        }

        public void Open(String file)
        {
            _file = file;
            _path = Path.GetDirectoryName(_file);
            LoadConfiguration();
        }

        public T Get<T>(String id)
        {
            _lock.EnterReadLock();
            try
            {
                JToken obj;
                if(_storage.TryGetValue(id, out obj))
                {
                    return JsonConvert.DeserializeObject<T>(obj.ToString(), _settings);
                }
                return default(T);
            }
            finally
            {
                _lock.ExitReadLock();	
            }
        }

        public T GetOrCreate<T>(String id)
            where T : new()
        {
            _lock.EnterReadLock();
            try
            {
	            JToken obj;
	            if(_storage.TryGetValue(id, out obj))
	            {
	                return JsonConvert.DeserializeObject<T>(obj.ToString(), _settings);
	            }
	            return new T();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool Exists(String id)
        {
            _lock.EnterReadLock();
            try
            {
            	return _storage[id] != null;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Set(String id, object obj)
        {
            _lock.EnterWriteLock();
            try
            {
            	_storage[id] = JToken.Parse(JsonConvert.SerializeObject(obj, Formatting.Indented, _settings));
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool Remove(String id)
        {
            _lock.EnterWriteLock();
            try
            {
            	return _storage.Remove(id);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool Persist()
        {
            if(_file == null)
                return false;

            if(!Directory.Exists(_path))
                Directory.CreateDirectory(_path);

            _lock.EnterReadLock();
            try
            {
                using(TextWriter textWriter = new StreamWriter(_file))
                {
                    JsonTextWriter writer = new JsonTextWriter(textWriter);
                    writer.Formatting = Formatting.Indented;
                    _storage.WriteTo(writer);
                    writer.Close();
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }

            return true;
        }

        private void LoadConfiguration()
        {
            if(!File.Exists(_file))
                return;

            _lock.EnterWriteLock();
            try
            {
                using(TextReader textReader = new StreamReader(_file))
                {
                    JsonTextReader reader = new JsonTextReader(textReader);
                    _storage = JObject.Load(reader);
                    reader.Close();
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}
