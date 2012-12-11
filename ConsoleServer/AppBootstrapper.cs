﻿using System;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using Autofac;
using Autofac.Integration.Mef;
using Gohla.Shared.Composition;
using ReactiveIRC.Client;
using ReactiveIRC.Interface;
using Veda.Command;
using Veda.Interface;
using Veda.Plugin;
using Veda.Storage;

namespace Veda.ConsoleServer
{
    public class AppBootstrapper
    {
        public AppBootstrapper()
        {
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            ContainerBuilder builder = new ContainerBuilder();
            AggregateCatalog catalog = new AggregateCatalog();

            catalog.Catalogs.Add(new AssemblyCatalog(currentAssembly));
            builder.RegisterComposablePartCatalog(catalog);

            builder.RegisterAssemblyTypes(currentAssembly)
                .AsImplementedInterfaces();

            // ReactiveIRC.Client
            builder.RegisterType<Client>()
                .As<IClient>()
                .Exported(x => x.As<IClient>())
                .SingleInstance()
                ;

            // Veda.Storage
            builder.RegisterType<JsonStorage>()
                .As<IStorage>()
                .Exported(x => x.As<IStorage>())
                .SingleInstance()
                ;
            builder.Register((c, p) => new StorageManager(p.Named<String>("path"), p.Named<String>("extension"),
                p.Named<String>("globalFile")))
                .As<IStorageManager>()
                .Exported(x => x.As<IStorageManager>())
                .SingleInstance()
                ;

            // Veda.Plugin
            builder.RegisterType<PluginManager>()
                .As<IPluginManager>()
                .Exported(x => x.As<IPluginManager>())
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

            // Veda
            builder.RegisterType<Bot>()
                .As<IBot>()
                .Exported(x => x.As<IBot>())
                .SingleInstance()
                ;

            CompositionManager.ConfigureDependencies(builder, catalog);
        }
    }
}
