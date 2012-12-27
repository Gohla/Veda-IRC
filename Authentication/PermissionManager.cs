using System;
using System.Collections.Generic;
using Veda.Interface;
using Veda.Storage;

namespace Veda.Authentication
{
    public class PermissionManager : IPermissionManager
    {
        private readonly IStorageManager _storage;
        private readonly Dictionary<Tuple<IPlugin, String>, IPermission> _pluginPermissions = 
            new Dictionary<Tuple<IPlugin, String>, IPermission>();
        private readonly Dictionary<Tuple<IPlugin, String>, ICustomPermission> _pluginCustomPermissions =
            new Dictionary<Tuple<IPlugin, String>, ICustomPermission>();
        private readonly Dictionary<String, IPermission> _permissions = 
            new Dictionary<String, IPermission>();
        private readonly Dictionary<String, ICustomPermission> _customPermissions = 
            new Dictionary<String, ICustomPermission>();

        public PermissionManager(IStorageManager storage)
        {
            _storage = storage;
        }

        public void Dispose()
        {

        }

        public IPermission GetPermission(ICommand command)
        {
            String permission = command.Name + "(" + command.ParameterTypes.ToString(", ") + ")";
            return GetPermission(command.Plugin, permission);
        }

        public IPermission GetPermission(IPlugin plugin, String permission)
        {
            return _pluginPermissions.GetOrCreate(Tuple.Create(plugin, permission), 
                () => CreatePermission(plugin, permission));
        }

        public ICustomPermission GetCustomPermission(IPlugin plugin, String permission)
        {
            return _pluginCustomPermissions.GetOrCreate(Tuple.Create(plugin, permission),
                () => CreateCustomPermission(plugin, permission));
        }

        public IPermission GetPermission(String permission)
        {
            return _permissions.GetOrCreate(permission, () => CreatePermission(permission));
        }

        public ICustomPermission GetCustomPermission(String permission)
        {
            return _customPermissions.GetOrCreate(permission, () => CreateCustomPermission(permission));
        }

        private IPermission CreatePermission(IPlugin plugin, String permission)
        {
            return new Permission(_storage.Global(plugin), permission);
        }

        private IPermission CreatePermission(String permission)
        {
            return new Permission(_storage.Global(), permission);
        }

        private ICustomPermission CreateCustomPermission(IPlugin plugin, String permission)
        {
            return new CustomPermission(_storage.Global(plugin), permission);
        }

        private ICustomPermission CreateCustomPermission(String permission)
        {
            return new CustomPermission(_storage.Global(), permission);
        }
    }
}
