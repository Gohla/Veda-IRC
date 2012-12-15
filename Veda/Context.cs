using ReactiveIRC.Interface;
using Veda.Interface;

namespace Veda
{
    internal class Context : IContext
    {
        public IBot Bot { get; set; }
        public ICommand Command { get; set; }
        public IStorage Storage { get; set; }
        public IReceiveMessage Message { get; set; }
        public IConversionContext ConversionContext { get; set; }
    }
}
