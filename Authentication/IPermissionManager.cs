using System;
using Veda.Interface;

namespace Veda.Authentication
{
    public interface IPermissionManager : IPluginPermissionManager
    {
        IPermission GetPermission(String name, IBotGroup group);
        ICustomPermission<T> GetCustomPermission<T>(String name, IBotGroup group);
    }
}
