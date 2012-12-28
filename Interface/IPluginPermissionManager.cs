using System;

namespace Veda.Interface
{
    public interface IPluginPermissionManager : IDisposable
    {
        IPermission GetPermission(ICommand command, IBotGroup group);
        IPermission GetPermission(IPlugin plugin, String name, IBotGroup group);
        ICustomPermission<T> GetCustomPermission<T>(IPlugin plugin, String name, IBotGroup group);
    }
}
