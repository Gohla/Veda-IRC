using System;

namespace Veda.Interface
{
    public interface IPluginPermissionManager : IDisposable
    {
        IPermission GetPermission(ICommand command);
        IPermission GetPermission(IPlugin plugin, String permission);
        ICustomPermission GetCustomPermission(IPlugin plugin, String permission);
    }
}
