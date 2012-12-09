using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Veda.Command;
using Veda.Interface;

namespace Veda.Plugin
{
    public static class PluginScanner
    {
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

            if(method.IsStatic)
                return CommandBuilder.CreateCommand(plugin, methodName, attribute.Description, method);
            else
                return CommandBuilder.CreateCommand(plugin, methodName, attribute.Description, method, instance);
        }
    }
}
