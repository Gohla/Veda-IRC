using System;
using System.Collections.Generic;
using Veda.Interface;
using Veda.Storage;

namespace Veda.Authentication
{
    public class PermissionManager : IPermissionManager
    {
        private readonly IStorageManager _storage;
        private readonly Dictionary<Tuple<IPlugin, String, IBotGroup>, IPermission> _pluginPermissions =
            new Dictionary<Tuple<IPlugin, String, IBotGroup>, IPermission>();
        private readonly Dictionary<Tuple<IPlugin, String, IBotGroup>, ICustomPermission> _pluginCustomPermissions =
            new Dictionary<Tuple<IPlugin, String, IBotGroup>, ICustomPermission>();
        private readonly Dictionary<Tuple<String, IBotGroup>, IPermission> _permissions =
            new Dictionary<Tuple<String, IBotGroup>, IPermission>();
        private readonly Dictionary<Tuple<String, IBotGroup>, ICustomPermission> _customPermissions =
            new Dictionary<Tuple<String, IBotGroup>, ICustomPermission>();

        public PermissionManager(IStorageManager storage)
        {
            _storage = storage;
        }

        public void Dispose()
        {

        }

        public IPermission GetPermission(ICommand command, IBotGroup group)
        {
            String permission = command.Name + "(" + command.ParameterTypes.ToString(", ") + ")";
            return GetPermission(command.Plugin, permission, group);
        }

        public IPermission GetPermission(IPlugin plugin, String name, IBotGroup group)
        {
            return _pluginPermissions.GetOrCreate(Tuple.Create(plugin, name, group),
                () => CreatePermission(plugin, name, group));
        }

        public ICustomPermission<T> GetCustomPermission<T>(IPlugin plugin, String name, IBotGroup group)
        {
            return _pluginCustomPermissions.GetOrCreate(Tuple.Create(plugin, name, group),
                () => CreateCustomPermission<T>(plugin, name, group)) as ICustomPermission<T>;
        }

        public bool HasPermission(ICommand command, IBotGroup group)
        {
            String permission = command.Name + "(" + command.ParameterTypes.ToString(", ") + ")";
            return HasPermission(command.Plugin, permission, group);
        }

        public bool HasPermission(IPlugin plugin, String name, IBotGroup group)
        {
            return _pluginPermissions.ContainsKey(Tuple.Create(plugin, name, group));
        }

        public bool HasCustomPermission(IPlugin plugin, String name, IBotGroup group)
        {
            return _pluginCustomPermissions.ContainsKey(Tuple.Create(plugin, name, group));
        }

        public IPermission GetPermission(String name, IBotGroup group)
        {
            return _permissions.GetOrCreate(Tuple.Create(name, group), () => CreatePermission(name, group));
        }

        public ICustomPermission<T> GetCustomPermission<T>(String name, IBotGroup group)
        {
            return _customPermissions.GetOrCreate(Tuple.Create(name, group),
                () => CreateCustomPermission<T>(name, group)) as ICustomPermission<T>;
        }

        private IPermission CreatePermission(IPlugin plugin, String name, IBotGroup group)
        {
            return new Permission(_storage.Global(plugin), name, group);
        }

        private IPermission CreatePermission(String name, IBotGroup group)
        {
            return new Permission(_storage.Global(), name, group);
        }

        private ICustomPermission CreateCustomPermission<T>(IPlugin plugin, String name, IBotGroup group)
        {
            return new CustomPermission<T>(_storage.Global(plugin), name, group);
        }

        private ICustomPermission CreateCustomPermission<T>(String name, IBotGroup group)
        {
            return new CustomPermission<T>(_storage.Global(), name, group);
        }
    }
}
