using System;
using System.Collections.Generic;
using Veda.Interface;

namespace Veda.Authentication
{
    public class CustomPermission : ICustomPermission
    {
        private readonly IStorage _storage;
        private readonly String _permission;
        private readonly Dictionary<String, object> _data;

        public CustomPermission(IStorage storage, String permission)
        {
            _storage = storage;
            _permission = permission;

            _data = _storage.GetOrCreate<Dictionary<String, object>>(_permission);
        }

        public void Default(IBotGroup group, object value)
        {
            if(!_data.ContainsKey(group.Name))
                _data.Add(group.Name, value);
        }

        public T Get<T>(IBotUser user)
        {
            return (T)Get(user);
        }

        public object Get(IBotUser user)
        {
            IBotGroup group = user.Group;
            if(!_data.ContainsKey(group.Name))
                return null;
            return _data[group.Name];
        }
    }
}
