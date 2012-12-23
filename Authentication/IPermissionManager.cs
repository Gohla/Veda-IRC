using System;
using Veda.Interface;

namespace Veda.Authentication
{
    public interface IPermissionManager : IPluginPermissionManager
    {
        IPermission GetPermission(String permission);
        ICustomPermission GetCustomPermission(String permission);
    }
}
