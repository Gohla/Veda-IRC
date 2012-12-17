using System;
using Veda.Interface;

namespace Veda.Authentication
{
    public class PermissionManager : IPermissionManager
    {
        public T GetPermission<T>(IPlugin plugin, IBotUser user, string permission)
        {
            throw new NotImplementedException();
        }

        public void SetPermission<T>(IPlugin plugin, IBotUser user, string permission, T obj)
        {
            throw new NotImplementedException();
        }

        public void SetPermission<T>(IPlugin plugin, IBotGroup group, string permission, T obj)
        {
            throw new NotImplementedException();
        }
    }
}
