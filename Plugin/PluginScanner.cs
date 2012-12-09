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
            // Plugin info
            PluginAttribute pluginAttribute = type.GetCustomAttribute<PluginAttribute>(true);
            if(pluginAttribute == null)
                return null;
            String pluginName = pluginAttribute.Name ?? type.Name;

            // Instance
            ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
            object instance = constructor == null ? null : constructor.Invoke(null);

            ScannedPlugin plugin = new ScannedPlugin(pluginName, pluginAttribute.Description, instance, null);

            // Command info
            IEnumerable<Tuple<CommandAttribute, MethodInfo>> commandAttributes = ScanCommands(type.GetMethods());
            IEnumerable<ICommand> commands = commandAttributes
                .Select(t => ToCommand(plugin, t.Item1, t.Item2, instance))
                .Where(c => c != null)
                ;
            plugin.AddCommands(commands);

            return plugin;
        }

        private static IEnumerable<Tuple<CommandAttribute, MethodInfo>> ScanCommands(MethodInfo[] methods)
        {
            foreach(MethodInfo method in methods)
            {
                CommandAttribute attribute = method.GetCustomAttribute<CommandAttribute>(true);
                if(attribute != null)
                    yield return Tuple.Create(attribute, method);
            }
        }

        private static ICommand ToCommand(IPlugin plugin, CommandAttribute attribute, MethodInfo method, 
            object instance)
        {
            String methodName = attribute.Name ?? method.Name;
            ParameterInfo[] parameters = method.GetParameters();
            if(parameters.Length == 0 || !parameters[0].ParameterType.Equals(typeof(IContext)))
            {
                _logger.Error("Command " + methodName + " from plugin " + plugin.Name 
                    + " does not have a parameter of type IContext as first parameter. Command will not be added.");
                return null;
            }

            if(method.IsStatic)
                return CommandBuilder.CreateCommand(plugin, methodName, attribute.Description, method);
            else
                return CommandBuilder.CreateCommand(plugin, methodName, attribute.Description, method, instance);
        }
    }
}
