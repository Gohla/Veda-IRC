using ReactiveIRC.Interface;
using Veda.Interface;

namespace Veda
{
    internal class Context : IContext
    {
        public IBot Bot { get; set; }
        public IClientConnection Connection { get; set; }
        public IReceiveMessage Message { get; set; }
    }
}
