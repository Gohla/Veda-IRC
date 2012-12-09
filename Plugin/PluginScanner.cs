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
            PluginAttribute pluginAttribute = type.GetCustomAttribute<PluginAttribute>(true);
            if(pluginAttribute == null)
                return null;

            ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
            if(constructor == null)
                return null;

            object instance = constructor.Invoke(null);

            IEnumerable<Tuple<CommandAttribute, MethodInfo>> commandAttributes = ScanCommands(type.GetMethods());
            IEnumerable<ICommand> commands = commandAttributes
                .Select(t => ToCommand(t.Item1, t.Item2, instance))
                ;

            // TODO: retrieve dispose method.
            return new ScannedPlugin(pluginAttribute.Name, pluginAttribute.Description, commands, instance, null);
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

        private static ICommand ToCommand(CommandAttribute attribute, MethodInfo method, object instance)
        {
            if(method.IsStatic)
                return CommandBuilder.CreateCommand(attribute.Name, attribute.Description, method);
            else
                return CommandBuilder.CreateCommand(attribute.Name, attribute.Description, method, instance);
        }
    }
}
