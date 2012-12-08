using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using Autofac;
using Autofac.Integration.Mef;
using Gohla.Shared.Composition;
using ReactiveIRC.Client;
using ReactiveIRC.Interface;

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

            // ReactiveIRC client
            builder.RegisterType<Client>()
                .As<IClient>()
                .Exported(x => x.As<IClient>())
                .SingleInstance()
                ;

            CompositionManager.ConfigureDependencies(builder, catalog);
        }
    }
}
