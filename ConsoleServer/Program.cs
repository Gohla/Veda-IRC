using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Autofac;
using Gohla.Shared.Composition;
using NLog;
using NLog.Config;
using NLog.Targets;
using ReactiveIRC.Interface;
using Veda.Command;
using Veda.Interface;
using Veda.Storage;

namespace Veda.ConsoleServer
{
    public static class Program
    {
        private static readonly String _applicationDataDirectory = "Veda";
        private static readonly String _storagePath = "Storage";
        private static readonly String _storageExtension = "json";
        private static readonly String _globalStorageFile = "Config";
        private static readonly String _logFile = "Log.txt";

        private static AppBootstrapper _bootstrapper;

        public static bool Portable { get; private set; }
        public static String BasePath { get; private set; }

        private static void Main(String[] args)
        {
            // Find out if in portable mode.
            String portablePath = AppDomain.CurrentDomain.BaseDirectory;
            String applicationDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                _applicationDataDirectory);
            String portableStorage = Path.Combine(portablePath, _storagePath, _globalStorageFile);
            String applicationDataStorage = Path.Combine(applicationDataPath, _storagePath, _globalStorageFile);

            bool portable = File.Exists(portableStorage);
            bool nonPortable = File.Exists(applicationDataStorage);
            Portable = portable || !nonPortable;

            if(Portable)
            {
                BasePath = portablePath;
            }
            else
            {
                BasePath = applicationDataPath;
            }

            // Add file logging
            FileTarget fileTarget = new FileTarget();
            fileTarget.Layout =
                "${longdate}|${level:uppercase=true}|${logger}|${message}|${exception:maxInnerExceptionLevel=5}";
            fileTarget.FileName = Path.Combine(BasePath, _logFile);
            LoggingRule fileRule = new LoggingRule("*", LogLevel.Warn, fileTarget);
            LogManager.Configuration.AddTarget("File", fileTarget);
            LogManager.Configuration.LoggingRules.Add(fileRule);
            LogManager.Configuration.Reload();

            // Bootstrap dependencies
            _bootstrapper = new AppBootstrapper();

            // Open configuration
            IStorageManager storage = CompositionManager.Get<IStorageManager>(new NamedParameter("path",
                Path.Combine(BasePath, _storagePath)), new NamedParameter("extension", _storageExtension),
                new NamedParameter("globalFile", _globalStorageFile));

            // Create command manager
            ICommandManager command = CompositionManager.Get<ICommandManager>();
            command.Add(CommandBuilder.CreateConverter<IEnumerable<ICommand>, ConversionContext>(
                (str, context) => context.Bot.Command.GetUnambigousCommands(str))
            );
            command.Add(CommandBuilder.CreateConverter<IPlugin, ConversionContext>(
                (str, context) => context.Bot.Plugin.Get(str))
            );
            command.Add(CommandBuilder.CreateConverter<IChannel, ConversionContext>(
                (str, context) => context.Message.Connection.GetExistingChannel(str))
            );

            // Create plugin manager
            IPluginManager plugin = CompositionManager.Get<IPluginManager>();
            GetAssemblies().Do(x => plugin.Load(x));

            // Create bot
            IClient client = CompositionManager.Get<IClient>();
            IBot bot = new Bot(client, storage, command, plugin);
            if(bot.Connections.IsEmpty())
            {
                Console.WriteLine("No connections have been set up yet, adding a connection now.");

                Console.WriteLine("Server address?");
                String address = Console.ReadLine();
                Console.WriteLine("Server port?");
                ushort port = Convert.ToUInt16(Console.ReadLine());

                Console.WriteLine("Nickname?");
                String nickname = Console.ReadLine();
                Console.WriteLine("Username?");
                String username = Console.ReadLine();
                Console.WriteLine("Real name?");
                String realname = Console.ReadLine();
                Console.WriteLine("Password? (leave empty for no password)");
                String password = Console.ReadLine();

                bot.Connect(address, port, nickname, username, realname, password);
            }

            // Loop
            bool run = true;
            Console.CancelKeyPress += (s, e) => { e.Cancel = true; run = false; };
            while(run)
            {
                Thread.Sleep(50);
            }

            // Clean up
            bot.Dispose();
            storage.Dispose();
        }

        private static IEnumerable<Assembly> GetAssemblies()
        {
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .ToList()
                ;
            var loadedPaths = loadedAssemblies
                .Where(a => !a.IsDynamic)
                .Select(a => a.Location)
                .ToArray()
                ;

            var referencedPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
            var toLoad = referencedPaths
                .Where(r => !loadedPaths.Contains(r, StringComparer.InvariantCultureIgnoreCase))
                .ToList()
                ;
            toLoad
                .ForEach(path => loadedAssemblies.Add(AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(path))))
                ;

            return AppDomain.CurrentDomain.GetAssemblies();
        }
    }
}
