using System;
using System.Collections.Generic;
using Veda.Interface;

namespace Veda.Authentication
{
    public class CustomPermission<T> : ICustomPermission<T>
    {
        private readonly IStorage _storage;
        private T _defaultData;
        private T _data;

        public String Name { get; private set; }
        public IBotGroup Group { get; private set; }

        public T Value
        {
            get
            {
                if(_data == null)
                    return _defaultData;
                return _data;
            }
            set
            {
                _data = _storage.Create<T>(Value, Group.Name, Name);
            }
        }

        public CustomPermission(IStorage storage, String name, IBotGroup group)
        {
            _storage = storage;
            Name = name;
            Group = group;

            _data = _storage.GetCast<T>(Group.Name, Name);
        }

        public void Default(T value)
        {
            _defaultData = value;
        }
    }
}
