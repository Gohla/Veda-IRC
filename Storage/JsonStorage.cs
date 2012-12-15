using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Veda.Interface;

namespace Veda.Storage
{
    public class JsonStorage : IOpenableStorage
    {
        private JsonSerializerSettings _settings = new JsonSerializerSettings();
        private JObject _storage = new JObject();
        private String _file = null;
        private String _path = null;

        public JsonStorage()
        {
            //_settings.TypeNameHandling = TypeNameHandling.Objects;
        }

        ~JsonStorage()
        {
            StoreConfiguration();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            StoreConfiguration();
        }

        public void Open(String file)
        {
            _file = file;
            _path = Path.GetDirectoryName(_file);
            LoadConfiguration();
        }

        public T Get<T>(String id)
        {
            JToken obj;
            if(_storage.TryGetValue(id, out obj))
            {
                return JsonConvert.DeserializeObject<T>(obj.ToString(), _settings);
            }
            return default(T);
        }

        public T GetOrCreate<T>(String id)
            where T : new()
        {
            JToken obj;
            if(_storage.TryGetValue(id, out obj))
            {
                return JsonConvert.DeserializeObject<T>(obj.ToString(), _settings);
            }
            return new T();
        }

        public bool Exists(String id)
        {
            return _storage[id] != null;
        }

        public void Set<T>(String id, T obj)
        {
            _storage[id] = JToken.Parse(JsonConvert.SerializeObject(obj, Formatting.Indented, _settings));
            StoreConfiguration();
        }

        public bool Remove(String id)
        {
            return _storage.Remove(id);
        }

        private void LoadConfiguration()
        {
            if(!File.Exists(_file))
                return;

            using(TextReader textReader = new StreamReader(_file))
            {
                JsonTextReader reader = new JsonTextReader(textReader);
                _storage = JObject.Load(reader);
                reader.Close();
            }
        }

        private void StoreConfiguration()
        {
            if(_file == null)
                return;

            if(!Directory.Exists(_path))
                Directory.CreateDirectory(_path);

            using(TextWriter textWriter = new StreamWriter(_file))
            {
                JsonTextWriter writer = new JsonTextWriter(textWriter);
                writer.Formatting = Formatting.Indented;
                _storage.WriteTo(writer);
                writer.Close();
            }
        }
    }
}
