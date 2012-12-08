using System;
using Gohla.Shared.Composition;
using ReactiveIRC.Interface;

namespace Veda.ConsoleServer
{
    class Program
    {
        static void Main(String[] args)
        {
            AppBootstrapper bootstrapper = new AppBootstrapper();

            IClient client = CompositionManager.Get<IClient>();
            Bot bot = new Bot(client);
        }
    }
}
