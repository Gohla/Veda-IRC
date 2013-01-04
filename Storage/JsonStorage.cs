using System;
using System.IO;
using System.Linq;
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
            _settings.Converters.Add(new NullableRefConverter());
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

        public object Get(Type type, params String[] id)
        {
            _lock.EnterReadLock();
            try
            {
                JToken token = GetNested(id);
                if(token != null)
                {
                    return JsonConvert.DeserializeObject(token.ToString(), type, new NullableRefConverter());
                }
                return null;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool Exists(params String[] id)
        {
            _lock.EnterReadLock();
            try
            {
                return GetNested(id) != null;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Set(object obj, params String[] id)
        {
            _lock.EnterWriteLock();
            try
            {
                SetNested(obj, id);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool Remove(params String[] id)
        {
            _lock.EnterWriteLock();
            try
            {
                JToken token = GetNested(id);
                if(token != null)
                {
                    token.Parent.Remove();
                    return true;
                }
                return false;
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

            if(_storage.Properties().Count() == 0)
            {
                if(File.Exists(_file))
                {
                    try
                    {
                        File.Delete(_file);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }

                return false;
            }

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

        private JToken GetNested(params String[] id)
        {
            if(id == null || id.Length == 0)
                throw new ArgumentException("Identifier cannot be null or empty.", "id");

            JObject jObject = _storage;
            foreach(String d in id.Take(id.Length - 1))
            {
                jObject = jObject[d] as JObject;
                if(jObject == null)
                    return null;
            }
            return jObject[id[id.Length - 1]];
        }

        private void SetNested(object obj, params String[] id)
        {
            if(id == null || id.Length == 0)
                throw new ArgumentException("Identifier cannot be null or empty.", "id");

            JObject jObject = _storage;
            foreach(String d in id.Take(id.Length - 1))
            {
                JObject innerJObject = jObject[d] as JObject;
                if(innerJObject == null)
                {
                    JObject newJObject = new JObject();
                    jObject[d] = newJObject;
                    jObject = newJObject;
                }
                else
                    jObject = innerJObject;
            }

            jObject[id[id.Length - 1]] = JToken.Parse(JsonConvert.SerializeObject(obj, Formatting.Indented, new NullableRefConverter()));
        }
    }
}
