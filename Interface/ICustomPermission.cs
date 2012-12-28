using System;

namespace Veda.Interface
{
    public interface ICustomPermission
    {
        String Name { get; }
        IBotGroup Group { get; }
    }

    public interface ICustomPermission<T> : ICustomPermission
    {
        T Value { get; set; }

        void Default(T value);
    }
}
