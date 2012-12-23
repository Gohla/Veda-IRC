using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NLog;
using Veda.Command;
using Veda.Interface;

namespace Veda.Plugin
{
    public static class PluginScanner
    {
        private static readonly Logger _logger = LogManager.GetLogger("PluginScanner");

        public static IEnumerable<ScannedPlugin> Scan(Assembly assembly)
        {
            foreach(Type type in assembly.GetTypes())
            {
                ScannedPlugin plugin = Scan(type);
                if(plugin != null)
                    yield return plugin;
            }
        }

        public static ScannedPlugin Scan(Type type)
        {
            // Find plugin information
            PluginAttribute pluginAttribute = type.GetCustomAttribute<PluginAttribute>(true);
            if(pluginAttribute == null)
                return null;
            String pluginName = pluginAttribute.Name ?? type.Name;

            // Construct instance of the plugin if it has a default constructor.
            ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
            object instance = constructor == null ? null : constructor.Invoke(null);

            // Find dispose method
            Action disposeAction = null;
            if(instance != null && typeof(IDisposable).IsAssignableFrom(type))
            {
                disposeAction = () => type.GetMethod("Dispose", Type.EmptyTypes).Invoke(instance, null);
            }

            MethodInfo[] methods = type.GetMethods();

            // Find loaded method
            MethodInfo loadedMethod = FindLoaded(methods, pluginName);
            Action<IPlugin, IBot> loadedAction = null;
            if(loadedMethod != null)
                loadedAction = (p, b) => loadedMethod.Invoke(instance, new object[] { p, b });

            ScannedPlugin plugin = new ScannedPlugin(pluginName, pluginAttribute.Description, type, instance, 
                loadedAction, disposeAction);

            // Find commands
            IEnumerable<Tuple<CommandAttribute, MethodInfo>> commandAttributes = FindCommands(methods);
            IEnumerable<ICommand> commands = commandAttributes
                .Select(t => ToCommand(plugin, t.Item1, t.Item2, instance))
                .Where(c => c != null)
                ;
            plugin.AddCommands(commands);

            return plugin;
        }

        private static MethodInfo FindLoaded(MethodInfo[] methods, String pluginName)
        {
            foreach(MethodInfo method in methods)
            {
                LoadedAttribute attribute = method.GetCustomAttribute<LoadedAttribute>(true);
                if(attribute != null)
                {
                    ParameterInfo[] parameters = method.GetParameters();

                    if(!method.IsPublic)
                        _logger.Error("Loaded method " + method.Name + " from plugin " + pluginName
                            + " is not public so it is not callable. Loaded method will not be used.");
                    else if(parameters.Length != 2 || !parameters[0].ParameterType.Equals(typeof(IPlugin))
                        || !parameters[1].ParameterType.Equals(typeof(IBot)))
                        _logger.Error("Loaded method " + method.Name + " from plugin " + pluginName
                            + " does not have signature IPlugin, IBot. Loaded method will not be used.");
                    else
                        return method;
                }
            }
            return null;
        }

        private static IEnumerable<Tuple<CommandAttribute, MethodInfo>> FindCommands(MethodInfo[] methods)
        {
            foreach(MethodInfo method in methods)
            {
                CommandAttribute attribute = method.GetCustomAttribute<CommandAttribute>(true);
                if(attribute != null)
                {
                    yield return Tuple.Create(attribute, method);
                }
            }
        }

        private static ICommand ToCommand(IPlugin plugin, CommandAttribute attribute, MethodInfo method,
            object instance)
        {
            String methodName = attribute.Name ?? method.Name;
            ParameterInfo[] parameters = method.GetParameters();

            // Check signature
            if(parameters.Length == 0 || !parameters[0].ParameterType.Equals(typeof(IContext)))
            {
                _logger.Error("Command " + methodName + " from plugin " + plugin.Name
                    + " does not have a parameter of type IContext as first parameter. Command will not be added.");
                return null;
            }
            else if(!method.IsPublic)
            {
                _logger.Error("Command " + methodName + " from plugin " + plugin.Name
                    + " is not public so it is not callable. Command will not be added.");
                return null;
            }

            // Gather default permissions
            PermissionAttribute[] permissions = new PermissionAttribute[0];
            IEnumerable<PermissionAttribute> defaultPermissions = method.GetCustomAttributes<PermissionAttribute>(true);
            IEnumerable<String> groupNames = defaultPermissions.Select(p => p.GroupName);
            if(groupNames.Distinct().Count() != defaultPermissions.Count())
            {
                _logger.Error("Command " + methodName + " from plugin " + plugin.Name
                    + " has multiple default permissions for the same group. Permissions will not be added.");
            }
            else
            {
                permissions = defaultPermissions.ToArray();
            }

            if(method.IsStatic)
                return CommandBuilder.CreateCommand(plugin, methodName, attribute.Description, permissions, 
                    attribute.Private, method);
            else
                return CommandBuilder.CreateCommand(plugin, methodName, attribute.Description, permissions, 
                    attribute.Private, method, instance);
        }
    }
}
