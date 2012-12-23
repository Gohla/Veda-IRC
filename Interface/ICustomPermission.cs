using System;

namespace Veda.Interface
{
    public interface ICustomPermission
    {
        void Default(IBotGroup group, object value);
        T Get<T>(IBotUser user);
        object Get(IBotUser user);
    }
}
