using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using Autofac;
using Autofac.Integration.Mef;
using Gohla.Shared.Composition;
using ReactiveIRC.Client;
using ReactiveIRC.Interface;
using Veda.Command;
using Veda.Storage;

namespace Veda.ConsoleServer
{
    public class AppBootstrapper
    {
        public AppBootstrapper()
        {
            var currentAssembly = Assembly.GetExecutingAssembly();
            var builder = new ContainerBuilder();
            var catalog = new AggregateCatalog();

            catalog.Catalogs.Add(new AssemblyCatalog(currentAssembly));
            builder.RegisterComposablePartCatalog(catalog);

            builder.RegisterAssemblyTypes(currentAssembly)
                .AsImplementedInterfaces();

            // Veda.Storage
            builder.RegisterType<JsonStorage>()
                .As<IStorage>()
                .Exported(x => x.As<IStorage>())
                .SingleInstance()
                ;
            builder.RegisterType<StorageManager>()
                .As<IStorageManager>()
                .Exported(x => x.As<IStorageManager>())
                .SingleInstance()
                ;

            // Veda.Command
            builder.RegisterType<CommandParser>()
                .As<ICommandParser>()
                .Exported(x => x.As<ICommandParser>())
                .SingleInstance()
                ;
            builder.RegisterType<CommandManager>()
                .As<ICommandManager>()
                .Exported(x => x.As<ICommandManager>())
                .SingleInstance()
                ;

            // ReactiveIRC.Client
            builder.RegisterType<Client>()
                .As<IClient>()
                .Exported(x => x.As<IClient>())
                .SingleInstance()
                ;

            CompositionManager.ConfigureDependencies(builder, catalog);
        }
    }
}
